
SELECT ss.userid, ss.client, ss.titleid, ss.userlocale, COUNT(ia.*) as utterancecount FROM whetstone.intent_action ia
INNER JOIN whetstone.story_session ss ON ia.sessionid = ss.id
WHERE ss.userid IN (SELECT DISTINCT userid FROM whetstone.story_session ss)
GROUP BY ss.userid, ss.client, ss.titleid, ss.userlocale
ORDER BY utterancecount DESC;


-- Retrieves the utterance count per user since the provided date
SELECT ss.userid, ss.client, ss.titleid, COUNT(ia.*) as utterancecount FROM whetstone.intent_action ia
INNER JOIN whetstone.story_session ss ON ia.sessionid = ss.id
WHERE ss.userid IN (SELECT DISTINCT userid FROM whetstone.story_session ss
WHERE startdate>'2019-01-26')
GROUP BY ss.userid, ss.client, ss.titleid, ss.userlocale
ORDER BY utterancecount DESC;



-- Retrieves number of sessions per user since the given date
SELECT userid, client, titleid, COUNT(*) as sessioncount FROM whetstone.story_session
WHERE userid IN (SELECT DISTINCT userid FROM whetstone.story_session ss
WHERE startdate>'2019-01-26')
GROUP BY userid, client, titleid
ORDER BY COUNT(*) DESC;


SELECT * FROM whetstone.intent_action 



SELECT ss.userid, ss.client, ss.titleid, COUNT(ia.*) as utterancecount FROM whetstone.intent_action ia
INNER JOIN whetstone.story_session ss ON ia.sessionid = ss.id
WHERE ss.userid IN (SELECT DISTINCT userid FROM whetstone.story_session ss
WHERE startdate>'2018-12-01')
GROUP BY ss.userid, ss.client, ss.titleid, ss.userlocale

ORDER BY utterancecount DESC, titleid, client;


SELECT foo.client, foo.titleid, COUNT(foo.userid) as utterancecounttotal

FROM
  (SELECT ss.userid,   ss.client, ss.titleid, COUNT(ia.*) as utterancecount FROM whetstone.intent_action ia
INNER JOIN whetstone.story_session ss ON ia.sessionid = ss.id
WHERE ss.userid IN (SELECT DISTINCT userid FROM whetstone.story_session ss
WHERE startdate>'2018-12-01')
GROUP BY ss.userid,  ss.client, ss.titleid
HAVING COUNT(ia.*) > 30
ORDER BY utterancecount DESC, titleid, client) as foo
GROUP BY foo.client, foo.titleid
ORDER BY foo.client, foo.titleid;



SELECT  ss.client, ss.titleid, COUNT(ss.sessionId) as sessioncount FROM

  whetstone.story_session ss
WHERE ss.userid IN (SELECT DISTINCT userid FROM whetstone.story_session ss
WHERE startdate>'2018-12-01')
GROUP BY  ss.client, ss.titleid
ORDER BY sessioncount DESC, titleid, client;


-- Unique users per client
SELECT foo.client, foo.titleid, COUNT(foo.userid)
  FROM
  (SELECT ss.client, ss.titleid, ss.userid FROM
  whetstone.story_session ss
WHERE startdate>'2018-12-01'
GROUP BY ss.client, ss.titleid, ss.userid) as foo
GROUP BY foo.client, foo.titleid
ORDER BY foo.client, foo.titleid;

-- Find a deployment
SELECT tvd.* from whetstone.titles t
inner join whetstone.titleversions tv ON t.id = tv.titleid
inner join whetstone.titleversiondeployments tvd ON tv.id = tvd.versionid
where t.shortname = 'whetstonetechnologies' and tv.version = '0.2' and tvd.client =2;

SELECT era.intentname, era.slots,  era.rawtext as rawtext, era.mappednode, era.selectiontime FROM whetstone.engine_requestaudit era
  INNER JOIN whetstone.engine_session es ON era.sessionid = es.id

  where es.deploymentid = '3201e795-560b-442d-8a08-4224f8140563'
ORDER BY selectiontime DESC;