CREATE SCHEMA IF NOT EXISTS whetstone;
CREATE TABLE IF NOT EXISTS whetstone.efmigrationhistory (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK_efmigrationhistory" PRIMARY KEY ("MigrationId")
);


DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE SCHEMA IF NOT EXISTS whetstone;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE EXTENSION IF NOT EXISTS hstore;
    CREATE EXTENSION IF NOT EXISTS postgis;
    CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE TABLE whetstone.phonenumbers (
        id uuid NOT NULL,
        phonenumber text NOT NULL,
        phonetype integer NOT NULL,
        isverified boolean NOT NULL,
        nationalformat text NULL,
        cangetsmsmessage boolean NOT NULL,
        countrycode text NULL,
        carriercountrycode text NULL,
        carriernetworkcode text NULL,
        carriername text NULL,
        carriererrorcode text NULL,
        url text NULL,
        registeredname text NULL,
        registeredtype text NULL,
        registerederrorcode text NULL,
        phoneservice text NULL,
        systemstatus integer NOT NULL,
        createdate timestamp without time zone NOT NULL,
        CONSTRAINT "PK_phonenumbers" PRIMARY KEY (id)
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE TABLE whetstone.titles (
        id uuid NOT NULL,
        shortname text NOT NULL,
        title text NOT NULL,
        description text NULL,
        CONSTRAINT "PK_titles" PRIMARY KEY (id)
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE TABLE whetstone.title_clientusers (
        id uuid NOT NULL,
        hashkey text NULL,
        titleid uuid NOT NULL,
        clientuserid text NOT NULL,
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
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE TABLE whetstone.titleversions (
        id uuid NOT NULL,
        titleid uuid NOT NULL,
        version text NOT NULL,
        description text NULL,
        isdeleted boolean NOT NULL,
        deletedate timestamp without time zone NULL,
        logfullclientmessages boolean NOT NULL,
        CONSTRAINT "PK_titleversions" PRIMARY KEY (id),
        CONSTRAINT "FK_titleversions_titles_titleid" FOREIGN KEY (titleid) REFERENCES whetstone.titles (id) ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
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
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
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
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
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
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
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
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE TABLE whetstone.outbound_batches (
        id uuid NOT NULL,
        titleclientid uuid NOT NULL,
        enginerequestid uuid NULL,
        smstonumberid uuid NULL,
        smsfromnumberid uuid NULL,
        consentid uuid NULL,
        smsprovider integer NULL,
        allsent boolean NOT NULL,
        CONSTRAINT "PK_outbound_batches" PRIMARY KEY (id),
        CONSTRAINT "FK_outbound_batches_userphoneconsents_consentid" FOREIGN KEY (consentid) REFERENCES whetstone.userphoneconsents (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_outbound_batches_phonenumbers_smsfromnumberid" FOREIGN KEY (smsfromnumberid) REFERENCES whetstone.phonenumbers (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_outbound_batches_phonenumbers_smstonumberid" FOREIGN KEY (smstonumberid) REFERENCES whetstone.phonenumbers (id) ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE TABLE whetstone.outbound_messages (
        id uuid NOT NULL,
        outboundbatchrecordid uuid NOT NULL,
        message text NULL,
        tags hstore NULL,
        status integer NULL,
        providermessageid text NULL,
        CONSTRAINT "PK_outbound_messages" PRIMARY KEY (id),
        CONSTRAINT "FK_outbound_messages_outbound_batches_id" FOREIGN KEY (id) REFERENCES whetstone.outbound_batches (id) ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE TABLE whetstone.outboundmessage_logs (
        id uuid NOT NULL,
        outboundmessageid uuid NOT NULL,
        isexception boolean NULL,
        httpstatus integer NULL,
        extendedstatus text NULL,
        status integer NOT NULL,
        logtime timestamp without time zone NOT NULL,
        providersendduration bigint NULL,
        CONSTRAINT "PK_outboundmessage_logs" PRIMARY KEY (id),
        CONSTRAINT "FK_outboundmessage_logs_outbound_messages_outboundmessageid" FOREIGN KEY (outboundmessageid) REFERENCES whetstone.outbound_messages (id) ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE INDEX "IX_engine_requestaudit_sessionid" ON whetstone.engine_requestaudit (sessionid);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE INDEX "IX_engine_session_titleuserid" ON whetstone.engine_session (titleuserid);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE INDEX "IX_outbound_batches_consentid" ON whetstone.outbound_batches (consentid);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE INDEX "IX_outbound_batches_smsfromnumberid" ON whetstone.outbound_batches (smsfromnumberid);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE INDEX "IX_outbound_batches_smstonumberid" ON whetstone.outbound_batches (smstonumberid);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE INDEX "IX_outboundmessage_logs_outboundmessageid" ON whetstone.outboundmessage_logs (outboundmessageid);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_phonenumbers_phonenumber" ON whetstone.phonenumbers (phonenumber);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_title_clientusers_titleid_client_clientuserid" ON whetstone.title_clientusers (titleid, client, clientuserid);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_titles_shortname" ON whetstone.titles (shortname);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE INDEX "IX_titleversiondeployments_versionid" ON whetstone.titleversiondeployments (versionid);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_titleversions_titleid_version" ON whetstone.titleversions (titleid, version);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE INDEX "IX_userphoneconsents_phoneid" ON whetstone.userphoneconsents (phoneid);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE INDEX "IX_userphoneconsents_titleclientuserid" ON whetstone.userphoneconsents (titleclientuserid);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    CREATE INDEX "IX_userphoneconsents_titleversionid" ON whetstone.userphoneconsents (titleversionid);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN

    BEGIN

    	do $BODY$
    		DECLARE role_exists integer;
    	BEGIN
    	   role_exists:=0;

    	  SELECT COUNT(*) INTO role_exists FROM pg_roles WHERE rolname='storyengineuser' LIMIT 1;

    	  if role_exists<1 then
    			CREATE USER storyengineuser WITH
    		LOGIN
    		NOSUPERUSER
    		NOCREATEDB
    		NOCREATEROLE
    		INHERIT
    		NOREPLICATION
    		CONNECTION LIMIT -1;

    	  end if;

    	END $BODY$;


    	DO $BODY$
    	begin
    	  execute format('grant connect on database %I to %I', current_database(), 'storyengineuser');
    	end $BODY$;

    	GRANT USAGE ON SCHEMA whetstone TO storyengineuser;

    	GRANT SELECT, INSERT, UPDATE ON ALL TABLES IN SCHEMA whetstone TO storyengineuser;


    	do $BODY$
    		DECLARE role_exists integer;
    	BEGIN
    	   role_exists:=0;

    	  SELECT COUNT(*) INTO role_exists FROM pg_roles WHERE rolname='lambda_proxy' LIMIT 1;

    	  if role_exists<1 then
    			CREATE USER lambda_proxy WITH
    		LOGIN
    		NOSUPERUSER
    		NOCREATEDB
    		NOCREATEROLE
    		INHERIT
    		NOREPLICATION
    		CONNECTION LIMIT -1;

    	  end if;

    	END $BODY$;


    	DO $BODY$
    	begin
    	  execute format('grant connect on database %I to %I', current_database(), 'lambda_proxy');
    	end $BODY$;

    	GRANT USAGE ON SCHEMA whetstone TO lambda_proxy;

    	GRANT rds_iam TO lambda_proxy;

    	GRANT SELECT, INSERT, UPDATE ON ALL TABLES IN SCHEMA whetstone TO lambda_proxy;




    	do $BODY$
    		DECLARE role_exists integer;
    	BEGIN
    	   role_exists:=0;

    	  SELECT COUNT(*) INTO role_exists FROM pg_roles WHERE rolname='lambda_sessionaudit' LIMIT 1;

    	  if role_exists<1 then
    			CREATE USER lambda_sessionaudit WITH
    		LOGIN
    		NOSUPERUSER
    		NOCREATEDB
    		NOCREATEROLE
    		INHERIT
    		NOREPLICATION
    		CONNECTION LIMIT -1;

    	  end if;

    	END $BODY$;



    	do $BODY$
    	begin
    	  execute format('grant connect on database %I to %I', current_database(), 'lambda_sessionaudit');
    	end $BODY$;

    	GRANT USAGE ON SCHEMA whetstone TO lambda_sessionaudit;

    	GRANT rds_iam TO lambda_sessionaudit;

    	GRANT SELECT, INSERT, UPDATE ON ALL TABLES IN SCHEMA whetstone TO lambda_sessionaudit;

    END;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
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
      foundtitleid uuid;
      clienttype int;
    BEGIN


    -- Get the title id from the deployment
    SELECT  tv.titleid, tvd.client
    INTO foundtitleid, clienttype
    FROM whetstone.titleversiondeployments tvd
    INNER JOIN whetstone.titleversions tv ON tvd.versionid = tv.id
    WHERE tvd.id = deploymentkeyid;


      BEGIN

        INSERT INTO whetstone.title_clientusers
            (id, titleid, client, clientuserid, userlocale, createdtime, lastaccesseddate)
          VALUES (engineuserid, foundtitleid, clienttype,  addintentaction.userid,
                   addintentaction.locale,  addintentaction.selectiontime, addintentaction.selectiontime);
        EXCEPTION WHEN SQLSTATE '23505' THEN
          -- Another process added the session and committed the transaction before this insert fired
          -- do nothing
        END;


      SELECT ss.lastaccesseddate
      INTO  lastaccesstime
      FROM whetstone.engine_session ss
      where ss.id = sessionkey;

      IF lastaccesstime  IS NULL THEN
        BEGIN
          INSERT INTO whetstone.engine_session
            (id, titleuserid, sessionid, userid, userlocale, deploymentid, startdate, lastaccesseddate, sessionattributes)
          VALUES (addintentaction.sessionkey, engineuserid, addintentaction.sessionid,
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



    GRANT EXECUTE ON FUNCTION whetstone.addintentaction(uuid, uuid, uuid, text, text, uuid, text, character varying, text, text, text, text, integer, timestamp without time zone, timestamp without time zone, hstore, integer, integer, jsonb, real, text, hstore, hstore, text, text, text, text) TO lambda_sessionaudit;

    GRANT EXECUTE ON FUNCTION whetstone.addintentaction(uuid, uuid, uuid, text, text, uuid, text, character varying, text, text, text, text, integer, timestamp without time zone, timestamp without time zone, hstore, integer, integer, jsonb, real, text, hstore, hstore, text, text, text, text) TO storyengineuser;


    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN



    create or replace function whetstone.addupdatephonenumber(p_id uuid, p_phonenumber text, p_phonetype integer, p_isverified boolean, p_nationalformat text, p_cangetsmsmessage boolean, p_countrycode text, p_carriercountrycode text, p_carriernetworkcode text, p_carriername text, p_carriererrorcode text, p_url text, p_registeredname text, p_registeredtype text, p_registerederrorcode text,
    p_phoneservice text, p_systemstatus int,
    p_createdate timestamp without time zone) 
        RETURNS void
        LANGUAGE 'plpgsql'

        COST 100
        VOLATILE
    AS $BODY$

    BEGIN




    	INSERT INTO whetstone.phonenumbers(id, phonenumber, phonetype, isverified, nationalformat, cangetsmsmessage,
    	   countrycode, carriercountrycode, carriernetworkcode, carriername, carriererrorcode, url, registeredname,
    	 registeredtype, registerederrorcode, phoneservice, systemstatus, createdate)
    		VALUES (p_id, p_phonenumber, p_phonetype, p_isverified, p_nationalformat, p_cangetsmsmessage,
    		p_countrycode, p_carriercountrycode, p_carriernetworkcode, p_carriername, p_carriererrorcode, p_url,
    			p_registeredname, p_registeredtype, p_registerederrorcode, p_phoneservice, p_systemstatus, p_createdate)
    	ON CONFLICT (id)
    		DO UPDATE SET

    		    phonenumber = EXCLUDED.phonenumber,
    			phonetype = EXCLUDED.phonetype,
    		   isverified = EXCLUDED.isverified,
    			nationalformat = EXCLUDED.nationalformat,
    			cangetsmsmessage = EXCLUDED.cangetsmsmessage,
    			countrycode = EXCLUDED.countrycode,
    			carriercountrycode = EXCLUDED.carriercountrycode,
    			carriernetworkcode = EXCLUDED.carriernetworkcode,
                  carriername = EXCLUDED.carriername,
    			carriererrorcode = EXCLUDED.carriererrorcode,
    			url = EXCLUDED.url,
    			registeredname = EXCLUDED.registeredname,
    			registeredtype = EXCLUDED.registeredtype,
    			registerederrorcode = EXCLUDED.registerederrorcode,
    			phoneservice = EXCLUDED.phoneservice,
    			systemstatus = EXCLUDED.systemstatus,
    			createdate = EXCLUDED.createdate;



    END;

    $BODY$;

    --alter function whetstone.addupdatephonenumber(uuid, text, integer, boolean, text, boolean, text, text, text, text, text, text, text, text, text, text, timestamp) owner to whetstoneadmin;

    GRANT EXECUTE ON FUNCTION whetstone.addupdatephonenumber(uuid, text, integer, boolean, text, boolean, text, text, text, text, text, text, text, text, text, text, int, timestamp without time zone) TO lambda_proxy;

    GRANT EXECUTE ON FUNCTION whetstone.addupdatephonenumber(uuid, text, integer, boolean, text, boolean, text, text, text, text, text, text, text, text, text, text, int, timestamp without time zone) TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN

    create or replace function whetstone.addupdatesmsconsent(p_id uuid, p_titleclientuserid uuid, p_phoneid uuid, p_titleversionid uuid, p_name text, p_isconsentgranted boolean, p_enginerequestid uuid,
        p_smsconsentdate timestamp without time zone)
        RETURNS void
        LANGUAGE 'plpgsql'

        COST 100
        VOLATILE
    AS $BODY$
    DECLARE
      foundtitleid uuid;
    BEGIN



    -- Get the title id from the deployment
    SELECT  tv.titleid
    INTO foundtitleid
    FROM  whetstone.titleversions tv 
    WHERE tv.id = p_titleversionid;


    	 BEGIN
    -- attempt to insert the user record in case it doesn't exist. This is just a place holder to
    -- satisfy the foreign key constraint.
    -- use the phone id for the phone number in this case since the phonenumber column is unique.
       INSERT INTO whetstone.title_clientusers(id, titleid, clientuserid, client, createdtime, lastaccesseddate)
    		VALUES (p_titleclientuserid, foundtitleid, p_titleclientuserid, 1, NOW(), NOW());
        EXCEPTION WHEN SQLSTATE '23505' THEN
    	  BEGIN
          -- Another process added the user and committed the transaction before this insert fired
          -- do nothing
        END;
       END;


    	 BEGIN
    -- attempt to insert the phone record in case it doesn't exist. This is just a place holder to
    -- satisfy the foreign key constraint.
    -- use the phone id for the phone number in this case since the phonenumber column is unique.
       INSERT INTO whetstone.phonenumbers(id, phonenumber, phonetype, isverified, cangetsmsmessage, systemstatus, createdate)
    		VALUES (p_phoneid, p_phoneid, 8, false,false,2, NOW());
        EXCEPTION WHEN SQLSTATE '23505' THEN
    	  BEGIN
          -- Another process added the phone number and committed the transaction before this insert fired
          -- do nothing
        END;
       END;



    	INSERT INTO whetstone.userphoneconsents(id, titleclientuserid, phoneid, titleversionid, name, isconsentgiven, smsconsentdate, enginerequestid)
    		VALUES (p_id, p_titleclientuserid, p_phoneid, p_titleversionid, p_name, p_isconsentgranted, p_smsconsentdate, p_enginerequestid)
    	ON CONFLICT (id)
    		DO UPDATE SET

    		    titleclientuserid = EXCLUDED.titleclientuserid,
    			phoneid = EXCLUDED.phoneid,
    		    titleversionid= EXCLUDED.titleversionid,
    			name = EXCLUDED.name,
    			isconsentgiven = EXCLUDED.isconsentgiven,
    			smsconsentdate = EXCLUDED.smsconsentdate,
    			enginerequestid = EXCLUDED.enginerequestid;



    END;

    $BODY$;

    --alter function whetstone.addupdatephonenumber(uuid, text, integer, boolean, text, boolean, text, text, text, text, text, text, text, text, text, text, timestamp) owner to whetstoneadmin;

    GRANT EXECUTE ON FUNCTION whetstone.addupdatesmsconsent(uuid, uuid,  uuid, uuid, text, boolean,uuid,  timestamp without time zone) to lambda_proxy;

    GRANT EXECUTE ON FUNCTION whetstone.addupdatesmsconsent(uuid, uuid,  uuid, uuid, text, boolean,uuid,  timestamp without time zone) to storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN

    create or replace function whetstone.addupdatetitleuser(p_id uuid, p_hashkey text, p_titleid uuid, p_clientuserid text, p_client int, p_userlocale text, p_storynodename text, p_nodename text, p_createdtime timestamp without time zone, p_lastaccesseddate timestamp without time zone, p_titlecrumbs jsonb, p_permanenttitlecrumbs jsonb)
        RETURNS void
        LANGUAGE 'plpgsql'

        COST 100
        VOLATILE
    AS $BODY$

    BEGIN


    	INSERT INTO whetstone.title_clientusers(id, hashkey, titleid, clientuserid, client, userlocale, storynodename, nodename, createdtime, lastaccesseddate, titlecrumbs, permanenttitlecrumbs)
    		VALUES (p_id, p_hashkey, p_titleid, p_clientuserid, p_client, p_userlocale, p_storynodename, p_nodename, p_createdtime, p_lastaccesseddate, p_titlecrumbs, p_permanenttitlecrumbs)
    	ON CONFLICT (id)
    		DO UPDATE SET

    		    hashkey = EXCLUDED.hashkey,
    			titleid = EXCLUDED.titleid,
    		    clientuserid= EXCLUDED.clientuserid,
    			client = EXCLUDED.client,
    			userlocale = EXCLUDED.userlocale,
    			storynodename = EXCLUDED.storynodename,
    			nodename = EXCLUDED.nodename,
    			createdtime = EXCLUDED.createdtime,
    			lastaccesseddate = EXCLUDED.lastaccesseddate,
    			titlecrumbs = EXCLUDED.titlecrumbs,
    			permanenttitlecrumbs = EXCLUDED.permanenttitlecrumbs;



    END;

    $BODY$;

    --alter function whetstone.addupdatephonenumber(uuid, text, integer, boolean, text, boolean, text, text, text, text, text, text, text, text, text, text, timestamp) owner to whetstoneadmin;

    GRANT EXECUTE ON FUNCTION whetstone.addupdatetitleuser(uuid,text, uuid, text, int,  text, text,  text,  timestamp without time zone, timestamp without time zone,jsonb, jsonb) to lambda_proxy;

    GRANT EXECUTE ON FUNCTION whetstone.addupdatetitleuser(uuid,text, uuid, text, int,  text, text,  text,  timestamp without time zone, timestamp without time zone,jsonb, jsonb) to storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN



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
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190811232825_InitialCreate') THEN
    INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
    VALUES ('20190811232825_InitialCreate', '3.1.5');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190822132939_MessageConsentReport') THEN
    ALTER TABLE whetstone.engine_session ADD isfirstsession boolean NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190822132939_MessageConsentReport') THEN
    CREATE INDEX "IX_outboundmessage_logs_logtime" ON whetstone.outboundmessage_logs (logtime);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190822132939_MessageConsentReport') THEN


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



    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190822132939_MessageConsentReport') THEN
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
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190822132939_MessageConsentReport') THEN
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
    	isfirstsession boolean,
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
      foundtitleid uuid;
      clienttype int;
    BEGIN


    -- Get the title id from the deployment
    SELECT  tv.titleid, tvd.client
    INTO foundtitleid, clienttype
    FROM whetstone.titleversiondeployments tvd
    INNER JOIN whetstone.titleversions tv ON tvd.versionid = tv.id
    WHERE tvd.id = deploymentkeyid;


      BEGIN

        INSERT INTO whetstone.title_clientusers
            (id, titleid, client, clientuserid, userlocale, createdtime, lastaccesseddate)
          VALUES (engineuserid, foundtitleid, clienttype,  addintentaction.userid,
                   addintentaction.locale,  addintentaction.selectiontime, addintentaction.selectiontime);
        EXCEPTION WHEN SQLSTATE '23505' THEN
          -- Another process added the session and committed the transaction before this insert fired
          -- do nothing
        END;


      SELECT ss.lastaccesseddate
      INTO  lastaccesstime
      FROM whetstone.engine_session ss
      where ss.id = sessionkey;

      IF lastaccesstime  IS NULL THEN
        BEGIN
          INSERT INTO whetstone.engine_session
            (id, titleuserid, sessionid, userid, userlocale, deploymentid, startdate, lastaccesseddate, sessionattributes, isfirstsession)
          VALUES (addintentaction.sessionkey, engineuserid, addintentaction.sessionid,
                  addintentaction.userid, addintentaction.locale, addintentaction.deploymentkeyid, addintentaction.selectiontime,
    			  addintentaction.selectiontime, addintentaction.sessionattributes, addintentaction.isfirstsession);
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



    GRANT EXECUTE ON FUNCTION whetstone.addintentaction(uuid, uuid, uuid, text, text, uuid, text, character varying, text, text, text, text, integer, timestamp without time zone, timestamp without time zone, hstore, integer, integer, jsonb, real, text, hstore, hstore, boolean, text, text, text, text) TO lambda_sessionaudit;



    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190822132939_MessageConsentReport') THEN
    INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
    VALUES ('20190822132939_MessageConsentReport', '3.1.5');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190917142811_GuestUserUpdate') THEN
    ALTER TABLE whetstone.title_clientusers ADD isguest boolean NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190917142811_GuestUserUpdate') THEN
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
    	isfirstsession boolean,
    	requestbodytext text,
    	responsebodytext text,
    	engineerrortext text,
    	responseconvtext text,
    	isguest boolean)
        RETURNS void
        LANGUAGE 'plpgsql'

        COST 100
        VOLATILE 
    AS $BODY$

    DECLARE
      lastaccesstime timestamp without time zone;
      foundtitleid uuid;
      clienttype int;
    BEGIN


    -- Get the title id from the deployment
    SELECT  tv.titleid, tvd.client
    INTO foundtitleid, clienttype
    FROM whetstone.titleversiondeployments tvd
    INNER JOIN whetstone.titleversions tv ON tvd.versionid = tv.id
    WHERE tvd.id = deploymentkeyid;


      BEGIN

        INSERT INTO whetstone.title_clientusers
            (id, titleid, client, clientuserid, userlocale, createdtime, lastaccesseddate, isguest)
          VALUES (engineuserid, foundtitleid, clienttype,  addintentaction.userid,
                   addintentaction.locale,  addintentaction.selectiontime, addintentaction.selectiontime, addintentaction.isguest);
        EXCEPTION WHEN SQLSTATE '23505' THEN
          -- Another process added the session and committed the transaction before this insert fired
          -- do nothing
        END;


      SELECT ss.lastaccesseddate
      INTO  lastaccesstime
      FROM whetstone.engine_session ss
      where ss.id = sessionkey;

      IF lastaccesstime  IS NULL THEN
        BEGIN
          INSERT INTO whetstone.engine_session
            (id, titleuserid, sessionid, userid, userlocale, deploymentid, startdate, lastaccesseddate, sessionattributes, isfirstsession)
          VALUES (addintentaction.sessionkey, engineuserid, addintentaction.sessionid,
                  addintentaction.userid, addintentaction.locale, addintentaction.deploymentkeyid, addintentaction.selectiontime,
    			  addintentaction.selectiontime, addintentaction.sessionattributes, addintentaction.isfirstsession);
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



    GRANT EXECUTE ON FUNCTION whetstone.addintentaction(uuid, uuid, uuid, text, text, uuid, text, character varying, text, text, text, text, integer, timestamp without time zone, timestamp without time zone, hstore, integer, integer, jsonb, real, text, hstore, hstore, boolean, text, text, text, text) TO lambda_sessionaudit;



    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190917142811_GuestUserUpdate') THEN

    create or replace function whetstone.addupdatetitleuser(p_id uuid, p_hashkey text, p_titleid uuid, p_clientuserid text, p_client int, p_userlocale text, p_storynodename text, p_nodename text, p_createdtime timestamp without time zone, p_lastaccesseddate timestamp without time zone, p_titlecrumbs jsonb, p_permanenttitlecrumbs jsonb, p_isguest boolean)
        RETURNS void
        LANGUAGE 'plpgsql'

        COST 100
        VOLATILE
    AS $BODY$

    BEGIN


    	INSERT INTO whetstone.title_clientusers(id, hashkey, titleid, clientuserid, client, userlocale, storynodename, nodename, createdtime, lastaccesseddate, titlecrumbs, permanenttitlecrumbs, isguest)
    		VALUES (p_id, p_hashkey, p_titleid, p_clientuserid, p_client, p_userlocale, p_storynodename, p_nodename, p_createdtime, p_lastaccesseddate, p_titlecrumbs, p_permanenttitlecrumbs, p_isguest)
    	ON CONFLICT (id)
    		DO UPDATE SET

    		    hashkey = EXCLUDED.hashkey,
    			titleid = EXCLUDED.titleid,
    		    clientuserid= EXCLUDED.clientuserid,
    			client = EXCLUDED.client,
    			userlocale = EXCLUDED.userlocale,
    			storynodename = EXCLUDED.storynodename,
    			nodename = EXCLUDED.nodename,
    			createdtime = EXCLUDED.createdtime,
    			lastaccesseddate = EXCLUDED.lastaccesseddate,
    			titlecrumbs = EXCLUDED.titlecrumbs,
    			permanenttitlecrumbs = EXCLUDED.permanenttitlecrumbs,
    			isguest = EXCLUDED.isguest;



    END;

    $BODY$;

    --alter function whetstone.addupdatephonenumber(uuid, text, integer, boolean, text, boolean, text, text, text, text, text, text, text, text, text, text, timestamp) owner to whetstoneadmin;

    GRANT EXECUTE ON FUNCTION whetstone.addupdatetitleuser(uuid,text, uuid, text, int,  text, text,  text,  timestamp without time zone, timestamp without time zone,jsonb, jsonb, boolean) to lambda_proxy;

    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20190917142811_GuestUserUpdate') THEN
    INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
    VALUES ('20190917142811_GuestUserUpdate', '3.1.5');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20191009182836_MsgBackKeyUpdate') THEN
    ALTER TABLE whetstone.outbound_messages DROP CONSTRAINT "FK_outbound_messages_outbound_batches_id";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20191009182836_MsgBackKeyUpdate') THEN
    CREATE INDEX "IX_outbound_messages_outboundbatchrecordid" ON whetstone.outbound_messages (outboundbatchrecordid);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20191009182836_MsgBackKeyUpdate') THEN
    ALTER TABLE whetstone.outbound_messages ADD CONSTRAINT "FK_outbound_messages_outbound_batches_outboundbatchrecordid" FOREIGN KEY (outboundbatchrecordid) REFERENCES whetstone.outbound_batches (id) ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20191009182836_MsgBackKeyUpdate') THEN
    INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
    VALUES ('20191009182836_MsgBackKeyUpdate', '3.1.5');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200304143902_ConsentReportUpdate') THEN


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
         LEFT JOIN whetstone.engine_requestaudit engreq ON ob.enginerequestid = engreq.id
         JOIN whetstone.userphoneconsents consent ON ob.consentid = consent.id
         JOIN whetstone.phonenumbers ph ON ob.smstonumberid = ph.id
         JOIN whetstone.title_clientusers cu ON consent.titleclientuserid = cu.id
      WHERE omlogs.status <> 1;

      -- Changing the whetstone.envine_requestaudit join to a LEFT join in order to accomodate
      -- a privacy update which skips the recording of user responses. 03/04/2020

      ALTER TABLE whetstone.messageconsents
        OWNER TO whetstoneadmin;

     GRANT SELECT ON TABLE whetstone.messageconsents TO lambda_proxy;
      GRANT ALL ON TABLE whetstone.messageconsents TO whetstoneadmin;



    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200304143902_ConsentReportUpdate') THEN
    INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
    VALUES ('20200304143902_ConsentReportUpdate', '3.1.5');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE TABLE whetstone.funcentitlements (
        id uuid NOT NULL,
        name text NOT NULL,
        description text NOT NULL,
        claim text NOT NULL,
        CONSTRAINT "PK_funcentitlements" PRIMARY KEY (id)
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE TABLE whetstone.roles (
        id uuid NOT NULL,
        name text NOT NULL,
        description text NOT NULL,
        CONSTRAINT "PK_roles" PRIMARY KEY (id)
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE TABLE whetstone.subscriptionlevels (
        id uuid NOT NULL,
        name text NOT NULL,
        description text NOT NULL,
        CONSTRAINT "PK_subscriptionlevels" PRIMARY KEY (id)
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE TABLE whetstone.users (
        id uuid NOT NULL,
        cognito_sub text NULL,
        CONSTRAINT "PK_users" PRIMARY KEY (id)
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE TABLE whetstone.funcent_role_xrefs (
        func_entitlement_id uuid NOT NULL,
        role_id uuid NOT NULL,
        CONSTRAINT pk_role_funcentitlement PRIMARY KEY (role_id, func_entitlement_id),
        CONSTRAINT "FK_funcent_role_xrefs_funcentitlements_func_entitlement_id" FOREIGN KEY (func_entitlement_id) REFERENCES whetstone.funcentitlements (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_funcent_role_xrefs_roles_role_id" FOREIGN KEY (role_id) REFERENCES whetstone.roles (id) ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE TABLE whetstone.organizations (
        id uuid NOT NULL,
        name text NOT NULL,
        description text NOT NULL,
        subscriptionlevel_id uuid NOT NULL,
        isenabled boolean NOT NULL,
        CONSTRAINT "PK_organizations" PRIMARY KEY (id),
        CONSTRAINT "FK_organizations_subscriptionlevels_subscriptionlevel_id" FOREIGN KEY (subscriptionlevel_id) REFERENCES whetstone.subscriptionlevels (id) ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE TABLE whetstone.groups (
        id uuid NOT NULL,
        name text NOT NULL,
        description text NOT NULL,
        organization_id uuid NOT NULL,
        CONSTRAINT "PK_groups" PRIMARY KEY (id),
        CONSTRAINT "FK_groups_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES whetstone.organizations (id) ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE TABLE whetstone.group_role_xrefs (
        group_id uuid NOT NULL,
        role_id uuid NOT NULL,
        CONSTRAINT pk_group_role PRIMARY KEY (group_id, role_id),
        CONSTRAINT "FK_group_role_xrefs_groups_group_id" FOREIGN KEY (group_id) REFERENCES whetstone.groups (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_group_role_xrefs_roles_role_id" FOREIGN KEY (role_id) REFERENCES whetstone.roles (id) ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE TABLE whetstone.title_group_xrefs (
        title_id uuid NOT NULL,
        group_id uuid NOT NULL,
        CONSTRAINT pk_group_title PRIMARY KEY (group_id, title_id),
        CONSTRAINT "FK_title_group_xrefs_groups_group_id" FOREIGN KEY (group_id) REFERENCES whetstone.groups (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_title_group_xrefs_titles_title_id" FOREIGN KEY (title_id) REFERENCES whetstone.titles (id) ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE TABLE whetstone.user_group_xrefs (
        user_id uuid NOT NULL,
        group_id uuid NOT NULL,
        CONSTRAINT pk_user_group PRIMARY KEY (group_id, user_id),
        CONSTRAINT "FK_user_group_xrefs_groups_group_id" FOREIGN KEY (group_id) REFERENCES whetstone.groups (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_user_group_xrefs_users_user_id" FOREIGN KEY (user_id) REFERENCES whetstone.users (id) ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE INDEX "IX_funcent_role_xrefs_func_entitlement_id" ON whetstone.funcent_role_xrefs (func_entitlement_id);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE INDEX "IX_group_role_xrefs_role_id" ON whetstone.group_role_xrefs (role_id);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE INDEX "IX_groups_organization_id" ON whetstone.groups (organization_id);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE INDEX "IX_organizations_subscriptionlevel_id" ON whetstone.organizations (subscriptionlevel_id);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE INDEX "IX_title_group_xrefs_title_id" ON whetstone.title_group_xrefs (title_id);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    CREATE INDEX "IX_user_group_xrefs_user_id" ON whetstone.user_group_xrefs (user_id);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN




    INSERT INTO whetstone.subscriptionlevels
       (id, name, description)
    VALUES
    	('8bc0ab56-64d4-4925-8c7a-cedb070436ae', 'Free Tier', 'This is the default subscription level for a new user'),
    	('bdbebc49-26fd-49ef-a5d9-2a86b6b77357', 'Designer', 'Paying single-user subscription'),
    	('4ecc9046-fa90-474b-8ce4-58b2db559dcb', 'Team', 'Small team subscription.'),
    	('b0c5254a-3f91-415b-bdf1-8fdf25545603', 'Enterprise', 'Large team subscription.');


    INSERT INTO whetstone.roles
       (id, name, description)
    VALUES
    	('023e1461-81ea-4272-b2ea-22c59cbe26ef', 'Project Administrator', 'Has all project, version, deployment, and reporting rights.'),
    	('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'Organization Administrator', 'Can manage groups, group membership, and invite users to the organization'),
    	('6d369d82-92ed-47e6-8550-c49fd01b20f3', 'Developer', 'Can edit project versions'),
    	('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', 'Release Manager', 'Can deploy project versions to channels'),
    	('8b60dab8-c479-4e65-8ac8-1d816266a4d0', 'Reader', 'Has read rights to projects, versions, and deployments. Cannot view reports.'),
    	('4fa00695-384a-4edc-b010-06c5fad42a2a', 'Report Viewer', 'Has the same rights as the reader in addition to being able to view and export reports'),
    	('5097a742-e600-4372-aecc-17f365587153', 'Marketing Manager', 'Can view and export reports and can edit channel deployment metadata');


    INSERT INTO whetstone.funcentitlements
      (id, name, description, claim)
    VALUES
      ('3cc90f71-504c-43c8-b227-b815ddb43fdd', 'create project', 'Grants rights to create a new project', 'project-create'),
      ('90593f7e-1597-4243-88ee-0e7c1cb2336d', 'view project', 'Grants rights to view and list projects', 'project-view'),
      ('8fcfc2ea-a94b-48b0-94d3-891d963597a0', 'update project', 'Grants rights to update an assigned project', 'project-update'),
      ('2cd80c4e-5d46-4dd1-a372-20b5778863ed', 'delete project', 'Grants rights to delete a project', 'project-delete'),
      ('e4d3cc5a-0e6a-4378-8a39-641816e06fa6', 'view report', 'Grants rights to view usage reports', 'report-view'),
      ('8de66d7c-dd74-42bc-bb71-3bd85f5ee6fe', 'export report', 'Grants rights to export reports', 'report-export'),
      ('ec916756-6af1-4034-9217-f051bef9300a', 'create version', 'Grants rights to create a version of an assigned project', 'version-create'),
      ('7f1e443d-c539-4d73-b9fb-ec78c89ab159', 'update version', 'Grants rights to update a version of an assigned project', 'version-update'),
      ('02abed44-9b82-48d0-ae06-1fe88e4b3d4e', 'view version', 'Grants rights to view a version of an assigned project', 'version-view'),
      ('99cff5bc-fdf0-4f3a-bb57-3e5cc80f3bbe', 'delete version', 'Grants rights to delete a version of an assigned project', 'version-delete'),
      ('b5f9b739-3175-4d8e-9c65-fcf2672d86df', 'add audio file', 'Grants rights to add an audio file to a version', 'audiofile-add'),
      ('9bb85188-6089-4acd-abf7-8a5c23f44e70', 'update audio file', 'Grants rights to update or replace an audio file in a version', 'audiofile-update'),
      ('8bf5bdb7-ce27-4d1f-a676-b308d10ad408', 'delete audio file', 'Grants rights to delete an audio file in a version', 'audiofile-delete'),
      ('1fcc5a27-d466-41e1-83c2-f0d529c213f3', 'play audio file', 'Grants rights to play an audio file in a version', 'audiofile-play'),
      ('152be0fc-37ed-455c-9e55-a5b37c5e9779', 'add image file', 'Grants rights to add an image to a version', 'imagefile-add'),
      ('728e64e2-bb8f-4e7e-8477-f5ab2efdfac6', 'view image file', 'Grants rights to view an imagse in a version', 'imagefile-view'),
      ('e87d941b-75d8-464d-9dbb-789bfa88254c', 'delete image file', 'Grants rights to delete an imagse in a version', 'imagefile-delete'),
      ('5ed13dc8-548a-4ac0-8974-fc0bc1d198c8', 'view deployment', 'Grants rights to view a version deployment', 'deployment-view'),
      ('728475d2-16d8-428d-ab03-3d0debd20e3c', 'remove deployment', 'Grants rights to remove a version deployment', 'deployment-remove'),
      ('539c22c2-f774-4575-80f7-2402504fb5e3', 'edit channel deployment metadata', 'Grants rights to edit metadata for Alexa, Google, etc. for channel deployments', 'channeldeploymentmetadata-edit'),  
      ('28d8b099-bd87-4aef-866e-090673b00e4a', 'view channel deployment metadata', 'Grants rights to view metadata for Alexa, Google, etc. for channel deployments', 'channeldeploymentmetadata-view'),
      ('46bf5cdd-6a76-4765-b782-4664bbb6a955', 'deploy version', 'Grants rights to deploy a version to a channel', 'deployment-create'),
      ('3c028526-bf53-42e1-a903-c2f0f71626d7', 'create group', 'Grants rights create a group for an organization', 'group-create'),
      ('17e0617d-118a-4235-b13f-15a86f06219d', 'edit group', 'Grants rights edit a group for an organization', 'group-edit'),
      ('ffb35a14-a447-4d55-a6c2-1b087976bce3', 'delete group', 'Grants rights delete a group for an organization', 'group-delete'),
      ('d800b6f0-581a-476e-8a58-043ed05893b9', 'view group', 'Grants rights view a group for an organization', 'group-view'),
      ('ba76c9b8-9aa0-45c3-8c79-d3e3ea60015c', 'add user to group', 'Grants rights add users to a group', 'group-user-add'),
      ('cbe77284-27cb-4018-9bb5-2e61ea8103b6', 'remove user from group', 'Grants rights remove users from a group', 'group-user-remove'),
       ('37a1373d-bb01-44bc-8fc8-1769f7c1a42e', 'invite user to organization', 'Grants rights invite a user to an organization', 'organization-user-invite');



    INSERT INTO whetstone.funcent_role_xrefs
      (role_id, func_entitlement_id)
    VALUES
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '3cc90f71-504c-43c8-b227-b815ddb43fdd'), -- Grant project-create to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '90593f7e-1597-4243-88ee-0e7c1cb2336d'), -- Grant project-view to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '8fcfc2ea-a94b-48b0-94d3-891d963597a0'), -- Grant project-update to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '2cd80c4e-5d46-4dd1-a372-20b5778863ed'), -- Grant project-delete to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', 'e4d3cc5a-0e6a-4378-8a39-641816e06fa6'), -- Grant report-view to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '8de66d7c-dd74-42bc-bb71-3bd85f5ee6fe'), -- Grant report-export to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', 'ec916756-6af1-4034-9217-f051bef9300a'), -- Grant version-create to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '7f1e443d-c539-4d73-b9fb-ec78c89ab159'), -- Grant version-update to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '02abed44-9b82-48d0-ae06-1fe88e4b3d4e'), -- Grant version-view to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '99cff5bc-fdf0-4f3a-bb57-3e5cc80f3bbe'), -- Grant version-delete to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', 'b5f9b739-3175-4d8e-9c65-fcf2672d86df'), -- Grant audiofile-add to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '9bb85188-6089-4acd-abf7-8a5c23f44e70'), -- Grant audiofile-update to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '8bf5bdb7-ce27-4d1f-a676-b308d10ad408'), -- Grant audiofile-delete to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '1fcc5a27-d466-41e1-83c2-f0d529c213f3'), -- Grant audiofile-play to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '152be0fc-37ed-455c-9e55-a5b37c5e9779'), -- Grant imagefile-add to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '728e64e2-bb8f-4e7e-8477-f5ab2efdfac6'), -- Grant imagefile-view to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', 'e87d941b-75d8-464d-9dbb-789bfa88254c'), -- Grant imagefile-delete to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '5ed13dc8-548a-4ac0-8974-fc0bc1d198c8'), -- Grant deployment-view to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '728475d2-16d8-428d-ab03-3d0debd20e3c'), -- Grant deployment-remove to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '539c22c2-f774-4575-80f7-2402504fb5e3'), -- Grant channeldeploymentmetadata-edit to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '28d8b099-bd87-4aef-866e-090673b00e4a'), -- Grant channeldeploymentmetadata-view to Project Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '46bf5cdd-6a76-4765-b782-4664bbb6a955'), -- Grant deployment-create to Project Administrator

    ('74236635-ab8f-4f7b-bf76-b1c1089c2030', '3c028526-bf53-42e1-a903-c2f0f71626d7'), -- Grants group-create to Organization Administrator
    ('74236635-ab8f-4f7b-bf76-b1c1089c2030', '17e0617d-118a-4235-b13f-15a86f06219d'), -- Grants group-edit to Organization Administrator
    ('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'ffb35a14-a447-4d55-a6c2-1b087976bce3'), -- Grants group-delete to Organization Administrator
    ('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'd800b6f0-581a-476e-8a58-043ed05893b9'), -- Grants group-view to Organization Administrator
    ('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'ba76c9b8-9aa0-45c3-8c79-d3e3ea60015c'), -- Grants group-user-add to Organization Administrator
    ('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'cbe77284-27cb-4018-9bb5-2e61ea8103b6'), -- Grants group-user-remove to Organization Administrator
    ('74236635-ab8f-4f7b-bf76-b1c1089c2030', '37a1373d-bb01-44bc-8fc8-1769f7c1a42e'), -- Grants organization-user-invite to Organization Administrator

    ('6d369d82-92ed-47e6-8550-c49fd01b20f3', '7f1e443d-c539-4d73-b9fb-ec78c89ab159'), -- Grants version-update to Developer
    ('6d369d82-92ed-47e6-8550-c49fd01b20f3', '02abed44-9b82-48d0-ae06-1fe88e4b3d4e'), -- Grants version-view to Developer
    ('6d369d82-92ed-47e6-8550-c49fd01b20f3', '90593f7e-1597-4243-88ee-0e7c1cb2336d'), -- Grants project-view to Developer
    ('6d369d82-92ed-47e6-8550-c49fd01b20f3', 'b5f9b739-3175-4d8e-9c65-fcf2672d86df'), -- Grant audiofile-add to Developer
    ('6d369d82-92ed-47e6-8550-c49fd01b20f3', '9bb85188-6089-4acd-abf7-8a5c23f44e70'), -- Grant audiofile-update to Developer
    ('6d369d82-92ed-47e6-8550-c49fd01b20f3', '8bf5bdb7-ce27-4d1f-a676-b308d10ad408'), -- Grant audiofile-delete to Developer
    ('6d369d82-92ed-47e6-8550-c49fd01b20f3', '1fcc5a27-d466-41e1-83c2-f0d529c213f3'), -- Grant audiofile-play to Developer
    ('6d369d82-92ed-47e6-8550-c49fd01b20f3', '152be0fc-37ed-455c-9e55-a5b37c5e9779'), -- Grant imagefile-add to Developer
    ('6d369d82-92ed-47e6-8550-c49fd01b20f3', '728e64e2-bb8f-4e7e-8477-f5ab2efdfac6'), -- Grant imagefile-view to Developer
    ('6d369d82-92ed-47e6-8550-c49fd01b20f3', 'e87d941b-75d8-464d-9dbb-789bfa88254c'), -- Grant imagefile-delete to Developer


    ('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', '90593f7e-1597-4243-88ee-0e7c1cb2336d'), -- Grants project-view to Release Manager
    ('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', '02abed44-9b82-48d0-ae06-1fe88e4b3d4e'), -- Grants version-view to Release Manager
    ('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', '5ed13dc8-548a-4ac0-8974-fc0bc1d198c8'), -- Grants deployment-view to Release Manager
    ('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', '46bf5cdd-6a76-4765-b782-4664bbb6a955'), -- Grant deployment-create to Release Manager
    ('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', '728475d2-16d8-428d-ab03-3d0debd20e3c'), -- Grant deployment-remove to Release Manager
    ('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', '28d8b099-bd87-4aef-866e-090673b00e4a'), -- Grant channeldeploymentmetadata-view to Release Manager

    ('8b60dab8-c479-4e65-8ac8-1d816266a4d0', '02abed44-9b82-48d0-ae06-1fe88e4b3d4e'), -- Grants version-view to Reader
    ('8b60dab8-c479-4e65-8ac8-1d816266a4d0', '90593f7e-1597-4243-88ee-0e7c1cb2336d'), -- Grants project-view to Reader
    ('8b60dab8-c479-4e65-8ac8-1d816266a4d0', '5ed13dc8-548a-4ac0-8974-fc0bc1d198c8'), -- Grant deployment-view to Reader
    ('8b60dab8-c479-4e65-8ac8-1d816266a4d0', '1fcc5a27-d466-41e1-83c2-f0d529c213f3'), -- Grant audiofile-play to Reader
    ('8b60dab8-c479-4e65-8ac8-1d816266a4d0', '728e64e2-bb8f-4e7e-8477-f5ab2efdfac6'), -- Grant imagefile-view to Reader

    ('4fa00695-384a-4edc-b010-06c5fad42a2a', '90593f7e-1597-4243-88ee-0e7c1cb2336d'), -- Grants project-view to Report Viewer
    ('4fa00695-384a-4edc-b010-06c5fad42a2a', '02abed44-9b82-48d0-ae06-1fe88e4b3d4e'), -- Grants version-view to Report Viewer
    ('4fa00695-384a-4edc-b010-06c5fad42a2a', '5ed13dc8-548a-4ac0-8974-fc0bc1d198c8'), -- Grants deployment-view to Report Viewer
    ('4fa00695-384a-4edc-b010-06c5fad42a2a', 'e4d3cc5a-0e6a-4378-8a39-641816e06fa6'), -- Grant report-view to Report Viewer
    ('4fa00695-384a-4edc-b010-06c5fad42a2a', '8de66d7c-dd74-42bc-bb71-3bd85f5ee6fe'), -- Grant report-export to Report Viewer

    ('5097a742-e600-4372-aecc-17f365587153', '90593f7e-1597-4243-88ee-0e7c1cb2336d'), -- Grants project-view to Marketing Manager
    ('5097a742-e600-4372-aecc-17f365587153', '02abed44-9b82-48d0-ae06-1fe88e4b3d4e'), -- Grants version-view to Marketing Manager
    ('5097a742-e600-4372-aecc-17f365587153', '5ed13dc8-548a-4ac0-8974-fc0bc1d198c8'), -- Grants deployment-view to Marketing Manager
    ('5097a742-e600-4372-aecc-17f365587153', 'e4d3cc5a-0e6a-4378-8a39-641816e06fa6'), -- Grant report-view to Marketing Manager
    ('5097a742-e600-4372-aecc-17f365587153', '8de66d7c-dd74-42bc-bb71-3bd85f5ee6fe'), -- Grant report-export to Marketing Manager
    ('5097a742-e600-4372-aecc-17f365587153', '539c22c2-f774-4575-80f7-2402504fb5e3'), -- Grant channeldeploymentmetadata-edit to Marketing Manager
    ('5097a742-e600-4372-aecc-17f365587153', '28d8b099-bd87-4aef-866e-090673b00e4a'); -- Grant channeldeploymentmetadata-view to Marketing Manager



    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    GRANT UPDATE,INSERT,SELECT ON whetstone.organizations TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    GRANT UPDATE,INSERT,SELECT ON whetstone.subscriptionlevels TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    GRANT UPDATE,INSERT,SELECT ON whetstone.roles TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    GRANT UPDATE,INSERT,SELECT ON whetstone.groups TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    GRANT UPDATE,INSERT,SELECT ON whetstone.users TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    GRANT UPDATE,INSERT,DELETE,SELECT ON whetstone.group_role_xrefs TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    GRANT UPDATE,INSERT,DELETE,SELECT ON whetstone.title_group_xrefs TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    GRANT UPDATE,INSERT,DELETE,SELECT ON whetstone.user_group_xrefs TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    GRANT UPDATE,INSERT,DELETE,SELECT ON whetstone.funcent_role_xrefs TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    GRANT UPDATE,INSERT,DELETE,SELECT ON whetstone.funcentitlements TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20200621171848_Security01Update') THEN
    INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
    VALUES ('20200621171848_Security01Update', '3.1.5');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20201201020725_Twitter01Update') THEN
    CREATE TABLE whetstone.org_twitterapplications (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        name text NOT NULL,
        description text NULL,
        twitter_app_id bigint NOT NULL,
        isenabled boolean NOT NULL,
        isdeleted boolean NOT NULL,
        title_version_id uuid NULL,
        CONSTRAINT "PK_org_twitterapplications" PRIMARY KEY (id),
        CONSTRAINT "FK_org_twitterapplications_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES whetstone.organizations (id) ON DELETE CASCADE,
        CONSTRAINT "FK_org_twitterapplications_titleversions_title_version_id" FOREIGN KEY (title_version_id) REFERENCES whetstone.titleversions (id) ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20201201020725_Twitter01Update') THEN
    CREATE TABLE whetstone.org_twittercredentials (
        id uuid NOT NULL,
        twitterapplication_id uuid NOT NULL,
        name text NOT NULL,
        description text NOT NULL,
        consumer_key text NOT NULL,
        consumer_secret text NOT NULL,
        access_token text NOT NULL,
        access_token_secret text NOT NULL,
        bearer_token text NOT NULL,
        isenabled boolean NOT NULL,
        isdeleted boolean NOT NULL,
        CONSTRAINT "PK_org_twittercredentials" PRIMARY KEY (id),
        CONSTRAINT "FK_org_twittercredentials_org_twitterapplications_twitterappli~" FOREIGN KEY (twitterapplication_id) REFERENCES whetstone.org_twitterapplications (id) ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20201201020725_Twitter01Update') THEN
    CREATE TABLE whetstone.twittersubscriptions (
        id uuid NOT NULL,
        application_id uuid NOT NULL,
        twitter_user_id bigint NOT NULL,
        enable_autofollowback boolean NOT NULL,
        CONSTRAINT "PK_twittersubscriptions" PRIMARY KEY (id),
        CONSTRAINT "FK_twittersubscriptions_org_twitterapplications_application_id" FOREIGN KEY (application_id) REFERENCES whetstone.org_twitterapplications (id) ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20201201020725_Twitter01Update') THEN
    CREATE INDEX "IX_org_twitterapplications_organization_id" ON whetstone.org_twitterapplications (organization_id);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20201201020725_Twitter01Update') THEN
    CREATE INDEX "IX_org_twitterapplications_title_version_id" ON whetstone.org_twitterapplications (title_version_id);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20201201020725_Twitter01Update') THEN
    CREATE UNIQUE INDEX "IX_org_twittercredentials_twitterapplication_id" ON whetstone.org_twittercredentials (twitterapplication_id);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20201201020725_Twitter01Update') THEN
    CREATE INDEX "IX_twittersubscriptions_application_id" ON whetstone.twittersubscriptions (application_id);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20201201020725_Twitter01Update') THEN



    INSERT INTO whetstone.funcentitlements
      (id, name, description, claim)
    VALUES
      ('fc758d8f-4076-427d-bdc6-d380885b621d', 'manage twitter credentials', 'Grants rights to create, enable, and disable Twitter credentials', 'twittercredentials-manage'),
      ('899e4111-c51e-4462-88c6-c81c401659d8', 'read twitter credentials', 'Grants rights to read Twitter credentials', 'twittercredentials-read'),
      ('042abef2-826b-45ab-afc9-5138fe0dc060', 'delete twitter credentials', 'Grants rights delete Twitter credentials', 'twittercredetials-delete'),
      ('da67f2ce-80bf-4430-95d8-78fb17081529', 'manage twitter applications', 'Grants rights to create, and edit Twitter applications', 'twitterapplicatios-manage'),
      ('7e687c39-cd65-4189-a12c-3b1b0b64cda4', 'read twitter applications', 'Grants rights to read Twitter applications', 'twitterapplicatios-read'),
      ('c867f368-2e84-4393-800f-aa0fb8bd9f89', 'delete twitter applications', 'Grants rights delete Twitter applications', 'twitterapplicatios-delete');

     

    INSERT INTO whetstone.funcent_role_xrefs
      (role_id, func_entitlement_id)
    VALUES
    ('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'fc758d8f-4076-427d-bdc6-d380885b621d'), -- Grants twittercredentials-manage to Organization Administrator
    ('74236635-ab8f-4f7b-bf76-b1c1089c2030', '899e4111-c51e-4462-88c6-c81c401659d8'), -- Grants twittercredentials-read to Organization Administrator
    ('74236635-ab8f-4f7b-bf76-b1c1089c2030', '042abef2-826b-45ab-afc9-5138fe0dc060'), -- Grants twittercredetials-delete to Organization Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '899e4111-c51e-4462-88c6-c81c401659d8'), -- Grant twittercredentials-read to Project Administrator
    ('6d369d82-92ed-47e6-8550-c49fd01b20f3', '899e4111-c51e-4462-88c6-c81c401659d8'), -- Grant twittercredentials-read to Developer
    ('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'da67f2ce-80bf-4430-95d8-78fb17081529'), -- Grants twitterapplicatios-manage to Organization Administrator
    ('74236635-ab8f-4f7b-bf76-b1c1089c2030', '7e687c39-cd65-4189-a12c-3b1b0b64cda4'), -- Grants twitterapplicatios-read to Organization Administrator
    ('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'c867f368-2e84-4393-800f-aa0fb8bd9f89'), -- Grants twitterapplicatios-delete to Organization Administrator
    ('023e1461-81ea-4272-b2ea-22c59cbe26ef', '7e687c39-cd65-4189-a12c-3b1b0b64cda4'), -- Grant twitterapplicatios-read to Project Administrator
    ('6d369d82-92ed-47e6-8550-c49fd01b20f3', '7e687c39-cd65-4189-a12c-3b1b0b64cda4'); -- Grant twitterapplicatios-read to Developer
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20201201020725_Twitter01Update') THEN
    GRANT UPDATE,INSERT,SELECT ON whetstone.org_twittercredentials TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20201201020725_Twitter01Update') THEN
    GRANT UPDATE,INSERT,SELECT ON whetstone.twittersubscriptions TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20201201020725_Twitter01Update') THEN
    GRANT UPDATE,INSERT,SELECT ON whetstone.org_twitterapplications TO storyengineuser;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM whetstone.efmigrationhistory WHERE "MigrationId" = '20201201020725_Twitter01Update') THEN
    INSERT INTO whetstone.efmigrationhistory ("MigrationId", "ProductVersion")
    VALUES ('20201201020725_Twitter01Update', '3.1.5');
    END IF;
END $$;
