using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Whetstone.StoryEngine.Models.Serialization
{

    public class ReferenceResolverWithUuid : IReferenceResolver
    {
        private readonly ReferenceCollection _collection = new ReferenceCollection();

        public object ResolveReference(object context, string reference)
        {
            return _collection.Get(reference);
        }

        public string GetReference(object context, object value)
        {
            if (_collection.Contains(value)) return _collection.GetReference(value);
            return _collection.Add(value);
        }

        public bool IsReferenced(object context, object value)
        {
            return _collection.Contains(value);
        }

        public void AddReference(object context, string reference, object value)
        {
            _collection.Add(value, reference);
        }

        class ReferenceCollection
        {
            private readonly Dictionary<Type, Dictionary<string, object>> _referenceDictionary = new Dictionary<Type, Dictionary<string, object>>();

            internal bool Contains(object obj)
            {
                var objType = obj.GetType();
                Dictionary<string, object> objDictionary;

                if (_referenceDictionary.TryGetValue(objType, out objDictionary))
                    return objDictionary.ContainsValue(obj);
                return false;
            }

            internal string Add(object obj, string key = null)
            {
                var objType = obj.GetType();
                Dictionary<string, object> objDictionary;
                if (!_referenceDictionary.TryGetValue(objType, out objDictionary))
                    _referenceDictionary.Add(objType, objDictionary = new Dictionary<string, object>());

                if (key == null)
                {
                    var objPropertyInfo = objType.GetProperty("UniqueId");
                    if (objPropertyInfo != null) key = ((Guid)objPropertyInfo.GetValue(obj)).ToString();
                    else key = objDictionary.Count.ToString();
                }

                if (!objDictionary.ContainsKey(key))
                    objDictionary.Add(key, obj);
                else
                    objDictionary[key] = obj;

                return key;
            }

            internal object Get(string reference)
            {
                //this requires that names be unique within a json file
                var typeDictionary = _referenceDictionary.Values.SingleOrDefault(typeDict => typeDict.ContainsKey(reference));
                if (typeDictionary == null) throw new InvalidOperationException(string.Format(@"value with for $ref: ""{0}"" hasn't been deserialized yet.", reference));
                return typeDictionary[reference];
            }

            internal string GetReference(object obj)
            {
                var objType = obj.GetType();
                var objPropertyInfo = objType.GetProperty("UniqueId");
                if (objPropertyInfo != null) return ((Guid)objPropertyInfo.GetValue(obj)).ToString();

                Dictionary<string, object> objDictionary;
                if (!_referenceDictionary.TryGetValue(objType, out objDictionary))
                    _referenceDictionary.Add(objType, objDictionary = new Dictionary<string, object>());
                return objDictionary.Count.ToString();
            }
        }
    }

}
