
# fire off the storyenginekeys template


aws s3 cp storeengineenvkey.yml s3://whetstone-utility/cloud-formation/

aws s3 cp denyupdatespolicy.json s3://whetstone-utility/cloud-formation/

aws s3 cp allowupdatespolicy.json s3://whetstone-utility/cloud-formation/

# Key Creation for Virginia
aws cloudformation create-stack --template-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/storeengineenvkey.yml --stack-name WhetstoneKey-Dev --capabilities CAPABILITY_IAM CAPABILITY_NAMED_IAM --notification-arns arn:aws:sns:us-east-1:940085449815:CloudFormationDeployment --stack-policy-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/allowupdatespolicy.json --region us-east-1

# Key creation in Ohio
aws cloudformation create-stack --template-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/storeengineenvkey.yml --stack-name WhetstoneKey-Prod --capabilities CAPABILITY_IAM CAPABILITY_NAMED_IAM --parameter ParameterKey=EnvironmentName,ParameterValue=prod,UsePreviousValue=false,ResolvedValue=prod ParameterKey=Purpose,ParameterValue=prod,UsePreviousValue=false,ResolvedValue=prod --notification-arns arn:aws:sns:us-east-2:940085449815:CloudFormationDeployment --stack-policy-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/allowupdatespolicy.json --region us-east-2


# For ongoing updates
aws cloudformation deploy --stack-name WhestoneKey-Dev --template-file storeengineenvironmentkey.template --parameter-overrides EnvironmentName=dev Purpose=dev 


aws s3 cp storyengine_vpc.yml s3://whetstone-utility/cloud-formation/
aws cloudformation validate-template --template-url  https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyengine_vpc.yml

aws s3 cp storyengineopenvpn.yml s3://whetstone-utility/cloud-formation/



aws cloudformation validate-template --template-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyengineopenvpn.yml
# create the key values

#Viginia
aws ssm put-parameter --name /storyengine/dev/twiliotokens --type SecureString --value [TWILIO KEYS] --overwrite --key-id "alias/devEnvironmentKey" --region us-east-1

#Ohio
aws ssm put-parameter --name /storyengine/prod/twiliotokens --type SecureString --value [TWILIO KEYS] --overwrite --key-id "alias/prodEnvironmentKey" --region us-east-2




#Oregone
# VPC admin password
aws ssm put-parameter --name /storyengine/prod/openvpnadminpassword --type SecureString --value [Open VPN password] --overwrite --key-id "alias/prodEnvironmentKey" --region us-east-2
aws ssm get-parameters --region us-east-2 --names /storyengine/prod/openvpnadminpassword --with-decryption --output text
# Elastic cache token
aws ssm put-parameter --name /storyengine/Prod/cachetoken --type SecureString --value q69tzLmnt8xz^@6j^S --overwrite --key-id "alias/ProdEnvironmentKey" --region us-west-2
aws ssm get-parameters --region us-west-2 --names /storyengine/Prod/cachetoken --with-decryption --output text
# Create VPN key pair
aws ec2 create-key-pair --region us-west-2 --key-name openvpnkeys-prod > openvpnkeys.txt



aws ssm put-parameter --name /storyengine/dev/cachetoken --type SecureString --value "@r.p(UH4G,+\y\pD" --overwrite --key-id "alias/devEnvironmentKey" --region us-east-1




#create the elastic cache

aws s3 cp storyengineelasticcache.yml s3://whetstone-utility/cloud-formation/
aws cloudformation create-stack --template-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyengineelasticcache.yml --parameter  ParameterKey=SubnetStackParam,ParameterValue=WhestoneSubnet-Dev,UsePreviousValue=false,ResolvedValue=WhestoneSubnet-Dev ParameterKey=ElasticCacheInstanceTypeParam,ParameterValue=cache.t2.micro,UsePreviousValue=false,ResolvedValue=cache.t2.micro ParameterKey=NumCacheReplicas,ParameterValue=2,UsePreviousValue=false,ResolvedValue=2 --stack-name WhestoneCache-Dev --notification-arns arn:aws:sns:us-east-1:940085449815:CloudFormationDeployment 


# create the bucket 
aws s3 cp storyenginebucket.yml s3://whetstone-utility/cloud-formation/

https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginebucket.yml

# create the bucket in Virginia
aws cloudformation create-stack --template-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginebucket.yml --capabilities CAPABILITY_IAM --parameter ParameterKey=KeyStack,ParameterValue=WhetstoneKey-Dev,UsePreviousValue=false,ResolvedValue=WhetstoneKey-Dev --stack-name WhetstoneBucket-Dev --notification-arns arn:aws:sns:us-east-1:940085449815:CloudFormationDeployment --stack-policy-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/allowupdatespolicy.json --region us-east-1

