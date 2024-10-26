﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AsuncionIpfs.Models.IPFS;
using Blockfrost.Api;
using Blockfrost.Api.Http;
using Blockfrost.Api.Models;
using Blockfrost.Api.Services;

namespace AsuncionIpfs.Services
{
    public partial interface IAddService : IBlockfrostService
    {
        IHealthService Health { get; set; }
        IMetricsService Metrics { get; set; }

        /// <summary>
        ///     Add a file to IPFS <c>/ipfs/add</c>
        /// </summary>
        /// <remarks>
        ///     See also <seealso href="https://docs.blockfrost.io/#tag/IPFS-Add/paths/~1ipfs~1add/post">/ipfs/add</seealso> on docs.blockfrost.io
        /// </remarks>
        /// <returns>Returns information about added IPFS object</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        [Post("/ipfs/add", "0.1.28")]
        Task<AddContentResponse> PostAddAsync(FileStream stream, CancellationToken cancellationToken = default);
        
        [Post("/ipfs/pin/add/{IPFS_path}", "0.1.28")]
        Task<PinStateContentResponse> PostPinAddAsync(string ipfsPath, CancellationToken cancellationToken = default);

        [Get("/ipfs/gateway/{IPFS_path}", "0.1.28")]
        Task<object> GetGatewayAsync(string ipfsPath, CancellationToken cancellationToken = default);


    }

}
