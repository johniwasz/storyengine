CREATE OR REPLACE FUNCTION public.GetVersion(title text, version text ) RETURNS SETOF refcursor AS $$
    DECLARE
      verTable refcursor;           -- Declare cursor variables
      stotTypes refcursor;
    BEGIN
      OPEN verTable  FOR  SELECT  c.Id,
            c.PublishDate,
             c.StoryId,
              c.Version
    FROM    public.StoryVersions c
    WHERE   c.StoryId = title AND c.Version = version;
    RETURN NEXT verTable;

    OPEN stotTypes FOR   SELECT  st.Id ,
            sv.Name
    FROM    public."SlotTypes" st
            INNER JOIN public."StoryVersions" sv ON st.DataStoryVersionId = sv.Id
    WHERE   c.State = param_state;
    RETURN NEXT stotTypes;

    END;
    $$ LANGUAGE plpgsql;