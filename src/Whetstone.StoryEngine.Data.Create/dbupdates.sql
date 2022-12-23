CREATE SCHEMA IF NOT EXISTS whetstone;
CREATE TABLE IF NOT EXISTS whetstone.efmigrationhistory (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK_efmigrationhistory" PRIMARY KEY ("MigrationId")
);

create or replace function whetstone.addintentaction(requestid text, sessionid text, client integer, titleid text, titleversion character varying, userid text, locale character varying,
  intentname text, prenodeactionlog text, mappednode text, postnodeactionlog text, requesttype integer,
  starttime timestamp without time zone, selectiontime timestamp without time zone, slots hstore, processduration integer, canfulfill integer, fulfillmentslots jsonb)
  returns void
language plpgsql
as $$
DECLARE
  sessionkey uuid;
  lastaccesstime timestamp without time zone;
BEGIN

  SELECT ss.id, ss.lastaccesseddate
  INTO sessionkey, lastaccesstime
  FROM whetstone.story_session ss
  where ss.sessionid = addintentaction.sessionid
        and ss.client = addintentaction.client
        AND ss.titleid = addintentaction.titleId;

  IF sessionKey IS NULL THEN

    sessionkey := uuid_generate_v4();

    BEGIN
      INSERT INTO whetstone.story_session
        (id, sessionid, client, userid, userlocale, titleid, titleversion, lastaccesseddate)
      VALUES (sessionkey, addintentaction.sessionid, addintentaction.client, addintentaction.userid,
              addintentaction.locale, addintentaction.titleid, addintentaction.titleversion, addintentaction.selectiontime);
    EXCEPTION WHEN SQLSTATE '23505' THEN
      -- Another process added the session and committed the transaction before this insert fired
             SELECT ss.id, ss.lastaccesseddate
                INTO sessionkey, lastaccesstime
            FROM whetstone.story_session ss
              where ss.sessionid = addintentaction.sessionid
              and ss.client = addintentaction.client
              AND ss.titleid = addintentaction.titleId;
     END;




   INSERT INTO whetstone.intent_action (requestid, sessionid,  intentname, prenodeactionlog, mappednode,
                  postnodeactionlog, requesttype, slots, selectiontime,
                  processduration, canFulfill, slotfulfillment)
    VALUES (addintentaction.requestid, sessionkey,  addintentaction.intentname, addintentaction.prenodeactionlog,
           addintentaction.mappednode, addintentaction.postnodeactionlog, addintentaction.requesttype,
            addintentaction.slots, addintentaction.selectiontime, addintentaction.processduration,
            addintentaction.canfulfill, addintentaction.fulfillmentslots);
      
  ELSE
   INSERT INTO whetstone.intent_action (requestid, sessionid,  intentname, prenodeactionlog, mappednode,
                  postnodeactionlog, requesttype, slots, selectiontime,
                  processduration, canFulfill, slotfulfillment)
    VALUES (addintentaction.requestid, sessionkey,  addintentaction.intentname, addintentaction.prenodeactionlog,
           addintentaction.mappednode, addintentaction.postnodeactionlog, addintentaction.requesttype,
            addintentaction.slots, addintentaction.selectiontime, addintentaction.processduration,
            addintentaction.canfulfill, addintentaction.fulfillmentslots);

    UPDATE whetstone.story_session
    SET lastaccesseddate = addintentaction.selectiontime
    WHERE
     id = sessionkey;

  END IF;


  IF starttime IS NOT NULL THEN
    UPDATE whetstone.story_session 
      SET startdate = addintentaction.starttime
      WHERE id = sessionkey;
  end if;

  if lastaccesstime < addintentaction.selectiontime THEN
    UPDATE whetstone.story_session 
      SET lastaccesseddate = addintentaction.selectiontime
      WHERE id = sessionkey;
  end if;




END;
$$;


GRANT EXECUTE ON FUNCTION whetstone.addintentaction(requestid text, sessionid text, client integer, titleid text, titleversion character varying, userid text, locale character varying,
  intentname text, prenodeactionlog text, mappednode text, postnodeactionlog text, requesttype integer,
  starttime timestamp without time zone, selectiontime timestamp without time zone, slots hstore, processduration integer, canfulfill integer, fulfillmentslots jsonb)
  TO lambda_proxy;

INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
VALUES ('20181002184735_UserContextFunctions', '2.2.4-servicing-10062');

ALTER TABLE whetstone.intent_action ADD requestbody text NULL;

