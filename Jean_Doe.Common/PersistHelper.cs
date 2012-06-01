using System.Xml.Serialization;
using System.IO;

namespace Jean_Doe.Common
{
    public class PersistHelper
    {
        public static void Save(object x, string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            path = Path.Combine(Global.BasePath, path);
            try
            {
                XmlSerializer Serializer = new XmlSerializer(x.GetType());
                using (var writer = new StreamWriter(path))
                    Serializer.Serialize(writer, x);
            }
            catch
            {
            }

        }
        public static T Load<T>(string path) where T : class
        {
            if (string.IsNullOrEmpty(path)) return null;
            path = Path.Combine(Global.BasePath, path);
            if (!File.Exists(path)) return null;
            try
            {
                XmlSerializer Serializer = new XmlSerializer(typeof(T));
                Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var res = (T)Serializer.Deserialize(stream);
                stream.Close();
                return res;
            }
            catch
            {
                return null;
            }
        }
    }
}