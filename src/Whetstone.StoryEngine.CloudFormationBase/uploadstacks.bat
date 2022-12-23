@echo off
set stackbucketpath=s3://whetstone-utility/cloud-formation/
set stackbaseurl=https://whetstone-utility.s3.amazonaws.com/cloud-formation/

REM upload the key stack
REM https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyengineenvkey.yml
aws s3 cp storyengineenvkey.yml %stackbucketpath%storyengineenvkey.yml                                                               
aws cloudformation validate-template --template-url %stackbaseurl%storyengineenvkey.yml

REM upload the cognito stack
REM https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginecognito.yml
aws s3 cp storyenginecognito.yml %stackbucketpath%storyenginecognito.yml                                                       
aws cloudformation validate-template --template-url %stackbaseurl%storyenginecognito.yml

REM Upload the VPC Stack
REM https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyengine_vpc.yml
aws s3 cp storyengine_vpc.yml %stackbucketpath%storyengine_vpc.yml                                                               
aws cloudformation validate-template --template-url %stackbaseurl%storyengine_vpc.yml


REM Upload the Queue Stack
REM https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginesessionqueue.yml
aws s3 cp storyenginesessionqueue.yml %stackbucketpath%storyenginesessionqueue.yml                                                              
aws cloudformation validate-template --template-url %stackbaseurl%storyenginesessionqueue.yml


REM Upload the Bucket Stack
REM https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginebucket.yml
aws s3 cp storyenginebucket.yml %stackbucketpath%storyenginebucket.yml                                                              
aws cloudformation validate-template --template-url %stackbaseurl%storyenginebucket.yml


REM Upload the OpenVpn
aws s3 cp storyengineopenvpn.yml  %stackbucketpath%storyengineopenvpn.yml                                                            
aws cloudformation validate-template --template-url %stackbaseurl%storyengineopenvpn.yml

REM Upload the dynamodb user table stack
REM https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginedynamodb.yml  
aws s3 cp storyenginedynamodb.yml  %stackbucketpath%storyenginedynamodb.yml                                                         
aws cloudformation validate-template --template-url %stackbaseurl%storyenginedynamodb.yml

REM Upload the caching table stack
REM https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginecachetable.yml  
aws s3 cp storyenginecachetable.yml  %stackbucketpath%storyenginecachetable.yml                                                         
aws cloudformation validate-template --template-url %stackbaseurl%storyenginecachetable.yml

REM Upload the database stack
aws s3 cp storyenginepostgresql.yml %stackbucketpath%storyenginepostgresql.yml                                                       
aws cloudformation validate-template --template-url %stackbaseurl%storyenginepostgresql.yml


REM Upload the aurora stack
aws s3 cp storyengineaurora.yml  %stackbucketpath%storyengineaurora.yml                                                       
aws cloudformation validate-template --template-url %stackbaseurl%storyengineaurora.yml

REM Uploade the API Secrets stack
REM https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyengineapikeys.yml
aws s3 cp storyengineapikeys.yml  %stackbucketpath%storyengineapikeys.yml                                                     
aws cloudformation validate-template --template-url %stackbaseurl%storyengineapikeys.yml

REM Upload the policy stack
aws s3 cp storyenginesharedpolicies.yml %stackbucketpath%storyenginesharedpolicies.yml
aws cloudformation validate-template --template-url %stackbaseurl%storyenginesharedpolicies.yml


REM Upload aurora back up stack
aws s3 cp snapshots_tool_aurora_source.json %stackbucketpath%snapshots_tool_aurora_source.json
aws cloudformation validate-template --template-url %stackbaseurl%snapshots_tool_aurora_source.json