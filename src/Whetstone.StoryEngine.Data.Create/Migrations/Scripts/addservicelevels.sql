



INSERT INTO whetstone.subscriptionlevels
   (id, name, description)
VALUES
	('8bc0ab56-64d4-4925-8c7a-cedb070436ae', 'Free Tier', 'This is the default subscription level for a new user'),
	('bdbebc49-26fd-49ef-a5d9-2a86b6b77357', 'Designer', 'Paying single-user subscription'),
	('4ecc9046-fa90-474b-8ce4-58b2db559dcb', 'Team', 'Small team subscription.'),
	('b0c5254a-3f91-415b-bdf1-8fdf25545603', 'Enterprise', 'Large team subscription.');


INSERT INTO whetstone.roles
   (id, name, description)
VALUES
	('023e1461-81ea-4272-b2ea-22c59cbe26ef', 'Project Administrator', 'Has all project, version, deployment, and reporting rights.'),
	('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'Organization Administrator', 'Can manage groups, group membership, and invite users to the organization'),
	('6d369d82-92ed-47e6-8550-c49fd01b20f3', 'Developer', 'Can edit project versions'),
	('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', 'Release Manager', 'Can deploy project versions to channels'),
	('8b60dab8-c479-4e65-8ac8-1d816266a4d0', 'Reader', 'Has read rights to projects, versions, and deployments. Cannot view reports.'),
	('4fa00695-384a-4edc-b010-06c5fad42a2a', 'Report Viewer', 'Has the same rights as the reader in addition to being able to view and export reports'),
	('5097a742-e600-4372-aecc-17f365587153', 'Marketing Manager', 'Can view and export reports and can edit channel deployment metadata');


INSERT INTO whetstone.funcentitlements
  (id, name, description, claim)
VALUES
  ('3cc90f71-504c-43c8-b227-b815ddb43fdd', 'create project', 'Grants rights to create a new project', 'project-create'),
  ('90593f7e-1597-4243-88ee-0e7c1cb2336d', 'view project', 'Grants rights to view and list projects', 'project-view'),
  ('8fcfc2ea-a94b-48b0-94d3-891d963597a0', 'update project', 'Grants rights to update an assigned project', 'project-update'),
  ('2cd80c4e-5d46-4dd1-a372-20b5778863ed', 'delete project', 'Grants rights to delete a project', 'project-delete'),
  ('e4d3cc5a-0e6a-4378-8a39-641816e06fa6', 'view report', 'Grants rights to view usage reports', 'report-view'),
  ('8de66d7c-dd74-42bc-bb71-3bd85f5ee6fe', 'export report', 'Grants rights to export reports', 'report-export'),
  ('ec916756-6af1-4034-9217-f051bef9300a', 'create version', 'Grants rights to create a version of an assigned project', 'version-create'),
  ('7f1e443d-c539-4d73-b9fb-ec78c89ab159', 'update version', 'Grants rights to update a version of an assigned project', 'version-update'),
  ('02abed44-9b82-48d0-ae06-1fe88e4b3d4e', 'view version', 'Grants rights to view a version of an assigned project', 'version-view'),
  ('99cff5bc-fdf0-4f3a-bb57-3e5cc80f3bbe', 'delete version', 'Grants rights to delete a version of an assigned project', 'version-delete'),
  ('b5f9b739-3175-4d8e-9c65-fcf2672d86df', 'add audio file', 'Grants rights to add an audio file to a version', 'audiofile-add'),
  ('9bb85188-6089-4acd-abf7-8a5c23f44e70', 'update audio file', 'Grants rights to update or replace an audio file in a version', 'audiofile-update'),
  ('8bf5bdb7-ce27-4d1f-a676-b308d10ad408', 'delete audio file', 'Grants rights to delete an audio file in a version', 'audiofile-delete'),
  ('1fcc5a27-d466-41e1-83c2-f0d529c213f3', 'play audio file', 'Grants rights to play an audio file in a version', 'audiofile-play'),
  ('152be0fc-37ed-455c-9e55-a5b37c5e9779', 'add image file', 'Grants rights to add an image to a version', 'imagefile-add'),
  ('728e64e2-bb8f-4e7e-8477-f5ab2efdfac6', 'view image file', 'Grants rights to view an imagse in a version', 'imagefile-view'),
  ('e87d941b-75d8-464d-9dbb-789bfa88254c', 'delete image file', 'Grants rights to delete an imagse in a version', 'imagefile-delete'),
  ('5ed13dc8-548a-4ac0-8974-fc0bc1d198c8', 'view deployment', 'Grants rights to view a version deployment', 'deployment-view'),
  ('728475d2-16d8-428d-ab03-3d0debd20e3c', 'remove deployment', 'Grants rights to remove a version deployment', 'deployment-remove'),
  ('539c22c2-f774-4575-80f7-2402504fb5e3', 'edit channel deployment metadata', 'Grants rights to edit metadata for Alexa, Google, etc. for channel deployments', 'channeldeploymentmetadata-edit'),  
  ('28d8b099-bd87-4aef-866e-090673b00e4a', 'view channel deployment metadata', 'Grants rights to view metadata for Alexa, Google, etc. for channel deployments', 'channeldeploymentmetadata-view'),
  ('46bf5cdd-6a76-4765-b782-4664bbb6a955', 'deploy version', 'Grants rights to deploy a version to a channel', 'deployment-create'),
  ('3c028526-bf53-42e1-a903-c2f0f71626d7', 'create group', 'Grants rights create a group for an organization', 'group-create'),
  ('17e0617d-118a-4235-b13f-15a86f06219d', 'edit group', 'Grants rights edit a group for an organization', 'group-edit'),
  ('ffb35a14-a447-4d55-a6c2-1b087976bce3', 'delete group', 'Grants rights delete a group for an organization', 'group-delete'),
  ('d800b6f0-581a-476e-8a58-043ed05893b9', 'view group', 'Grants rights view a group for an organization', 'group-view'),
  ('ba76c9b8-9aa0-45c3-8c79-d3e3ea60015c', 'add user to group', 'Grants rights add users to a group', 'group-user-add'),
  ('cbe77284-27cb-4018-9bb5-2e61ea8103b6', 'remove user from group', 'Grants rights remove users from a group', 'group-user-remove'),
   ('37a1373d-bb01-44bc-8fc8-1769f7c1a42e', 'invite user to organization', 'Grants rights invite a user to an organization', 'organization-user-invite');



INSERT INTO whetstone.funcent_role_xrefs
  (role_id, func_entitlement_id)
VALUES
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '3cc90f71-504c-43c8-b227-b815ddb43fdd'), -- Grant project-create to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '90593f7e-1597-4243-88ee-0e7c1cb2336d'), -- Grant project-view to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '8fcfc2ea-a94b-48b0-94d3-891d963597a0'), -- Grant project-update to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '2cd80c4e-5d46-4dd1-a372-20b5778863ed'), -- Grant project-delete to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', 'e4d3cc5a-0e6a-4378-8a39-641816e06fa6'), -- Grant report-view to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '8de66d7c-dd74-42bc-bb71-3bd85f5ee6fe'), -- Grant report-export to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', 'ec916756-6af1-4034-9217-f051bef9300a'), -- Grant version-create to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '7f1e443d-c539-4d73-b9fb-ec78c89ab159'), -- Grant version-update to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '02abed44-9b82-48d0-ae06-1fe88e4b3d4e'), -- Grant version-view to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '99cff5bc-fdf0-4f3a-bb57-3e5cc80f3bbe'), -- Grant version-delete to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', 'b5f9b739-3175-4d8e-9c65-fcf2672d86df'), -- Grant audiofile-add to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '9bb85188-6089-4acd-abf7-8a5c23f44e70'), -- Grant audiofile-update to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '8bf5bdb7-ce27-4d1f-a676-b308d10ad408'), -- Grant audiofile-delete to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '1fcc5a27-d466-41e1-83c2-f0d529c213f3'), -- Grant audiofile-play to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '152be0fc-37ed-455c-9e55-a5b37c5e9779'), -- Grant imagefile-add to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '728e64e2-bb8f-4e7e-8477-f5ab2efdfac6'), -- Grant imagefile-view to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', 'e87d941b-75d8-464d-9dbb-789bfa88254c'), -- Grant imagefile-delete to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '5ed13dc8-548a-4ac0-8974-fc0bc1d198c8'), -- Grant deployment-view to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '728475d2-16d8-428d-ab03-3d0debd20e3c'), -- Grant deployment-remove to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '539c22c2-f774-4575-80f7-2402504fb5e3'), -- Grant channeldeploymentmetadata-edit to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '28d8b099-bd87-4aef-866e-090673b00e4a'), -- Grant channeldeploymentmetadata-view to Project Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '46bf5cdd-6a76-4765-b782-4664bbb6a955'), -- Grant deployment-create to Project Administrator

