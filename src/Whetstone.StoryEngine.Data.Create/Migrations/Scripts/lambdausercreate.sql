
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