using System.Text.Json.Serialization;

namespace GymAssist.Api.Dtos
{
    public record AgenteResponse(
        AssuntoAtendimento assunto,
        string mensagem,
        bool interesseIdentificado,
        bool encaminharParaAtendente,
        bool informacaoEncontrada
    );

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AssuntoAtendimento
    {
        TREINO,
        EXERCICIO,
        ALIMENTACAO,
        RECUPERACAO,
        OUTRO
    }
}
