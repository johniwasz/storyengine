using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Renci.SshNet.Common;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Reporting.Models;
using Whetstone.StoryEngine.Repository;

namespace Whetstone.StoryEngine.Reporting.ReportRepository
{
    public class ReportSenderSftp : IReportSender
    {

        private readonly IFileRepository _fileRep = null;
        private readonly ILogger<ReportSenderSftp> _logger;
        private readonly ISftpClient _sftpClient = null;
        private readonly ISecretStoreReader _secretReader = null;

        public ReportSenderSftp(IFileRepository fileRep, ISftpClient sftpClient, ISecretStoreReader secretReader, ILogger<ReportSenderSftp> logger)
        {
            _sftpClient = sftpClient ?? throw new ArgumentNullException($"{nameof(sftpClient)}");

            _fileRep = fileRep ?? throw new ArgumentNullException($"{nameof(fileRep)}");

            _secretReader = secretReader ??
                            throw new ArgumentNullException($"{nameof(secretReader)}");


            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }

        
        public async Task<ReportSendStatus> SendReportsAsync(ReportSendStatus sendStatus)
        {

            _logger.LogInformation("Entered SFTP report sender");
            if(sendStatus == null)
                throw new ArgumentNullException($"{nameof(sendStatus)}");

            var destination = sendStatus.Destination;

            if (sendStatus.Destination == null)
                throw new ArgumentNullException($"{nameof(sendStatus)}", "Destination property cannot be null");

            var reportList = sendStatus.OutputFiles;

            if(reportList == null)
                throw new ArgumentNullException($"{nameof(sendStatus)}", "OutputFiles cannot be null");


            if(!(destination is ReportDestinationSftp sftpDestination))
                throw new ArgumentException($"{nameof(destination)} must be of type ReportDestinationSftp");

            if (string.IsNullOrWhiteSpace(sftpDestination.SecretStore))
                throw new ArgumentException($"{nameof(sftpDestination)} SecretStore property cannot be null or empty");



            // Get a list of files that haven't yet been sent
            var filesToSend = sendStatus.OutputFiles.Where(x => !x.IsSent).ToList();


            if (!sendStatus.AllSent && filesToSend.Count>0)
            {

                _logger.LogInformation($"Files to send: {filesToSend.Count}");


                SftpConfig ftpConfig = await GetSecretValAsync(sftpDestination.SecretStore);
                foreach (var outFile in filesToSend)
                {
                    outFile.IsSent = await SendFileAsync(outFile.FilePath, sftpDestination, ftpConfig);
                }
            }


            sendStatus.AllSent = !sendStatus.OutputFiles.Exists(x => !x.IsSent);
            sendStatus.SendAttempts++;


            return sendStatus;
        }

        private async Task<bool> SendFileAsync(string filePath, ReportDestinationSftp sftpDestination, SftpConfig ftpConfig)
        {
            bool isSent = false;

            // Get the file contents.
            string fileContents = await _fileRep.GetTextContentAsync(filePath);


            // build the destination path of the file.
            string[] fileParts = filePath.Split('/');

            string fileName = fileParts[fileParts.Length - 1];

            string destinationPath = string.Empty;

            if (!string.IsNullOrWhiteSpace(sftpDestination.Directory))
            {
                destinationPath = sftpDestination.Directory;

                if (!destinationPath[destinationPath.Length - 1].Equals('/'))
                {

                    destinationPath = $"{destinationPath}/";
                }

                if (!destinationPath[0].Equals('/'))
                {
                    destinationPath = $"/{destinationPath}";
                }

            }

            destinationPath = $"{destinationPath}{fileName}";

            _logger.LogInformation($"Sending file {fileName} to FTP path {destinationPath}");


            try
            {

                await _sftpClient.UploadFileAsync(ftpConfig, destinationPath, fileContents);
                isSent = true;

            }
            catch (SftpPermissionDeniedException permEx)
            {
                _logger.LogError(permEx, $"Permission denied uploading file {fileName} to {destinationPath} to SFTP host {ftpConfig.Host}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file {fileName} to {destinationPath} to SFTP host {ftpConfig.Host}");
            }

            return isSent;

        }

        private async Task<SftpConfig> GetSecretValAsync(string secretParam)
        {

            _logger.LogInformation($"Getting secret store: {secretParam}");

            string secretText= await _secretReader.GetValueAsync(secretParam);
            _logger.LogInformation($"Retrieved secret store: {secretParam}");

            if (string.IsNullOrWhiteSpace(secretText))
                throw new Exception($"SFTP secure credentials in {secretParam} not found");

            SftpConfig config = null;

            try
            {
                config = JsonConvert.DeserializeObject<SftpConfig>(secretText);
            }
            catch (Exception ex)
            {
               _logger.LogError(ex, $"Error deserializing secret value stored in {secretParam}");
                throw;
            }
  
            return config;

        }

    }
}
