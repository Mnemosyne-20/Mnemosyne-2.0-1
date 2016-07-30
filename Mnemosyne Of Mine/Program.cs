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
            "TweetPoster",
            "RemindMeBot"
        };
        #region constants
        internal static Regex exclude = new Regex(@"(gyazo.com|sli.mg|imgur.com|youtube.com|archive.today|archive.is|web.archive.org|webcache.googleusercontent.com|youtu.be|wiki/rules|politics_feedback_results_and_where_it_goes_from)");
        internal static string d_head = "Archives for links in this post: \n\n";
        internal static string p_head = "Archive for this post: \n\n";
        internal static string c_head = "Archives for links in comments: \n\n";
        internal static string footer = "----\nI am Mnemosyne 2.0, ";
        internal static string botsrights = "^^^^/r/botsrights ^^^^[Contribute](https://github.com/chuggafan/Mnemosyne-2.0-1) ^^^^[Website](https://mnemosyne-20.github.io/Mnemosyne-2.0-1/)";
        #endregion

        static void Main(string[] args)
        {
            bool readyToDeploy = true;
            Console.Title = "Mnemosyne by chugga_fan and Lord_Spoot, Archive AWAY!";
            if (!File.Exists(@"./config.xml"))
            {
                Console.WriteLine("Config file doesn't exist, let's setup a config file");
                createNewConfig();
            }
            UserData ReleventInfo = new UserData(@"./config.xml");
            createFiles();
            IBotStateTracker BotState = null;
            if (false)
            {
                BotState = new FlatFileBotStateTracker();
            }
            else
            {
                BotState = new SQLBotStateTracker();
            }
            Reddit reddit;
            AuthProvider OAuthProvider;
			WebAgent RedditWebAgent = new WebAgent ();
            string OAuthToken = "";
            bool bAuthenticated = true;
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
                OAuthProvider = new AuthProvider(ReleventInfo.OAuthClientID, ReleventInfo.OAuthClientSecret, ReleventInfo.RedirectURI, RedditWebAgent);
                RedditWebAgent.AccessToken = OAuthProvider.GetOAuthToken(ReleventInfo.Username, ReleventInfo.Password);
                Console.WriteLine("OAuth Token: " + RedditWebAgent.AccessToken);
                reddit = new Reddit(RedditWebAgent);
                WebAgent.RootDomain = "oauth.reddit.com";
                reddit.InitOrUpdateUser();
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
            Subreddit sub = reddit.GetSubreddit(ReleventInfo.SubReddit); // TODO: handle exceptions when reddit is under heavy load and fecal matter hits the rotary impeller
            Subreddit repostSub;                                         // LOL AT THIS ^
            if (ReleventInfo.Repost != null && ReleventInfo.Repost != "")
            {
                repostSub = reddit.GetSubreddit(ReleventInfo.Repost);
            }
#pragma warning disable CS0219 // Variable is assigned but its value is never used
            bool isMnemosyneThereAlready = false; // ignore visual studio complaining about this
#pragma warning restore CS0219 // Variable is assigned but its value is never used
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
                        if (BotState.DoesBotCommentExist(post.Id))
                        {
                            break;
                        }
                        if (new Regex("(archive.today|archive.is|youtube.com|youtu.be|webcache.googleusercontent.com|web.archive.org)").IsMatch(post.Url.ToString()))
                        {
                            continue;
                        }
                        foreach (var comment in post.Comments)
                        {
                            if (!bDoPostArchiving)
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
                            if (post.SelfTextHtml == null)
                            {
                                continue;
                            }
                            List<string> ArchiveLinks = new List<string>();
                            ///<summary>
                            ///This checks if we should archive or not based on ITSigno yelling at me
                            /// </summary>
                            if (bDoPostArchiving)
                            {
                                string archiveURL = Archiving.Archive(@"archive.is", post.Url.ToString()).Result;
                                if (readyToDeploy)
                                {
                                    if (!exclude.IsMatch(post.Url.ToString()))
                                    {
                                        BotState.AddArchiveCount(post.Url.ToString());
                                    }
                                }
                                if (Archiving.VerifyArchiveResult(post.Permalink.ToString(), archiveURL))
                                {
                                    ArchiveLinks.Add($"* **Post due to 0001 not working currently** {archiveURL}\n");
                                }
                            }
                            List<string> FoundLinks = LinkFinder.FindLinks(post.SelfTextHtml);
                            if (FoundLinks.Count >= 1)
                            {
                                    if (readyToDeploy)
                                    {
                                        foreach (var link in FoundLinks) { if (!exclude.IsMatch(link)) BotState.AddArchiveCount(link); }
                                    }
                                    ArchiveLinks.AddRange(ArchivePostLinks(FoundLinks, exclude));
                            }
                            if (ArchiveLinks.Count >= 1)
                            {
                                CommentArchiver.PostArchiveLinks(ReleventInfo, BotState, d_head, post, ArchiveLinks);
                            }
                        }
                    }
                    ///<summary>
                    /// grabs new comments, may need to see if we need to do changes to the ReqLimit to be able to do this, as i currently have a low one of 30
                    ///</summary>
                    foreach (Comment comment in sub.Comments.Take(ReleventInfo.ReqLimit))
                    {
                        if (ArchiveBots.Contains(comment.Author) || BotState.HasCommentBeenChecked(comment.Id))
                        {
                            continue;
                        }
                        List<string> FoundLinks = LinkFinder.FindLinks(comment.BodyHtml);
                        if (FoundLinks.Count >= 1 && !BotState.HasCommentBeenChecked(comment.Id))
                        {
                            if (readyToDeploy)
                            {
                                foreach (var link in FoundLinks) { if (!exclude.IsMatch(link)) BotState.AddArchiveCount(link); }
                            }
                            CommentArchiver.ArchiveCommentLinks(ReleventInfo, BotState, reddit, comment, FoundLinks);
                        }
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
                            if (BotState.DoesBotCommentExist(post.Id))
                            {
                                break;
                            }
                            double repostPer = post.checkRepost();
                            if (repostPer > .5 && !double.IsInfinity(repostPer))
                            {
                                string comment = $"Your post had a {repostPer} similarity match for the title to the fake karly cross guys code vs girls code image\n\n----\n\n Please message /u/chugga_fan if this is incorrect, you can also ask for the source from him^^^^/r/botsrights";
                                //Comment botComment = post.Comment(comment);
                                Console.Write(comment);
                                BotState.AddBotComment(post.Id, ""); // plug botComment.Id in here when/if you use this
                            }
                        }
                    }
