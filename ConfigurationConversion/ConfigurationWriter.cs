using System.IO;
using System.Xml;
namespace ConfigurationConversion
{
    internal partial class Config
    {
        /// <summary>
        /// Holy shit i know this parameter list is massive, but it writes the new configuration format
        /// </summary>
        /// <param name="location">This is a stream because i can't create a filestream inside of a portable class</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="usingOauth"></param>
        /// <param name="ID"></param>
        /// <param name="URI"></param>
        /// <param name="secret"></param>
        /// <param name="subreddit"></param>
        /// <param name="reqlimit"></param>
        /// <param name="sleeptime"></param>
        /// <remarks>The parameters are self explanitory except the one i actually write for</remarks>
        public static void WriteNewData(Stream location, string username, string password, bool usingOauth, string ID, string URI, string secret, string subreddit, string reqlimit, string sleeptime)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.WriteEndDocumentOnClose = true;
            #region writer
            using (XmlWriter writer = XmlWriter.Create(location,settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("settings");
                writer.WriteAttributeString("New", true.ToString().ToLower());
                writer.WriteElementString("Username", username);
                writer.WriteAttributeString("Password", password);
                writer.WriteFullEndElement();
                writer.WriteElementString("OAuth", usingOauth.ToString().ToLower());
                writer.WriteAttributeString("Secret", secret);
                writer.WriteAttributeString("ID", ID);
                writer.WriteAttributeString("RedirURI", URI);
                writer.WriteFullEndElement();
                writer.WriteElementString("Subreddit", subreddit);
                writer.WriteFullEndElement();
                writer.WriteElementString("ReqLimit", reqlimit);
                writer.WriteAttributeString("SleepTime", sleeptime);
                writer.WriteStartElement("flavortext");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            #endregion
        }
        /// <summary>
        /// Only exists for the unit test
        /// </summary>
        /// <param name="location"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="usingOauth"></param>
        /// <param name="ID"></param>
        /// <param name="URI"></param>
        /// <param name="secret"></param>
        /// <param name="subreddit"></param>
        /// <param name="reqlimit"></param>
        /// <param name="sleeptime"></param>
        /// <returns>string to test again</returns>
        public static string WritseNewData(StringWriter location, string username, string password, bool usingOauth, string ID, string URI, string secret, string subreddit, string reqlimit, string sleeptime)
        {
            //XmlWriterSettings settings = new XmlWriterSettings();
            //settings.Indent = true;
            //settings.IndentChars = "\t";
            //settings.WriteEndDocumentOnClose = true;
            #region writer
            using (XmlWriter writer = XmlWriter.Create(location))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("settings");
                writer.WriteAttributeString("New", true.ToString().ToLower());
                writer.WriteElementString("Username", username);
                writer.WriteAttributeString("Password", password);
                writer.WriteFullEndElement();
                writer.WriteElementString("OAuth", usingOauth.ToString().ToLower());
                writer.WriteAttributeString("Secret", secret);
                writer.WriteAttributeString("ID", ID);
                writer.WriteAttributeString("RedirURI", URI);
                writer.WriteFullEndElement();
                writer.WriteElementString("Subreddit", subreddit);
                writer.WriteFullEndElement();
                writer.WriteElementString("ReqLimit", reqlimit);
                writer.WriteAttributeString("SleepTime", sleeptime);
                writer.WriteStartElement("flavortext");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            #endregion
            return location.ToString();
        }
    }
}