ALTER TABLE whetstone.intent_action ADD responsebody text NULL;

INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
VALUES ('20190127043623_ClientBodyLogging', '2.2.4-servicing-10062');

create or replace function whetstone.addintentactionbody(sessionidparam text, requestidparam text, client integer, body text, bodytype int)
  returns void
language plpgsql
as $$
DECLARE
  sessionkey uuid;
BEGIN

  SELECT ss.id
  INTO sessionkey
  FROM whetstone.story_session ss
  where ss.sessionid = addintentactionbody.sessionidparam
        and ss.client = addintentactionbody.client;


  IF sessionKey IS NULL THEN
        RAISE EXCEPTION 'Session not found % for client %', sessionidparam, client USING ERRCODE = 'WS00001';
  ELSE 

     IF EXISTS (SELECT 1 FROM whetstone.intent_action WHERE requestid = requestidparam and sessionid = sessionkey) THEN
			IF bodytype = 1 THEN
				UPDATE whetstone.intent_action 
					SET  requestbody = addintentactionbody.body
					WHERE
					 requestid = addintentactionbody.requestidparam AND  sessionid = sessionKey;

			ELSE
					UPDATE whetstone.intent_action 
					SET  responsebody = addintentactionbody.body
					WHERE
					 requestid = addintentactionbody.requestidparam AND sessionid = sessionKey;
			END IF;
	ELSE

			RAISE EXCEPTION 'Request id % not found for session % and client %',  resquestidparam, sessionidparam, client USING ERRCODE = 'WS00001';

	END IF;



  END IF;




END;
$$;


GRANT EXECUTE ON FUNCTION whetstone.addintentactionbody(sessionidparam text, requestidparam text, client integer, body text, bodytype int)
  TO lambda_proxy;

INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
VALUES ('20190127063623_RequestBodyFunctions', '2.2.4-servicing-10062');

CREATE TABLE whetstone.outbound_sms_batch (
    id uuid NOT NULL,
    userid text NOT NULL,
    applicationid text NOT NULL,
    sessionid text NOT NULL,
    requestid text NOT NULL,
    smstonumber text NOT NULL,
    smsfromnumber text NULL,
    sendservice text NULL,
    allsent boolean NOT NULL,
    CONSTRAINT "PK_outbound_sms_batch" PRIMARY KEY (id)
);

CREATE TABLE whetstone.sms_text_messages (
    id uuid NOT NULL,
    outboundsmsbatchid uuid NOT NULL,
    message text NULL,
    status integer NULL,
    providermessageid text NULL,
    CONSTRAINT "PK_sms_text_messages" PRIMARY KEY (id),
    CONSTRAINT "FK_sms_text_messages_outbound_sms_batch_outboundsmsbatchid" FOREIGN KEY (outboundsmsbatchid) REFERENCES whetstone.outbound_sms_batch (id) ON DELETE CASCADE
);

CREATE TABLE whetstone.sms_message_log (
    id uuid NOT NULL,
    "outboundsmsmessageId" uuid NOT NULL,
    isexception boolean NULL,
    httpstatus integer NULL,
    extendedstatus text NULL,
    status integer NOT NULL,
    queuemessageid text NULL,
    logtime timestamp without time zone NOT NULL,
    CONSTRAINT "PK_sms_message_log" PRIMARY KEY (id),
    CONSTRAINT "FK_sms_message_log_sms_text_messages_outboundsmsmessageId" FOREIGN KEY ("outboundsmsmessageId") REFERENCES whetstone.sms_text_messages (id) ON DELETE CASCADE
);

CREATE INDEX "IX_sms_message_log_outboundsmsmessageId" ON whetstone.sms_message_log ("outboundsmsmessageId");

CREATE INDEX "IX_sms_text_messages_outboundsmsbatchid" ON whetstone.sms_text_messages (outboundsmsbatchid);

INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
VALUES ('20190127154453_SmsMessageLogging01', '2.2.4-servicing-10062');

create or replace function whetstone.addintentactionbodies(sessionidparam text, requestidparam text, client integer, requestbodyparam text, responsebodyparam text)
  returns void
language plpgsql
as $$
DECLARE
  sessionkey uuid;
