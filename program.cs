using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace CloudflareR2AwsSdkNet
{
    class Program
    {
        private static IAmazonS3 s3Client = null!;

        public static async Task Main(string[] args)
        {
            // Enter your Cloudflare R2 API credentials below
            // (See: https://developers.cloudflare.com/r2/api/tokens)
            var accessKey = "<ACCESS_KEY_ID>";
            var secretKey = "<SECRET_ACCESS_KEY>";
            var accountId = "<ACCOUNT_ID>";

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            s3Client = new AmazonS3Client(credentials, new AmazonS3Config
            {
                ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
            });

            Console.WriteLine("--- Cloudflare R2 with AWS SDK for .NET Demo ---\n");

            try
            {
                // Uncomment the methods below to test different operations

                await ListBuckets();

                // await ListObjectsV2("my-bucket");
                // await PutObject("my-bucket", @"C:\path\to\file.txt");
                // await GetObject("my-bucket", "file.txt");
                // GeneratePresignedUrl("my-bucket", "file.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.WriteLine("\nDone! Press any key to exit.");
            Console.ReadKey();
        }

        static async Task ListBuckets()
        {
            Console.WriteLine("Listing all R2 buckets...");
            var response = await s3Client.ListBucketsAsync();

            foreach (var s3Bucket in response.Buckets)
            {
                Console.WriteLine($"- {s3Bucket.BucketName}");
            }
        }

        static async Task ListObjectsV2(string bucketName)
        {
            Console.WriteLine($"\nListing objects in bucket '{bucketName}'...");
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName
            };

            var response = await s3Client.ListObjectsV2Async(request);

            foreach (var s3Object in response.S3Objects)
            {
                Console.WriteLine($"- {s3Object.Key} (Size: {s3Object.Size} bytes)");
            }
        }

        static async Task PutObject(string bucketName, string filePath)
        {
            Console.WriteLine($"\nUploading file '{filePath}' to '{bucketName}'...");
            var request = new PutObjectRequest
            {
                FilePath = filePath,
                BucketName = bucketName,
                // These two flags are critical for compatibility with Cloudflare R2
                DisablePayloadSigning = true,
                DisableDefaultChecksumValidation = true
            };

            var response = await s3Client.PutObjectAsync(request);
            Console.WriteLine($"Upload successful! ETag: {response.ETag}");
        }

        static async Task GetObject(string bucketName, string key)
        {
            Console.WriteLine($"\nRetrieving object '{key}' from '{bucketName}'...");
            var response = await s3Client.GetObjectAsync(bucketName, key);
            Console.WriteLine($"Retrieval successful! ETag: {response.ETag}");
            Console.WriteLine($"Content-Type: {response.Headers.ContentType}");
        }

        static string? GeneratePresignedUrl(string bucketName, string key)
        {
            Console.WriteLine($"\nGenerating presigned URL for '{key}'...");
            AWSConfigsS3.UseSignatureVersion4 = true;
            var presign = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.Now.AddDays(7),
            };

            var presignedUrl = s3Client.GetPreSignedURL(presign);
            Console.WriteLine($"Presigned URL:\n{presignedUrl}");

            return presignedUrl;
        }
    }
}