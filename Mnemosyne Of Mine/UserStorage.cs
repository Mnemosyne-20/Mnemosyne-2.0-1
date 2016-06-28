using System;
using System.IO;
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
            var readers = new StringReader(File.ReadAllText(path));
            using (XmlReader reader = XmlReader.Create(readers))
            {
                reader.ReadToFollowing("Settings");
                try
                {

                }
                catch
                {

                }
                reader.ReadToFollowing("subreddit");
                SubReddit = reader.ReadElementContentAsString();
                reader.ReadToFollowing("ReqLimit");
                ReqLimit = int.Parse(reader.ReadElementContentAsString());
                reader.ReadToFollowing("SleepTime");
                SleepTime = reader.ReadElementContentAsInt();
                reader.ReadToFollowing("UseOAuth");
                bUseOAuth = reader.ReadElementContentAsBoolean();
                if (bUseOAuth)
                {
                    reader.ReadToFollowing("OAuthClientID");
                    OAuthClientID = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("OAuthClientSecret");
                    OAuthClientSecret = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("RedirectURI");
                    RedirectURI = reader.ReadElementContentAsString();
                }
                reader.ReadToFollowing("Username");
                Username = reader.ReadElementContentAsString();
                try
                { 
                    reader.ReadToFollowing("Password");
                    Password = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("flavortext");
                    FlavorText = reader.ReadElementContentAsString().Split('\"'); // split by a " because commas
                    reader.ReadToFollowing("Repost");
                    Repost = reader.ReadElementContentAsString();
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    
                }
            }
        }
        private static void ReadNewData(XmlReader reader)
        {

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
        public string SubReddit { get; private set; }
        public string Repost { get; private set; }
    }
}
