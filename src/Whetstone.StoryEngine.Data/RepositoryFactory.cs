//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Text;
//using MongoDB.Driver;
//using Sbs.StoryEngine.Data.MongoDb;
//using Microsoft.Extensions.Caching.Distributed;
//using Sbs.StoryEngine.Models.Configuration;

//namespace Sbs.StoryEngine.Data
//{
//    public static class RepositoryFactory
//    {


//        private static ConcurrentDictionary<string, IMongoDatabase> _mongoDBDict = new ConcurrentDictionary<string, IMongoDatabase>();



       


//        //public static ISessionRepository GetSessionRepository(DbConnections cons)
//        //{
//        //    ISessionRepository sessionRepository = null;

//        //    switch (cons.DbType)
//        //    {
//        //        case ConnectionType.Mongo:
//        //            IMongoDatabase monDb = GetMongoClient(cons.MongoDb);
//        //            sessionRepository = new MongoDbSessionRepository(monDb);
//        //            break;
//        //    }

//        //    return sessionRepository;
//        //}


//        //public static IStoryNodeRepository GetStoryNodeRepository(DbConnections cons)
//        //{
//        //    IStoryNodeRepository storyNodeRepository = null;

//        //    switch (cons.DbType)
//        //    {
//        //        case ConnectionType.Mongo:
//        //            IMongoDatabase monDb = GetMongoClient(cons.MongoDb);
//        //            storyNodeRepository = new MongoDbStoryNodeRepository(monDb);
//        //            break;
//        //    }

//        //    return storyNodeRepository;
//        //}


//        private static IMongoDatabase GetMongoClient(Sbs.StoryEngine.Models.Configuration.MongoDbConfig config)
//        {
//            string monKey = string.Concat(config.ConnectionString, "|", config.StoryEngineDb);

//            IMongoDatabase retMonDatabase = null;

//            if (_mongoDBDict.Keys.Contains(monKey))
//            {
//                retMonDatabase = _mongoDBDict[monKey];
//            }
//            else
//            {
//                var monClient = new MongoClient(config.ConnectionString);
//                var monDb = monClient.GetDatabase(config.StoryEngineDb);
//              //  retMonDatabase =  _mongoDBDict.AddOrUpdate(monKey, monDb,   

//            }



//            return retMonDatabase;
//        }

//        //public static IIntentRepository GetIntentRepository(DbConnections cons)
//        //{
//        //    IIntentRepository intentRepository = null;

//        //    switch (cons.DbType)
//        //    {
//        //        case ConnectionType.Mongo:
//        //            IMongoDatabase monDb = GetMongoClient(cons.MongoDb);
//        //            intentRepository = new MongoDbIntentRepository(monDb);
//        //            break;
//        //    }

//        //    return intentRepository;
//        //}
//    }
//}
