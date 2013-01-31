using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using Windows.Data.Json;
sealed  class JsonParser
{
    public static IJsonValue Parse(string json)
    {
        if (string.IsNullOrEmpty(json))
            throw new ArgumentNullException("json");
        json = json.Trim();
        IJsonValue res = null;
        if (json.StartsWith("{"))
        {
            JsonObject o = null;
            if (JsonObject.TryParse(json, out o))
            {
                res = o.GetObject();
            }
        }
        else if (json.StartsWith("["))
        {
            JsonArray a = null;
            if (JsonArray.TryParse(json, out a))
            {
                res = a.GetArray();
            }
        }
        else {
            throw new ArgumentException("not json object or list");
        }
        return res;
    }
}