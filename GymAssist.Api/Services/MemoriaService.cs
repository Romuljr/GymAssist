using GymAssist.Api.Entities;
using MongoDB.Driver;
using System.Text;

namespace GymAssist.Api.Services
{
    public class MemoriaService
    {
        private readonly IMongoCollection<Conversa> _collection;

        public MemoriaService(IMongoDatabase database, IConfiguration configuration)
        {
            var collectionName = configuration["MongoDB:Collection"]!;

            _collection = database.GetCollection<Conversa>(collectionName);
        }

        public async Task Gravar(string nomeUsuario, string pergunta, string reposta)
        {
            var conversa = new Conversa
            {
                Id = Guid.NewGuid(),
                NomeUsuario = nomeUsuario,
                Pergunta = pergunta,
                Resposta = reposta,
                DataHora = DateTime.UtcNow
            };

            await _collection.InsertOneAsync(conversa);
        }

        public async Task<List<Conversa>> BuscarUltimasConversas(string nomeUsuario, int quantidade = 3)
        {
            var filter = Builders<Conversa>.Filter.Eq(c => c.NomeUsuario, nomeUsuario);

            var conversas = await _collection.Find(filter).SortByDescending(c => c.DataHora).Limit(quantidade).ToListAsync();

            conversas.Reverse();

            return conversas;   
        }

        public async Task<string> BuscarHistorico(string nomeUsuario)
        {
            var conversas = await BuscarUltimasConversas(nomeUsuario);

            if (conversas.Count == 0)
                return "Nenhuma conversa anterior";

            var historico = new StringBuilder();

            foreach ( var conversa in conversas)
            {
                historico.AppendLine($"Usuario: {conversa.Pergunta}");

                historico.AppendLine($"Atendente: {conversa.Resposta}");

                historico.AppendLine();
            }

            return historico.ToString();
        }
    }
}
