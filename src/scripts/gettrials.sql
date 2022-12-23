create or replace function "clinicaltrials"."gettrials"(city text, condition text)
  returns SETOF refcursor
language plpgsql
as $$
 BEGIN


SELECT DISTINCT ct."Id", ct."StudySource", ct."TitleBrief", ct."TitleFull" FROM clinicaltrials."ClinicalTrials" ct
INNER JOIN clinicaltrials."ClinicalTrialAddressXRefs" addrxref ON ct."Id" = addrxref."ClinicalTrialId"
INNER JOIN clinicaltrials."Addresses" addr ON addrxref."AddressId" = addr."Id"
INNER JOIN clinicaltrials."ClinicalTrialPrimaryConditionXRefs" condxref ON ct."Id" = condxref."ClinicalTrialId"
INNER JOIN clinicaltrials."HealthConditions" hc ON condxref."HealthConditionId" = hc."Id"
WHERE addr."City" = city AND hc."Name" = condition AND ct."IsActive";

    END;
$$;
