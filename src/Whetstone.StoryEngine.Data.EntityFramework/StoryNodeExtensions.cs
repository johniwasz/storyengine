using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Ssml;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    internal static class StoryNodeExtensions
    {


        internal static List<string> FindExportFiles(this StoryNode saveNode)
        {

            List<string> exportFiles = new List<string>();


            if (saveNode.ResponseSet != null)
            {
                foreach (var response in saveNode.ResponseSet)
                {

                    if ((response.LocalizedResponses?.Any()).GetValueOrDefault(false))
                    {
                        foreach (LocalizedResponse locResponse in response.LocalizedResponses)
                        {
                            if ((locResponse.SpeechResponses?.Any()).GetValueOrDefault(false))
                            {
                                foreach (var speechResp in locResponse.SpeechResponses)
                                {
                                    if ((speechResp.SpeechFragments?.Any()).GetValueOrDefault(false))
                                    {
                                        foreach (var clientResp in speechResp.SpeechFragments)
                                        {
                                            if (clientResp is AudioFile)
                                            {
                                                AudioFile clientAudio = (AudioFile)clientResp;
                                                if (!exportFiles.Contains(clientAudio.FileName))
                                                {
                                                    exportFiles.Add(clientAudio.FileName);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if ((locResponse.RepromptSpeechResponses?.Any()).GetValueOrDefault(false))
                            {
                                foreach (var speechResp in locResponse.RepromptSpeechResponses)
                                {
                                    if ((speechResp.SpeechFragments?.Any()).GetValueOrDefault(false))
                                    {
                                        foreach (var clientResp in speechResp.SpeechFragments)
                                        {
                                            if (clientResp is AudioFile)
                                            {
                                                AudioFile clientAudio = (AudioFile)clientResp;
                                                if (!exportFiles.Contains(clientAudio.FileName))
                                                {
                                                    exportFiles.Add(clientAudio.FileName);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (locResponse.LargeImageFile != null)
                            {
                                if (!exportFiles.Contains(locResponse.LargeImageFile))
                                {
                                    exportFiles.Add(locResponse.LargeImageFile);
                                }
                            }

                            if (locResponse.SmallImageFile != null)
                            {
                                if (!exportFiles.Contains(locResponse.SmallImageFile))
                                {
                                    exportFiles.Add(locResponse.SmallImageFile);
                                }
                            }
                        }
                    }
                }
            }


            return exportFiles;
        }

    }
}