BEGIN

  SELECT ss.id
  INTO sessionkey
  FROM whetstone.story_session ss
  where ss.sessionid = addintentactionbodies.sessionidparam
        and ss.client = addintentactionbodies.client;


  IF sessionKey IS NULL THEN
        RAISE EXCEPTION 'Session not found % for client %', sessionidparam, client USING ERRCODE = 'WS00001';
  ELSE

     IF EXISTS (SELECT 1 FROM whetstone.intent_action WHERE requestid = requestidparam and sessionid = sessionkey) THEN

			IF requestbodyparam IS NULL AND responsebodyparam IS NOT NULL THEN
				UPDATE whetstone.intent_action
					SET  responsebody = addintentactionbodies.responsebodyparam
					WHERE
					 requestid = addintentactionbodies.requestidparam AND  sessionid = sessionKey;

			ELSEIF requestbodyparam IS NOT NULL AND responsebodyparam IS NULL THEN
					UPDATE whetstone.intent_action
					SET  requestbody = addintentactionbodies.requestbodyparam
					WHERE
					 requestid = addintentactionbodies.requestidparam AND sessionid = sessionKey;
			ELSE
					UPDATE whetstone.intent_action
					SET  requestbody = addintentactionbodies.requestbodyparam, 
					     responsebody = addintentactionbodies.responsebodyparam
					WHERE
					 requestid = addintentactionbodies.requestidparam AND sessionid = sessionKey;
			END IF;
	  ELSE

			RAISE EXCEPTION 'Request id % not found for session % and client %',  requestidparam, sessionidparam, client USING ERRCODE = 'WS00001';
     END IF;
	END IF;


END;
$$;


GRANT EXECUTE ON FUNCTION whetstone.addintentactionbodies(sessionidparam text, requestidparam text, client integer, requestbodyparam text, responsebodyparam text)
  TO lambda_proxy;

INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
VALUES ('20190128063623_RequestBodiesFunctions', '2.2.4-servicing-10062');

ALTER TABLE whetstone.story_session ADD sessionattributes hstore NULL;

ALTER TABLE whetstone.intent_action ADD intentconfidence real NULL;

ALTER TABLE whetstone.intent_action ADD rawtext text NULL;

ALTER TABLE whetstone.intent_action ADD requestattributes hstore NULL;

create or replace function whetstone.addintentaction(requestid text, sessionid text, client integer, titleid text, titleversion character varying, userid text, locale character varying,
  intentname text, prenodeactionlog text, mappednode text, postnodeactionlog text, requesttype integer,
  starttime timestamp without time zone, selectiontime timestamp without time zone, slots hstore, processduration integer, canfulfill integer, fulfillmentslots jsonb, 	intentconfidence real,
	rawtext text,
	requestattributes hstore,
	sessionattributes hstore)
  returns void
language plpgsql
as $$
DECLARE
  sessionkey uuid;
  lastaccesstime timestamp without time zone;
BEGIN

  SELECT ss.id, ss.lastaccesseddate
  INTO sessionkey, lastaccesstime
  FROM whetstone.story_session ss
  where ss.sessionid = addintentaction.sessionid
        and ss.client = addintentaction.client
        AND ss.titleid = addintentaction.titleId;

  IF sessionKey IS NULL THEN

    sessionkey := uuid_generate_v4();

    BEGIN
      INSERT INTO whetstone.story_session
        (id, sessionid, client, userid, userlocale, titleid, titleversion, lastaccesseddate, sessionattributes)
      VALUES (sessionkey, addintentaction.sessionid, addintentaction.client, addintentaction.userid,
              addintentaction.locale, addintentaction.titleid, addintentaction.titleversion, 
			  addintentaction.selectiontime, addintentaction.sessionattributes);
    EXCEPTION WHEN SQLSTATE '23505' THEN
      -- Another process added the session and committed the transaction before this insert fired
             SELECT ss.id, ss.lastaccesseddate
                INTO sessionkey, lastaccesstime
            FROM whetstone.story_session ss
              where ss.sessionid = addintentaction.sessionid
              and ss.client = addintentaction.client
              AND ss.titleid = addintentaction.titleId;
     END;




   INSERT INTO whetstone.intent_action (requestid, sessionid,  intentname, prenodeactionlog, mappednode,
                  postnodeactionlog, requesttype, slots, selectiontime,
                  processduration, canFulfill, slotfulfillment, requestattributes, rawtext, intentconfidence)
    VALUES (addintentaction.requestid, sessionkey,  addintentaction.intentname, addintentaction.prenodeactionlog,
           addintentaction.mappednode, addintentaction.postnodeactionlog, addintentaction.requesttype,
            addintentaction.slots, addintentaction.selectiontime, addintentaction.processduration,
            addintentaction.canfulfill, addintentaction.fulfillmentslots, 
			addintentaction.requestattributes, addintentaction.rawtext, addintentaction.intentconfidence);
      
  ELSE
   INSERT INTO whetstone.intent_action (requestid, sessionid,  intentname, prenodeactionlog, mappednode,
                  postnodeactionlog, requesttype, slots, selectiontime,
                  processduration, canFulfill, slotfulfillment, requestattributes, rawtext, intentconfidence)
    VALUES (addintentaction.requestid, sessionkey,  addintentaction.intentname, addintentaction.prenodeactionlog,
           addintentaction.mappednode, addintentaction.postnodeactionlog, addintentaction.requesttype,
            addintentaction.slots, addintentaction.selectiontime, addintentaction.processduration,
            addintentaction.canfulfill, addintentaction.fulfillmentslots,
			addintentaction.requestattributes, addintentaction.rawtext, addintentaction.intentconfidence);

    UPDATE whetstone.story_session
    SET lastaccesseddate = addintentaction.selectiontime
    WHERE
     id = sessionkey;

  END IF;


  IF starttime IS NOT NULL THEN
    UPDATE whetstone.story_session 
      SET startdate = addintentaction.starttime
      WHERE id = sessionkey;
  end if;

  if lastaccesstime < addintentaction.selectiontime THEN
    UPDATE whetstone.story_session 
      SET lastaccesseddate = addintentaction.selectiontime
      WHERE id = sessionkey;
  end if;




