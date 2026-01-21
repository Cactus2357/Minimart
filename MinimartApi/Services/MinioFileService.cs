
using Microsoft.Extensions.Options;
using MinimartApi.Configurations;
using Minio;
using Minio.DataModel.Args;

namespace MinimartApi.Services
{
    public class MinioFileService : IFileService
    {
        private readonly IMinioClient minio;
        private readonly MinioOptions options;

        private static readonly string[] AllowedTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public MinioFileService(IMinioClient minio, IOptions<MinioOptions> options)
        {
            this.minio = minio;
            this.options = options.Value;
        }


        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty.");

            if (!AllowedTypes.Contains(file.ContentType))
                throw new Exception("Unsupported file type.");

            if (file.Length > MaxFileSize)
                throw new Exception($"File size exceeds the limit of {MaxFileSize} MB.");

            await EnsureBucketExist();

            var ext = Path.GetExtension(file.FileName);
            var objectName = $"{folder}/{Guid.NewGuid()}{ext}";

            await using var stream = file.OpenReadStream();

            await minio.PutObjectAsync(new PutObjectArgs()
                .WithBucket(options.Bucket)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(file.Length)
                .WithContentType(file.ContentType)
            );

            return BuilrUrl(objectName);
        }

        private async Task EnsureBucketExist()
        {
            var exists = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(options.Bucket));

            if (!exists)
            {
                await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(options.Bucket));
            }
        }

        private string BuilrUrl(string objectName)
        {
            var schema = options.UseSSL ? "https" : "http";
            return $"{schema}://{options.Endpoint}/{options.Bucket}/{objectName}";
        }
    }
}
