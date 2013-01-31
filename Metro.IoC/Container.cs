using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metro.IoC
{
    public static class Container
    {
        static Dictionary<string, object> DataDict = new Dictionary<string, object>();
        public static object Get(string key)
        {
            if (!DataDict.ContainsKey(key))
                return null;
            return DataDict[key];
        }
        public static T Get<T>(string key)
        {
            try
            {
                return (T)DataDict[key];
            }
            catch (Exception e)
            {
                return default(T);
            }
        }
        public static void Set(string key, object value)
        {
           DataDict[key] = value;
        }
        public static Dictionary<string, object> GetAll()
        {
            return DataDict;
        }
    }
}