END;
$$;


GRANT EXECUTE ON FUNCTION whetstone.addintentaction(requestid text, sessionid text, client integer, titleid text, titleversion character varying, userid text, locale character varying,
  intentname text, prenodeactionlog text, mappednode text, postnodeactionlog text, requesttype integer,
  starttime timestamp without time zone, selectiontime timestamp without time zone, slots hstore, processduration integer, canfulfill integer, fulfillmentslots jsonb, 	intentconfidence real,
	rawtext text,
	requestattributes hstore,
	sessionattributes hstore)
  TO lambda_proxy;

INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
VALUES ('20190131143512_UserLoggingUpdate01', '2.2.4-servicing-10062');

create or replace function whetstone.addintentaction(requestid text, sessionid text, client integer, titleid text, titleversion character varying, userid text, locale character varying,
  intentname text, prenodeactionlog text, mappednode text, postnodeactionlog text, requesttype integer,
  starttime timestamp without time zone, selectiontime timestamp without time zone, slots hstore, processduration integer, canfulfill integer, fulfillmentslots jsonb, 	intentconfidence real,
	rawtext text,
	requestattributes hstore,
	sessionattributes hstore,
  requestbodytext text,
  responsebodytext text)
  returns void
language plpgsql
as $$
DECLARE
  sessionkey uuid;
  lastaccesstime timestamp without time zone;
