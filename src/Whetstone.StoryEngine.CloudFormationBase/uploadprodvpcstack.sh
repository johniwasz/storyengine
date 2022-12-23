


                                   
aws s3 cp storyengine_vpc.yml s3://whetstonekey-prod-s3utilitybucket-1tun4h8azk37v/cloud-formation

                                                    
aws cloudformation validate-template --template-url  https://whetstonekey-prod-s3utilitybucket-1tun4h8azk37v.s3.us-east-2.amazonaws.com/cloud-formation/storyengine_vpc.yml