# create the prod bucket in Ohio
aws cloudformation create-stack --template-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginebucket.yml --capabilities CAPABILITY_IAM --parameter ParameterKey=KeyStack,ParameterValue=WhetstoneKey-Prod,UsePreviousValue=false,ResolvedValue=WhestoneKey-Prod --stack-name WhetstoneBucket-Prod --notification-arns arn:aws:sns:us-east-2:940085449815:CloudFormationDeployment --stack-policy-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/allowupdatespolicy.json --region us-east-2

# create the queues
aws s3 cp storyenginesessionqueue.yml s3://whetstone-utility/cloud-formation/

# create the queue in Virginia
aws cloudformation create-stack --template-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginesessionqueue.yml --capabilities CAPABILITY_IAM --parameter ParameterKey=KeyStack,ParameterValue=WhetstoneKey-Dev,UsePreviousValue=false,ResolvedValue=WhetstoneKey-Dev --stack-name WhetstoneQueue-Dev --notification-arns arn:aws:sns:us-east-1:940085449815:CloudFormationDeployment --stack-policy-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/allowupdatespolicy.json --region us-east-1

# create the queue in Ohio
aws cloudformation create-stack --template-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginesessionqueue.yml --capabilities CAPABILITY_IAM --parameter ParameterKey=KeyStack,ParameterValue=WhetstoneKey-Prod,UsePreviousValue=false,ResolvedValue=WhetstoneKey-Prod --stack-name WhetstoneQueue-Prod --notification-arns arn:aws:sns:us-east-2:940085449815:CloudFormationDeployment --stack-policy-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/allowupdatespolicy.json --region us-east-2



# Create VPC in default us-east-1 Virginia
aws cloudformation create-stack --template-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyengine_vpc.yml --parameter ParameterKey=ClassB,ParameterValue=50,UsePreviousValue=false,ResolvedValue=50 --stack-name WhetstoneVpc-Dev --notification-arns arn:aws:sns:us-east-1:940085449815:CloudFormationDeployment --stack-policy-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/allowupdatespolicy.json --region us-east-1

# Create VPC in us-east-2 Ohio
aws cloudformation create-stack --template-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyengine_vpc.yml --parameter ParameterKey=ClassB,ParameterValue=100,UsePreviousValue=false,ResolvedValue=100 ParameterKey=IsProd,ParameterValue=true,UsePreviousValue=false,ResolvedValue=true --stack-name WhetstoneVpc-Prod --notification-arns arn:aws:sns:us-east-2:940085449815:CloudFormationDeployment --stack-policy-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/allowupdatespolicy.json --region us-east-2

# Upload the elastic cache template
aws s3 cp storyengineelasticcache.yml s3://whetstone-utility/cloud-formation/
aws cloudformation validate-template --template-url  https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyengineelasticcache.yml

# Upload the dynamo db cache template
aws s3 cp storyenginecachetable.yml s3://whetstone-utility/cloud-formation/
aws cloudformation validate-template --template-url  https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginecachetable.yml


# Upload the dynamo db store template
aws s3 cp storyenginedynamodb.yml s3://whetstone-utility/cloud-formation/
aws cloudformation validate-template --template-url  https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginedynamodb.yml


aws s3 cp storyengine_vpc.yml s3://whetstone-utility/cloud-formation/
aws cloudformation validate-template --template-url  https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyengine_vpc.yml

aws s3 cp storyenginepostgresql.yml s3://whetstone-utility/cloud-formation/

aws s3 cp dbbootstrap.zip s3://whetstone-utility/cloud-formation/

aws s3 cp dbbootstrap.zip s3://whetstonekey-prod-s3utilitybucket-1tun4h8azk37v/cloud-formation/

aws s3 cp storyenginepostgresql.yml s3://whetstonekey-prod-s3utilitybucket-1tun4h8azk37v/cloud-formation/

#--stack-policy-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/denyupdatespolicy.json

# aws cloudformation validate-template --template-url  https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginesessionqueue.template

# aws cloudformation validate-template --template-url  https://whetstone-utility.s3.amazonaws.com/cloud-formation/postgresql.yml

 aws cloudformation validate-template --template-url  https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyenginepostgresql.yml

 aws cloudformation validate-template --template-url  https://whetstonekey-prod-s3utilitybucket-1tun4h8azk37v.s3.amazonaws.com/cloud-formation/storyenginepostgresql.yml

aws s3 cp storyengineelasticcache.yml s3://whetstonekey-prod-s3utilitybucket-1tun4h8azk37v/cloud-formation/
 aws cloudformation validate-template --template-url  https://whetstonekey-prod-s3utilitybucket-1tun4h8azk37v.s3.amazonaws.com/cloud-formation/storyengineelasticcache.yml
#Ohio create postgres stack

