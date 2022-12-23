using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Encryption;
using Amazon.S3.Model;
using NpgsqlTypes;
using Whetstone.StoryEngine.Reporting.Models;
using Whetstone.StoryEngine.Reporting.Models.Serialization;
using Xunit;

namespace Whetstone.StoryEngine.Reporting.ReportGenerator.Tests
{
    public class SerializationTests
    {
        [Fact]
        public async Task SerializeReportDefinitionTest()
        {
            ReportSourceDatabaseFunction dbFunctionSource = new ReportSourceDatabaseFunction();
            dbFunctionSource.FunctionName = "whetstone.getconsentreport";

            dbFunctionSource.Parameters = new List<FunctionParameter>();

            dbFunctionSource.Parameters.Add(new FunctionParameter
            {
                ParameterName = "ptitleid",
                ParameterType = NpgsqlDbType.Uuid
            });

            dbFunctionSource.Parameters.Add(new FunctionParameter
            {
                ParameterName = "pstarttime",
                ParameterType = NpgsqlDbType.Timestamp
            });

            dbFunctionSource.Parameters.Add(new FunctionParameter
            {
                ParameterName = "pendtime",
                ParameterType = NpgsqlDbType.Timestamp
            });


            ReportDefinition def = new ReportDefinition();
            def.DataSource = dbFunctionSource;
            def.Destination = GetSftpDestination();
            def.FileNameFormat = "@@FormatUTC(yyyy-MM-ddTHHmmss)@@_Whetstone_AZ_Stateline_VoiceAssistantAPILog";

            // define the output columns.


            ReportOutputCsv csvOutput = new ReportOutputCsv();
            csvOutput.Columns = new List<CsvColumnDefinition>();

            CsvColumnDefinition successCol = new CsvColumnDefinition();
            successCol.DataType = OutputDataType.Boolean;
            successCol.Name = "successstatus";
            successCol.Casing = ValueCasing.Upper;
            csvOutput.Columns.Add(successCol);


            CsvColumnDefinition userIdCol = new CsvColumnDefinition();
            userIdCol.DataType = OutputDataType.Guid;
            userIdCol.Name = "userid";
            csvOutput.Columns.Add(userIdCol);


            CsvColumnDefinition phoneNumberCol = new CsvColumnDefinition();
            phoneNumberCol.DataType = OutputDataType.String;
            phoneNumberCol.RegularExpressionFilter = new RegexReplacement();
            phoneNumberCol.RegularExpressionFilter.RegularExpression = "[^0-9a-zA-Z]+";
            phoneNumberCol.RegularExpressionFilter.ReplacementText = string.Empty;
            phoneNumberCol.Name = "phonenumber";
            csvOutput.Columns.Add(phoneNumberCol);

            CsvColumnDefinition sendTimeCol = new CsvColumnDefinition();
            sendTimeCol.DataType = OutputDataType.DateTime;
            sendTimeCol.Name = "sendtime";
            csvOutput.Columns.Add(sendTimeCol);


            CsvColumnDefinition providerMessageIdCol = new CsvColumnDefinition();
            providerMessageIdCol.DataType = OutputDataType.String;
            providerMessageIdCol.Name = "providermessageid";
            csvOutput.Columns.Add(providerMessageIdCol);

            CsvColumnDefinition codeCol = new CsvColumnDefinition();
            codeCol.DataType = OutputDataType.String;
            codeCol.Name = "code";
            csvOutput.Columns.Add(codeCol);

            CsvColumnDefinition sessionIdCol = new CsvColumnDefinition();
            sessionIdCol.DataType = OutputDataType.Guid;
            sessionIdCol.Name = "sessionid";
            csvOutput.Columns.Add(sessionIdCol);

            CsvColumnDefinition consentDateCol = new CsvColumnDefinition();
            consentDateCol.DataType = OutputDataType.DateTime;
            consentDateCol.Name = "smsconsentdate";
            csvOutput.Columns.Add(consentDateCol);


            def.Output = csvOutput;

           var yamlDeser = YamlReportSerializer.GetYamlSerializer();

            string reportDef = yamlDeser.Serialize(def);


            // Upload report definition
            string kmsId = "d839c3f9-4dfe-4e5e-bad0-3c0386c34efa";



           EncryptionMaterials encMats = new EncryptionMaterials(kmsId);


            using (var s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
            {
                PutObjectRequest putReq = new PutObjectRequest();
                
                putReq.Key = "definitions/crxstateline.yml";
                putReq.ContentBody = reportDef;
                putReq.ContentType = "application/yaml";
                putReq.BucketName = "whetstonebucket-dev-reportingbucket-1ody0ub0db8ln";

                //putReq.BucketName = "whetstone-bucket-prod-reportingbucket-1y0zvvnvnwmj4";

                try
                {
                    await s3Client.PutObjectAsync(putReq);
                }
                catch (AmazonS3Exception s3Ex)
                {
                    Console.WriteLine(s3Ex);
                    throw;
                }
           


            }
        }


        private ReportDestinationBase GetSftpDestination()
        {

            ReportDestinationSftp sftpDestination = new ReportDestinationSftp();

            sftpDestination.SecretStore = "dev/somediscountproviderrx/sftpreport";
            sftpDestination.Directory = "pharma/stateline";

            return sftpDestination;



        }

        [Fact]
        public void PhoneNumberFormatTest()
        {
            string phoneNumberOrig = "+12675551212";
            string outFile =    Regex.Replace(phoneNumberOrig, "[^0-9a-zA-Z]+", "");

            string regExText = @"^(\(?[0-9]{3}\)?)?\-?[0-9]{3}\-?[0-9]{4}$";

            Regex formatter = new Regex(regExText);

            string outText = formatter.Replace(phoneNumberOrig, regExText);


        }

    }
}
