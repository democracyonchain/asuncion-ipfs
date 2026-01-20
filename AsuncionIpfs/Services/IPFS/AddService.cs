using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AsuncionIpfs.Models.IPFS;
using AsuncionIpfs.Services;

namespace AsuncionIpfs.Services.IPFS
{
    public class AddService : IAddService
    {
        private readonly HttpClient _http;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AddService(HttpClient http)
        {
            _http = http;
        }

        // POST https://ipfs.blockfrost.io/api/v0/ipfs/add
        public async Task<AddContentResponse> PostAddAsync(FileStream stream, CancellationToken cancellationToken = default)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            using var form = new MultipartFormDataContent();
            using var fileContent = new StreamContent(stream);
            form.Add(fileContent, "file", "uploaded_file");

            using var resp = await _http.PostAsync("ipfs/add", form, cancellationToken).ConfigureAwait(false);
            var body = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"IPFS add falló ({(int)resp.StatusCode}): {body}");

            var result = JsonSerializer.Deserialize<AddContentResponse>(body, JsonOpts);
            return result ?? throw new Exception("Respuesta nula en /ipfs/add");
        }

        // POST https://ipfs.blockfrost.io/api/v0/ipfs/pin/add/{ipfsPath}
        public async Task<PinStateContentResponse> PostPinAddAsync(string ipfsPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ipfsPath))
                throw new ArgumentException("ipfsPath requerido", nameof(ipfsPath));

            using var content = new StringContent(string.Empty);
            var safePath = Uri.EscapeDataString(ipfsPath);

            using var resp = await _http.PostAsync($"ipfs/pin/add/{safePath}", content, cancellationToken).ConfigureAwait(false);
            var body = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"IPFS pin add falló ({(int)resp.StatusCode}): {body}");

            var result = JsonSerializer.Deserialize<PinStateContentResponse>(body, JsonOpts);
            return result ?? throw new Exception("Respuesta nula en /ipfs/pin/add");
        }

        // GET https://ipfs.blockfrost.io/api/v0/ipfs/gateway/{ipfsPath}
        // OJO: tu controller espera "object" pero maneja byte[]; devolvemos byte[].
        public async Task<object> GetGatewayAsync(string ipfsPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ipfsPath))
                throw new ArgumentException("ipfsPath requerido", nameof(ipfsPath));

            var safePath = Uri.EscapeDataString(ipfsPath);

            using var resp = await _http.GetAsync($"ipfs/gateway/{safePath}", cancellationToken).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new HttpRequestException($"IPFS gateway falló ({(int)resp.StatusCode}): {body}");
            }

            // Blockfrost gateway normalmente devuelve bytes (octet-stream / image/*)
            var bytes = await resp.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            return bytes;
        }
    }
}