BEGIN

  SELECT ss.id, ss.lastaccesseddate
  INTO sessionkey, lastaccesstime
  FROM whetstone.story_session ss
  where ss.sessionid = addintentaction.sessionid
        and ss.client = addintentaction.client
        AND ss.titleid = addintentaction.titleId;

  IF sessionKey IS NULL THEN

    sessionkey := uuid_generate_v4();

    BEGIN
      INSERT INTO whetstone.story_session
        (id, sessionid, client, userid, userlocale, titleid, titleversion, lastaccesseddate, sessionattributes)
      VALUES (sessionkey, addintentaction.sessionid, addintentaction.client, addintentaction.userid,
              addintentaction.locale, addintentaction.titleid, addintentaction.titleversion,
			  addintentaction.selectiontime, addintentaction.sessionattributes);
    EXCEPTION WHEN SQLSTATE '23505' THEN
      -- Another process added the session and committed the transaction before this insert fired
             SELECT ss.id, ss.lastaccesseddate
                INTO sessionkey, lastaccesstime
            FROM whetstone.story_session ss
              where ss.sessionid = addintentaction.sessionid
              and ss.client = addintentaction.client
              AND ss.titleid = addintentaction.titleId;
     END;




   INSERT INTO whetstone.intent_action (requestid, sessionid,  intentname, prenodeactionlog, mappednode,
                  postnodeactionlog, requesttype, slots, selectiontime,
                  processduration, canFulfill, slotfulfillment, requestattributes, rawtext, intentconfidence,
        requestbody, responsebody)
    VALUES (addintentaction.requestid, sessionkey,  addintentaction.intentname, addintentaction.prenodeactionlog,
           addintentaction.mappednode, addintentaction.postnodeactionlog, addintentaction.requesttype,
            addintentaction.slots, addintentaction.selectiontime, addintentaction.processduration,
            addintentaction.canfulfill, addintentaction.fulfillmentslots,
			addintentaction.requestattributes, addintentaction.rawtext, addintentaction.intentconfidence,
      addintentaction.requestbodytext, addintentaction.responsebodytext);

  ELSE
   INSERT INTO whetstone.intent_action (requestid, sessionid,  intentname, prenodeactionlog, mappednode,
                  postnodeactionlog, requesttype, slots, selectiontime,
                  processduration, canFulfill, slotfulfillment, requestattributes, rawtext, intentconfidence,
                      requestbody, responsebody)
    VALUES (addintentaction.requestid, sessionkey,  addintentaction.intentname, addintentaction.prenodeactionlog,
           addintentaction.mappednode, addintentaction.postnodeactionlog, addintentaction.requesttype,
            addintentaction.slots, addintentaction.selectiontime, addintentaction.processduration,
            addintentaction.canfulfill, addintentaction.fulfillmentslots,
			addintentaction.requestattributes, addintentaction.rawtext, addintentaction.intentconfidence,
      addintentaction.requestbodytext, addintentaction.responsebodytext);


    UPDATE whetstone.story_session
    SET lastaccesseddate = addintentaction.selectiontime
    WHERE
     id = sessionkey;

  END IF;


  IF starttime IS NOT NULL THEN
    UPDATE whetstone.story_session
      SET startdate = addintentaction.starttime
      WHERE id = sessionkey;
  end if;

  if lastaccesstime < addintentaction.selectiontime THEN
    UPDATE whetstone.story_session
      SET lastaccesseddate = addintentaction.selectiontime
      WHERE id = sessionkey;
  end if;




END;
$$;


GRANT EXECUTE ON FUNCTION whetstone.addintentaction(requestid text, sessionid text, client integer, titleid text, titleversion character varying, userid text, locale character varying,
  intentname text, prenodeactionlog text, mappednode text, postnodeactionlog text, requesttype integer,
  starttime timestamp without time zone, selectiontime timestamp without time zone, slots hstore, processduration integer, canfulfill integer, fulfillmentslots jsonb, 	intentconfidence real,
	rawtext text,
	requestattributes hstore,
	sessionattributes hstore,
    requestbodytext text,
  responsebodytext text)
  TO lambda_proxy;

INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
VALUES ('20190217_SaveSessionConsolidatedFunctions', '2.2.4-servicing-10062');

