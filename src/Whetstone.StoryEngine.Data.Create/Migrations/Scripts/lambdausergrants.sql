GRANT USAGE ON SCHEMA public TO lambda_proxy;

GRANT USAGE ON SCHEMA whetstone TO lambda_proxy;

GRANT INSERT ON whetstone.intent_action TO lambda_proxy;

GRANT INSERT ON whetstone.story_session TO lambda_proxy;

GRANT SELECT ON whetstone.intent_action TO lambda_proxy;

GRANT SELECT ON whetstone.story_session TO lambda_proxy;

GRANT UPDATE ON whetstone.story_session TO lambda_proxy;

--GRANT lambda_sessionaudit TO sonibridgeuser;

--GRANT lambda_sessionaudit TO storyengineeuser;


--GRANT lambda_proxy TO sonibridgeuser;