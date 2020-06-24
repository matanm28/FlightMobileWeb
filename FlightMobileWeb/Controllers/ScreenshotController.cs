using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FlightMobileWeb.Controllers {
    using System.Net.Http;
    using System.Threading;

    [Route("api/[controller]")]
    [ApiController]
    public class ScreenshotController : ControllerBase {
        public static bool debugMode = false;
        private static int picNumber;
        private readonly HttpClient httpClient;

        /// <inheritdoc />
        public ScreenshotController(HttpClient httpClient) {
            this.httpClient = httpClient;
        }

        // GET: api/Screenshot
        [HttpGet]
        public async Task<ActionResult<byte[]>> GetScreenshot() {
            try {
                byte[] bytesArr = await this.httpClient.GetByteArrayAsync("screenshot?type=jpg");
                if (debugMode) {
                    new Thread(delegate() { SaveImageToMyPictures(bytesArr); }).Start();
                }
                return File(bytesArr, "image/jpeg");
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return BadRequest();
            }
        }

        private static void SaveImageToMyPictures(byte[] imageBytes) {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string localFileName = @"\FlightMobileWeb\pic_" + picNumber + ".jpg";
            picNumber++;
            string localPath = documentsPath + localFileName;
            System.IO.File.WriteAllBytes(localPath, imageBytes);
        }
    }
}
