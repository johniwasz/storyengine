CREATE SCHEMA IF NOT EXISTS whetstone;
CREATE TABLE IF NOT EXISTS whetstone.efmigrationhistory (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK_efmigrationhistory" PRIMARY KEY ("MigrationId")
);

CREATE SCHEMA IF NOT EXISTS whetstone;

CREATE EXTENSION IF NOT EXISTS hstore;
CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE whetstone.phonenumbers (
    id uuid NOT NULL,
    phonenumber text NOT NULL,
    type integer NOT NULL,
    isverified boolean NOT NULL,
    nationalformat text NULL,
    cangetsmsmessage boolean NOT NULL,
    countrycode text NULL,
    carriercountrycode text NULL,
    carriernetworkcode text NULL,
    "carrierName" text NULL,
    carriererrorcode text NULL,
    url text NULL,
    registeredname text NULL,
    registeredtype text NULL,
    registerederrorcode text NULL,
    phoneservice text NULL,
    createdate timestamp without time zone NOT NULL,
    CONSTRAINT "PK_phonenumbers" PRIMARY KEY (id)
);

CREATE TABLE whetstone.titles (
    id uuid NOT NULL,
    shortname text NOT NULL,
    title text NOT NULL,
    description text NULL,
    CONSTRAINT "PK_titles" PRIMARY KEY (id)
);

CREATE TABLE whetstone.title_clientusers (
    id uuid NOT NULL,
    titleid uuid NOT NULL,
    clientuserid text NULL,
    client integer NOT NULL,
    userlocale text NULL,
    storynodename text NULL,
    nodename text NULL,
    createdtime timestamp without time zone NOT NULL,
    lastaccesseddate timestamp without time zone NOT NULL,
    titlecrumbs jsonb NULL,
    permanenttitlecrumbs jsonb NULL,
    CONSTRAINT "PK_title_clientusers" PRIMARY KEY (id),
    CONSTRAINT "FK_title_clientusers_titles_titleid" FOREIGN KEY (titleid) REFERENCES whetstone.titles (id) ON DELETE CASCADE
);

CREATE TABLE whetstone.titleversions (
    id uuid NOT NULL,
    titleid uuid NOT NULL,
    version text NOT NULL,
    description text NULL,
    isdeleted boolean NOT NULL,
    deletedate timestamp without time zone NULL,
    CONSTRAINT "PK_titleversions" PRIMARY KEY (id),
    CONSTRAINT "FK_titleversions_titles_titleid" FOREIGN KEY (titleid) REFERENCES whetstone.titles (id) ON DELETE CASCADE
);

CREATE TABLE whetstone.engine_session (
    id uuid NOT NULL,
    titleuserid uuid NOT NULL,
    sessionid text NULL,
    userid text NULL,
    userlocale varchar(10) NULL,
    deploymentid uuid NOT NULL,
    startdate timestamp without time zone NULL,
    lastaccesseddate timestamp without time zone NULL,
    sessionattributes hstore NULL,
    CONSTRAINT "PK_engine_session" PRIMARY KEY (id),
    CONSTRAINT "FK_engine_session_title_clientusers_titleuserid" FOREIGN KEY (titleuserid) REFERENCES whetstone.title_clientusers (id) ON DELETE CASCADE
);

CREATE TABLE whetstone.titleversiondeployments (
    id uuid NOT NULL,
    versionid uuid NOT NULL,
    client integer NOT NULL,
    clientidentifier text NULL,
    alias text NULL,
    publishdate timestamp without time zone NULL,
    isdeleted boolean NOT NULL,
    deletedate timestamp without time zone NULL,
    CONSTRAINT "PK_titleversiondeployments" PRIMARY KEY (id),
    CONSTRAINT "FK_titleversiondeployments_titleversions_versionid" FOREIGN KEY (versionid) REFERENCES whetstone.titleversions (id) ON DELETE CASCADE
);

CREATE TABLE whetstone.userphoneconsents (
    id uuid NOT NULL,
    titleclientuserid uuid NOT NULL,
    phoneid uuid NOT NULL,
    titleversionid uuid NOT NULL,
    name text NULL,
    isconsentgiven boolean NOT NULL,
    smsconsentdate timestamp without time zone NULL,
    enginerequestid uuid NOT NULL,
    CONSTRAINT "PK_userphoneconsents" PRIMARY KEY (id),
    CONSTRAINT "FK_userphoneconsents_phonenumbers_phoneid" FOREIGN KEY (phoneid) REFERENCES whetstone.phonenumbers (id) ON DELETE CASCADE,
    CONSTRAINT "FK_userphoneconsents_title_clientusers_titleclientuserid" FOREIGN KEY (titleclientuserid) REFERENCES whetstone.title_clientusers (id) ON DELETE CASCADE,
    CONSTRAINT "FK_userphoneconsents_titleversions_titleversionid" FOREIGN KEY (titleversionid) REFERENCES whetstone.titleversions (id) ON DELETE CASCADE
);

