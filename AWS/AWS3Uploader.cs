using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace Ikrito_Fulfillment_Platform.AWS
{
    internal static class AWS3Uploader
    {

        public static async Task<PutObjectResponse> UploadFileAsync(string bucketName, string keyName, string filePath)
        {
            var client = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1);

            try
            {
                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    FilePath = filePath,
                    ContentType = "text/plain"
                };

                var response = await client.PutObjectAsync(putRequest);
                return response;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
        }
    }
}
