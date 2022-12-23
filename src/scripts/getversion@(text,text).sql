


create or replace function "public"."getversion"(shortname text, version text)
  returns SETOF refcursor
language plpgsql
as $$
DECLARE
      storyId bigint;
      retVersionId bigint;
      vertable refcursor;           -- Declare cursor variables
      slotTypes refcursor;
      intentValues refcursor;
      slotMappings refcursor;
      chapters refcursor;
      storyNodes refcursor;
      standardNodes refcursor;
      nodeVisitConditions refcursor;
      nodeVisitConditionXrefs refcursor;
      inventoryConditions refcursor;
      inventoryItems refcursor;
      inventoryItemXrefs refcursor;
      locResponseSets refcursor;
      locResponse refcursor;
      clientFrags refcursor;
      speechFrags refcursor;
      fragInventoryConditions refcursor;
      fragNodeVisitConditions refcursor;
 BEGIN

    storyId := (SELECT st."Id" from "public"."Story" st where st."ShortName" = shortname);

    retVersionId := (SELECT sv."Id" from "public"."StoryVersions" sv where sv."Version" = version AND NOT sv."IsDeleted"  AND sv."StoryId" = storyId);

      OPEN vertable  FOR  SELECT sv."Id",
            sv."PublishDate",
            sv."StoryId",
              sv."Version",
        sv."IsDeleted",
        sv."UniqueId"
    FROM    "public"."StoryVersions" AS sv
    WHERE   sv."Id" = retVersionId;
    RETURN NEXT vertable;

    OPEN slotTypes FOR   SELECT  st."Id",
            st."Name",
      st."UniqueId", st."VersionId", st."ValuesJson"
    FROM    "public"."SlotTypes" AS st
    WHERE  st."VersionId" = retVersionId;
    RETURN NEXT slotTypes;


    OPEN intentValues  FOR SELECT ints."Id", ints."Name", ints."UniqueId", ints."VersionId", ints."LocalizedIntents"
    FROM public."Intents" as ints
    WHERE ints."VersionId" = retVersionId;
    RETURN NEXT intentValues;



   OPEN slotMappings FOR SELECT  sm."Alias", sm."IntentId", sm."SlotTypeId"
FROM public."IntentSlotMappings" as sm
INNER JOIN public."Intents" as ints  ON ints."Id" = sm."IntentId"
 WHERE ints."VersionId" = retVersionId;
  RETURN NEXT slotMappings;


  OPEN chapters FOR SELECT chap."Id", chap."VersionId", chap."Sequence"
  FROM public."Chapters" chap
  WHERE chap."VersionId" = retVersionId;
    RETURN NEXT chapters;


  OPEN storyNodes FOR SELECT sn."Id", sn."ChapterId", sn."Name", sn."ResponseBehavior", sn."Coordinates", sn."VersionId"
FROM public."StoryNodes" sn
 
WHERE sn."VersionId" = retVersionId;

  RETURN NEXT storyNodes;


  OPEN standardNodes FOR SELECT stn."Id", stn."VersionId", stn."NodeId", stn."NodeType"
FROM public."StandardNodes" stn
  WHERE stn."VersionId" = retVersionId;

  RETURN NEXT standardNodes;


OPEN  nodeVisitConditions FOR SELECT cnv."Id", cnv."Name", cnv."RequiredOutcome"
FROM public."ConditionsNodeVisit" cnv
WHERE cnv."VersionId" = retVersionId;

  RETURN NEXT  nodeVisitConditions;

OPEN  nodeVisitConditionXrefs FOR
SELECT nvx."ConditionId", nvx."NodeId" FROM public."NodeVisitConditionXRefs" nvx
INNER JOIN public."ConditionsNodeVisit" nv ON nvx."ConditionId" = nv."Id"
WHERE nv."VersionId" =retVersionId;

  RETURN NEXT nodeVisitConditions;

OPEN  inventoryConditions FOR
SELECT cv."Id", cv."Name", cv."RequiredOutcome" FROM
  public."ConditionsInventory" cv
