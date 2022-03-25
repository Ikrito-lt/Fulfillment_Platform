using System;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Ikrito_Fulfillment_Platform.AWS
{
    internal static class AWS3Uploader
    {

        public static async Task<string> UploadFileAsync(string bucketName, string keyName, string filePath, string contentType = null)
        {
            var creds = new BasicAWSCredentials("AKIASPWFMGKNZGZALSAP", "mphBwilxSC12+AKvKZDaVKaKAb7IWSrN6J9zB+02");
            var client = new AmazonS3Client(creds, Amazon.RegionEndpoint.EUCentral1);

            try
            {
                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    FilePath = filePath,
                    ContentType = contentType
                };

                var response = await client.PutObjectAsync(putRequest);
                var message = $"HttP {response.HttpStatusCode}\n {response.ResponseMetadata}\n{response.ContentLength}";
                return message;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    return "Check the provided AWS Credentials.";
                }
                else
                {
                    return "Error occurred: " + amazonS3Exception.Message;
                }
            }
        }
    }
}
