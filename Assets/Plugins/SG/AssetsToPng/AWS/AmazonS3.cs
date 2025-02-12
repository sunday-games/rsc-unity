#if UNITY_EDITOR && AWS
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Amazon.CognitoIdentity;

namespace SG
{
    public class AmazonS3 : MonoBehaviour
    {
        public string identityId;
        public string identityRegion;
        [Space(5)]
        public string bucketName;
        public string bucketRegion;

        AWSCredentials _credentials;
        AWSCredentials credentials => _credentials != null ? _credentials : _credentials = new CognitoAWSCredentials(identityId, RegionEndpoint.GetBySystemName(identityRegion));

        IAmazonS3 _s3;
        IAmazonS3 s3 => _s3 != null ? _s3 : _s3 = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(bucketRegion));

        void Start()
        {
            UnityInitializer.AttachToGameObject(gameObject);
            AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
        }

        public void Setup(string identityId, string identityRegion, string bucketName, string bucketRegion)
        {
            this.identityId = identityId;
            this.identityRegion = identityRegion;
            this.bucketName = bucketName;
            this.bucketRegion = bucketRegion;
        }

        public void Download(string fileName, Action<string> callback = null)
        {
            Log.Debug($"AWS S3 - Downloading {bucketName}/{fileName}...");

            s3.GetObjectAsync(
                bucketName,
                fileName,
                responseObj =>
                {
                    string data = null;

                    var response = responseObj.Response;
                    if (response.ResponseStream != null)
                        using (StreamReader reader = new StreamReader(response.ResponseStream))
                        {
                            data = reader.ReadToEnd();
                        }

                    if (data.IsEmpty())
                        Log.Error($"AWS S3 - Download Error: {fileName}");
                    else
                        Log.Debug($"AWS S3 - Download Successful: {fileName}");

                    callback?.Invoke(data);
                });
        }

        public void Upload(string fileName, byte[] file, Action<bool> callback = null)
        {
            Log.Debug($"AWS S3 - Uploading {bucketName}/{fileName}...");

            s3.PutObjectAsync(
                new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = fileName,
                    InputStream = new MemoryStream(file),
                    CannedACL = S3CannedACL.PublicRead,
                },
                responseObj =>
                {
                    if (responseObj.Exception != null)
                    {
                        Log.Error($"AWS S3 - Upload Error: {responseObj.Exception.ToString()}");
                        callback?.Invoke(false);
                        return;
                    }

                    Log.Debug($"AWS S3 - Upload Successful: {fileName}");

                    callback?.Invoke(true);
                });
        }

        public void Delete(string fileName, Action<bool> callback = null)
        {
            Log.Debug($"AWS S3 - Deleting {bucketName}/{fileName}...");

            s3.DeleteObjectsAsync(
                new DeleteObjectsRequest()
                {
                    BucketName = bucketName,
                    Objects = new List<KeyVersion>() { new KeyVersion() { Key = fileName } }
                },
                responseObj =>
                {
                    if (responseObj.Exception != null)
                    {
                        Log.Error($"AWS S3 - Delete Error: {responseObj.Exception.ToString()}");
                        callback?.Invoke(false);
                        return;
                    }

                    responseObj.Response.DeletedObjects.ForEach(dObj =>
                    {
                        Log.Debug($"AWS S3 - Delete Successful: {dObj.Key}");
                    });

                    callback?.Invoke(true);
                });
        }
    }
}
#endif