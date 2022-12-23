using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Whetstone.StoryEngine.Models.Configuration
{

    // For Troubleshooting stuck templates:
    // https://aws.amazon.com/premiumsupport/knowledge-center/cloudformation-lambda-resource-delete/

    public enum ResourceRequestType
    {
        Create = 1,
        Update =2,
        Delete =3 

    }

    /// <summary>
    /// The template developer uses the AWS CloudFormation resource, AWS::CloudFormation::CustomResource, to specify a custom resource in a template.
    /// In AWS::CloudFormation::CustomResource, all properties are defined by the custom resource provider.There is only one required property: ServiceToken.
    /// </summary>
    /// <remarks>
    /// Sent by the CloudFormation action. Properties are based on the documentation available here:
    /// https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/crpg-ref-requests.html
    /// </remarks>
    public class CustomResourceRequest
    {



        /// <summary>
        /// The service token (an Amazon SNS topic or AWS Lambda function Amazon Resource Name) that is obtained from the custom resource provider to access the service.
        /// The service token must be in the same region in which you are creating the stack.
        /// </summary>
        /// <remarks>
        /// All other fields in the resource properties are optional and are sent, verbatim, to the custom resource provider in the request's
        /// ResourceProperties field. The provider defines both the names and the valid contents of these fields.
        /// </remarks>
        [JsonProperty(PropertyName = "ServiceToken", Required = Required.Always)]
        public string ServiceToken { get; set; }



        /// <summary>
        ///  Per Amazon documentation, this can be "Create", "Update", or "Delete" 
        /// </summary>
        [JsonProperty(PropertyName = "RequestType", Required =  Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResourceRequestType RequestType { get; set; }


        /// <summary>
        /// The response URL identifies a presigned S3 bucket that receives responses from the custom resource
        /// provider to AWS CloudFormation.
        /// </summary>
        [JsonProperty(PropertyName= "ResponseURL", Required =  Required.Always)]
        public string ResponseUrl { get; set; }

        /// <summary>
        /// The Amazon Resource Name (ARN) that identifies the stack that contains the custom resource.
        /// Combining the StackId with the RequestId forms a value that you can use to uniquely identify a
        /// request on a particular custom resource.
        /// </summary>
        [JsonProperty(PropertyName = "StackId", Required = Required.Always)]
        public string StackId { get; set; }


        /// <summary>
        ///A unique ID for the request.
        /// Combining the StackId with the RequestId forms a value that you can use to uniquely identify a
        /// request on a particular custom resource.
        /// </summary>
        [JsonProperty(PropertyName = "RequestId", Required = Required.Always)]
        public string RequestId { get; set; }

        /// <summary>
        /// The template developer-chosen resource type of the custom resource in the AWS CloudFormation template.
        /// Custom resource type names can be up to 60 characters long and can include alphanumeric and the following
        /// characters: _@-.
        /// </summary>
        [JsonProperty(PropertyName = "ResourceType", Required = Required.Always)]
        public string ResourceType { get; set; }

        /// <summary>
        /// A required custom resource provider-defined physical ID that is unique for that provider.
        /// Always sent with Update and Delete requests; never sent with Create.
        /// </summary>
        [JsonProperty(PropertyName = "PhysicalResourceId", Required = Required.Default)]
        public string PhysicalResourceId { get; set; }



        /// <summary>
        /// The template developer-chosen name (logical ID) of the custom resource in the AWS
        /// CloudFormation template. This is provided to facilitate communication between the custom
        /// resource provider and the template developer.
        /// </summary>
        [JsonProperty(PropertyName = "LogicalResourceId", Required = Required.Always)]
        public string LogicalResourceId { get; set; }

        /// <summary>
        /// This field contains the contents of the Properties object sent by the template developer.
        /// Its contents are defined by the custom resource provider.
        /// </summary>
        [JsonProperty(PropertyName = "ResourceProperties", Required = Required.Default)]
        public dynamic ResourceProperties { get; set; }

        /// <summary>
        /// Used only for Update requests. Contains the resource properties that were declared
        /// previous to the update request.
        /// </summary>
        [JsonProperty(PropertyName = "OldResourceProperties", Required = Required.Default)]
        public dynamic OldResourceProperties { get; set; }



    }
}
