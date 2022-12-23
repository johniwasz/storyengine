DROP TABLE whetstone.funcent_role_xrefs;

DROP TABLE whetstone.group_role_xrefs;

DROP TABLE whetstone.title_group_xrefs;

DROP TABLE whetstone.user_group_xrefs;

DROP TABLE whetstone.funcentitlements;

DROP TABLE whetstone.roles;

DROP TABLE whetstone.groups;

DROP TABLE whetstone.users;

DROP TABLE whetstone.organizations;

DROP TABLE whetstone.subscriptionlevels;

DELETE FROM whetstone.efmigrationhistory
WHERE "MigrationId" = '20200621171848_Security01Update';

