using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Entities;
using LinqToDB;
using Npgsql;

namespace Backend.DataLayer
{
    public class ImageRepository
    {
        private readonly IDbConnection _connection;
        private readonly NpgsqlLargeObjectManager _largeObjectManager;

        public ImageRepository(IDbConnection connection, NpgsqlLargeObjectManager largeObjectManager)
        {
            _connection = connection;
            _largeObjectManager = largeObjectManager;
        }

        public Task<ImageInfo> InsertImage(ImageInfo imageInfo, Stream image)
        {
            return InsertImage(imageInfo, image, CancellationToken.None);
        }

        public async Task<ImageInfo> InsertImage(ImageInfo imageInfo, Stream image,
            CancellationToken token)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                imageInfo.ImageId = await _largeObjectManager.CreateAsync(imageInfo.ImageId, token);
                using (var stream = await _largeObjectManager.OpenReadWriteAsync(imageInfo.ImageId, token))
                {
                    await image.CopyToAsync(stream);
                }
                imageInfo.Id = _connection.InsertId(imageInfo);
                transaction.Commit();
                return imageInfo;
            }
        }

//        public async Task GetImage(int id, Func<Stream, ImageInfo, Task> callback)
//        {
//            var imageInfo = await _connection.Images.SingleOrDefaultAsync(info => info.Id == id);
//            using (var trans = _connection.BeginTransaction())
//            {
//                using (var stream = await _largeObjectManager.OpenReadAsync(imageInfo.ImageId, CancellationToken.None))
//                {
//                    await callback(stream, imageInfo);
//                }
//            }
//        }

//        public IEnumerable<ImageInfo> Images()
//        {
//            return _connection.Images;
//        }
//
//        public void Delete(int id, uint? oid)
//        {
//            if (!oid.HasValue)
//            {
//                oid = _connection.Images.Where(info => info.Id == id).Select(info => info.ImageId).SingleOrDefault();
//            }
//            using (var transaction = _connection.BeginTransaction())
//            {
//                _largeObjectManager.Unlink(oid.Value);
//                _connection.Images.Where(info => info.Id == id).Delete();
//                transaction.Commit();
//            }
//        }
    }
}