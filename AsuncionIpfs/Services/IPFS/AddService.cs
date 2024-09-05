using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AsuncionIpfs.Models.IPFS;
using AsuncionIpfs.Utils;
using Blockfrost.Api;
using Blockfrost.Api.Extensions;
using Blockfrost.Api.Http;
using Blockfrost.Api.Services;
using Blockfrost.Api.Utils;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace AsuncionIpfs.Services.IPFS
{
    public partial class AddService : ABlockfrostService,IAddService
    {
       
        /// <summary> 
        ///     Initializes a new <see cref="AddService"></see> with the specified <see cref="HttpClient"></see> 
        /// </summary>
        /// <remarks>
        ///     See also <seealso href="https://docs.blockfrost.io/#tag/IPFS-Add">IPFS » Add</seealso> on docs.blockfrost.io
        /// </remarks>
        public AddService(IHealthService health, IMetricsService metrics, HttpClient httpClient) : base(httpClient)
        {
            Health = health;
            Metrics = metrics;
        }

        public IHealthService Health { get; set; }

        public IMetricsService Metrics { get; set; }

        /// <summary>
        ///     Add a file to IPFS <c>/ipfs/add</c>
        /// </summary>
        /// <remarks>
        ///     See also <seealso href="https://docs.blockfrost.io/#tag/IPFS-Add/paths/~1ipfs~1add/post">/ipfs/add</seealso> on docs.blockfrost.io
        /// </remarks>
        /// <returns>Returns information about added IPFS object</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        [Post("/ipfs/add", "0.1.28")]
        public async Task<Models.IPFS.AddContentResponse> PostAddAsync(FileStream stream, CancellationToken cancellationToken = default)
        {
            // Construir la URL de la API
            var builder = GetUrlBuilder("/api/v0/ipfs/add");

            // Preparar el contenido multipart/form-data
            var content = PrepareHttpContent(stream);

            // Agregar el encabezado `project_id`
            content.Headers.Add("project_id", Constants.ENV_BFCLI_API_KEY);

            // Enviar la solicitud utilizando SendPostRequestAsync
            return await SendPostRequestAsync<Models.IPFS.AddContentResponse>(content, builder, cancellationToken);
        }

        [Post("/ipfs/pin/add/{IPFS_path}", "0.1.28")]
        public async Task<PinStateContentResponse> PostPinAddAsync(string ipfsPath, CancellationToken cancellationToken)
        {
            var builder = GetUrlBuilder("/api/v0/ipfs/pin/add/{IPFS_path}");
            _ = builder.SetRouteParameter("{IPFS_path}", ipfsPath);

            var content = PreparePinHttpContent();
            content.Headers.Add("project_id", Constants.ENV_BFCLI_API_KEY);

            return await SendPostRequestAsync<PinStateContentResponse>(content, builder, cancellationToken);

        }
       
        [Get("/ipfs/gateway/{IPFS_path}", "0.1.28")]
        public async Task<object> GetGatewayAsync(string ipfsPath, CancellationToken cancellationToken = default)
        {
            if (ipfsPath == null)
            {
                throw new System.ArgumentNullException(nameof(ipfsPath));
            }

            var builder = GetUrlBuilder("/api/v0/ipfs/gateway/{IPFS_path}");
            _ = builder.SetRouteParameter("{IPFS_path}", ipfsPath);

            return await SendGetRequestAsync<object>(builder, cancellationToken);

                         

        }


        protected HttpContent PrepareHttpContent(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            // Crear el contenido multipart/form-data
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(stream), "file", "uploaded_file");
            // No configures el Content-Type aquí; se manejará automáticamente por MultipartFormDataContent
            return content;
        }
        protected HttpContent PreparePinHttpContent()
        {
            var content = new StringContent("", null, "text/plain");
            return content;
        }
        protected async Task<TResponse> SendGetRequestAsync<TResponse>(StringBuilder builder, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage();
            request.Method = new HttpMethod("GET");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            request.Headers.Add("project_id", Constants.ENV_BFCLI_API_KEY);


            return await SendRequestAsync<TResponse>(builder, request, cancellationToken).ConfigureAwait(false);
        }
        private async Task<TResponse> SendRequestAsync<TResponse>(StringBuilder builder, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string url = builder.ToString();
            request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);

            PrepareRequest(HttpClient, request, builder);

            var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            bool disposeResponse = true;
            try
            {
                var headers = ProcessResponseHeaders(response);

                ProcessResponse(HttpClient, response);

                return await ReadResponseAsync<TResponse>(response, headers, cancellationToken);
            }
            finally
            {
                if (disposeResponse)
                {
                    response.Dispose();
                }
            }
        }

        private static Dictionary<string, IEnumerable<string>> ProcessResponseHeaders(HttpResponseMessage response)
        {
            var headers = System.Linq.Enumerable.ToDictionary(response.Headers, header => header.Key, header => header.Value);

            if (response.Content == null || response.Content.Headers == null)
            {
                return headers;
            }

            foreach (var item in response.Content.Headers)
            {
                headers[item.Key] = item.Value;
            }

            return headers;
        }

        private async Task<TResponse> ReadResponseAsync<TResponse>(HttpResponseMessage response, Dictionary<string, IEnumerable<string>> headers, CancellationToken cancellationToken)
        {
            int statusCode = (int)response.StatusCode;
            switch (statusCode) 
            {
                case 200:
                    {
                        if (response.Content.Headers.ContentType.MediaType.StartsWith("image/") ||
                        response.Content.Headers.ContentType.MediaType == "application/octet-stream")
                        {
                            var byteArray = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                            return (TResponse)(object)byteArray;
                        }


                        // Continuar con el procesamiento normal para otros tipos de respuesta
                        var objectResponse = await ReadObjectResponseAsync<TResponse>(response, headers, cancellationToken).ConfigureAwait(false);
                        return objectResponse.Value == null
                            ? throw new ApiException("Response was null which was not expected.", statusCode, objectResponse.Text, headers, null)
                            : objectResponse.Value;
                    }

                case 202:
                    {
                        var objectResponse = await ReadObjectResponseAsync<TResponse>(response, headers, cancellationToken).ConfigureAwait(false);
                        return objectResponse.Value == null
                            ? throw new ApiException("Response was null which was not expected.", statusCode, objectResponse.Text, headers, null)
                            : objectResponse.Value;
                    }

                case 400:
                    {
                        var objectResponse = await ReadObjectResponseAsync<BadRequestResponse>(response, headers, cancellationToken).ConfigureAwait(false);
                        if (objectResponse.Value == null)
                        {
                            throw new ApiException("Response was null which was not expected.", statusCode, objectResponse.Text, headers, null);
                        }

                        throw new ApiException<BadRequestResponse>("Bad request", statusCode, objectResponse.Text, headers, objectResponse.Value, null);
                    }

                case 403:
                    {
                        var objectResponse = await ReadObjectResponseAsync<ForbiddenResponse>(response, headers, cancellationToken).ConfigureAwait(false);
                        if (objectResponse.Value == null)
                        {
                            throw new ApiException("Response was null which was not expected.", statusCode, objectResponse.Text, headers, null);
                        }

                        throw new ApiException<ForbiddenResponse>("Authentication secret is missing or invalid", statusCode, objectResponse.Text, headers, objectResponse.Value, null);
                    }

                case 404:
                    {
                        var objectResponse = await ReadObjectResponseAsync<NotFoundResponse>(response, headers, cancellationToken).ConfigureAwait(false);
                        if (objectResponse.Value == null)
                        {
                            throw new ApiException("Response was null which was not expected.", statusCode, objectResponse.Text, headers, null);
                        }

                        throw new ApiException<NotFoundResponse>("Component not found", statusCode, objectResponse.Text, headers, objectResponse.Value, null);
                    }

                case 418:
                    {
                        var objectResponse = await ReadObjectResponseAsync<UnsupportedMediaTypeResponse>(response, headers, cancellationToken).ConfigureAwait(false);
                        if (objectResponse.Value == null)
                        {
                            throw new ApiException("Response was null which was not expected.", statusCode, objectResponse.Text, headers, null);
                        }

                        throw new ApiException<UnsupportedMediaTypeResponse>("IP has been auto-banned for extensive sending of requests after usage limit has been reached", statusCode, objectResponse.Text, headers, objectResponse.Value, null);
                    }

                case 429:
                    {
                        var objectResponse = await ReadObjectResponseAsync<TooManyRequestsResponse>(response, headers, cancellationToken).ConfigureAwait(false);
                        if (objectResponse.Value == null)
                        {
                            throw new ApiException("Response was null which was not expected.", statusCode, objectResponse.Text, headers, null);
                        }

                        throw new ApiException<TooManyRequestsResponse>("Usage limit reached", statusCode, objectResponse.Text, headers, objectResponse.Value, null);
                    }

                case 500:
                    {
                        var objectResponse = await ReadObjectResponseAsync<InternalServerErrorResponse>(response, headers, cancellationToken).ConfigureAwait(false);
                        if (objectResponse.Value == null)
                        {
                            throw new ApiException("Response was null which was not expected.", statusCode, objectResponse.Text, headers, null);
                        }

                        throw new ApiException<InternalServerErrorResponse>("Internal Server Error", statusCode, objectResponse.Text, headers, objectResponse.Value, null);
                    }

                default:
                    {
                        string responseData = response.Content == null ? null : await response.Content
#if NET5_0_OR_GREATER
                            .ReadAsStringAsync(cancellationToken)
#else
                            .ReadAsStringAsync()
#endif
                            .ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + statusCode + ").", statusCode, responseData, headers, null);
                    }
            }
        }


    }
}
