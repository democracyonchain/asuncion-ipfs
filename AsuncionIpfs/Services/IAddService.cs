using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AsuncionIpfs.Models.IPFS;

namespace AsuncionIpfs.Services
{
    public interface IAddService
    {
        Task<AddContentResponse> PostAddAsync(FileStream stream, CancellationToken cancellationToken = default);

        Task<PinStateContentResponse> PostPinAddAsync(string ipfsPath, CancellationToken cancellationToken = default);

        Task<object> GetGatewayAsync(string ipfsPath, CancellationToken cancellationToken = default);
    }
}
