using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Linq;
using System.Collections.Generic;
namespace Jean_Doe.Common
{
    public class PersistHelper
    {
        public static string SqliteDbPath = Path.Combine(Global.BasePath, "songs.sqlite");
        static object lck = new object();
        public static void Delete<T>(T[] items) where T : IHasId, new()
        {
            using (var db = new SQLite.SQLiteConnection(SqliteDbPath))
            {
                lock (lck)
                {
                    db.CreateTable<T>();
                    db.BeginTransaction();
                    foreach (var item in items)
                    {
                        var d = db.Get<T>(item.Id);
                        if (d != null)
                            db.Delete(d);
                    }
                    db.Commit();
                }
            }
        }
        static bool always_true<T>(T o) { return true; }
        public static List<T> Load<T>(Func<T, bool> condition = null) where T : new()
        {
            var res = new List<T>();
            SQLite.SQLiteConnection db = null;
            if (condition == null)
                condition = always_true;
            using (db = new SQLite.SQLiteConnection(SqliteDbPath))
            {
                db.CreateTable<T>();
                res = (from s in db.Table<T>() select s).Where(condition).ToList();
            }
            return res;
        }
        public static void Save<T>(T[] items) where T : new()
        {
            using (var db = new SQLite.SQLiteConnection(SqliteDbPath))
            {
                lock (lck)
                {
                    db.CreateTable<T>();
                    db.BeginTransaction();
                    foreach (var item in items)
                    {
                        db.InsertOrReplace(item);
                    }
                    db.Commit();
                }
            }
        }
    }
}