from __future__ import print_function
import boto3
import cfnresponse
import logging
import os
import sys
# import DB-API 2.0 compliant module for PygreSQL 
from pgdb import connect
import base64
from botocore.exceptions import ClientError
import json

logger = logging.getLogger()
logger.setLevel(logging.INFO)

DBHOST = os.environ['DBHost']
DBPORT = os.environ['DBPort']
DBNAME = os.environ['DBName']
DBUSER = os.environ['DBUser']
SECRET_ARN = os.environ['Secret_ARN']
REGION_NAME = os.environ['Region_Name']

# Here cfnresponse.SUCCESS is sent even when the operations fail as we dont want the whole cloudformation stack to rollback because the bootstrap of the DB failed
# You can always inspect(using cloudwatch log stream assosiated with the lambda function) why the bootstrap SQLs failed and manually run the SQLs to bootstrap the newly created Database

def handler(event, context):
    try:
        responseData = {}
      
        try:
            DBPASS = get_secret(SECRET_ARN,REGION_NAME)
            # Connection to SSL enabled Aurora PG database using RDS root certificate
            HOSTPORT=DBHOST + ':' + str(DBPORT)
            my_connection = connect(database=DBNAME, host=HOSTPORT, user=DBUSER, password=DBPASS, sslmode='require', sslrootcert = 'rds-combined-ca-bundle.pem')
            logger.info("SUCCESS: Connection to RDS PG instance succeeded")
      
        except Exception as e:
            logger.error('Exception: ' + str(e))
            logger.error("ERROR: Unexpected error: Couldn't connect to Aurora PostgreSQL instance.")
            responseData['Data'] = "ERROR: Unexpected error: Couldn't connect to Aurora PostgreSQL instance."
            cfnresponse.send(event, context, cfnresponse.SUCCESS, responseData, "None")
            sys.exit()
     
        if event['RequestType'] == 'Create':
            try:
                with my_connection.cursor() as cur:
                    #Execute bootstrap SQLs
                    cur.execute("create extension if not exists pg_stat_statements")
                    cur.execute("create extension if not exists pgaudit")
                    my_connection.commit()
                    cur.close()
                    my_connection.close()
                    responseData['Data'] = "SUCCESS: Executed SQL statements successfully."
                    cfnresponse.send(event, context, cfnresponse.SUCCESS, responseData, "None")
            except Exception as e:
                logger.error('Exception: ' + str(e))
                responseData['Data'] = "ERROR: Exception encountered!"
                cfnresponse.send(event, context, cfnresponse.SUCCESS, responseData, "None")
        else:
            responseData['Data'] = "{} is unsupported stack operation for this lambda function.".format(event['RequestType'])
            cfnresponse.send(event, context, cfnresponse.SUCCESS, responseData, "None")
          
    except Exception as e:
        logger.error('Exception: ' + str(e))
        responseData['Data'] = str(e)
        cfnresponse.send(event, context, cfnresponse.SUCCESS, responseData, "None")
        
def get_secret(secret_arn,region_name):

    # Create a Secrets Manager client
    session = boto3.session.Session()
    client = session.client(
        service_name='secretsmanager',
        region_name=region_name
    )

    try:
        get_secret_value_response = client.get_secret_value(
            SecretId=secret_arn
        )
    except ClientError as e:
        if e.response['Error']['Code'] == 'DecryptionFailureException':
            logger.error("Secrets Manager can't decrypt the protected secret text using the provided KMS key")
        elif e.response['Error']['Code'] == 'InternalServiceErrorException':
            logger.error("An error occurred on the server side")
        elif e.response['Error']['Code'] == 'InvalidParameterException':
            logger.error("You provided an invalid value for a parameter")
        elif e.response['Error']['Code'] == 'InvalidRequestException':
            logger.error("You provided a parameter value that is not valid for the current state of the resource")
        elif e.response['Error']['Code'] == 'ResourceNotFoundException':
            logger.error("We can't find the resource that you asked for")
    else:
        # Decrypts secret using the associated KMS CMK.
        secret = json.loads(get_secret_value_response['SecretString'])['password']
        return secret