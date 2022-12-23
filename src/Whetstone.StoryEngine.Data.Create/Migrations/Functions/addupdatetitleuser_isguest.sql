
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
