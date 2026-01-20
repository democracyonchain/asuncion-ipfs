using AsuncionIpfs.Services.Cardano;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

using AsuncionIpfs.Dtos;


namespace AsuncionIpfs.Controllers
{
    [ApiController]
    [Route("api/traceability")]
    public class TraceabilityController : ControllerBase
    {
        private readonly BlockfrostService _blockfrost;

        public TraceabilityController(BlockfrostService blockfrost)
        {
            _blockfrost = blockfrost;
        }

        [HttpGet("tx-metadata")]
        public async Task<IActionResult> TxMetadata([FromQuery] string txHash, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(txHash))
                return BadRequest(new { message = "txHash es requerido" });

            JsonElement raw = await _blockfrost.GetTxMetadataRawAsync(txHash, ct);

            // Normalizamos a DTO: label + json
            var labels = new List<TxMetadataLabelDto>();

            foreach (var item in raw.EnumerateArray())
            {
                var label = item.TryGetProperty("label", out var l) ? (l.GetString() ?? "") : "";
                object? json = null;

                if (item.TryGetProperty("json_metadata", out var jm))
                    json = JsonSerializer.Deserialize<object>(jm.GetRawText());

                labels.Add(new TxMetadataLabelDto
                {
                    label = label,
                    json = json
                });
            }

            var dto = new TxMetadataDto
            {
                txHash = txHash,
                network = _blockfrost.GetNetworkName(),
                labels = labels
            };

            return Ok(dto);
        }
    }
}
