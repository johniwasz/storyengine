CREATE USER lambda_sessionaudit WITH LOGIN;

GRANT CONNECT ON DATABASE devsbsstoryengine TO lambda_sessionaudit;

GRANT rds_iam TO lambda_sessionaudit;

GRANT USAGE ON SCHEMA whetstone TO lambda_sessionaudit;


GRANT SELECT,INSERT,UPDATE ON whetstone.engine_session  TO lambda_sessionaudit;

GRANT INSERT ON whetstone.engine_requestaudit  TO lambda_sessionaudit;