CREATE TABLE whetstone.engine_requestaudit (
    requestid text NOT NULL,
    sessionid uuid NOT NULL,
    id uuid NOT NULL,
    intentname text NULL,
    slots hstore NULL,
    selectiontime timestamp without time zone NOT NULL,
    processduration bigint NOT NULL,
    prenodeactionlog text NULL,
    postnodeactionlog text NULL,
    mappednode text NULL,
    requesttype integer NOT NULL,
    canfulfill integer NULL,
    slotfulfillment jsonb NULL,
    requestattributes hstore NULL,
    rawtext text NULL,
    intentconfidence real NULL,
    responsebody text NULL,
    requestbody text NULL,
    engineerror text NULL,
    responseconversionerror text NULL,
    CONSTRAINT "PK_engine_requestaudit" PRIMARY KEY (sessionid, requestid),
    CONSTRAINT "AK_engine_requestaudit_id" UNIQUE (id),
    CONSTRAINT "FK_engine_requestaudit_engine_session_sessionid" FOREIGN KEY (sessionid) REFERENCES whetstone.engine_session (id) ON DELETE CASCADE
);

CREATE TABLE whetstone.sms_outbound_batches (
    id uuid NOT NULL,
    titlecientid uuid NOT NULL,
    enginerequestid uuid NULL,
    smstonumberid uuid NOT NULL,
    smsfromnumberid uuid NULL,
    consentid uuid NULL,
    provider integer NOT NULL,
    allsent boolean NOT NULL,
    CONSTRAINT "PK_sms_outbound_batches" PRIMARY KEY (id),
    CONSTRAINT "FK_sms_outbound_batches_userphoneconsents_consentid" FOREIGN KEY (consentid) REFERENCES whetstone.userphoneconsents (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_sms_outbound_batches_phonenumbers_smsfromnumberid" FOREIGN KEY (smsfromnumberid) REFERENCES whetstone.phonenumbers (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_sms_outbound_batches_phonenumbers_smstonumberid" FOREIGN KEY (smstonumberid) REFERENCES whetstone.phonenumbers (id) ON DELETE CASCADE
);

CREATE TABLE whetstone.sms_outbound_messages (
    id uuid NOT NULL,
    smsoutboundbatchrecordid uuid NOT NULL,
    message text NULL,
    status integer NULL,
    providermessageid text NULL,
    "OutboundSmsBatchId" uuid NULL,
    CONSTRAINT "PK_sms_outbound_messages" PRIMARY KEY (id),
    CONSTRAINT "FK_sms_outbound_messages_sms_outbound_batches_OutboundSmsBatch~" FOREIGN KEY ("OutboundSmsBatchId") REFERENCES whetstone.sms_outbound_batches (id) ON DELETE RESTRICT
);

CREATE TABLE whetstone.sms_outboundmessage_logs (
    id uuid NOT NULL,
    "outboundsmsmessageId" uuid NOT NULL,
    isexception boolean NULL,
    httpstatus integer NULL,
    extendedstatus text NULL,
    status integer NOT NULL,
    queuemessageid text NULL,
    logtime timestamp without time zone NOT NULL,
    CONSTRAINT "PK_sms_outboundmessage_logs" PRIMARY KEY (id),
    CONSTRAINT "FK_sms_outboundmessage_logs_sms_outbound_messages_outboundsmsm~" FOREIGN KEY ("outboundsmsmessageId") REFERENCES whetstone.sms_outbound_messages (id) ON DELETE CASCADE
);

CREATE INDEX "IX_engine_requestaudit_sessionid" ON whetstone.engine_requestaudit (sessionid);

CREATE INDEX "IX_engine_session_titleuserid" ON whetstone.engine_session (titleuserid);

CREATE UNIQUE INDEX "IX_phonenumbers_phonenumber" ON whetstone.phonenumbers (phonenumber);

CREATE INDEX "IX_sms_outbound_batches_consentid" ON whetstone.sms_outbound_batches (consentid);

CREATE INDEX "IX_sms_outbound_batches_smsfromnumberid" ON whetstone.sms_outbound_batches (smsfromnumberid);

CREATE INDEX "IX_sms_outbound_batches_smstonumberid" ON whetstone.sms_outbound_batches (smstonumberid);

CREATE INDEX "IX_sms_outbound_messages_OutboundSmsBatchId" ON whetstone.sms_outbound_messages ("OutboundSmsBatchId");

CREATE INDEX "IX_sms_outboundmessage_logs_outboundsmsmessageId" ON whetstone.sms_outboundmessage_logs ("outboundsmsmessageId");

CREATE INDEX "IX_title_clientusers_titleid" ON whetstone.title_clientusers (titleid);

CREATE UNIQUE INDEX "IX_titles_shortname" ON whetstone.titles (shortname);

CREATE INDEX "IX_titleversiondeployments_versionid" ON whetstone.titleversiondeployments (versionid);

CREATE UNIQUE INDEX "IX_titleversions_titleid_version" ON whetstone.titleversions (titleid, version);

CREATE INDEX "IX_userphoneconsents_phoneid" ON whetstone.userphoneconsents (phoneid);

CREATE INDEX "IX_userphoneconsents_titleclientuserid" ON whetstone.userphoneconsents (titleclientuserid);

CREATE INDEX "IX_userphoneconsents_titleversionid" ON whetstone.userphoneconsents (titleversionid);

-- FUNCTION: whetstone.addintentaction(uuid, uuid, uuid, text, text, uuid, text, character varying, text, text, text, text, integer, timestamp without time zone, timestamp without time zone, hstore, integer, integer, jsonb, real, text, hstore, hstore, text, text, text, text)

-- DROP FUNCTION whetstone.addintentaction(uuid, uuid, uuid, text, text, uuid, text, character varying, text, text, text, text, integer, timestamp without time zone, timestamp without time zone, hstore, integer, integer, jsonb, real, text, hstore, hstore, text, text, text, text);

CREATE OR REPLACE FUNCTION whetstone.addintentaction(
	engineuserid uuid,
	sessionkey uuid,
	enginerequestid uuid,
	requestid text,
	sessionid text,
	deploymentkeyid uuid,
	userid text,
	locale character varying,
	intentname text,
	prenodeactionlog text,
	mappednode text,
	postnodeactionlog text,
	requesttype integer,
	starttime timestamp without time zone,
	selectiontime timestamp without time zone,
	slots hstore,
	processduration integer,
	canfulfill integer,
	fulfillmentslots jsonb,
	intentconfidence real,
	rawtext text,
	requestattributes hstore,
	sessionattributes hstore,
	requestbodytext text,
	responsebodytext text,
	engineerrortext text,
	responseconvtext text)
    RETURNS void
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
AS $BODY$

DECLARE
  lastaccesstime timestamp without time zone;
BEGIN



  SELECT ss.lastaccesseddate
  INTO  lastaccesstime
  FROM whetstone.engine_session ss
  where ss.id = sessionkey;

  IF lastaccesstime  IS NULL THEN
    BEGIN
      INSERT INTO whetstone.engine_session
        (id, titleuserid, sessionid, userid, userlocale, deploymentid, startdate, lastaccesseddate, sessionattributes)
      VALUES (addintentaction.sessionkey, addintentaction.engineuserid, addintentaction.sessionid,
              addintentaction.userid, addintentaction.locale, addintentaction.deploymentkeyid, addintentaction.selectiontime,
			  addintentaction.selectiontime, addintentaction.sessionattributes);
    EXCEPTION WHEN SQLSTATE '23505' THEN
      -- Another process added the session and committed the transaction before this insert fired
             SELECT  ss.lastaccesseddate
                INTO lastaccesstime
            FROM whetstone.engine_session ss
              where ss.id = addintentaction.sessionkey;
      END;
  ELSE


    UPDATE whetstone.engine_session
    SET lastaccesseddate = addintentaction.selectiontime
    WHERE
     id = sessionkey;

  END IF;

     INSERT INTO whetstone.engine_requestaudit (id, requestid, sessionid,  intentname, prenodeactionlog, mappednode,
                  postnodeactionlog, requesttype, slots, selectiontime,
                  processduration, canFulfill, slotfulfillment, requestattributes, rawtext, intentconfidence,
        requestbody, responsebody, engineerror, responseconversionerror)
    VALUES (addintentaction.enginerequestid,  addintentaction.requestid, sessionkey,  addintentaction.intentname, addintentaction.prenodeactionlog,
           addintentaction.mappednode, addintentaction.postnodeactionlog, addintentaction.requesttype,
            addintentaction.slots, addintentaction.selectiontime, addintentaction.processduration,
            addintentaction.canfulfill, addintentaction.fulfillmentslots,
			addintentaction.requestattributes, addintentaction.rawtext, addintentaction.intentconfidence,
      addintentaction.requestbodytext, addintentaction.responsebodytext,
	  addintentaction.engineerrortext, addintentaction.responseconvtext);


  IF starttime IS NOT NULL THEN
    UPDATE whetstone.engine_session
      SET startdate = addintentaction.starttime
      WHERE id = sessionkey;
  end if;

  if lastaccesstime < addintentaction.selectiontime THEN
    UPDATE whetstone.engine_session
      SET lastaccesseddate = addintentaction.selectiontime
      WHERE id = sessionkey;
  end if;




END;

$BODY$;

ALTER FUNCTION whetstone.addintentaction(uuid, uuid, uuid, text, text, uuid, text, character varying, text, text, text, text, integer, timestamp without time zone, timestamp without time zone, hstore, integer, integer, jsonb, real, text, hstore, hstore, text, text, text, text)
    OWNER TO whetstoneadmin;

GRANT EXECUTE ON FUNCTION whetstone.addintentaction(uuid, uuid, uuid, text, text, uuid, text, character varying, text, text, text, text, integer, timestamp without time zone, timestamp without time zone, hstore, integer, integer, jsonb, real, text, hstore, hstore, text, text, text, text) TO lambda_sessionaudit;





INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
VALUES ('20190603153838_InitialCreate', '2.2.4-servicing-10062');