#endif
                }
                #region errors
                catch (System.Net.WebException) // I would prefer to find *why* this is even throwing at all // known reason it's throwing, i failed to verify account, besides, this also works to get a new token when token fails
                {
                    try
                    {
                        if (ReleventInfo.bUseOAuth)
                        {
                            OAuthToken = new AuthProvider(ReleventInfo.OAuthClientID, ReleventInfo.OAuthClientSecret, ReleventInfo.RedirectURI).GetOAuthToken(ReleventInfo.Username,ReleventInfo.Password);
                            reddit = new Reddit(OAuthToken);
                        }

                        reddit.InitOrUpdateUser();
                    }
                    catch
                    {

                    }
                }
                catch (ArchiveLibrary.FailureToArchiveException ex)
                {
                    File.AppendAllText(@"./Failed.txt", ex.Message + '\n');
                }
                catch (Exception e)
                {
                    File.AppendAllText(@"./Errors.txt", $"Error: {e.Message}\n{e}");
                }
                #endregion
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
            if (!File.Exists(@"./ReplyTracker.txt"))
            {
                File.Create(@"./ReplyTracker.txt").Dispose();
            }
            if (!File.Exists(@"./Failed.txt"))
            {
                File.Create(@"./Failed.txt").Dispose();
            }
            if (!File.Exists(@"./Comments_Seen.txt")) // this might end up being an absolutely terrible idea. //  I've got 300 gbs of storage space and can get a lot of decomissioned but not degaussed hardrives, it's fine
            {
                File.Create(@"./Comments_Seen.txt").Dispose();
            }
            if(!File.Exists(@"./ArchiveCount.txt"))
            {
                File.Create(@"./ArchiveCount.txt").Dispose();
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
        /// Archives all links in a post
        /// </summary>
        /// <param name="FoundLinks">links found by the linkfinder</param>
        /// <param name="exclusions">exclusions from archiving</param>
        /// <returns>archives</returns>
        static List<string> ArchivePostLinks(List<string> FoundLinks, Regex exclusions)
        {
            List<string> ArchiveLinks = new List<string>();
            int counter = 1;
            foreach (string link in FoundLinks)
            {
                if (!exclusions.IsMatch(link))
                {
                    string archiveURL = Archiving.Archive(@"archive.is", link).Result;
                    if (Archiving.VerifyArchiveResult(link, archiveURL))
                    {
                        string hostname = new Uri(link).Host.Replace("www.", "");
                        ArchiveLinks.Add($"* **Link: {counter.ToString()}** ([{hostname}]({link})): {archiveURL}\n");
                    }
                }
                ++counter;
            }
            return ArchiveLinks;
        }
    }
}
