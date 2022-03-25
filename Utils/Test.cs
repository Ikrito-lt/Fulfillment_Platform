using System;
using System.IO;

namespace Ikrito_Fulfillment_Platform.Utils
{
    class Test {
        public Test() {
            //S3UploadTest();
            T();
        }

        private static void T() {
            Console.WriteLine(Directory.GetCurrentDirectory());
        }

        //private static async void S3UploadTest() {

        //    string bucketName = "pigu-xml";
        //    string keyName = "file.xml";
        //    string filePath = "C:\\Users\\Luke\\Desktop\\test_cat.xml";

        //    var aWS3Uploader = await AWS3Uploader.UploadFileAsync(bucketName, keyName, filePath);
        //    var aws3uploader = await AWS3Uploader.UploadFileAsync(bucketName, keyName + "1", filePath);
        //}
    }
}