('74236635-ab8f-4f7b-bf76-b1c1089c2030', '3c028526-bf53-42e1-a903-c2f0f71626d7'), -- Grants group-create to Organization Administrator
('74236635-ab8f-4f7b-bf76-b1c1089c2030', '17e0617d-118a-4235-b13f-15a86f06219d'), -- Grants group-edit to Organization Administrator
('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'ffb35a14-a447-4d55-a6c2-1b087976bce3'), -- Grants group-delete to Organization Administrator
('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'd800b6f0-581a-476e-8a58-043ed05893b9'), -- Grants group-view to Organization Administrator
('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'ba76c9b8-9aa0-45c3-8c79-d3e3ea60015c'), -- Grants group-user-add to Organization Administrator
('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'cbe77284-27cb-4018-9bb5-2e61ea8103b6'), -- Grants group-user-remove to Organization Administrator
('74236635-ab8f-4f7b-bf76-b1c1089c2030', '37a1373d-bb01-44bc-8fc8-1769f7c1a42e'), -- Grants organization-user-invite to Organization Administrator

('6d369d82-92ed-47e6-8550-c49fd01b20f3', '7f1e443d-c539-4d73-b9fb-ec78c89ab159'), -- Grants version-update to Developer
('6d369d82-92ed-47e6-8550-c49fd01b20f3', '02abed44-9b82-48d0-ae06-1fe88e4b3d4e'), -- Grants version-view to Developer
('6d369d82-92ed-47e6-8550-c49fd01b20f3', '90593f7e-1597-4243-88ee-0e7c1cb2336d'), -- Grants project-view to Developer
('6d369d82-92ed-47e6-8550-c49fd01b20f3', 'b5f9b739-3175-4d8e-9c65-fcf2672d86df'), -- Grant audiofile-add to Developer
('6d369d82-92ed-47e6-8550-c49fd01b20f3', '9bb85188-6089-4acd-abf7-8a5c23f44e70'), -- Grant audiofile-update to Developer
('6d369d82-92ed-47e6-8550-c49fd01b20f3', '8bf5bdb7-ce27-4d1f-a676-b308d10ad408'), -- Grant audiofile-delete to Developer
('6d369d82-92ed-47e6-8550-c49fd01b20f3', '1fcc5a27-d466-41e1-83c2-f0d529c213f3'), -- Grant audiofile-play to Developer
('6d369d82-92ed-47e6-8550-c49fd01b20f3', '152be0fc-37ed-455c-9e55-a5b37c5e9779'), -- Grant imagefile-add to Developer
('6d369d82-92ed-47e6-8550-c49fd01b20f3', '728e64e2-bb8f-4e7e-8477-f5ab2efdfac6'), -- Grant imagefile-view to Developer
('6d369d82-92ed-47e6-8550-c49fd01b20f3', 'e87d941b-75d8-464d-9dbb-789bfa88254c'), -- Grant imagefile-delete to Developer


('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', '90593f7e-1597-4243-88ee-0e7c1cb2336d'), -- Grants project-view to Release Manager
('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', '02abed44-9b82-48d0-ae06-1fe88e4b3d4e'), -- Grants version-view to Release Manager
('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', '5ed13dc8-548a-4ac0-8974-fc0bc1d198c8'), -- Grants deployment-view to Release Manager
('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', '46bf5cdd-6a76-4765-b782-4664bbb6a955'), -- Grant deployment-create to Release Manager
('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', '728475d2-16d8-428d-ab03-3d0debd20e3c'), -- Grant deployment-remove to Release Manager
('a0df4f6e-d4d1-424c-a33e-73cbe9f71788', '28d8b099-bd87-4aef-866e-090673b00e4a'), -- Grant channeldeploymentmetadata-view to Release Manager

('8b60dab8-c479-4e65-8ac8-1d816266a4d0', '02abed44-9b82-48d0-ae06-1fe88e4b3d4e'), -- Grants version-view to Reader
('8b60dab8-c479-4e65-8ac8-1d816266a4d0', '90593f7e-1597-4243-88ee-0e7c1cb2336d'), -- Grants project-view to Reader
('8b60dab8-c479-4e65-8ac8-1d816266a4d0', '5ed13dc8-548a-4ac0-8974-fc0bc1d198c8'), -- Grant deployment-view to Reader
('8b60dab8-c479-4e65-8ac8-1d816266a4d0', '1fcc5a27-d466-41e1-83c2-f0d529c213f3'), -- Grant audiofile-play to Reader
('8b60dab8-c479-4e65-8ac8-1d816266a4d0', '728e64e2-bb8f-4e7e-8477-f5ab2efdfac6'), -- Grant imagefile-view to Reader

('4fa00695-384a-4edc-b010-06c5fad42a2a', '90593f7e-1597-4243-88ee-0e7c1cb2336d'), -- Grants project-view to Report Viewer
('4fa00695-384a-4edc-b010-06c5fad42a2a', '02abed44-9b82-48d0-ae06-1fe88e4b3d4e'), -- Grants version-view to Report Viewer
('4fa00695-384a-4edc-b010-06c5fad42a2a', '5ed13dc8-548a-4ac0-8974-fc0bc1d198c8'), -- Grants deployment-view to Report Viewer
('4fa00695-384a-4edc-b010-06c5fad42a2a', 'e4d3cc5a-0e6a-4378-8a39-641816e06fa6'), -- Grant report-view to Report Viewer
('4fa00695-384a-4edc-b010-06c5fad42a2a', '8de66d7c-dd74-42bc-bb71-3bd85f5ee6fe'), -- Grant report-export to Report Viewer

('5097a742-e600-4372-aecc-17f365587153', '90593f7e-1597-4243-88ee-0e7c1cb2336d'), -- Grants project-view to Marketing Manager
('5097a742-e600-4372-aecc-17f365587153', '02abed44-9b82-48d0-ae06-1fe88e4b3d4e'), -- Grants version-view to Marketing Manager
('5097a742-e600-4372-aecc-17f365587153', '5ed13dc8-548a-4ac0-8974-fc0bc1d198c8'), -- Grants deployment-view to Marketing Manager
('5097a742-e600-4372-aecc-17f365587153', 'e4d3cc5a-0e6a-4378-8a39-641816e06fa6'), -- Grant report-view to Marketing Manager
('5097a742-e600-4372-aecc-17f365587153', '8de66d7c-dd74-42bc-bb71-3bd85f5ee6fe'), -- Grant report-export to Marketing Manager
('5097a742-e600-4372-aecc-17f365587153', '539c22c2-f774-4575-80f7-2402504fb5e3'), -- Grant channeldeploymentmetadata-edit to Marketing Manager
('5097a742-e600-4372-aecc-17f365587153', '28d8b099-bd87-4aef-866e-090673b00e4a'); -- Grant channeldeploymentmetadata-view to Marketing Manager


