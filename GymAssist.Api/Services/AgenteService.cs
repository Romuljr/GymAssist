
using GymAssist.Api.Dtos;
using Npgsql;
using OpenAI.Chat;
using Pgvector;
using System.Text.Json;

namespace GymAssist.Api.Services
{
    public class AgenteService
    {
        private readonly EmbeddingService _embeddingService;
        private readonly NpgsqlDataSource _dataSource;
        private readonly ChatClient _chatClient;

        // Configurações para controle de tokens e custos
        private readonly string _model;
        private readonly decimal _tokensBase;
        private readonly decimal _precoEntrada;
        private readonly decimal _precoSaida;

        public AgenteService(
            EmbeddingService embeddingService,
            NpgsqlDataSource dataSource,
            IConfiguration configuration)
        {
            _embeddingService = embeddingService;
            _dataSource = dataSource;

            var apiKey = configuration["OpenAI:ApiKey"]!;
            _model = configuration["OpenAI:ChatModel"]!;

            _chatClient = new ChatClient(_model, apiKey);

            // Configurações de preços dos tokens
            _tokensBase = configuration.GetValue<decimal>("Tokens:Base");
            _precoEntrada = configuration.GetValue<decimal>("Tokens:PrecoEntrada");
            _precoSaida = configuration.GetValue<decimal>("Tokens:PrecoSaida");
        }

        public async Task<AgenteResponse> ProcessarPergunta(
            AgenteRequest request)
        {
            #region Converter a pergunta enviada em Embedding

            var embedding = await _embeddingService.Gerar(request.pergunta);

            #endregion

            #region Procurar no banco vetorial os registros que mais se assemelham à pergunta

            var contexto = await BuscarContexto(embedding);

            #endregion

            #region Engenharia de prompt

            var mensagens = new List<ChatMessage>
            {
                new SystemChatMessage("""
                    Você é o GymAssist, um assistente virtual especializado em
                    musculação, treinamento físico e conhecimentos relacionados
                    à prática de atividades físicas.

                    Sua função é auxiliar os usuários com dúvidas, orientações
                    e informações relacionadas ao universo fitness.

                    Utilize exclusivamente as informações fornecidas no contexto
                    da base de conhecimentos para elaborar suas respostas.

                    Regras:

                    1. Seja educado, objetivo, acolhedor e profissional.
                       Trate o usuário de maneira respeitosa e amigável.

                    2. Responda de forma clara e natural, utilizando uma linguagem
                       simples e acessível, evitando termos técnicos desnecessários.

                    3. Não invente informações, recomendações ou dados que não
                       estejam presentes no contexto fornecido.

                    4. Não invente exercícios, séries, repetições, técnicas de
                       treinamento ou orientações nutricionais.

                    5. Caso a informação solicitada não esteja disponível no
                       contexto, informe educadamente que não possui essa
                       informação no momento.

                    6. Não realize diagnósticos médicos, não prescreva
                       medicamentos e não substitua a orientação de profissionais
                       de saúde, nutricionistas ou educadores físicos.

                    7. Não recomende práticas que possam colocar a saúde ou
                       integridade física do usuário em risco.

                    8. Não mencione termos técnicos como RAG, embeddings,
                       chunks, banco vetorial ou contexto para o usuário.

                    9. Responda somente à pergunta realizada pelo usuário.

                    10. Não escreva informações adicionais que não sejam
                        necessárias para responder à pergunta.

                    11. Mesmo que existam registros no contexto, caso eles
                        não respondam à pergunta realizada, defina
                        informacaoEncontrada como false.

                    12. Quando o contexto possuir informações suficientes
                        para responder à pergunta, defina
                        informacaoEncontrada como true.

                    13. Classifique o assunto da pergunta utilizando
                        exclusivamente uma das seguintes categorias:
                        TREINO, EXERCICIO, ALIMENTACAO, RECUPERACAO ou OUTRO.

                    14. Quando a pergunta não estiver relacionada ao universo
                        fitness, classifique o assunto como OUTRO e informe
                        educadamente o escopo de atuação do GymAssist.

                    15. Organize a resposta de acordo com a estrutura
                        AgenteResponse, preenchendo corretamente os campos:
                        assunto, mensagem e informacaoEncontrada.

                    16. Não inclua informações adicionais fora da estrutura
                        de resposta solicitada.

                    17. Nunca apresente informações como se fossem provenientes
                        da base de conhecimentos quando não estiverem disponíveis
                        no contexto fornecido.

                    18. Seu objetivo é oferecer respostas confiáveis, claras
                        e úteis, auxiliando os usuários em sua jornada fitness.
                    """),

                new UserChatMessage($"""
                    CONTEXTO DA BASE DE CONHECIMENTO:
                    {contexto}

                    PERGUNTA DO USUÁRIO:
                    Nome: {request.nomeUsuario}
                    Pergunta: {request.pergunta}

                    Com base exclusivamente no contexto apresentado acima,
                    elabore uma resposta para o usuário.

                    Retorne a resposta seguindo rigorosamente a estrutura
                    AgenteResponse.
                    """)
            };

            #endregion

            #region Definir a estrutura JSON de resposta

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat
                    .CreateJsonSchemaFormat(

                        jsonSchemaFormatName: "gymassist_response",

                        jsonSchema: BinaryData.FromBytes(
                        """
                        {
                            "type": "object",
                            "properties": {
                                "assunto": {
                                    "type": "string",
                                    "enum": [
                                        "TREINO",
                                        "EXERCICIO",
                                        "ALIMENTACAO",
                                        "RECUPERACAO",
                                        "OUTRO"
                                    ]
                                },
                                "mensagem": {
                                    "type": "string"
                                },
                                "informacaoEncontrada": {
                                    "type": "boolean"
                                }
                            },
                            "required": [
                                "assunto",
                                "mensagem",
                                "informacaoEncontrada"
                            ],
                            "additionalProperties": false
                        }
                        """u8.ToArray()
                        ),
                        jsonSchemaIsStrict: true
                    )
            };

