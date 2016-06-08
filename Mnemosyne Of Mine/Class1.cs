using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Mnemosyne_Of_Mine
{
    internal class DataStorage
    {
        public DataStorage(string path)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(File.ReadAllText(@".\config.xml"))))
            {
                reader.ReadToFollowing("Settings");
                reader.ReadToFollowing("subreddit");
                SubReddit = reader.ReadElementContentAsString();
                reader.ReadToFollowing("ReqLimit");
                ReqLimit = int.Parse(reader.ReadElementContentAsString());
                reader.ReadToFollowing("SleepTime");
                SleepTime = reader.ReadElementContentAsInt();
                reader.ReadToFollowing("Username");
                Username = reader.ReadElementContentAsString();
                reader.ReadToFollowing("Password");
                Password = reader.ReadElementContentAsString();
                reader.ReadToFollowing("flavortext");
                FlavorText = reader.ReadElementContentAsString().Split('\"'); // split by a " because commas
                reader.ReadToFollowing("Oauth");
                OAuth = reader.ReadElementContentAsString();
                reader.ReadToFollowing("Repost");
                Repost = reader.ReadElementContentAsString();
            }
        }
        public string Password { get; set; }
        public string OAuth { get; private set; }
        public string[] FlavorText { get; private set; }
        public string Username { get; private set; }
        public int ReqLimit { get; private set; }
        public int SleepTime { get; private set; }
        public string SubReddit { get; private set; }
        public string Repost { get; private set; }
    }
}
