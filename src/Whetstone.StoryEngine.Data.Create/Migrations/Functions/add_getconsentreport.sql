create or replace function whetstone.getconsentreport(ptitleid uuid, 
                                                      pstarttime timestamp without time zone, 
													  pendtime timestamp without time zone)
   RETURNS TABLE(
  successstatus boolean,
  userid uuid,
  phonenumber text,
  sendtime timestamp without time zone,
  providermessageid text,
  code text,
  sessionid uuid,
  smsconsentdate timestamp without time zone
  )
language 'plpgsql'
as $BODY$
BEGIN

RETURN QUERY
select mc."status" as successstatus, mc.userid, mc.phonenumber, mc.sendtime, mc.providermessageid, 
           mc.code, mc.sessionid, mc.smsconsentdate  
     from whetstone.messageconsents mc
   where mc.sendtime>=getconsentreport.pstarttime AND mc.sendtime<=getconsentreport.pendtime AND mc.titleid=getconsentreport.ptitleid
   ORDER BY mc.sendtime DESC;


END;
$BODY$;


GRANT EXECUTE ON FUNCTION whetstone.getconsentreport(uuid, timestamp without time zone, timestamp without time zone) to lambda_proxy;