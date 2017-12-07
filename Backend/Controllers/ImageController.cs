using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Backend.DataLayer;
using Backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class ImageController : MyController
    {
        private readonly ImageRepository _imageRepository;

        public ImageController(ImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        [HttpPost("{category}/{name}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Upload(string category, string name)
        {
            var imageInfo = new ImageInfo
            {
                Category = category,
                Name = name,
                Type = Request.ContentType
            };
            imageInfo = await _imageRepository.InsertImage(imageInfo, Request.Body);

            return Json(imageInfo);
        }

        [HttpGet("{id}")]
        public async Task Get(int id)
        {
            await _imageRepository.GetImage(id, (stream, info) =>
            {
                Response.StatusCode = 200;
                Response.ContentLength = stream.Length;
                Response.ContentType = info.Type;
                return stream.CopyToAsync(Response.Body);
            });
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IList<ImageInfo> Images()
        {
            return _imageRepository.Images().ToList();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id, uint? oid)
        {
            _imageRepository.Delete(id, oid);
            return Ok();
        }
    }
}