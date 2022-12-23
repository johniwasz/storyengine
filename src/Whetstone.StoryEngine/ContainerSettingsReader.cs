using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Amazon;
using Microsoft.Extensions.Logging;
using Whetstone.StoryEngine.ConfigurationExtensions;
using Whetstone.StoryEngine.Models.Configuration;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine
{
    public class ContainerSettingsReader : IContainerSettingsReader
    {
        /// <summary>
        /// Points to the system-managed parameter store which tells the lambda where to pull environment
        /// load values from.
        /// </summary>
        public static readonly string BOOTSTRAP = "BOOTSTRAP";

        /// <summary>
        /// The AWS_DEFAULT_REGION environment variable comes in on the lambda function and indicates the
        /// current region hosting the lambda. 
        /// </summary>
        public static readonly string AWSDEFAULTREGION = "AWS_DEFAULT_REGION";


       


        // TODO Pull from environment.
        public LogLevel LogLevel => LogLevel.Debug;



        public string BootstrapParameter => GetBootstrapParameter();

        public RegionEndpoint GetAwsEndpoint()
        {
            return GetAwsEndpointInternal();
        }

        public static RegionEndpoint GetAwsDefaultEndpoint()
        {
            return GetAwsEndpointInternal();
        }


        private static string GetBootstrapParameter()
        {
            return System.Environment.GetEnvironmentVariable(BOOTSTRAP);

        }


        private static RegionEndpoint GetAwsEndpointInternal()
        {
            string curRegion = System.Environment.GetEnvironmentVariable(AWSDEFAULTREGION);
            RegionEndpoint curRegionEndpoint;
            if (string.IsNullOrWhiteSpace(curRegion))
                curRegionEndpoint = RegionEndpoint.USEast1;
            else
                curRegionEndpoint = RegionEndpoint.GetBySystemName(curRegion);

            return curRegionEndpoint;
        }

    }
}
