using System.Net.Http.Headers;
using System.Text.Json;
namespace AsuncionIpfs.Services.Cardano
{
    public class BlockfrostService
    {
        private readonly HttpClient _http;
        private readonly string _network;

        public BlockfrostService(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _network = (cfg["Blockfrost:Network"] ?? "preview").ToLowerInvariant();
        }

        public string GetNetworkName() => _network;

        /// <summary>
        /// Blockfrost devuelve:
        /// [{ "label": "674", "json_metadata": {...} }, ...]
        /// </summary>
        public async Task<JsonElement> GetTxMetadataRawAsync(string txHash, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(txHash))
                throw new ArgumentException("txHash es requerido", nameof(txHash));

            var res = await _http.GetAsync($"txs/{txHash}/metadata", ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new Exception($"Blockfrost error ({(int)res.StatusCode}): {body}");

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.Clone();
        }
    }
}
