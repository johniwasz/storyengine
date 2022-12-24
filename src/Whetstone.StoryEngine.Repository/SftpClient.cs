using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.Repository
{
    public class SftpClient : ISftpClient
    {
        private readonly ILogger<SftpClient> _logger;

        public SftpClient(ILogger<SftpClient> logger)
        {

            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
        }


        public async Task UploadFileAsync(SftpConfig serverConfig, string destinationPath, string contents)
        {
            await UploadFileAsync(serverConfig, destinationPath, contents, Encoding.UTF8);
        }

        public async Task UploadFileAsync(SftpConfig serverConfig, string destinationPath, string contents, Encoding textEncoding)
        {
            await Task.Run(() =>
            {


                if (serverConfig == null)
                    throw new ArgumentNullException($"{nameof(serverConfig)}");

                if (string.IsNullOrWhiteSpace(serverConfig.Host))
                    throw new ArgumentNullException($"{nameof(serverConfig)}", "Host property cannot be null or empty");

                if (string.IsNullOrWhiteSpace(serverConfig.UserName))
                    throw new ArgumentNullException($"{nameof(serverConfig)}", "UserName property cannot be null or empty");

                if (string.IsNullOrWhiteSpace(serverConfig.UserSecret))
                    throw new ArgumentNullException($"{nameof(serverConfig)}", "UserSecret property cannot be null or empty");

                if (string.IsNullOrWhiteSpace(destinationPath))
                    throw new ArgumentNullException($"{nameof(destinationPath)}");

                if (string.IsNullOrWhiteSpace(contents))
                    throw new ArgumentNullException($"{nameof(contents)}");

                if (textEncoding == null)
                    throw new ArgumentNullException($"{nameof(textEncoding)}");



                var connectionInfo = serverConfig.Port.HasValue ?
                        new ConnectionInfo(serverConfig.Host, serverConfig.Port.Value,
                            serverConfig.UserName,
                            new PasswordAuthenticationMethod(serverConfig.UserName, serverConfig.UserSecret))
                        : new ConnectionInfo(serverConfig.Host,
                    serverConfig.UserName,
                    new PasswordAuthenticationMethod(serverConfig.UserName, serverConfig.UserSecret));

                using (var client = new Renci.SshNet.SftpClient(connectionInfo))
                {
                    try
                    {
                        Stopwatch connectTime = new Stopwatch();
                        connectTime.Start();
                        client.Connect();
                        connectTime.Stop();
                        _logger.LogInformation($"Time to connect to SFTP host {serverConfig.Host} with user name {serverConfig.UserName}: {connectTime.ElapsedMilliseconds}ms");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error connecting to SFTP host {serverConfig.Host} with user name {serverConfig.UserName}");
                        throw;
                    }


                    try
                    {
                        byte[] fileBytes = textEncoding.GetBytes(contents);
                        using (MemoryStream upFileStream = new MemoryStream(fileBytes))
                        {
                            Stopwatch uploadTime = new Stopwatch();
                            uploadTime.Start();
                            client.UploadFile(upFileStream, destinationPath);
                            uploadTime.Stop();
                            _logger.LogInformation(
                                $"Time to upload file {destinationPath} to SFTP host {serverConfig.Host}: {uploadTime.ElapsedMilliseconds}ms");

                        }
                    }
                    catch (SftpPermissionDeniedException permEx)
                    {
                        _logger.LogError(permEx, $"Permission denied uploading file {destinationPath} to SFTP host {serverConfig.Host} with user {serverConfig.UserName}");
                        throw;

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error uploading file {destinationPath} to SFTP host {serverConfig.Host}");
                        throw;
                    }


                }
            });

        }
    }
}
