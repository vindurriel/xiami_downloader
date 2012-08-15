using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Jean_Doe.Common;
namespace Jean_Doe.Music.Console
{
    class Program
    {
        static void print(string s)
        {
            System.Console.WriteLine(s);
        }
        static void Main(string[] args)
        {
        }
        static async void getlrc()
        {
            var lrc = await NetAccess.GetUrlLrc("2080986");
            print(lrc);
        }
        private static void old()
        {
            var s1 = new Song { Name = "titla?dfe<>", ArtistName = "afe", AlbumName = "sfef3", AlbumId = "1231", Id = "122" };
            var s2 = new Song { Name = "feafd", ArtistName = "afe", AlbumName = "sfef3", AlbumId = "1231", Id = "122" };
            var songs = new Songs { s1, s2 };
            PersistHelper.Save(songs, "songs.xml");
            var a = PersistHelper.Load<Songs>("songs.xml");
            foreach(var song in a)
            {
                System.Console.WriteLine(song.Name);
            }
            System.Console.ReadKey();
        }
        static Regex RegExtension(string ext)
        {
            return new Regex("\\." + ext + "$");
        }

        static IEnumerable<string> WalkDirectoryTree(DirectoryInfo root, Regex filter = null)
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;
            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch(Exception e)
            {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                print(e.Message);
            }
            if(files != null)
            {
                foreach(FileInfo fi in files)
                {
                    var ok = false;
                    if(filter == null) ok = true;
                    else
                    {
                        var m = filter.Match(fi.FullName);
                        if(m.Success) ok = true;
                    }
                    if(ok) yield return fi.FullName;
                }
            }
            // Now find all the subdirectories under this directory.
            subDirs = root.GetDirectories();

            foreach(DirectoryInfo dirInfo in subDirs)
            {
                WalkDirectoryTree(dirInfo);
            }
        }
    }
}
