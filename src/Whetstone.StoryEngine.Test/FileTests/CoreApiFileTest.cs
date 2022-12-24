using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Admin;
using Xunit;

namespace Whetstone.StoryEngine.Test.FileTests
{
    public class CoreApiFileTest : TestServerFixture
    {
        [Fact(DisplayName = "Audio File Upload/GetInfo/Delete")]
        public async Task AudioFileIntegrationTest()
        {
            try
            {
                const string FILE_PATH = "AudioFiles/trollgrowl.mp3";
                const string FILE_NAME = "trollgrowl.mp3";

                byte[] audioFile = File.ReadAllBytes(FILE_PATH);


                IProjectRepository projectRepository = Services.GetRequiredService<IProjectRepository>();

                IFileRepository fileRep = Services.GetRequiredService<IFileRepository>();


                // Find the Whetstone Technologies project
                IEnumerable<Project> projects = await projectRepository.GetProjectsAsync();
                Project whetstoneProject = projects.First<Project>(x => x.ShortName == "whetstonetechnologies");

                if (whetstoneProject == null)
                    throw new InvalidOperationException("Whetstone Technologies project not found");

                Guid projectId = whetstoneProject.Id.GetValueOrDefault();

                IEnumerable<ProjectVersion> versions = await projectRepository.GetVersionsAsync(projectId);

                if (versions == null || versions.Count() == 0)
                    throw new InvalidOperationException($"No versions for whetstonetechnologies project: {whetstoneProject.Id}");

                ProjectVersion version = versions.Last();

                Guid versionId = version.Id.GetValueOrDefault();

                AudioFileInfo fileInfo = null;
                using (MemoryStream stm = new MemoryStream(audioFile))
                {
                    fileInfo = await fileRep.StoreAudioFileAsync(projectId, versionId, FILE_NAME, stm);

                    if (fileInfo.FileName != FILE_NAME)
                        throw new InvalidOperationException($"StoreAudioFileAsync Filename: {fileInfo.FileName} does not equal expected value: {FILE_NAME}");
                }

                fileInfo = await fileRep.GetAudioFileInfoAsync(projectId, versionId, FILE_NAME);

                if (fileInfo.FileName != FILE_NAME)
                    throw new InvalidOperationException($"GetAudioFileInfoAsync Filename: {fileInfo.FileName} does not equal expected value: {FILE_NAME}");

                await fileRep.DeleteAudioFileAsync(projectId, versionId, FILE_NAME);

                Console.WriteLine("AudioFileIntegrationTest successful!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }

        }
    }
}
