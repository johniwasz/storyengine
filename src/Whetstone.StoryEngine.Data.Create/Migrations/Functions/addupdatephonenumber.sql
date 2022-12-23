


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