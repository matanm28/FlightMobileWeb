using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FlightMobileWeb.Controllers {
    using System.IO;
    using System.Net.Http;

    [Route("api/[controller]")]
    [ApiController]
    public class ScreenshotController : ControllerBase {
        public static bool debugMode = false;
        private static int picNumber = 0;
        private readonly HttpClient httpClient;

        /// <inheritdoc />
        public ScreenshotController(HttpClient httpClient) {
            this.httpClient = httpClient;
        }

        // POST: api/Screenshot
        [HttpGet]
        public async Task<ActionResult<byte[]>> GetScreenshot() {
            try {
                byte[] bytesArr = await this.httpClient.GetByteArrayAsync("screenshot?type=jpg");
                if (debugMode) {
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    string localFileName = @"\FlightMobileWeb\pic_" + picNumber + ".jpg";
                    picNumber++;
                    string localPath = documentsPath + localFileName;
                    await System.IO.File.WriteAllBytesAsync(localPath, bytesArr);
                }

                return File(bytesArr,"image/jpeg");
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return BadRequest();
            }
        }
    }
}
