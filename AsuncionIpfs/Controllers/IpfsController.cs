using AsuncionIpfs.Services;
using AsuncionIpfs.Services.IPFS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
namespace AsuncionIpfs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IpfsController : ControllerBase
    {
        private readonly IAddService _addService;

        public IpfsController(IAddService addService)
        {
            _addService = addService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            string tempFilePath = null;

            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                // Crear un archivo temporal
                tempFilePath = Path.GetTempFileName();

                // Guardar el archivo en el sistema de archivos
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Reabrir el archivo como FileStream
                using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                {
                    var response = await _addService.PostAddAsync(fileStream);
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            finally
            {
                // Eliminar el archivo temporal si existe
                if (tempFilePath != null && System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
        }

        [HttpPost("pin/{ipfsPath}")]
        public async Task<IActionResult> PinFile(string ipfsPath)
        {
            try
            {
                var response = await _addService.PostPinAddAsync(ipfsPath, CancellationToken.None);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error pinning file: {ex.Message}");
            }
        }

        [HttpGet("getFile/{ipfsPath}")]
        /*public async Task<IActionResult> GetFile(string ipfsPath)
        {
            try
            {
                var response = await _addService.GetGatewayAsync(ipfsPath, CancellationToken.None);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error pinning file: {ex.Message}");
            }
        }*/
        public async Task<IActionResult> GetFile(string ipfsPath)
        {
            try
            {
                var response = await _addService.GetGatewayAsync(ipfsPath, CancellationToken.None);

                if (response is byte[] fileBytes)
                {
                    string contentType = "image/tif"; // Cambia esto si necesitas otro tipo de contenido
                    //return File(fileBytes, contentType);
                    return File(fileBytes, "image/tiff", "filename.tiff");

                }
                else
                {
                    return BadRequest("The response is not a valid file.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching file: {ex.Message}");
            }
        }




    }
}
