using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Configuration
{

    /// <summary>
    /// Returned by the Lambda used by the AWS::CloudFormation:CustomResource.
    /// The total size of the response body cannot exceed 4096 bytes.
    /// </summary>
    /// <remarks>
    /// Returned by the custom CloudFormation action. Properties are based on the documentation available here:
    /// https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/crpg-ref-responses.html
    /// </remarks>
    public class CustomResourceResponse
    {


        public static readonly string SUCCESS_STATUS = "SUCCESS";
        public static readonly string FAILED_STATUS = "FAILED";


        /// <summary>
        /// The status value sent by the custom resource provider in response to an AWS CloudFormation-generated request.
        /// Must be either SUCCESS or FAILED.
        /// </summary>
        [JsonProperty(PropertyName = "Status", Required = Required.Always)]
        public string Status { get; set; }


        /// <summary>
        /// Describes the reason for a failure response.
        /// Required if Status is FAILED. It's optional otherwise.
        /// </summary>
        [JsonProperty(PropertyName = "Reason", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Reason { get; set; }

        /// <summary>
        /// This value should be an identifier unique to the custom resource vendor, and can be up to 1 Kb in size.
        /// The value must be a non-empty string and must be identical for all responses for the same resource.
        /// </summary>
        [JsonProperty(PropertyName = "PhysicalResourceId", Required = Required.Always)]
        public string PhysicalResourceId { get; set; }

        /// <summary>
        /// The Amazon Resource Name (ARN) that identifies the stack that contains the custom resource.
        /// This response value should be copied <i>verbatim</i> from the request.
        /// </summary>
        [JsonProperty(PropertyName = "StackId", Required = Required.Always)]
        public string StackId { get; set; }



        /// <summary>
        /// A unique ID for the request. This response value should be copied <i>verbatim</i> from the request.
        /// </summary>
        [JsonProperty(PropertyName = "RequestId", Required = Required.Always)]
        public string RequestId { get; set; }


        /// <summary>
        /// The template developer-chosen name (logical ID) of the custom resource in the AWS CloudFormation template.
        /// This response value should be copied <i>verbatim</i> from the request.
        /// </summary>
        [JsonProperty(PropertyName = "LogicalResourceId", Required = Required.Always)]
        public string LogicalResourceId { get; set; }

        /// <summary>
        /// Optional. Indicates whether to mask the output of the custom resource when retrieved by using the Fn::GetAtt function.
        /// If set to true, all returned values are masked with asterisks (*****). The default value is false.
        /// </summary>
        [JsonProperty(PropertyName = "NoEcho", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? NoEcho { get; set; }



        /// <summary>
        /// Optional. The custom resource provider-defined name-value pairs to send with the response.
        /// You can access the values provided here by name in the template with Fn::GetAtt.
        /// <b>Important</b>: If the name-value pairs contain sensitive information, you should use the
        /// NoEcho field to mask the output of the custom resource. Otherwise, the values are visible through
        /// APIs that surface property values (such as DescribeStackEvents).
        /// </summary>
        [JsonProperty(PropertyName = "Data", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public dynamic Data { get; set; }

    }
}