WHERE cv."VersionId" = retVersionId;

  RETURN NEXT inventoryConditions;

OPEN  inventoryItems FOR
SELECT i."Id", i."Name", i."IsMultiItem" FROM
  public."InventoryItems" i
WHERE i."VersionId" = retVersionId;

  RETURN NEXT inventoryItems;

OPEN  inventoryItemXrefs  FOR
SELECT icx."ConditionId", icx."ItemId" FROM public."InventoryConditionItemXRefs" icx
INNER JOIN public."ConditionsNodeVisit" cnv ON icx."ConditionId" = cnv."Id"
WHERE cnv."VersionId" = retVersionId;

RETURN NEXT inventoryItemXrefs;

-- Load all the nodes that need to be exported into a single array
-- This will be used to get all subsequent node-related data.

OPEN locResponseSets FOR
SELECT locSet."Id", locSet."StoryNodeId"
FROM public."LocResponseSet" locSet INNER JOIN public."StoryNodes" sn ON locset."StoryNodeId" =sn."Id"
WHERE sn."VersionId" = retVersionId;

  RETURN NEXT locResponseSets;

  OPEN  locResponse  FOR SELECT locResp."Id", locResp."CardTitle", locResp."DataLocalizedResponseSetId", locResp."LargeImageFile",
     locResp."Locale", locResp."RepromptTextResponse", locResp."SendCardResponse", locResp."SmallImageFile"
FROM public."LocResponse" locResp
  INNER JOIN public."LocResponseSet" locSet ON locResp."DataLocalizedResponseSetId" = locSet."Id"
   INNER JOIN public."StoryNodes" sn ON locSet."StoryNodeId" = sn."Id"
  WHERE sn."VersionId" = retVersionId;

  RETURN NEXT locResponse;

  OPEN clientFrags FOR SELECT csf."Id", csf."SpeechClient", csf."LocResponseId", csf."LocRepromptResponseId"
FROM public."ClientSpeechFrags" csf
LEFT JOIN public."LocResponse" lr ON csf."LocRepromptResponseId" = lr."Id" OR csf."LocResponseId" = lr."Id"
INNER JOIN public."LocResponseSet" lrs ON lr."DataLocalizedResponseSetId" = lrs."Id"
  INNER JOIN public."StoryNodes" sn ON lrs."StoryNodeId" = sn."Id"
  WHERE sn."VersionId" = retVersionId;

   RETURN NEXT clientFrags;



OPEN speechFrags FOR
SELECT sf."Id", sf."ClientSpeechFragId", sf."TrueResultParentId", sf."FalseResultParentId", sf."Text", sf."FileName",
  sf."AudioUrl", sf."Discriminator", sf."Comment", sf."Sequence", sf."VersionId", sf."Voice"
   FROM public."SpeechFragments" sf
  WHERE sf."VersionId" = retVersionId;


  RETURN NEXT speechFrags;


OPEN fragInventoryConditions FOR
  SELECT fnv."ConditionFragmentId", fnv."ConditionId" FROM public."FragmentInventoryConditionXRefs" fnv
    INNER JOIN public."ConditionsInventory" ci on fnv."ConditionId" = ci."Id"
  where ci."VersionId" = retVersionId;

  RETURN NEXT fragInventoryConditions;

OPEN   fragNodeVisitConditions FOR
SELECT fnv."ConditionFragmentId", fnv."ConditionId" FROM public."FragmentNodeVisitConditionXRefs" fnv
    INNER JOIN  public."ConditionsNodeVisit" nv on fnv."ConditionId" = nv."Id"

 WHERE nv."VersionId" = retVersionId;
  RETURN NEXT fragNodeVisitConditions;



    END;
$$;



--SELECT sf."Id", sf."Text", sf."AudioUrl", sf."ConditionalFragmentId", sf."ConditionalFragmentId1", sf. FROM public."SpeechFragments" sf