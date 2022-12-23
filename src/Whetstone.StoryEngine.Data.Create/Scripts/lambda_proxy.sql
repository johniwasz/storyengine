CREATE USER lambda_proxy WITH LOGIN; 

GRANT CONNECT ON DATABASE devsbsstoryengine TO lambda_proxy;

GRANT rds_iam TO lambda_proxy;

GRANT USAGE ON SCHEMA whetstone TO lambda_proxy;


GRANT SELECT ON whetstone.titles  TO lambda_proxy;

GRANT SELECT ON whetstone.titleversiondeployments TO lambda_proxy;

GRANT SELECT ON whetstone.titleversions TO lambda_proxy;


GRANT SELECT,INSERT,UPDATE ON whetstone.phonenumbers  TO lambda_proxy;


GRANT SELECT,INSERT,UPDATE ON whetstone.title_clientusers  TO lambda_proxy;

GRANT SELECT,INSERT,UPDATE ON whetstone.userphoneconsents  TO lambda_proxy;


GRANT ALL PRIVILEGES ON whetstone.titleversions  TO lambda_proxy;

GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA whetstone TO lambda_proxy;