ParameterKey=DBUsername,ParameterValue=whetstoneadmin,UsePreviousValue=false,ResolvedValue=whetstoneadmin ParameterKey=DBName,ParameterValue=voiceconnectr_prod,UsePreviousValue=false,ResolvedValue=voiceconnectr_prod ParameterKey=DBInstanceClass,ParameterValue=db.t3.micro,UsePreviousValue=false,ResolvedValue=db.t3.micro ParameterKey=ParentOpenVpnStack,ParameterValue=WhetstoneOpenVpn-Prod,UsePreviousValue=false,ResolvedValue=WhetstoneOpenVpn-Prod ParameterKey=ParentVpcStack,ParameterValue=WhetstoneVpc-Prod,UsePreviousValue=false,ResolvedValue=WhetstoneVpc-Prod ParameterKey=DBEngineVersion,ParameterValue=11.2,UsePreviousValue=false,ResolvedValue=11.2

# use this to get a list of all db family sets
# aws rds describe-db-engine-versions --query "DBEngineVersions[].DBParameterGroupFamily"

aws s3 cp  storyenginepostgresql.yml s3://whetstonekey-prod-s3utilitybucket-1tun4h8azk37v/cloud-formation

aws cloudformation create-stack --template-url https://whetstonekey-prod-s3utilitybucket-1tun4h8azk37v.s3.amazonaws.com/cloud-formation/storyenginepostgresql.yml --capabilities CAPABILITY_IAM CAPABILITY_NAMED_IAM CAPABILITY_AUTO_EXPAND --parameter ParameterKey=EnvironmentKeyStack,ParameterValue=WhetstoneKey-Prod,UsePreviousValue=false,ResolvedValue=WhetstoneKey-Prod ParameterKey=IsProduction,ParameterValue=true,UsePreviousValue=false,ResolvedValue=true ParameterKey=DBUsername,ParameterValue=whetstoneadmin,UsePreviousValue=false,ResolvedValue=whetstoneadmin ParameterKey=DBInstanceClass,ParameterValue=db.t3.micro,UsePreviousValue=false,ResolvedValue=db.t3.micro ParameterKey=ParentOpenVpnStack,ParameterValue=WhetstoneOpenVpn-Prod,UsePreviousValue=false,ResolvedValue=WhetstoneOpenVpn-Prod ParameterKey=ParentVpcStack,ParameterValue=WhetstoneVpc-Prod,UsePreviousValue=false,ResolvedValue=WhetstoneVpc-Prod ParameterKey=DBEngineVersion,ParameterValue=11.2,UsePreviousValue=false,ResolvedValue=11.2 --stack-name WhetstonePostgreSQL-Prod --notification-arns arn:aws:sns:us-east-2:940085449815:CloudFormationDeployment --stack-policy-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/allowupdatespolicy.json --region us-east-2

#aws ssm put-parameter --name /storyengine/dev/cachetoken --type SecureString --value [ELASTICCACHETOKEN] --overwrite --key-id "alias/devEnvironmentKey" --region us-east-1 --description "dev parameter used to store the Elastic cache key"

# create the queues and bucket

# create the step functions 


aws s3 cp regionutilbucket.yml s3://whetstone-utility/cloud-formation/

aws cloudformation validate-template --template-url  https://whetstone-utility.s3.amazonaws.com/cloud-formation/storeengineenvkey.yml


aws s3 cp storeengineenvkey.yml s3://whetstone-utility/cloud-formation/

aws s3 cp storyengineelasticcache.yml s3://whetstone-utility/cloud-formation/

aws s3 cp storyengineelasticcacheunsecure.yml s3://whetstone-utility/cloud-formation/

https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyengineelasticcacheunsecure.yml

# Get Cloud Formation parameters and output

aws cloudformation --region us-east-1 describe-stacks --stack-name WhetstoneCache-Dev --query "Stacks[0].Outputs[?OutputKey=='CacheEndpoint'].OutputValue" --output text

aws cloudformation --region us-east-1 describe-stacks --stack-name WhetstoneCache-Dev --query "Stacks[0].Outputs[?OutputKey=='CacheReadOnlyAddresses'].OutputValue" --output text

aws cloudformation --region us-east-2 describe-stacks --stack-name WhetstoneOpenVpn-Prod --query "Stacks[0].Parameters[?ParameterKey=='OpenVPNASAdminUser'].ParameterValue" --output text

aws ssm get-parameters --region us-east-2 --names /storyengine/prod/openvpnadminpassword --with-decryption --output text




aws cloudformation create-stack --template-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/storyengineaurora.yml ^
  --stack-name Whetstone-Aurora-Dev --capabilities CAPABILITY_IAM CAPABILITY_NAMED_IAM CAPABILITY_AUTO_EXPAND ^
  --notification-arns arn:aws:sns:us-east-1:940085449815:CloudFormationDeployment ^
  --stack-policy-url https://whetstone-utility.s3.amazonaws.com/cloud-formation/allowupdatespolicy.json --region us-east-1