            #endregion

            #region Enviar prompt para o modelo e obter a resposta

            var response = await _chatClient.CompleteChatAsync(
                mensagens, options);

            #endregion

            #region Capturar tokens, calcular custos e gravar consumo

            var tokensEntrada = response.Value.Usage.InputTokenCount;
            var tokensSaida = response.Value.Usage.OutputTokenCount;
            var totalTokens = response.Value.Usage.TotalTokenCount;

            var custos = CalcularCusto(tokensEntrada, tokensSaida);

            await GravarConsumo(
                request,
                tokensEntrada,
                tokensSaida,
                totalTokens,
                custos.custoEntrada,
                custos.custoSaida,
                custos.custoTotal
            );

            #endregion

            #region Retornar a resposta estruturada

            var json = response.Value.Content[0].Text;

            var data = JsonSerializer.Deserialize<AgenteResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return data!;

            #endregion
        }

        public async Task<string> BuscarContexto(float[] embedding)
        {
            await using var connection =
                await _dataSource.OpenConnectionAsync();

            const string sql = """
                SELECT texto
                FROM conhecimentos
                ORDER BY embedding <=> @embedding
                LIMIT 3
                """;

            await using var command =
                new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "embedding",
                new Vector(embedding)
            );

            await using var reader = await command.ExecuteReaderAsync();

            var conhecimentos = new List<string>();

            while (await reader.ReadAsync())
            {
                conhecimentos.Add(reader.GetString(0));
            }

            return string.Join("\n", conhecimentos);
        }

        #region Calcular custo dos tokens

        private (decimal custoEntrada, decimal custoSaida, decimal custoTotal)
            CalcularCusto(int tokensEntrada, int tokensSaida)
        {
            var custoEntrada =
                (tokensEntrada / _tokensBase) * _precoEntrada;

            var custoSaida =
                (tokensSaida / _tokensBase) * _precoSaida;

            var custoTotal = custoEntrada + custoSaida;

            return (
                custoEntrada,
                custoSaida,
                custoTotal
            );
        }

        #endregion

        #region Gravar consumo de tokens no banco de dados

        private async Task GravarConsumo(
            AgenteRequest request,
            int tokensEntrada,
            int tokensSaida,
            int totalTokens,
            decimal custoEntrada,
            decimal custoSaida,
            decimal custoTotal)
        {
            await using var connection =
                await _dataSource.OpenConnectionAsync();

            const string sql = """
                INSERT INTO consumos_tokens
                (
                    id,
                    nome_usuario,
                    pergunta,
                    modelo,
                    tokens_entrada,
                    tokens_saida,
                    total_tokens,
                    custo_entrada,
                    custo_saida,
                    custo_total
                )
                VALUES
                (
                    @id,
                    @nomeUsuario,
                    @pergunta,
                    @modelo,
                    @tokensEntrada,
                    @tokensSaida,
                    @totalTokens,
                    @custoEntrada,
                    @custoSaida,
                    @custoTotal
                );
                """;

            await using var command =
                new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", Guid.NewGuid());

            command.Parameters.AddWithValue(
                "nomeUsuario",
                (object?)request.nomeUsuario ?? DBNull.Value
            );

            command.Parameters.AddWithValue(
                "pergunta",
                request.pergunta
            );

            command.Parameters.AddWithValue("modelo", _model);

            command.Parameters.AddWithValue(
                "tokensEntrada",
                tokensEntrada
            );

            command.Parameters.AddWithValue(
                "tokensSaida",
                tokensSaida
            );

            command.Parameters.AddWithValue(
                "totalTokens",
                totalTokens
            );

            command.Parameters.AddWithValue(
                "custoEntrada",
                custoEntrada
            );

            command.Parameters.AddWithValue(
                "custoSaida",
                custoSaida
            );

            command.Parameters.AddWithValue(
                "custoTotal",
                custoTotal
            );

            await command.ExecuteNonQueryAsync();
        }

        #endregion
    }
}
