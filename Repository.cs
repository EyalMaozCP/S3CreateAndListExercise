using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3CreateAndList
{
    public class Repository
    {
        public static async Task<ListBucketsResponse> GetAllBuckets(AmazonS3Client s3Client)
        {
            var listBucketResponse = await s3Client.ListBucketsAsync();
            return listBucketResponse;
        }

        public static async Task<List<S3Object>> GetFilesFromBucket(AmazonS3Client s3Client, string bucketName)
        {
            var listBucketResponse = await s3Client.ListBucketsAsync();
            foreach (S3Bucket b in listBucketResponse.Buckets)
            {
                if (b.BucketName.Equals(bucketName))
                {
                    ListObjectsRequest request = new ListObjectsRequest
                    {
                        BucketName = bucketName,
                    };
                    ListObjectsResponse response = await s3Client.ListObjectsAsync(request);
                    return response.S3Objects;
                }
            }
            return null;
        }

        public static async Task DeleteFileFromS3Bucket(AmazonS3Client s3Client, string bucketName, string fileName)
        {
            try
            {
                await s3Client.DeleteObjectAsync(bucketName, fileName);
                Console.WriteLine("File deleted");
            }
            catch (Exception)
            {
                Console.WriteLine("Cant delete the file specified");
            }
        }

        public static async Task<bool> UploadFileFromS3Bucket(AmazonS3Client s3Client, string bucketName, string filePath)
        {
            try
            {
                PutObjectRequest req = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = Path.GetFileName(filePath),
                    FilePath = filePath
                };
                var response = await s3Client.PutObjectAsync(req);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}