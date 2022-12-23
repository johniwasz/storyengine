using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Whetstone.StoryEngine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Microsoft.Extensions.Logging;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Data.Amazon
{
    internal class S3Storage
    {

        //private static readonly ILogger Logger = StoryEngineLogFactory.CreateLogger<S3Storage>();


        internal static async Task StoreFileAsync(IAmazonS3 s3Client, string container, string storagePath,
            string mimeType, byte[] contents)
        {

            try
            {


                using (MemoryStream memStream = new MemoryStream(contents))
                {
                    PutObjectRequest putRequestStore = new PutObjectRequest
                    {
                        BucketName = container,
                        Key = storagePath,
                        InputStream = memStream,
                        ContentType = mimeType

                    };

                    PutObjectResponse putResponse = await s3Client.PutObjectAsync(putRequestStore);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error storing binary file {container} in bucket {storagePath}", ex);
            }
        }


        internal static async Task StoreFileAsync(IAmazonS3 s3Client, string container, string storagePath, string mimeType, string contents)
        {

            try
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (StreamWriter writer = new StreamWriter(memStream))
                    {
                        writer.Write(contents);
                        writer.Flush();
                        memStream.Position = 0;

                        PutObjectRequest putRequestStore = new PutObjectRequest
                        {
                            BucketName = container,
                            Key = storagePath,
                            InputStream = memStream,
                            ContentType = mimeType

                        };

                        PutObjectResponse putResponse = await s3Client.PutObjectAsync(putRequestStore);
                    }
                }
            }
            catch (AmazonS3Exception amzS3Exception)
            {
                StringBuilder amzText = new StringBuilder();
                amzText.AppendLine($"Error storing text file {storagePath} in bucket {container}. Responsebody: ");
                amzText.AppendLine(amzS3Exception.ResponseBody);
                if (!string.IsNullOrWhiteSpace(amzS3Exception.AmazonCloudFrontId))
                {
                    amzText.AppendLine($"AmazonCloudFrontId: {amzS3Exception.AmazonCloudFrontId}");
                }

                if (!string.IsNullOrWhiteSpace(amzS3Exception.AmazonId2))
                {
                    amzText.AppendLine($"AmazonId2: {amzS3Exception.AmazonId2}");
                }

                throw new Exception(amzText.ToString(), amzS3Exception);


            }
            catch (Exception ex)
            {
                throw new Exception($"Error storing text file {storagePath} in bucket {container}", ex);
            }
        }

        internal static async Task StoreFileAsync(IAmazonS3 s3Client, string container, string storagePath, string mimeType, Stream stm)
        {

            try
            {


                PutObjectRequest putRequestStore = new PutObjectRequest
                {
                    BucketName = container,
                    Key = storagePath,
                    InputStream = stm,
                    ContentType = mimeType

                };

                PutObjectResponse putResponse = await s3Client.PutObjectAsync(putRequestStore);

            }
            catch (Exception ex)
            {
                throw new Exception($"Error storing binary file {container} in bucket {storagePath}", ex);
            }
        }


        internal static async Task<bool> DoesFileExistAsync(IAmazonS3 s3Client, string containerName, string path)
        {
            bool doesExist = false;

            try
            {
              

                    ListObjectsV2Request request = new ListObjectsV2Request
                    {
                        BucketName = containerName,
                        Prefix = path
                    };

                    ListObjectsV2Response response = await s3Client.ListObjectsV2Async(request);

                    if (response.S3Objects.Count > 0)
                        doesExist = true;

                
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if file {path} exists in bucket {containerName}", ex);
            }


            return doesExist;
        }

        internal static async Task<byte[]> GetFileFromStoreAsync(IAmazonS3 s3Client, string containerName, string path)
        {

            byte[] returnContents = null;
            try
            {

                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = containerName,
                    Key = path
                };

                using (GetObjectResponse response = await s3Client.GetObjectAsync(request))
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        await response.ResponseStream.CopyToAsync(memStream);
                        returnContents = memStream.ToArray();
                    }
                }


            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving binary file {path} from bucket {containerName}", ex);
            }

            return returnContents;
        }


        internal static async Task<Stream> GetFileStreamFromStoreAsync(IAmazonS3 s3Client, string containerName, string path)
        {

            Stream returnContents = new MemoryStream();
            try
            {


                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = containerName,
                    Key = path
                };

                using (GetObjectResponse response = await s3Client.GetObjectAsync(request))
                {
                    await response.ResponseStream.CopyToAsync(returnContents);
                    returnContents.Seek(0, SeekOrigin.Begin);
                }


            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving binary file {path} from bucket {containerName}", ex);
            }

            return returnContents;
        }


    

        internal static async Task<string> GetConfigTextContentsAsync(IAmazonS3 s3Client, string containerName, string path)
        {
            string configContents = null;
            try
            {


                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = containerName,
                    Key = path
                };

                using (GetObjectResponse response = await s3Client.GetObjectAsync(request))
                {
                    using (BufferedStream buffer = new BufferedStream(response.ResponseStream))
                    {
                        using (StreamReader reader = new StreamReader(buffer))
                        {
                            configContents = reader.ReadToEnd();
                        }
                    }
                }

            }
            catch (AmazonS3Exception s3Ex)
            {
                throw new Exception($"Error retrieving text file {path} from bucket {containerName}", s3Ex);

            }
            catch (AmazonServiceException servEx)
            {
                throw new Exception($"Error retrieving text file {path} from bucket {containerName}", servEx);

            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving text file {path} from bucket {containerName}", ex);
            }

            return configContents;
        }

        internal static async Task<List<string>> ListFilesAsync(IAmazonS3 s3Client, string containerName, string path)
        {
            List<string> foundFiles = new List<string>();


            try
            {

                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = containerName,
                    Prefix = path

                };

                ListObjectsV2Response response = await s3Client.ListObjectsV2Async(request);

                foreach (var obj in response.S3Objects)
                {


                    string subPath = obj.Key.Substring(path.Length + 1);

                    if (!string.IsNullOrWhiteSpace(subPath))
                    {
                        if (subPath[subPath.Length - 1] != '/')
                            foundFiles.Add(subPath);
                    }

                }


            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting file list in directory {path} in bucket {containerName}", ex);
            }


            return foundFiles;
        }


        internal static async Task<List<AudioFileInfo>> ListAudioFileInfoAsync(IAmazonS3 s3Client, string containerName, string path)
        {
            List<AudioFileInfo> foundFiles = new List<AudioFileInfo>();


            try
            {
                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = containerName,
                    Prefix = path

                };

                ListObjectsV2Response response = await s3Client.ListObjectsV2Async(request);

                foreach (var obj in response.S3Objects)
                {


                    string subPath = obj.Key.Substring(path.Length + 1);

                    if (!string.IsNullOrWhiteSpace(subPath))
                    {
                        if (subPath[subPath.Length - 1] != '/')
                        {


                            AudioFileInfo foundFile = new AudioFileInfo
                            {
                                FileName = subPath,
                                Size = obj.Size,
                                LastModified = obj.LastModified

                            };
                            foundFiles.Add(foundFile);
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting file list in directory {path} in bucket {containerName}", ex);
            }


            return foundFiles;
        }

        internal static async Task<AudioFileInfo> GetAudioFileInfoAsync(IAmazonS3 s3Client, string containerName, string path, string filename)
        {
            AudioFileInfo foundFile = null;


            try
            {

                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = containerName,
                    Key = path
                };

                GetObjectResponse response = await s3Client.GetObjectAsync(request);

                foundFile = new AudioFileInfo
                {
                    FileName = filename,
                    Size = response.ContentLength,
                    LastModified = response.LastModified
                };


            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting fileinfo for {path}, filename {filename} in bucket {containerName}",
                    ex);
            }


            return foundFile;
        }

        internal static async Task DeleteAudioFileAsync(IAmazonS3 s3Client, string containerName, string path)
        {

            try
            {
                DeleteObjectRequest request = new DeleteObjectRequest
                {
                    BucketName = containerName,
                    Key = path
                };

                DeleteObjectResponse response = await s3Client.DeleteObjectAsync(request);


            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting {path} in bucket {containerName}", ex);
            }


        }

        internal static async Task CopyFileDirectoryAsync(IAmazonS3 s3Client, string containerName, string sourcePath, string destPath, ILogger logger)
        {
            if (s3Client == null)
                throw new ArgumentNullException(nameof(s3Client));

            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentNullException(nameof(containerName));

            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath));

            if (string.IsNullOrWhiteSpace(destPath))
                throw new ArgumentNullException(nameof(destPath));


            List<string> sourceFiles = await ListFilesAsync(s3Client, containerName, sourcePath);

            string sourcePathRoot = sourcePath;

            if (sourcePathRoot[sourcePathRoot.Length - 1] != '/')
            {
                sourcePathRoot = string.Concat(sourcePathRoot, '/');
            }

            string destPathRoot = destPath;

            if (destPathRoot[destPathRoot.Length - 1] != '/')
            {
                destPathRoot = string.Concat(destPathRoot, '/');
            }


            List<Exception> copyExceptions = new List<Exception>();


            
            Parallel.ForEach(sourceFiles, async (sourceFile) =>
            {
                string fullSourcePath = string.Concat(sourcePathRoot, sourceFile);
                string fullDestinationPath = string.Concat(destPathRoot, sourceFile);
                try
                {

                    await CopyObjectAsync(s3Client, containerName, fullSourcePath, fullDestinationPath);
                    logger.LogDebug($"Copied file {fullSourcePath} to {fullDestinationPath} in bucket {containerName}");
                }
                catch(Exception ex)
                {
                    string errorMsg = $"Error copying file {fullSourcePath} to {fullDestinationPath} in bucket {containerName}";
                    logger.LogError(ex,errorMsg);

                    copyExceptions.Add(new Exception(errorMsg, ex));
                }
            });
                

            

            if (copyExceptions.Any())
                throw new AggregateException(copyExceptions);

        }

        private static async Task CopyObjectAsync(IAmazonS3 client, string bucket, string sourceFile, string destFile)
        {

            CopyObjectRequest copyReq = new CopyObjectRequest();
            copyReq.DestinationBucket = bucket;
            copyReq.SourceBucket = bucket;

            copyReq.SourceKey = sourceFile;
            copyReq.DestinationKey = destFile;

            await client.CopyObjectAsync(copyReq);


        }


        internal async static Task RenameFile(IAmazonS3 s3Client, string containerName, string origName, string newName)
        {
            if (s3Client == null)
                throw new ArgumentNullException(nameof(s3Client));

            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentNullException(nameof(containerName));

            if (string.IsNullOrWhiteSpace(origName))
                throw new ArgumentNullException(nameof(origName));


            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentNullException(nameof(newName));


            try
            {

                await CopyObjectAsync(s3Client, containerName, origName, newName);
                await DeleteFile(s3Client, containerName, origName);

            }
            catch (Exception ex)
            {
                throw new Exception($"Error renaming file {origName} to {newName} in bucket {containerName}", ex);
            }


        }

        internal static async Task DeleteDirectoryAsync(IAmazonS3 s3Client, string containerName, string dirPath)
        {
            if (s3Client == null)
                throw new ArgumentNullException(nameof(s3Client));

            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentNullException(nameof(containerName));

            if (string.IsNullOrWhiteSpace(dirPath))
                throw new ArgumentNullException(nameof(dirPath));


            List<string> dirFiles = await ListFilesAsync(s3Client, containerName, dirPath);

            string dirPathRoot = dirPath;

            if (dirPathRoot[dirPathRoot.Length - 1] != '/')
            {
                dirPathRoot = string.Concat(dirPathRoot, '/');
            }


            List<Exception> deleteExceptions = new List<Exception>();


            List<DeleteObjectsRequest> deleteRequests = new List<DeleteObjectsRequest>();


            string[] audioFileArray = dirFiles.ToArray();

            int portions = 10;
            List<string[]> breakdowns = new List<string[]>();

            for (int i = 0; i < audioFileArray.Length; i += portions)
            {
                int maxCount = (i + portions <= audioFileArray.Length) ? portions : audioFileArray.Length % portions;
                string[] buffer = new string[maxCount];
                Array.Copy(audioFileArray, i, buffer, 0, maxCount);

                DeleteObjectsRequest deleteReq = new DeleteObjectsRequest();
                deleteReq.BucketName = containerName;

                foreach(string fileName in buffer)
                {
                    string fullPath = string.Concat(dirPathRoot, fileName);
                    deleteReq.AddKey(fullPath);
                }
                deleteRequests.Add(deleteReq);
            }


            var tasks = deleteRequests
                .Select(deleteRequest => DeleteObjects(s3Client, deleteRequest));

            DeleteObjectsResult[] delResponses = await Task.WhenAll(tasks);


            List<Exception> delExceptions = new List<Exception>();


            // Review the delete responses for errors and report them in an aggregate exception.
            foreach (var deleteResult in delResponses)
            {

                if (deleteResult.DeleteException != null)
                {
                    delExceptions.Add(deleteResult.DeleteException);
                }
                else
                {
                    DeleteObjectsResponse delObjResponse = deleteResult.DeleteObjectsResponse;

                    if (delObjResponse.DeleteErrors.Any())
                    {
                        foreach (var deleteError in delObjResponse.DeleteErrors)
                        {
                            string errorMessage = $"Error deleting {deleteError.Key} and version {deleteError.VersionId} - Error code {deleteError.Code}: {deleteError.Message}";
                            Exception delEx = new Exception(errorMessage);
                            deleteExceptions.Add(delEx);
                        }
                    }
                }
            }

            if (deleteExceptions.Any())
                throw new AggregateException(delExceptions);


        }



        private static async Task<DeleteObjectsResult> DeleteObjects(IAmazonS3 s3Client, DeleteObjectsRequest deleteRequest)
        {
            DeleteObjectsResult result = null;

            try
            {
                var deleteObjectResult = await s3Client.DeleteObjectsAsync(deleteRequest);
                result = new DeleteObjectsResult(deleteObjectResult);
            }
            catch (Exception ex)
            {

                result = new DeleteObjectsResult(ex);
            }

            return result;
        }

        internal static async Task DeleteFile(IAmazonS3 s3Client, string container, string filePath)
        {

            DeleteObjectRequest delObjRequest = new DeleteObjectRequest
            {
                Key = filePath,
                BucketName = container
            };

            try
            {

                await s3Client.DeleteObjectAsync(delObjRequest);
                
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting file {filePath} in bucket {container}", ex);
            }


        }
    }


    internal class DeleteObjectsResult
    {

        internal DeleteObjectsResult()
        {
        }

        internal DeleteObjectsResult(DeleteObjectsResponse deleteResult)
        {

            DeleteObjectsResponse = deleteResult;

        }

        internal DeleteObjectsResult(Exception ex)
        {
            DeleteException = ex;
        }


        internal Exception DeleteException { get; set; }


        internal DeleteObjectsResponse DeleteObjectsResponse { get; set; }

    }

}
