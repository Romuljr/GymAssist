# GymAssist API

API para um assistente virtual inteligente especializado em musculação, treinamento físico e conhecimentos relacionados ao universo fitness.

O projeto utiliza **.NET, OpenAI e PostgreSQL com pgvector** para desenvolver um agente capaz de interpretar perguntas, recuperar informações relevantes de uma base de conhecimentos e gerar respostas contextualizadas.

## Tecnologias utilizadas

- **C# / .NET 10** – Desenvolvimento da API.
- **OpenAI GPT-5.4 nano** – Processamento de perguntas e geração de respostas.
- **text-embedding-3-small** – Geração de embeddings.
- **PostgreSQL** – Armazenamento de dados.
- **pgvector** – Armazenamento e busca por similaridade vetorial.
- **Npgsql** – Comunicação com o banco de dados.
- **Swagger** – Documentação e testes dos endpoints.

## Funcionalidades implementadas

- Cadastro e ingestão de conhecimentos na base de dados.
- Geração de embeddings a partir dos conteúdos e perguntas.
- Busca semântica dos três conhecimentos mais relevantes.
- Arquitetura RAG (Retrieval-Augmented Generation).
- Engenharia de prompt para orientar o comportamento do agente.
- Structured Outputs com JSON Schema.
- Classificação automática de assuntos.
- Identificação de informações disponíveis na base de conhecimentos.
- Monitoramento do consumo de tokens e cálculo de custos por interação.
- Registro dos consumos no PostgreSQL.

## Como funciona

1. O usuário envia uma pergunta para a API.
2. A pergunta é convertida em embedding.
3. O PostgreSQL com pgvector realiza uma busca semântica na base de conhecimentos.
4. Os três registros mais relevantes são recuperados e utilizados como contexto.
5. O contexto é enviado ao modelo GPT-5.4 nano junto às instruções do agente.
6. O modelo retorna uma resposta estruturada.
7. O consumo de tokens e os custos da interação são registrados no banco de dados.

## Próximos passos

- Implementar memória conversacional utilizando MongoDB.
- Desenvolver chamadas de funções (*Function Calling*).
- Ampliar o monitoramento de custos, incluindo o consumo de embeddings.

## Objetivo

Explorar o desenvolvimento de agentes inteligentes com .NET, aplicando conceitos de engenharia de prompt, RAG, busca vetorial, integração com modelos de linguagem e persistência de dados.