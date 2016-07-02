using ArchiveLibrary;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Mnemosyne_Of_Mine
{
    static class Program
    {

        static List<string> ArchiveBots = new List<string>()
        {
            "mnemosyne-0001",
            "mnemosyne-0002",// I've seen you!
            "SpootsTestBot", // hey I know you!
            "Mentioned_Videos",
            "AutoModerator",
            "TotesMessenger",
            "TweetPoster"
        };
        #region constants
        internal static Regex exclude = new Regex(@"(youtube.com|archive.is|web.archive.org|webcache.googleusercontent.com|youtu.be|wiki/rules|politics_feedback_results_and_where_it_goes_from)");
        internal static string d_head = "Archives for links in this post: \n\n";
        internal static string p_head = "Archive for this post: \n\n";
        internal static string c_head = "Archives for links in comments: \n\n";
        internal static string footer = "----\nI am Mnemosyne 2.0, ";
        internal static string botsrights = "^^^^/r/botsrights ^^^^[Contribute](https://github.com/chuggafan/Mnemosyne-2.0-1) ^^^^[Website](https://mnemosyne-20.github.io/Mnemosyne-2.0-1/)";
        internal static string[] AnnoyCuckbot = { "Fun Fact!\n\nIn male feminist culture, it is [a great compliment](https://anonmgur.com/up/94297d3df43d5d4829141eafce3f127a.png) to be called [\"beta as fuck\"](https://www.youtube.com/watch?v=XCkEdB87yGA).","This is a public service announcement by Hipsterdom Who's Who.\n\nThe culmination of feminist courtship is the marriage ceremony, [after exchanging vows the happy couple share a platonic hug before the female goes off to mate with all the other attendees while the male cries over his water gun collection](https://www.youtube.com/watch?v=XshS_6evfp4).", "Did you know [female feminists often regard male feminist as creepy sex offenders](https://anonmgur.com/up/e4f611022fe46e6548d8b03170070cde.png)? Now you do, and knowing is half the battle!", "ALERT!\n\nAttempting to keep a wife/girlfriend from exploring her sexuality with friends, co-workers, neighbors, mail/milkmen, random strangers in bars/clubs, or crack addicts living under a bridge is unfeminist!\n\nCuckoldry is mandatory for feminism! Non-feminism is literally rape! Report all known non-cucks to law enforcement immediately!"};
        #endregion
        static void Main(string[] args)
        {
            Console.Title = "Mnemosyne by chugga_fan and Lord_Spoot, Archive AWAY!";
            if (!File.Exists(@".\config.xml"))
            {
                Console.WriteLine("File doesn't exist, let's setup a config file");
                createNewConfig();
            }
            UserData ReleventInfo = new UserData(@".\config.xml");
#if SQL
            if (!File.Exists(@".\RepliedToList.mdf")) //TODO: ADD THIS
            {
                    Sql.CreateDatabase();
            }
#endif
            Reddit reddit;
            AuthProvider OAuthProvider;
            string OAuthToken = "";
            bool bAuthenticated = false;
            bool newMessages = false;
            #region password and OAuth
            if (ReleventInfo.Password == "Y")
            {
                Console.WriteLine("Type in your password");
                ReleventInfo.Password = Console.ReadLine();
                Console.Clear();
            }
            if (ReleventInfo.bUseOAuth)
            {
                OAuthProvider = new AuthProvider(ReleventInfo.OAuthClientID, ReleventInfo.OAuthClientSecret, ReleventInfo.RedirectURI);
                OAuthToken = OAuthProvider.GetOAuthToken(ReleventInfo.Username, ReleventInfo.Password);
                reddit = new Reddit(OAuthToken);
            }
            else
            {
                reddit = new Reddit(WebAgent.RateLimitMode.Pace);
                reddit.logIn(ReleventInfo);
                reddit.InitOrUpdateUser();
            }
            bAuthenticated = (reddit.User != null);
            if (!bAuthenticated)
            {
                Console.WriteLine("User authentication failed");
            }
            #endregion
            createFiles();
            Subreddit sub = reddit.GetSubreddit(ReleventInfo.SubReddit); // TODO: handle exceptions when reddit is under heavy load and fecal matter hits the rotary impeller
            Subreddit repostSub;                                         // LOL AT THIS ^
            if (ReleventInfo.Repost != null && ReleventInfo.Repost != "")
            {
                repostSub = reddit.GetSubreddit(ReleventInfo.Repost);
            }
#pragma warning disable CS0219 // Variable is assigned but its value is never used
            bool isMnemosyneThereAlready = false; // ignore visual studio complaining about this
#pragma warning restore CS0219 // Variable is assigned but its value is never used
            Dictionary<string, string> ReplyDict = CommentArchiver.ReadReplyTrackingFile(@".\ReplyTracker.txt");
            string[] commentsSeen = File.ReadAllLines(@".\Comments_Seen.txt");
            List<string> commentsSeenList = commentsSeen.ToList();
            bool bDoPostArchiving = false; // temp off switch for archiving self posts themselves
            #region postChecking
            while (true)
            {
                Console.Title = $"Checking sub: {sub.Name} New messages: {newMessages}";
                try
                {
                    if (reddit.User.UnreadMessages.Count() >= 1)
                    {
                        newMessages = true;
                    }
                    else
                    {
                        newMessages = false;
                    }
                    foreach (var post in sub.New.Take(ReleventInfo.ReqLimit))
                    {
                        Console.Title = $"Finding posts in {sub.Name}";
                        if (ReplyDict.ContainsKey(post.Id))
                        {
                            break;
                        }
                        if (exclude.IsMatch(post.Url.ToString()))
                        {
                            continue;
                        }
                        foreach (var comment in post.Comments)
                        {
                            if(!bDoPostArchiving)
                            {
                                break;
                            }
                            if (ArchiveBots.Contains(comment.Author) && !post.IsSelfPost)
                            {
                                isMnemosyneThereAlready = true; // check for the other bot, will add option for more later TODO: check other bots, inc, self
                                break;
                            }
                            System.Threading.Thread.Sleep(2000);
                        }
                        if (post.IsSelfPost)
                        {
                            if(post.SelfTextHtml == null)
                            {
                                continue;
                            }
                            List<string> ArchiveLinks = new List<string>();
                            ///<summary>
                            ///This checks if we should archive or not based on ITSigno yelling at me
                            /// </summary>
                            if(bDoPostArchiving)
                            {
                                string archiveURL = Archiving.Archive(@"archive.is", post.Url.ToString());
                                if(archiveURL.Contains("archive.is"))
                                {
                                    Console.WriteLine(archiveURL);
                                    Console.ReadLine();
                                }
                                if (Archiving.VerifyArchiveResult(post.Permalink.ToString(), archiveURL))
                                {
                                    ArchiveLinks.Add($"* **Post** {archiveURL}\n");
                                }
                            }
                            List<string> FoundLinks = LinkFinder.FindLinks(post.SelfTextHtml);
                            if (FoundLinks.Count >= 1)
                            {
                                ArchiveLinks.AddRange(ArchivePostLinks(ReleventInfo, FoundLinks, exclude));                                
                            }
                            if (ArchiveLinks.Count >= 1)
                            {
                                ReplyDict = CommentArchiver.PostArchiveLinks(ReleventInfo, ReplyDict, d_head, post, ArchiveLinks);
                                CommentArchiver.WriteReplyTrackingFile(ReplyDict); // this should probably be done elsewhere
                            }
                        }
                    }
                    ///<summary>
                    ///grabs new comments, may need to see if we need to do changes to the ReqLimit to be able to do this, as i currently have a low one of 30
                    ///</summary>
                    foreach (Comment comment in sub.Comments.Take(ReleventInfo.ReqLimit))
                    {
#if ANNOYTHECUCKBOT
                        if (comment.Author == "CuckyMcCuckFace" && !commentsSeenList.Contains(comment.Id))
                        {
                            Random rand = new Random();
                            comment.Reply(AnnoyCuckbot[rand.Next(3)]);
                            commentsSeenList.Add(comment.Id);
                            continue;
                        }
#endif
                        if (ArchiveBots.Contains(comment.Author))
                        {
                            continue;
                        }
                        List<string> FoundLinks = LinkFinder.FindLinks(comment.BodyHtml);
                        if (FoundLinks.Count >= 1) // should fix empty comments bug
                        {
                            if (!commentsSeenList.Contains(comment.Id) && !ArchiveBots.Contains(comment.Author))
                            {
                                var ass = CommentArchiver.ArchiveCommentLinks(ReleventInfo, ReplyDict, reddit, comment, FoundLinks, commentsSeenList);
                                ReplyDict = ass.Item1;
                                commentsSeenList = ass.Item2;
                                File.WriteAllLines(@".\Comments_Seen.txt", commentsSeenList.ToArray());

                            }
                        }
                        CommentArchiver.WriteReplyTrackingFile(ReplyDict);
                    }
                    Console.Title = $"waiting for next batch from {sub.Name} New messages: {newMessages}";
#if REPOSTCHECK
                    if(repostSub != null)
                    {
                        foreach (var post in repostSub.New.Take(10))
                        {
                            Console.Title = "Checking sub: " + post.SubredditName + " for reposts";
                            if (!post.Url.ToString().Contains("imgur") || post.IsSelfPost)
                            {
                                continue;
                            }
                            if (repliedList.Contains(post.Id))
                            {
                                break;
                            }
                            double repostPer = post.checkRepost();
                            if (repostPer > .5 && !double.IsInfinity(repostPer))
                            {
                                string comment = $"Your post had a {repostPer} similarity match for the title to the fake karly cross guys code vs girls code image\n\n----\n\n Please message /u/chugga_fan if this is incorrect, you can also ask for the source from him^^^^/r/botsrights";
                                //post.Comment(comment);
                                Console.Write(comment);
                                repliedList.Add(post.Id);
                            }
                        }
                    }
#endif
                }
                catch (System.Net.WebException) // I would prefer to find *why* this is even throwing at all // known reason it's throwing, i failed to verify account, besides, this also works to get a new token when token fails
                {
                    if (ReleventInfo.bUseOAuth)
                    {
                        OAuthProvider = new AuthProvider(ReleventInfo.OAuthClientID, ReleventInfo.OAuthClientSecret, ReleventInfo.RedirectURI);
                        OAuthToken = OAuthProvider.GetOAuthToken(ReleventInfo.Username, ReleventInfo.Password);
                        reddit = new Reddit(OAuthToken);
                    }
                        reddit.InitOrUpdateUser();
                }
                catch (FailureToArchiveException ex)
                {
                    File.AppendAllText(@".\Failed.txt", ex.Message + '\n');
                }
                catch (Exception e)
                {
                    File.AppendAllText(@".\Errors.txt", $"Error: {e.Message}\n{e}");
                }
#if REPOSTCHECK
                Console.WriteLine("Repost check done");
#endif

                Console.Title = $"Sleeping New messages: {newMessages}";
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ReleventInfo.SleepTime));
#endregion
            }
        }
        /// <summary>
        /// Creates our files
        /// </summary>
        static void createFiles()
        {
            if (!File.Exists(@".\ReplyTracker.txt"))
            {
                File.Create(@".\ReplyTracker.txt").Dispose();
            }
            if (!File.Exists(@".\Failed.txt"))
            {
                File.Create(@".\Failed.txt").Dispose();
            }
            if (!File.Exists(@".\Comments_Seen.txt")) // this might end up being an absolutely terrible idea. //  I've got 300 gbs of storage space and can get a lot of decomissioned but not degaussed hardrives, it's fine
            {
                File.Create(@".\Comments_Seen.txt").Dispose();
            }
        }
        /// <summary>
        /// This creates a new config file at the location of .\config.xml
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
        /// % likelyhood match
        /// </summary>
        /// <param name="post">Post from redditsharp see <seealso cref="RedditSharp.Things.Post"/></param>
        /// <returns>a percent match to the annoying post</returns>
        static double checkRepost(this RedditSharp.Things.Post post)
        {
            double perMatch = 0;
            string[] h = "difference between girls and guys code programs".Split(' ');
            foreach (var word in h)
            {
                if (post.Title.Contains(word))
                {
                    perMatch++;
                }
            }
            return (post.Title.Split(' ').Length / perMatch) / 10;
        }
        /// <summary>
        /// Archives all links in a post
        /// </summary>
        /// <param name="config">userconfig</param>
        /// <param name="FoundLinks">links found by the linkfinder</param>
        /// <param name="exclusions">exclusions from archiving</param>
        /// <returns>archives</returns>
        static List<string> ArchivePostLinks(UserData config, List<string> FoundLinks, Regex exclusions)
        {
            List<string> ArchiveLinks = new List<string>();            
            int counter = 1;
            foreach (string link in FoundLinks)
            {
                if (!exclusions.IsMatch(link))
                {
                    string archiveURL = Archiving.Archive(@"archive.is", link);
                    if (Archiving.VerifyArchiveResult(link, archiveURL))
                    {
                        string hostname = new Uri(link).Host.Replace("www.","");
                        ArchiveLinks.Add($"* **Link: {counter.ToString()}** ([{hostname}]({link})): {archiveURL}\n");
                    }
                }
                ++counter;
            }
            return ArchiveLinks;            
        }
    }
}
