using Ikrito_Fulfillment_Platform.AWS;
using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Utils;
using System.Collections.Generic;

namespace Ikrito_Fulfillment_Platform.Utils
{
    class Test {
        public Test() {
            //S3UploadTest();
        }

        private static async void S3UploadTest() {

            string bucketName = "pigu-xml";
            string keyName = "file.xml";
            string filePath = "C:\\Users\\Luke\\Desktop\\test_cat.xml";

            var aWS3Uploader = await AWS3Uploader.UploadFileAsync(bucketName, keyName, filePath);
            var aws3uploader = await AWS3Uploader.UploadFileAsync(bucketName, keyName + "1", filePath);
        }
    }
}