CREATE TABLE whetstone.titles (
    id uuid NOT NULL,
    shortname text NOT NULL,
    title text NOT NULL,
    description text NULL,
    CONSTRAINT "PK_titles" PRIMARY KEY (id)
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

CREATE TABLE whetstone.titleversiondeployments (
    id uuid NOT NULL,
    versionid uuid NOT NULL,
    client integer NOT NULL,
    clientidentifier text NULL,
    publishdate timestamp without time zone NULL,
    isdeleted boolean NOT NULL,
    deletedate timestamp without time zone NULL,
    CONSTRAINT "PK_titleversiondeployments" PRIMARY KEY (id),
    CONSTRAINT "FK_titleversiondeployments_titleversions_versionid" FOREIGN KEY (versionid) REFERENCES whetstone.titleversions (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_titles_shortname" ON whetstone.titles (shortname);

CREATE INDEX "IX_titleversiondeployments_versionid" ON whetstone.titleversiondeployments (versionid);

CREATE UNIQUE INDEX "IX_titleversions_titleid_version" ON whetstone.titleversions (titleid, version);

INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
VALUES ('20190227164242_TitleUserUpdate01', '2.2.4-servicing-10062');

ALTER TABLE whetstone.intent_action ADD engineerror text NULL;

ALTER TABLE whetstone.intent_action ADD responseconversionerror text NULL;

create or replace function whetstone.addintentaction(requestid text, sessionid text, client integer, titleid text, titleversion character varying, userid text, locale character varying,
  intentname text, prenodeactionlog text, mappednode text, postnodeactionlog text, requesttype integer,
  starttime timestamp without time zone, selectiontime timestamp without time zone, slots hstore, processduration integer, canfulfill integer, fulfillmentslots jsonb, 	intentconfidence real,
	rawtext text,
	requestattributes hstore,
	sessionattributes hstore,
  requestbodytext text,
  responsebodytext text,
  engineerrortext text,
  responseconvtext text)
  returns void
language plpgsql
as $$
DECLARE
  sessionkey uuid;
  lastaccesstime timestamp without time zone;
BEGIN

  SELECT ss.id, ss.lastaccesseddate
  INTO sessionkey, lastaccesstime
  FROM whetstone.story_session ss
  where ss.sessionid = addintentaction.sessionid
        and ss.client = addintentaction.client
        AND ss.titleid = addintentaction.titleId;

  IF sessionKey IS NULL THEN

    sessionkey := uuid_generate_v4();

    BEGIN
      INSERT INTO whetstone.story_session
        (id, sessionid, client, userid, userlocale, titleid, titleversion, lastaccesseddate, sessionattributes)
      VALUES (sessionkey, addintentaction.sessionid, addintentaction.client, addintentaction.userid,
              addintentaction.locale, addintentaction.titleid, addintentaction.titleversion,
			  addintentaction.selectiontime, addintentaction.sessionattributes);
    EXCEPTION WHEN SQLSTATE '23505' THEN
      -- Another process added the session and committed the transaction before this insert fired
             SELECT ss.id, ss.lastaccesseddate
                INTO sessionkey, lastaccesstime
            FROM whetstone.story_session ss
              where ss.sessionid = addintentaction.sessionid
              and ss.client = addintentaction.client
              AND ss.titleid = addintentaction.titleId;
     END;




   INSERT INTO whetstone.intent_action (requestid, sessionid,  intentname, prenodeactionlog, mappednode,
                  postnodeactionlog, requesttype, slots, selectiontime,
                  processduration, canFulfill, slotfulfillment, requestattributes, rawtext, intentconfidence,
        requestbody, responsebody, engineerror, responseconversionerror)
    VALUES (addintentaction.requestid, sessionkey,  addintentaction.intentname, addintentaction.prenodeactionlog,
           addintentaction.mappednode, addintentaction.postnodeactionlog, addintentaction.requesttype,
            addintentaction.slots, addintentaction.selectiontime, addintentaction.processduration,
            addintentaction.canfulfill, addintentaction.fulfillmentslots,
			addintentaction.requestattributes, addintentaction.rawtext, addintentaction.intentconfidence,
      addintentaction.requestbodytext, addintentaction.responsebodytext, 
	  addintentaction.engineerrortext, addintentaction.responseconvtext);

  ELSE
   INSERT INTO whetstone.intent_action (requestid, sessionid,  intentname, prenodeactionlog, mappednode,
                  postnodeactionlog, requesttype, slots, selectiontime,
                  processduration, canFulfill, slotfulfillment, requestattributes, rawtext, intentconfidence,
                      requestbody, responsebody, engineerror, responseconversionerror)
    VALUES (addintentaction.requestid, sessionkey,  addintentaction.intentname, addintentaction.prenodeactionlog,
           addintentaction.mappednode, addintentaction.postnodeactionlog, addintentaction.requesttype,
            addintentaction.slots, addintentaction.selectiontime, addintentaction.processduration,
            addintentaction.canfulfill, addintentaction.fulfillmentslots,
			addintentaction.requestattributes, addintentaction.rawtext, addintentaction.intentconfidence,
      addintentaction.requestbodytext, addintentaction.responsebodytext,
	  	  addintentaction.engineerrortext, addintentaction.responseconvtext);



    UPDATE whetstone.story_session
    SET lastaccesseddate = addintentaction.selectiontime
    WHERE
     id = sessionkey;

  END IF;


  IF starttime IS NOT NULL THEN
    UPDATE whetstone.story_session
      SET startdate = addintentaction.starttime
      WHERE id = sessionkey;
  end if;

  if lastaccesstime < addintentaction.selectiontime THEN
    UPDATE whetstone.story_session
      SET lastaccesseddate = addintentaction.selectiontime
      WHERE id = sessionkey;
  end if;




END;
$$;


GRANT EXECUTE ON FUNCTION whetstone.addintentaction(requestid text, sessionid text, client integer, titleid text, titleversion character varying, userid text, locale character varying,
  intentname text, prenodeactionlog text, mappednode text, postnodeactionlog text, requesttype integer,
  starttime timestamp without time zone, selectiontime timestamp without time zone, slots hstore, processduration integer, canfulfill integer, fulfillmentslots jsonb, 	intentconfidence real,
	rawtext text,
	requestattributes hstore,
	sessionattributes hstore,
    requestbodytext text,
  responsebodytext text,
    engineerrortext text,
  responseconvtext text)
  TO lambda_proxy;

INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
VALUES ('20190307190857_SessionErrorUpdate', '2.2.4-servicing-10062');

DROP TABLE whetstone.sms_message_log;

DROP TABLE whetstone.sms_text_messages;

DROP TABLE whetstone.outbound_sms_batch;

ALTER TABLE whetstone.titleversiondeployments ADD alias text NULL;

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

CREATE TABLE whetstone.outbound_sms_batches (
    id uuid NOT NULL,
    titlecientid uuid NOT NULL,
    enginerequestid uuid NOT NULL,
    smstonumberid uuid NOT NULL,
    smsfromnumberid uuid NULL,
    consentid uuid NULL,
    sendservice text NULL,
    allsent boolean NOT NULL,
    CONSTRAINT "PK_outbound_sms_batches" PRIMARY KEY (id),
    CONSTRAINT "FK_outbound_sms_batches_userphoneconsents_consentid" FOREIGN KEY (consentid) REFERENCES whetstone.userphoneconsents (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_outbound_sms_batches_phonenumbers_smsfromnumberid" FOREIGN KEY (smsfromnumberid) REFERENCES whetstone.phonenumbers (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_outbound_sms_batches_phonenumbers_smstonumberid" FOREIGN KEY (smstonumberid) REFERENCES whetstone.phonenumbers (id) ON DELETE CASCADE
);

CREATE TABLE whetstone.sms_outbound_messages (
    id uuid NOT NULL,
    outboundsmsbatchid uuid NOT NULL,
    message text NULL,
    status integer NULL,
    providermessageid text NULL,
    CONSTRAINT "PK_sms_outbound_messages" PRIMARY KEY (id),
    CONSTRAINT "FK_sms_outbound_messages_outbound_sms_batches_outboundsmsbatch~" FOREIGN KEY (outboundsmsbatchid) REFERENCES whetstone.outbound_sms_batches (id) ON DELETE CASCADE
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

CREATE UNIQUE INDEX "IX_engine_session_sessionid" ON whetstone.engine_session (sessionid);

CREATE INDEX "IX_engine_session_titleuserid" ON whetstone.engine_session (titleuserid);

CREATE INDEX "IX_outbound_sms_batches_consentid" ON whetstone.outbound_sms_batches (consentid);

CREATE INDEX "IX_outbound_sms_batches_smsfromnumberid" ON whetstone.outbound_sms_batches (smsfromnumberid);

CREATE INDEX "IX_outbound_sms_batches_smstonumberid" ON whetstone.outbound_sms_batches (smstonumberid);

CREATE UNIQUE INDEX "IX_phonenumbers_phonenumber" ON whetstone.phonenumbers (phonenumber);

CREATE INDEX "IX_sms_outbound_messages_outboundsmsbatchid" ON whetstone.sms_outbound_messages (outboundsmsbatchid);

CREATE INDEX "IX_sms_outboundmessage_logs_outboundsmsmessageId" ON whetstone.sms_outboundmessage_logs ("outboundsmsmessageId");

CREATE INDEX "IX_title_clientusers_titleid" ON whetstone.title_clientusers (titleid);

CREATE INDEX "IX_userphoneconsents_phoneid" ON whetstone.userphoneconsents (phoneid);

CREATE INDEX "IX_userphoneconsents_titleclientuserid" ON whetstone.userphoneconsents (titleclientuserid);

CREATE INDEX "IX_userphoneconsents_titleversionid" ON whetstone.userphoneconsents (titleversionid);

create or replace function whetstone.addengineaudit(engineuserid uuid, sessionkey uuid, enginerequestid uuid, requestid text, sessionid text, deploymentkeyid uuid, userid text, locale character varying, intentname text, prenodeactionlog text, mappednode text, postnodeactionlog text, requesttype integer, starttime timestamp without time zone, selectiontime timestamp without time zone, slots hstore, processduration integer, canfulfill integer, fulfillmentslots jsonb, intentconfidence real, rawtext text, requestattributes hstore, sessionattributes hstore, requestbodytext text, responsebodytext text, engineerrortext text, responseconvtext text)
  returns void
language plpgsql
as $$
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
$$;


GRANT EXECUTE ON FUNCTION whetstone.addengineaudit(engineuserid uuid, sessionkey uuid, enginerequestid uuid, requestid text, sessionid text, deploymentkeyid uuid, userid text, locale character varying,
  intentname text, prenodeactionlog text, mappednode text, postnodeactionlog text, requesttype integer,
  starttime timestamp without time zone, selectiontime timestamp without time zone, slots hstore, processduration integer, canfulfill integer, fulfillmentslots jsonb, 	intentconfidence real,
	rawtext text,
	requestattributes hstore,
	sessionattributes hstore,
  requestbodytext text,
  responsebodytext text,
  engineerrortext text,
  responseconvtext text)
  TO lambda_proxy;

INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
VALUES ('20190424164624_UserInfo01', '2.2.4-servicing-10062');

ALTER TABLE whetstone.outbound_sms_batches DROP CONSTRAINT "FK_outbound_sms_batches_userphoneconsents_consentid";

ALTER TABLE whetstone.outbound_sms_batches DROP CONSTRAINT "FK_outbound_sms_batches_phonenumbers_smsfromnumberid";

ALTER TABLE whetstone.outbound_sms_batches DROP CONSTRAINT "FK_outbound_sms_batches_phonenumbers_smstonumberid";

ALTER TABLE whetstone.sms_outbound_messages DROP CONSTRAINT "FK_sms_outbound_messages_outbound_sms_batches_outboundsmsbatch~";

ALTER TABLE whetstone.outbound_sms_batches DROP CONSTRAINT "PK_outbound_sms_batches";

ALTER TABLE whetstone.outbound_sms_batches DROP COLUMN sendservice;

ALTER TABLE whetstone.outbound_sms_batches RENAME TO sms_outbound_batches;

ALTER TABLE whetstone.sms_outbound_messages RENAME COLUMN outboundsmsbatchid TO "OutboundSmsBatchId";

ALTER INDEX whetstone."IX_sms_outbound_messages_outboundsmsbatchid" RENAME TO "IX_sms_outbound_messages_OutboundSmsBatchId";

ALTER INDEX whetstone."IX_outbound_sms_batches_smstonumberid" RENAME TO "IX_sms_outbound_batches_smstonumberid";

ALTER INDEX whetstone."IX_outbound_sms_batches_smsfromnumberid" RENAME TO "IX_sms_outbound_batches_smsfromnumberid";

ALTER INDEX whetstone."IX_outbound_sms_batches_consentid" RENAME TO "IX_sms_outbound_batches_consentid";

ALTER TABLE whetstone.sms_outbound_messages ALTER COLUMN "OutboundSmsBatchId" TYPE uuid;
ALTER TABLE whetstone.sms_outbound_messages ALTER COLUMN "OutboundSmsBatchId" DROP NOT NULL;
ALTER TABLE whetstone.sms_outbound_messages ALTER COLUMN "OutboundSmsBatchId" DROP DEFAULT;

ALTER TABLE whetstone.sms_outbound_messages ADD smsoutboundbatchrecordid uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE whetstone.sms_outbound_batches ADD provider integer NOT NULL DEFAULT 0;

ALTER TABLE whetstone.sms_outbound_batches ADD CONSTRAINT "PK_sms_outbound_batches" PRIMARY KEY (id);

ALTER TABLE whetstone.sms_outbound_batches ADD CONSTRAINT "FK_sms_outbound_batches_userphoneconsents_consentid" FOREIGN KEY (consentid) REFERENCES whetstone.userphoneconsents (id) ON DELETE RESTRICT;

ALTER TABLE whetstone.sms_outbound_batches ADD CONSTRAINT "FK_sms_outbound_batches_phonenumbers_smsfromnumberid" FOREIGN KEY (smsfromnumberid) REFERENCES whetstone.phonenumbers (id) ON DELETE RESTRICT;

ALTER TABLE whetstone.sms_outbound_batches ADD CONSTRAINT "FK_sms_outbound_batches_phonenumbers_smstonumberid" FOREIGN KEY (smstonumberid) REFERENCES whetstone.phonenumbers (id) ON DELETE CASCADE;

ALTER TABLE whetstone.sms_outbound_messages ADD CONSTRAINT "FK_sms_outbound_messages_sms_outbound_batches_OutboundSmsBatch~" FOREIGN KEY ("OutboundSmsBatchId") REFERENCES whetstone.sms_outbound_batches (id) ON DELETE RESTRICT;

INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
VALUES ('20190425020418_PhoneUpdate01', '2.2.4-servicing-10062');


