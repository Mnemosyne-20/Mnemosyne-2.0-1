using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Net.Http;
using System.Text.RegularExpressions;
using RedditSharp;
namespace Mnemosyne_Of_Mine
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();
            string archiveURL;
            string Oauth = null;
            Console.Title = "Mnemosyne by chugga_fan, copying off of /u/ITSigno's code as a backup";
            #region constants
            var exclude = new Regex(@"(youtube.com|archive.is|web.archive.org|webcache.googleusercontent.com|youtu.be)");
            string d_head = "Archive links for this discussion: \n\n";
            string p_head = "Archive links for this post: \n\n";
            string footer = "----\n\nI am Mnemosyne 2.0, ";
            string botsrights = "^^^^/r/botsrights";
            #endregion
            if(!File.Exists(@".\config.xml"))
            {
                Console.WriteLine("File doesn't exist, let's setup a config file");
                createNewPath();
            }
            DataStorage ReleventInfo = new DataStorage(@".\config.xml");
            //if(!File.Exists("D:\\RepliedToList.mdf")) //TODO: ADD THIS
            //{
            //    CreateDatabase();
            //}
            if (ReleventInfo.Password == "Y")
            {
                Console.WriteLine("Type in your password");
                ReleventInfo.Password = Console.ReadLine();
                Console.Clear();
            }
            Reddit reddit = null;
            if (Oauth != null)
            {
                reddit = new Reddit(Oauth);
            }
            else
            {
                reddit = new Reddit(WebAgent.RateLimitMode.Pace);
                reddit.LogIn(ReleventInfo.Username, ReleventInfo.Password);
            }
            reddit.InitOrUpdateUser();
            bool authenticated = (reddit.User != null);
            if (!authenticated)
            {
                Console.WriteLine("Invalid token");
            }
            createFiles();
            var sub = reddit.GetSubreddit(ReleventInfo.SubReddit);
            bool isMnemosyneThereAlready = false;
            string[] repliedTo = File.ReadAllLines(@".\Replied_To.txt");
            var repliedList = repliedTo.ToList();
            #region postChecking
            while (true)
            {
                try
                {
                    foreach (var post in sub.New.Take(ReleventInfo.ReqLimit))
                    {
                        if (exclude.IsMatch(post.Url.ToString()))
                        {
                            continue;
                        }
                        foreach (var comment in post.Comments)
                        {
                            if (comment.Author == "mnemosyne-0001")
                            {
                                isMnemosyneThereAlready = true; // check for the other bot, will add option for more later TODO: check other bots, inc, self
                                break;
                            }
                            System.Threading.Thread.Sleep(2000);
                        }
                        if (isMnemosyneThereAlready == true || repliedList.Contains(post.Id))
                        {
                            break;
                        }
                        archiveURL = Archive(@"archive.is", post.Url.ToString());
                        Console.WriteLine(archiveURL);
                        repliedList.Add(post.Id);
                        File.WriteAllLines(@".\Replied_To.txt", repliedList.ToArray());
                        if (archiveURL == null || archiveURL == "http://archive.is/submit/")
                        {
                            File.AppendAllText(@".\Failed.txt", "Failed to archive: " + post.Permalink + "\nurl: " + archiveURL + "\n");
                            continue;
                        }
                        // logic for which header needs to be posted
                        #region commentlogic
                        string head = post.IsSelfPost ? d_head : p_head;
                        string c = head + "* **Archive** " + archiveURL + "\n\n" + footer + ReleventInfo.FlavorText[random.Next(0, ReleventInfo.FlavorText.Length - 1)] + botsrights; //archive for a post or a discussion, archive, footer, flavortext, botsrights link
                        Console.WriteLine("waiting");
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
                        post.Comment(c);
                        Console.WriteLine(c);
                        #endregion
                    }
                }
                catch (Exception e)
                { 
                    File.AppendAllText(@".\Errors.txt", "Error: " + e.Message + "\n" + e.StackTrace + '\n');
                }
                Console.WriteLine("waiting for next batch");
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ReleventInfo.SleepTime));
            }
            #endregion

        }
        /// <summary>
        /// Creates our files
        /// </summary>
        static void createFiles()
        {
            if (!File.Exists(@".\Replied_To.txt"))
            {
                File.Create(@".\Replied_To.txt").Dispose();
            }
            if (!File.Exists(@".\Failed.txt"))
            {
                File.Create(@".\Failed.txt").Dispose();
            }
        }
        /// <summary>
        /// This creates a new config file at the location of .\config.xml
        /// </summary>
        static void createNewPath()
        {
            XmlWriter writer = null;
            Console.Clear();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true; // will indent
            settings.IndentChars = ("\t"); // tabs, because fuck spaces
            settings.OmitXmlDeclaration = true;
            try
            {
                using (writer = XmlWriter.Create(@".\config.xml", settings)) //this should be obvious
                {
                    #region writers
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Settings");
                    Console.WriteLine("So, what is your subreddit this bot will run on?");
                    string subreddit = Console.ReadLine();
                    writer.WriteStartElement("subreddit");
                    writer.WriteString(subreddit);
                    writer.WriteEndElement();
                    Console.WriteLine("What do you want the request limit to be?");
                    string reqlimit = Console.ReadLine();
                    writer.WriteStartElement("ReqLimit");
                    writer.WriteString(reqlimit);
                    writer.WriteEndElement();
                    Console.WriteLine("What is the sleep time?");
                    string sleepTime = Console.ReadLine();
                    writer.WriteStartElement("SleepTime");
                    writer.WriteString(sleepTime);
                    writer.WriteEndElement();
                    Console.WriteLine("What is your username?");
                    string username = Console.ReadLine();
                    writer.WriteStartElement("Username");
                    writer.WriteString(username);
                    writer.WriteEndElement();
                    Console.WriteLine("What about password? note: this is stored in plaintext, don't actually send out in a git or type Y (just \"Y\") to not use a password in the config, and require one on startup");
                    string password = Console.ReadLine();
                    writer.WriteStartElement("Password"); //Password feild, as OAuth hasn't been setup yet
                    writer.WriteString(password);
                    writer.WriteEndElement();
                    Console.WriteLine("You have to add flavortext manually after the fact, go into the config file and seperate each flavor text with a \"");
                    writer.WriteStartElement("flavortext");
                    writer.WriteEndElement();
                    Console.WriteLine("What's your Oauth token?");
                    string oAuth = Console.ReadLine();
                    writer.WriteStartElement("Oauth");
                    writer.WriteString(oAuth);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                    #endregion
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.ReadKey();
            }
        }
        ///// <summary>
        ///// TODO: MAKE THIS WORK
        ///// </summary>
        //static void CreateDatabase()
        //{
        //    string str;
        //    SqlConnection connection = new SqlConnection();
        //    str = "CREATE DATABASE RepliedTo ON PRIMARY " +
        //        "(NAME = RepliedTo_Data, " +
        //        "FILENAME = 'D:\\RepliedToList.mdf' " +
        //        "SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%) " +
        //        "FILENAME = 'D:\\RepliedToLog.ldf', " +
        //        "SIZE = 1MB, " +
        //        "MAXSIZE = 5MB, " +
        //        "FILEGROWTH = 10%)";
        //    SqlCommand command = new SqlCommand(str, connection);
        //    try
        //    {
        //        connection.Open();
        //        command.ExecuteNonQuery();
        //        Console.WriteLine("Database successfully created");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message + '\n' + e.StackTrace);
        //    }
        //    finally
        //    {
        //        if (connection.State == System.Data.ConnectionState.Open)
        //        {
        //            connection.Close();
        //        }
        //    }
        //}
        /// <summary>
        /// Gets the url of the Archive, goddamn once this is finished i will have no idea how this works
        /// </summary>
        /// <param name="serviceURL">Archiving service, generally archive.is</param>
        /// <param name="url">The url that we're archiving</param>
        /// <returns>the archive url</returns>
        static string Archive(string serviceURL, string url)
        {
            string archiveURL = null;
            HttpClientHandler handle = new HttpClientHandler();
            handle.AllowAutoRedirect = true;
            using (var client = new HttpClient(handle))
            {
                var values = new Dictionary<string, string>
                {
                    {"url", url }
                };
                var content = new FormUrlEncodedContent(values);
                serviceURL = "http://" + serviceURL + "/submit/";
                Console.WriteLine("Damnit");
                /// <summary>
                /// This puts a request to the archive site, so yhea...
                /// </summary>
                var response = client.PostAsync(serviceURL, content);
                var loc = response.Result;
                archiveURL = loc.RequestMessage.RequestUri.ToString();
                if (archiveURL == "http://archive.is/submit/")
                {
                    #region fixing it
                    StringReader reader = new StringReader(loc.ToString());
                    for (int i = 0; i < 3; i++)
                    {
                        reader.ReadLine();
                    }
                    string wanted = reader.ReadLine();
                    string[] sides = wanted.Split('=');
                    Console.WriteLine(sides[1]);
                    archiveURL = sides[1];
                    #endregion
                }
            }
            return archiveURL;
        }
    }
}
