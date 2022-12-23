


create or replace function purgeversion(delversionid bigint)
  returns void
language plpgsql
as $$
BEGIN

DELETE FROM public."ChoiceConditionVisitXRef" cv WHERE cv."ChoiceId" IN
         (SELECT c."Id" FROM public."Choices" c INNER JOIN public."StoryNodes" sn ON c."StoryNodeId" = sn."Id"
                INNER JOIN public."Chapters" ch ON sn."ChapterId" = ch."Id" WHERE ch."VersionId" = delversionid);


--DELETE FROM public."ChoiceConditionInventoryXRef" ci WHERE ci."ChoiceId" IN
 --        (SELECT c."Id" FROM public."Choices" c INNER JOIN public."StoryNodes" sn ON c."StoryNodeId" = sn."Id"
 --               INNER JOIN public."Chapters" ch ON sn."ChapterId" = ch."Id" WHERE ch."VersionId" = delversionid);

DELETE FROM public."FragmentNodeVisitConditionXRefs" viscon WHERE viscon."ConditionId" IN
         (SELECT con."Id" FROM public."ConditionsNodeVisit" con WHERE con."VersionId" =delversionid);


DELETE FROM public."FragmentInventoryConditionXRefs" incon WHERE incon."ConditionId" IN
         (SELECT con."Id" FROM public."ConditionsInventory" con WHERE con."VersionId" =delversionid);


DELETE FROM public."InventoryConditionItemXRefs" ixref WHERE ixref."ItemId" IN
         (SELECT i."Id" FROM public."InventoryItems" i WHERE i."VersionId" =delversionid);


DELETE FROM public."NodeVisitXRefs" vr WHERE vr."NodeId" IN (SELECT sn."Id" FROM public."StoryNodes" sn
    INNER JOIN public."Chapters" c ON sn."ChapterId" = c."Id" WHERE c."VersionId" = delversionid);


DELETE FROM public."NodeActions" na WHERE na."StoryNodeId" IN  (SELECT sn."Id" FROM public."StoryNodes" sn
    INNER JOIN public."Chapters" c ON sn."ChapterId" = c."Id" WHERE c."VersionId" = delversionid);


DELETE FROM public."Choices" c
    WHERE c."IntentId" IN (SELECT i."Id"  FROM public."Intents" i
          WHERE i."VersionId" =delversionid);

DELETE  FROM public."Choices" c WHERE c."StoryNodeId" IN (SELECT sn."Id" FROM public."StoryNodes" sn
     WHERE sn."VersionId" =delversionid);


DELETE FROM public."ConditionsNodeVisit" cnv WHERE cnv."VersionId" = delversionid;

  DELETE FROM public."IntentSlotMappings" ism
   WHERE ism."IntentId" IN (SELECT i."Id" FROM public."Intents" i
       WHERE i."VersionId" = delversionid);

DELETE FROM public."Intents" i WHERE i."VersionId" = delversionid;


DELETE FROM public."SlotTypes" st WHERE st."VersionId" = delversionid;

DELETE FROM public."SpeechFragments" sf
   WHERE sf."VersionId" = delversionid;

DELETE FROM public."ClientSpeechFrags" csp
  WHERE csp."LocRepromptResponseId"  IN (SELECT lr."Id" from public."LocResponse" lr
                    INNER JOIN public."LocResponseSet" lrs ON lr."DataLocalizedResponseSetId" = lrs."Id"
                    INNER JOIN public."StoryNodes" sn on lrs."StoryNodeId" = sn."Id"
                    INNER JOIN public."Chapters" c on sn."ChapterId" = c."Id"
                    WHERE c."VersionId" =delversionid);


DELETE FROM public."ClientSpeechFrags" csp
  WHERE csp."LocResponseId" IN (SELECT lr."Id" from public."LocResponse" lr
                    INNER JOIN public."LocResponseSet" lrs ON lr."DataLocalizedResponseSetId" = lrs."Id"
                    INNER JOIN public."StoryNodes" sn on lrs."StoryNodeId" = sn."Id"
                    INNER JOIN public."Chapters" c on sn."ChapterId" = c."Id"
                    WHERE c."VersionId" =delversionid);

DELETE FROM public."LocResponse" lr
   WHERE lr."DataLocalizedResponseSetId" IN (SELECT lrs."Id" from public."LocResponseSet" lrs
                    INNER JOIN public."StoryNodes" sn on lrs."StoryNodeId" = sn."Id"
                    INNER JOIN public."Chapters" c on sn."ChapterId" = c."Id"
                    WHERE c."VersionId" =delversionid);

DELETE FROM public."LocResponseSet" ls
   WHERE ls."StoryNodeId" IN ( SELECT sn."Id"
                            FROM public."StoryNodes" sn
                            INNER JOIN public."Chapters" c ON sn."ChapterId" = c."Id"
                            WHERE  c."VersionId" = delversionid);

  DELETE FROM public."ConditionsInventory" ci WHERE ci."VersionId" = delversionid;

  DELETE FROM public."ConditionsNodeVisit" cnv WHERE cnv."VersionId" = delversionid;

  DELETE FROM public."StandardNodes" sn WHERE sn."VersionId" = delversionid;

    DELETE FROM public."StoryNodes" sn where sn."VersionId" = delversionid;

   DELETE FROM public."InventoryItems" i WHERE i."VersionId" = delversionid;

    DELETE FROM public."Chapters" ch where ch."VersionId" = delversionid;

   DELETE  FROM public."StoryVersions" sv WHERE sv."Id" = delversionid;


END
$$;

