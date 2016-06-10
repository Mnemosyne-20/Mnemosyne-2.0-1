using System;
using System.Linq;
using System.IO;
using System.Xml;
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
            if (!File.Exists(@".\config.xml"))
            {
                Console.WriteLine("File doesn't exist, let's setup a config file");
                createNewConfig();
            }
            UserData ReleventInfo = new UserData(@".\config.xml");
            if (!File.Exists(@".\RepliedToList.mdf")) //TODO: ADD THIS
            {
                try
                {
                    Sql.CreateDatabase();
                }
                catch
                {
                    // ignore, as not implemented
                }
            }
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
                reddit.logIn(ReleventInfo);
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
                        if (repliedList.Contains(post.Id))
                        {
                            break;
                        }
                        if (exclude.IsMatch(post.Url.ToString()))
                        {
                            continue;
                        }
                        foreach (var comment in post.Comments)
                        {
                            if (comment.Author == "mnemosyne-0001" || comment.Author == ReleventInfo.Username)
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
                        archiveURL = ArchiveMethods.Archive(@"archive.is", post.Url.ToString());
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
                    Console.WriteLine("waiting for next batch from sub1");
                    foreach (var post in repostSub.New.Take(10))
                    {
                        if (repliedList.Contains(post.Id))
                        {
                            break;
                        }
                        if (post.IsSelfPost)
                        {
                            continue;
                        }
                        double repostPer = post.checkRepost();
                        if (repostPer > .5 && post.Url.ToString().Contains("imgur") && !double.IsInfinity(repostPer))
                        {
                            string comment = $"Your post had a {repostPer} similarity match for the title to the fake karly cross guys code vs girls code image\n\n----\n\n Please message /u/chugga_fan if this is incorrect, you can also ask for the source from him^^^^/r/botsrights";
                            post.Comment(comment);
                            Console.Write(comment);
                            repliedList.Add(post.Id);
                        }
                    }
                }
                catch (Exception e)
                {
                    File.AppendAllText(@".\Errors.txt", "Error: " + e.Message + "\n" + e.StackTrace + '\n');
                }
                Console.WriteLine("Repost check done");
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ReleventInfo.SleepTime));
                #endregion
            }
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
        static void createNewConfig()
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
        /// logs you in, simpler for the UserData method
        /// </summary>
        /// <param name="reddit">the reddit object</param>
        /// <param name="user">The user data storage from the config file</param>
        static void logIn(this Reddit reddit, UserData user)
        {
            reddit.LogIn(user.Username, user.Password);
        }
        /// <summary>
        /// This is there for checking the % chance that /r/programmerhumor has a repost
        /// </summary>
        /// <param name="storage">the userdata storage you have</param>
        /// <param name="title"></param>
        /// <returns></returns>
        static double checkRepost(this RedditSharp.Things.Post post)
        {
            double perMatch = 0;
            string[] h = "The difference between girls and guys code programs".Split(' ');
            foreach (var word in h)
            {
                if (post.Title.Contains(word))
                {
                    perMatch++;
                }
            }
            return (post.Title.Split(' ').Length / perMatch) / 10;
        }
    }
}
