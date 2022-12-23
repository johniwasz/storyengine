SELECT NOT msglog.isexception as successtatus,
  tcu.id  as userid, LTRIM(pn.phonenumber, '+1') as phonenumber, msglog.logtime as sendtime, msg.providermessageid,
 CASE tcu.client
      WHEN 1 THEN 'Stateline-Alexa'
      WHEN 2 THEN 'Stateline-Google'
      ELSE 'Unknown'
  END as "code",
 es.id as sessionid,
 pc.smsconsentdate

  from whetstone.titles t
INNER JOIN whetstone.title_clientusers tcu on t.id = tcu.titleid
INNER JOIN whetstone.sms_outbound_batches batches on tcu.id = batches.titleclientid
 INNER JOIN whetstone.phonenumbers pn ON batches.smstonumberid = pn.id
INNER JOIN whetstone.sms_outbound_messages msg on batches.id = msg."OutboundSmsBatchId"
 INNER JOIN whetstone.sms_outboundmessage_logs msglog ON msg.id = msglog."outboundsmsmessageId"
INNER JOIN whetstone.engine_requestaudit ra on batches.enginerequestid = ra.id
  INNER JOIN whetstone.engine_session es on ra.sessionid = es.id
   INNER JOIN whetstone.userphoneconsents pc on batches.consentid = pc.id
where shortname = 'whetstonetechnologies' and msglog.status = 2
 and (pn.phonenumber = '+16105551212' OR pn.phonenumber = '+12675551212')

ORDER BY logtime DESC
limit 30;