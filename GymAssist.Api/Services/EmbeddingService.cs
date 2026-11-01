using OpenAI.Embeddings;

namespace GymAssist.Api.Services
{
    public class EmbeddingService
    {
        private readonly EmbeddingClient _client;

        public EmbeddingService(IConfiguration configuration)
        {
            var apiKey = configuration["OpenAI:ApiKey"]!;

            var model = configuration["OpenAI:EmbeddingModel"]!;

            _client = new EmbeddingClient(
                model,
                apiKey
            );
        }

        public async Task<float[]> Gerar(string texto)
        {
            var response = await _client.GenerateEmbeddingAsync(texto);

            return response.Value
                .ToFloats()
                .ToArray();
        }
    }
}
