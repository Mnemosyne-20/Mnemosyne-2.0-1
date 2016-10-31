using System.Xml;
namespace Mnemosyne_Of_Mine
{
    internal class UserData
    {
        /// <summary>
        /// This is the userdata, stored in an object, this helps lol
        /// </summary>
        /// <param name="path">the path to the config file</param>
        /// <param name="helper">There to allow you to convert</param>
        public UserData(string path)
        {
            XmlDocument configXML = new XmlDocument();
            configXML.PreserveWhitespace = true;
            configXML.Load(path);
#if DEBUG2
            SubReddits = TryGetElementValue(configXML, "subreddit", new string[1] { "" });
#else
            SubReddit = TryGetElementValue(configXML, "subreddit", "");
#endif
            ReqLimit = TryGetElementValue(configXML, "ReqLimit", 30);
            SleepTime = TryGetElementValue(configXML, "SleepTime", 5);
            bUseOAuth = TryGetElementValue(configXML, "UseOAuth", false);
            if (bUseOAuth)
            {
                OAuthClientID = TryGetElementValue(configXML, "OAuthClientID", "");
                OAuthClientSecret = TryGetElementValue(configXML, "OAuthClientSecret", "");
                RedirectURI = TryGetElementValue(configXML, "RedirectURI", "");
            }
            Username = TryGetElementValue(configXML, "Username", "");
            Password = TryGetElementValue(configXML, "Password", "");
            FlavorText = TryGetElementValue(configXML, "flavortext", "Sample Text").Split('\"');
            Repost = TryGetElementValue(configXML, "Repost", "");
            SQLite = TryGetElementValue(configXML, "UseSQLite", false);
        }
        /// <summary>
        /// Will read the new data
        /// </summary>
        /// <param name="reader">reader from the method right above us</param>
        /// <remarks>This really needs work done on it so it can go "production"</remarks>
        private void ReadNewData(XmlReader reader)
        {
            reader.ReadToFollowing("Username");
            Username = reader.ReadElementContentAsString();
        }
        public string Password { get; set; }
        public string OAuth { get; private set; }
        public string[] FlavorText { get; private set; }
        public string Username { get; private set; }
        public int ReqLimit { get; private set; }
        public int SleepTime { get; private set; }
        public bool bUseOAuth { get; private set; }
        public string OAuthClientID { get; private set; }
        public string OAuthClientSecret { get; private set; }
        public string RedirectURI { get; private set; }
#if DEBUG2
        public string[] SubReddits { get; private set; }
#else
        public string SubReddit { get; private set; }
#endif
        public string Repost { get; private set; }
        public bool SQLite { get; private set; }
        private string[] TryGetElementValue(XmlDocument doc, string elementID, string[] defaultValue)
        {
            string[] r = defaultValue;
            string s = TryGetElementValue(doc, elementID, "");
            if (s != null)
            {
                r = s.Split(',');
            }
            return r;
        }
        private string TryGetElementValue(XmlDocument doc, string elementID, string defaultValue)
        {
            string r = defaultValue;
            XmlNode element = doc.DocumentElement.SelectSingleNode(elementID);
            if (element != null)
            {
                r = element.InnerText;
            }
            return r;
        }

        private int TryGetElementValue(XmlDocument doc, string elementID, int defaultValue)
        {
            int r = defaultValue;
            XmlNode element = doc.DocumentElement.SelectSingleNode(elementID);
            if (element != null)
            {
                int i;
                if (int.TryParse(element.InnerText, out i))
                {
                    r = i;
                }
            }
            return r;
        }

        private bool TryGetElementValue(XmlDocument doc, string elementID, bool defaultValue)
        {
            bool r = defaultValue;
            XmlNode element = doc.DocumentElement.SelectSingleNode(elementID);
            if (element != null)
            {
                bool b;
                if (bool.TryParse(element.InnerText, out b))
                {
                    r = b;
                }
            }
            return r;
        }
    }
}