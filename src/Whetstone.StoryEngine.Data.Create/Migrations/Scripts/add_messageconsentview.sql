

  CREATE OR REPLACE VIEW whetstone.messageconsents AS
 SELECT cu.titleid,
        CASE
            WHEN om.status = 2 THEN true
            ELSE false
        END AS status,
    consent.titleclientuserid AS userid,
    ph.phonenumber,
    omlogs.logtime AS sendtime,
    om.providermessageid,
    om.message AS code,
    engreq.sessionid,
    consent.smsconsentdate
   FROM whetstone.outbound_messages om
     JOIN whetstone.outboundmessage_logs omlogs ON om.id = omlogs.outboundmessageid
     JOIN whetstone.outbound_batches ob ON om.outboundbatchrecordid = ob.id
     JOIN whetstone.engine_requestaudit engreq ON ob.enginerequestid = engreq.id
     JOIN whetstone.userphoneconsents consent ON ob.consentid = consent.id
     JOIN whetstone.phonenumbers ph ON ob.smstonumberid = ph.id
     JOIN whetstone.title_clientusers cu ON consent.titleclientuserid = cu.id
  WHERE omlogs.status <> 1;

  ALTER TABLE whetstone.messageconsents
    OWNER TO whetstoneadmin;

 GRANT SELECT ON TABLE whetstone.messageconsents TO lambda_proxy;
  GRANT ALL ON TABLE whetstone.messageconsents TO whetstoneadmin;


