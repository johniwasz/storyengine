
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
    VALUES ('20190917142811_GuestUserUpdate', '2.2.6-servicing-10079');
    END IF;
END $$;
