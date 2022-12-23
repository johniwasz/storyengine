create or replace function whetstone.addsmstwiliolog(twiliomessageid text, isexception boolean, httpstatus int, extendedstatus text, messagestatus int, queuemessageid text)
  returns void
language plpgsql
as $$
DECLARE
  parentmessageid uuid;
  logtime timestamp without time zone;
BEGIN


  SELECT id
  INTO parentmessageid
  FROM whetstone.sms_text_messages
  where providermessageid = twiliomessageid;

  IF parentmessageid IS NULL THEN
     RAISE EXCEPTION 'Twilio provider message id not found: %', twiliomessageid USING ERRCODE = 'WS00001';
  END IF;
END;
$$;


--GRANT EXECUTE ON FUNCTION whetstone.addsmstwiliolog(twiliomessageid text, isexception boolean, httpstatus int, extendedstatus text, messagestatus int, queuemessageid text)
--  TO lambda_proxy;