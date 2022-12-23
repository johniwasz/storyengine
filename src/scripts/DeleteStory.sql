


create or replace function purgestory(delstoryid bigint)
   returns void
language plpgsql
as $$
BEGIN




  DELETE FROM public."Story" st WHERE st."Id" = delstoryid;


END
$$;

