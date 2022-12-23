@echo off

set stackbucketpath=s3://whetstone-key-prod-s3utilitybucket-kb4k4vpl95k9/cloud-formation/
set stackbaseurl=https://whetstone-key-prod-s3utilitybucket-kb4k4vpl95k9.s3.amazonaws.com/cloud-formation/

REM Upload the key stack
REM https://whetstone-key-prod-s3utilitybucket-kb4k4vpl95k9.s3.amazonaws.com/cloud-formation/storyengineenvkey.yml
aws s3 cp storyengineenvkey.yml %stackbucketpath%storyengineenvkey.yml                                                               
aws cloudformation validate-template --template-url %stackbaseurl%storyengineenvkey.yml

REM upload the cognito stack.
REM https://whetstone-key-prod-s3utilitybucket-kb4k4vpl95k9.s3.amazonaws.com/cloud-formation/storyenginecognito.yml
aws s3 cp storyenginecognito.yml %stackbucketpath%storyenginecognito.yml                                                            
aws cloudformation validate-template --template-url %stackbaseurl%storyenginecognito.yml

REM Upload the VPC Stack
REM https://whetstone-key-prod-s3utilitybucket-kb4k4vpl95k9.s3.amazonaws.com/cloud-formation/storyengine_vpc.yml  
aws s3 cp storyengine_vpc.yml %stackbucketpath%storyengine_vpc.yml                                                               
aws cloudformation validate-template --template-url %stackbaseurl%storyengine_vpc.yml


REM Upload the Queue Stack
aws s3 cp storyenginesessionqueue.yml %stackbucketpath%storyenginesessionqueue.yml                                                              
aws cloudformation validate-template --template-url %stackbaseurl%storyenginesessionqueue.yml


REM Upload the Bucket Stack
aws s3 cp storyenginebucket.yml %stackbucketpath%storyenginebucket.yml                                                              
aws cloudformation validate-template --template-url %stackbaseurl%storyenginebucket.yml


REM Upload the OpenVpn
REM https://whetstone-key-prod-s3utilitybucket-kb4k4vpl95k9.s3.amazonaws.com/cloud-formation/storyengineopenvpn.yml 
aws s3 cp storyengineopenvpn.yml  %stackbucketpath%storyengineopenvpn.yml                                                            
aws cloudformation validate-template --template-url %stackbaseurl%storyengineopenvpn.yml

REM Upload the dynamodb cache table

REM https://whetstone-key-prod-s3utilitybucket-kb4k4vpl95k9.s3.amazonaws.com/cloud-formation/storyenginecachetable.yml      
aws s3 cp storyenginecachetable.yml  %stackbucketpath%storyenginecachetable.yml                                                         
aws cloudformation validate-template --template-url %stackbaseurl%storyenginecachetable.yml

REM Upload the dynamodb user tables
aws s3 cp storyenginedynamodb.yml  %stackbucketpath%storyenginedynamodb.yml                                                         
aws cloudformation validate-template --template-url %stackbaseurl%storyenginedynamodb.yml

REM Upload the policy stack
aws s3 cp storyenginesharedpolicies.yml %stackbucketpath%storyenginesharedpolicies.yml
aws cloudformation validate-template --template-url %stackbaseurl%storyenginesharedpolicies.yml

REM Upload the Aurora database stack
REM https://whetstone-key-prod-s3utilitybucket-kb4k4vpl95k9.s3.amazonaws.com/cloud-formation/storyengineaurora.yml    
aws s3 cp storyengineaurora.yml %stackbucketpath%storyengineaurora.yml                                                              
aws cloudformation validate-template --template-url %stackbaseurl%storyengineaurora.yml  