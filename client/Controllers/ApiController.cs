using client.App_Code;
using client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Diagnostics;
using client.Models;
using Microsoft.EntityFrameworkCore;

namespace client.Controllers
{
    public class ApiController : Controller
    {
        private readonly AzureBlobStorageService _blobStorage;
        private readonly MehujuhlatContext _context;

        public ApiController(AzureBlobStorageService blobStorage, MehujuhlatContext context) 
        {
            _blobStorage = blobStorage;
            _context = context;
        }


        public ActionResult GenQR(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("QR-koodin data puuttuu");
            }
            try
            {
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(id, QRCodeGenerator.ECCLevel.Q))
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeImage = qrCode.GetGraphic(5);
                    return File(qrCodeImage, "image/png");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return StatusCode(500, "QR-koodin luonti epäonnistui");
            }
        }


        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Tiedosto puuttuu");
                var imageUrl = await _blobStorage.UploadImageAsync(file);

                Image img = new Image
                {
                    Description = "Desc",
                    Title = "Title",
                    Url = imageUrl,
                    EventId = 1
                };
                _context.Images.Add(img);
                await _context.SaveChangesAsync();

                return Ok(new { Url = imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }



    }
}
