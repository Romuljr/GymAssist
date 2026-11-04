using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GymAssist.Api.Entities
{
    public class Conversa
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("nomeUsuario")]
        public string NomeUsuario { get; set; } = string.Empty;

        [BsonElement("Pergunta")]
        public string Pergunta { get; set; } = string.Empty;

        [BsonElement("resposta")]
        public string Resposta { get; set; } = string.Empty;

        [BsonElement("dataHora")]
        public DateTime DataHora { get; set; }
    }
}
