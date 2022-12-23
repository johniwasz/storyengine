
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