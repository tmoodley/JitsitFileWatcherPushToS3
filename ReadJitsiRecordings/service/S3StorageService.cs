using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ReadJitsiRecordings.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ReadJitsiRecordings
{
    public class S3StorageService: IStorageService
    {
        private readonly AmazonS3Client s3Client;
        private const string BUCKET_NAME = "";
        private const string FOLDER_NAME = "";
        private const string objectKey = "";
        private const string accessKey = "";
        private const string secretKey = "";
        private const double DURATION = 24;
        private static readonly HttpClient client = new HttpClient();
        public S3StorageService()
        {
            s3Client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.USEast2);
        } 

        public async Task<bool> UploadFileAsync(string directory, string path, string name)
        {
            try
            { 
                //var fileTransferUtility = new TransferUtility(s3Client);
                //await fileTransferUtility.UploadAsync(FileStream, BUCKET_NAME, objectKey);
                PutObjectRequest request = new PutObjectRequest()
                {
                    FilePath = path, 
                    BucketName = BUCKET_NAME,
                    Key = name + ".mp4",
                    CannedACL = S3CannedACL.PublicRead
                };
                PutObjectResponse response = await s3Client.PutObjectAsync(request);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    File.Delete(path);
                    File.Delete(directory + "/metadata.json");
                    Directory.Delete(directory, true);
                    //update api with recording data
                    var stringTask = client.GetStringAsync("/api/meetings/recording/" + name);
                    return true;
                } 
                else
                    return false;
            }
            catch (Exception ex)
            { 
                throw ex;
            }
        }

        private string GeneratePreSignedURL(string objectKey)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = BUCKET_NAME,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddHours(DURATION)
            };

            string url = s3Client.GetPreSignedURL(request);
            return url;
        }
    }
}
