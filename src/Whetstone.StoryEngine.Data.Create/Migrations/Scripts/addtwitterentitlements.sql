


INSERT INTO whetstone.funcentitlements
  (id, name, description, claim)
VALUES
  ('fc758d8f-4076-427d-bdc6-d380885b621d', 'manage twitter credentials', 'Grants rights to create, enable, and disable Twitter credentials', 'twittercredentials-manage'),
  ('899e4111-c51e-4462-88c6-c81c401659d8', 'read twitter credentials', 'Grants rights to read Twitter credentials', 'twittercredentials-read'),
  ('042abef2-826b-45ab-afc9-5138fe0dc060', 'delete twitter credentials', 'Grants rights delete Twitter credentials', 'twittercredetials-delete'),
  ('da67f2ce-80bf-4430-95d8-78fb17081529', 'manage twitter applications', 'Grants rights to create, and edit Twitter applications', 'twitterapplicatios-manage'),
  ('7e687c39-cd65-4189-a12c-3b1b0b64cda4', 'read twitter applications', 'Grants rights to read Twitter applications', 'twitterapplicatios-read'),
  ('c867f368-2e84-4393-800f-aa0fb8bd9f89', 'delete twitter applications', 'Grants rights delete Twitter applications', 'twitterapplicatios-delete');

 

INSERT INTO whetstone.funcent_role_xrefs
  (role_id, func_entitlement_id)
VALUES
('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'fc758d8f-4076-427d-bdc6-d380885b621d'), -- Grants twittercredentials-manage to Organization Administrator
('74236635-ab8f-4f7b-bf76-b1c1089c2030', '899e4111-c51e-4462-88c6-c81c401659d8'), -- Grants twittercredentials-read to Organization Administrator
('74236635-ab8f-4f7b-bf76-b1c1089c2030', '042abef2-826b-45ab-afc9-5138fe0dc060'), -- Grants twittercredetials-delete to Organization Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '899e4111-c51e-4462-88c6-c81c401659d8'), -- Grant twittercredentials-read to Project Administrator
('6d369d82-92ed-47e6-8550-c49fd01b20f3', '899e4111-c51e-4462-88c6-c81c401659d8'), -- Grant twittercredentials-read to Developer
('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'da67f2ce-80bf-4430-95d8-78fb17081529'), -- Grants twitterapplicatios-manage to Organization Administrator
('74236635-ab8f-4f7b-bf76-b1c1089c2030', '7e687c39-cd65-4189-a12c-3b1b0b64cda4'), -- Grants twitterapplicatios-read to Organization Administrator
('74236635-ab8f-4f7b-bf76-b1c1089c2030', 'c867f368-2e84-4393-800f-aa0fb8bd9f89'), -- Grants twitterapplicatios-delete to Organization Administrator
('023e1461-81ea-4272-b2ea-22c59cbe26ef', '7e687c39-cd65-4189-a12c-3b1b0b64cda4'), -- Grant twitterapplicatios-read to Project Administrator
('6d369d82-92ed-47e6-8550-c49fd01b20f3', '7e687c39-cd65-4189-a12c-3b1b0b64cda4'); -- Grant twitterapplicatios-read to Developer