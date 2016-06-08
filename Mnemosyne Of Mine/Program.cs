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
    static class Program
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
                logIn(reddit, ReleventInfo);
            }
            reddit.InitOrUpdateUser();
            bool authenticated = (reddit.User != null);
            if (!authenticated)
            {
                Console.WriteLine("Invalid token");
            }
            createFiles();
            var sub = reddit.GetSubreddit(ReleventInfo.SubReddit);
            var repostSub = reddit.GetSubreddit(ReleventInfo.Repost);
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
                        if(repliedList.Contains(post.Id))
                        {
                            break;
                        }
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
                        if (isMnemosyneThereAlready == true)
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
                        string c = head 
                            + "* **Archive** " 
                            + archiveURL 
                            + "\n\n" + footer 
                            + ReleventInfo.FlavorText[random.Next(0, ReleventInfo.FlavorText.Length - 1)]
                            + botsrights; //archive for a post or a discussion, archive, footer, flavortext, botsrights link
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
                Console.WriteLine("waiting for next batch from sub1");
                foreach(var post in repostSub.New.Take(10))
                {
                    if(repliedList.Contains(post.Id))
                    {
                        break;
                    }
                    if(post.IsSelfPost)
                    {
                        continue;
                    }
                    double repostPer = ReleventInfo.checkRepost(post.Title);
                    if(repostPer > .5 && post.Url.ToString().Contains("imgur"))
                    {
                        string comment = $"Your post had a {repostPer} similarity match for the title to the fake karly cross guys code vs girls code image\n\n----\n\n Please message /u/chugga_fan if this is incorrect, you can also ask for the source from him^^^^/r/botsrights";
                        post.Comment(comment);
                        Console.Write(comment);
                        repliedList.Add(post.Id);
                    }

                }
                Console.WriteLine("Repost check done");
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
            Console.Clear();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true; // will indent
            settings.IndentChars = ("\t"); // tabs, because fuck spaces
            settings.OmitXmlDeclaration = true;
            try
            {
                ConfigWriter.writeConfig(settings);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.ReadKey();
            }
        }
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
        static void logIn(Reddit reddit, DataStorage user)
        {
            reddit.LogIn(user.Username, user.Password);
        }
        static double checkRepost(this DataStorage storage, string title)
        {
            double perMatch = 0;
            string[] h = "The difference between girls and guys code programs".Split(' ');
            foreach (var word in h)
            {
                if (title.Contains(word))
                {
                    perMatch++;
                }
            }
            return (title.Split(' ').Length / perMatch) / 10;
        }
    }
}
