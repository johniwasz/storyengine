using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Whetstone.StoryEngine.AlexaProcessor;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Serialization;
using Xunit;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class RequestSerializationTest : LambdaTestBase
    {
        [Fact(DisplayName = "Add App Mapping")]
        public async Task AppMappingsTest()
        {

            string storageBucket = "prod-sbsstoryengine";
            string storagePath = "global/appmappings.yaml";

            string newMappingText = "Animal Farm P I";
            string titleMapping = "animalfarmpi";


            RegionEndpoint endpoint = RegionEndpoint.USEast1;

            string configContents;
            using (IAmazonS3 client = new AmazonS3Client(endpoint))
            {

                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = storageBucket,
                    Key = storagePath
                };

                using (GetObjectResponse response = await client.GetObjectAsync(request))
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




            var yamlDeser = YamlSerializationBuilder.GetYamlDeserializer();

            Dictionary<string, string> appMappings = yamlDeser.Deserialize<Dictionary<string, string>>(configContents);

            if (!appMappings.ContainsKey(newMappingText))
            {
                appMappings.Add(newMappingText, titleMapping);

                var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
                string yamlText = yamlSer.Serialize(appMappings);

                using (IAmazonS3 client = new AmazonS3Client(endpoint))
                {

                    PutObjectRequest request = new PutObjectRequest
                    {
                        BucketName = storageBucket,
                        Key = storagePath,
                        ContentBody = yamlText
                    };

                    PutObjectResponse response = await client.PutObjectAsync(request);


                }

            }
        }

        [Fact(DisplayName = "SendStatus Slotted Request")]
        public async Task SendSlottedRequest()
        {

            string appMappingPath = @"localstore/global/appmappings.yaml";

            string appMappingText = File.ReadAllText(appMappingPath);


            RegionEndpoint endpoint = RegionEndpoint.USEast1;

            await StoreFileAsync(endpoint, "dev-sbsstoryengine", "global/appmappings.yaml", "text/yaml", appMappingText);




            //string fileContents = await GetConfigTextContentsAsync("dev-sbsstoryengine", "global/appmappings.yaml");

            string testDataFile = @"requestsamples\gotobarnslot.json";

            string slotRequestText = File.ReadAllText(testDataFile);


            AlexaRequest request = JsonConvert.DeserializeObject<AlexaRequest>(slotRequestText);


            StoryRequest storyRequest = request.ToStoryRequest();

            var context = GetLambdaContext();

            var function = new AlexaFunctionProxy();
            var returnVal = function.FunctionHandlerAsync(request, context);




        }


        internal static async Task StoreFileAsync(RegionEndpoint endpoint, string container, string storagePath, string mimeType, string contents)
        {


            try
            {
                using (IAmazonS3 client = new AmazonS3Client(endpoint))
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

                            PutObjectResponse putResponse = await client.PutObjectAsync(putRequestStore);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error storing text file {0} in bucket {1}", container, storagePath), ex);
            }
        }

        internal static async Task<string> GetConfigTextContentsAsync(RegionEndpoint endpoint, string containerName, string path)
        {
            string configContents = null;
            try
            {
                using (IAmazonS3 client = new AmazonS3Client(endpoint))
                {

                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = containerName,
                        Key = path
                    };

                    using (GetObjectResponse response = await client.GetObjectAsync(request))
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
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error retreiving text file {0} from bucket {1}", path, containerName), ex);
            }
            return configContents;
        }

        [Fact(DisplayName = "Load Slotted Request")]
        public void ImportStoryTitleZipYaml()
        {
            // Azure Mongo
            // mongodb://sbs-sandbox-db:mVNZC0y99WarHlyJq4crIOjxVXANMdBnrUDFDDw4zVX2L2AJJCJ7nTtUSqm7mintTgoH0jClGIX72bm07z1f8w==@sbs-sandbox-db.documents.azure.com:10255/?ssl=true&replicaSet=globaldb
            // Local host
            // mongodb://localhost:27017

            // "ConnectionString": "mongodb://sbs-sandbox-db:mVNZC0y99WarHlyJq4crIOjxVXANMdBnrUDFDDw4zVX2L2AJJCJ7nTtUSqm7mintTgoH0jClGIX72bm07z1f8w==@sbs-sandbox-db.documents.azure.com:10255/?ssl=true&replicaSet=globaldb",
            //  "SslProtocols": "Tls12"
            // mongodb://localhost:27017

            // Atlas Cluster
            // "ConnectionString": "mongodb://sbsadmin:W!sanctuary17@devcluster-shard-00-00-azwnh.mongodb.net:27017,devcluster-shard-00-01-azwnh.mongodb.net:27017,devcluster-shard-00-02-azwnh.mongodb.net:27017/test?ssl=true&replicaSet=DevCluster-shard-0&authSource=admin"


            string testDataFile = @"requestsamples\slotrequest.json";

            string slotRequestText = File.ReadAllText(testDataFile);


            AlexaRequest request = JsonConvert.DeserializeObject<AlexaRequest>(slotRequestText);

            //var configOpts = new EnvironmentConfigOptions("dev");
            //IFileRepository fileRep = new S3FileStore(configOpts);

            //IStoryTitleImporter storyImporter = new S3StoryTitleImporter(fileRep);

            //storyImporter.ImportFromZip(importZip);

        }



    }
}
