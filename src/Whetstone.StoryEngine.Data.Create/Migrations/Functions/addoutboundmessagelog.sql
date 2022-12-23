


create or replace function whetstone.addoutboundmessagelog(p_id uuid, p_outboundmessageid uuid, p_isexception boolean, p_httstatus int, p_extendedstatus text, 
p_status int, p_providersendduration int, p_logtime timestamp without time zone)
    RETURNS void
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE
AS $BODY$

BEGIN

 
UPDATE whetstone.outbound_messages SET status = p_status
WHERE id = p_outboundmessageid;


	INSERT INTO whetstone.outboundmessage_logs(id, outboundmessageid, isexception, httpstatus, extendedstatus, status, providersendduration, logtime)
		VALUES (p_id, p_outboundmessageid, p_isexception, p_httpstatus, p_extendedstatus, p_status, p_providersendduration, p_logtime);


END;

$BODY$;

--alter function whetstone.addupdatephonenumber(uuid, text, integer, boolean, text, boolean, text, text, text, text, text, text, text, text, text, text, timestamp) owner to whetstoneadmin;

GRANT EXECUTE ON FUNCTION whetstone.addoutboundmessagelog(uuid,  uuid, boolean, int, text, int, int, timestamp without time zone)  to lambda_proxy;

GRANT EXECUTE ON FUNCTION whetstone.addoutboundmessagelog(uuid,  uuid, boolean, int, text, int, int, timestamp without time zone)  to storyengineuser;