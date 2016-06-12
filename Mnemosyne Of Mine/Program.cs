using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using RedditSharp;
using System.Collections.Generic;
using ArchiveLibrary;
using RedditSharp.Things;
using System.Text;

namespace Mnemosyne_Of_Mine
{
    static class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.Write(System.String)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "RedditSharp.Things.Post.Comment(System.String)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "chuggafan")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "botsrights")]

        static Random random = new Random();
        static List<string> ArchiveBots = new List<string>()
        {
            "mnemosyne-0001",
            "mnemosyne-0002",
            "SpootsTestBot" // hey I know you!
        };

        static void Main(string[] args)
        {
            Console.Title = "Mnemosyne by chugga_fan, Archive AWAY!";
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
            if (ReleventInfo.bUseOAuth)
            {
                OAuthProvider = new AuthProvider(ReleventInfo.OAuthClientID, ReleventInfo.OAuthClientSecret, ReleventInfo.RedirectURI);
                if (ReleventInfo.Password == "Y")
                {
                    Console.WriteLine("Type in your password");
                    ReleventInfo.Password = Console.ReadLine();
                    Console.Clear();
                }
                OAuthToken = OAuthProvider.GetOAuthToken(ReleventInfo.Username, ReleventInfo.Password);
                reddit = new Reddit(OAuthToken);
            }
            else
            {
                if (ReleventInfo.Password == "Y")
                {
                    Console.WriteLine("Type in your password");
                    ReleventInfo.Password = Console.ReadLine();
                    Console.Clear();
                }
                reddit = new Reddit(WebAgent.RateLimitMode.Pace);
                reddit.logIn(ReleventInfo);
                reddit.InitOrUpdateUser();
            }
            bAuthenticated = (reddit.User != null);
            if (!bAuthenticated)
            {
                Console.WriteLine("User authentication failed");
            }
            createFiles();
            Subreddit sub = reddit.GetSubreddit(ReleventInfo.SubReddit);
            Subreddit repostSub;
            if (ReleventInfo.Repost != null && ReleventInfo.Repost != "")
            {
                repostSub = reddit.GetSubreddit(ReleventInfo.Repost);
            }
            bool isMnemosyneThereAlready = false;
            Dictionary<string, string> ReplyDict = ReadReplyTrackingFile(@".\ReplyTracker.txt");
            string[] commentsSeen = File.ReadAllLines(@".\Comments_Seen.txt");
            List<string> commentsSeenList = commentsSeen.ToList();
#region postChecking
            while (true)
            {
                Console.Title = "Checking sub: " + sub.Name;
                try
                {
                    foreach (var post in sub.New.Take(ReleventInfo.ReqLimit))
                    {
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
                            if (ArchiveBots.Contains(comment.Author))
                            {
                                isMnemosyneThereAlready = true; // check for the other bot, will add option for more later TODO: check other bots, inc, self
                                break;
                            }
                            System.Threading.Thread.Sleep(2000);
                        }
                        if (!isMnemosyneThereAlready || post.IsSelfPost)
                        {
                            List<string> ArchiveLinks = new List<string>();
                            bool bDoPostArchiving = false; // temp off switch for archiving self posts themselves
                            if(bDoPostArchiving)
                            {
                                string archiveURL = Archiving.Archive(@"archive.is", post.Url.ToString());
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
                                PostArchiveLinks(ReleventInfo, ReplyDict, d_head, p_head, footer, botsrights, post, ArchiveLinks);
                                WriteReplyTrackingFile(ReplyDict); // this should probably be done elsewhere
                            }
                        }
                    }
                    foreach (Comment comment in sub.Comments.Take(ReleventInfo.ReqLimit)) // not entirely happy on this, hopefully redditsharp throttles things on its own if needed
                    {
                        if (!commentsSeenList.Contains(comment.Id) && !ArchiveBots.Contains(comment.Author))
                        {
                            ArchiveCommentLinks(ReleventInfo, ReplyDict, comment, exclude, commentsSeenList);
                        }
                        File.WriteAllLines(@".\Comments_Seen.txt", commentsSeenList.ToArray()); // may be better to write this after the loop?
                    }
                    Console.Title = $"waiting for next batch from {sub.Name}";
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
                catch (System.Net.WebException) // I would prefer to find *why* this is even throwing at all
                {
                    OAuthProvider = new AuthProvider(ReleventInfo.OAuthClientID, ReleventInfo.OAuthClientSecret, ReleventInfo.RedirectURI);
                    OAuthToken = OAuthProvider.GetOAuthToken(ReleventInfo.Username, ReleventInfo.Password);
                    reddit = new Reddit(OAuthToken);
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
                Console.Title = "Sleeping";
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
            if (!File.Exists(@".\Comments_Seen.txt")) // this might end up being an absolutely terrible idea.
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
                        ++counter;
                    }
                }
                // putting counter increment here would fix the "Link X isn't the Xth link" situation when posts also have links that are excluded from archiving
            }
            return ArchiveLinks;            
        }

        // this isn't anywhere near complete, usable, ready, or sanitary. do not ingest.
        static void ArchiveCommentLinks(UserData config, Dictionary<string,string> ReplyDict, Comment comment, Regex exclusions, List<string> commentsSeenList)
        {
            List<string> FoundLinks = LinkFinder.FindLinks(comment.BodyHtml);
            List<string> ArchivedLinks = new List<string>();
            string commentID = comment.Id;
            foreach (string link in FoundLinks)
            {
                // foreach already handles empty collection case
                if (!exclusions.IsMatch(link))
                {
                    if (!commentsSeenList.Contains(commentID))
                    {
                        string actualLinkID = comment.LinkId.Substring(3); // because apparently the link id starting with t3_ is intended for reasons but it's useless here
                        bool bHasPostITT = ReplyDict.ContainsKey(actualLinkID);
                        if (bHasPostITT)
                        {
                            Console.WriteLine($"Found {link} in comment {commentID} and I have a post in {actualLinkID}");
                            /*string archiveURL = Archiving.Archive(@"archive.is", link);
                            if (Archiving.VerifyArchiveResult(link, archiveURL))
                            {
                                string hostname = new Uri(link).Host;
                                ArchivedLinks.Add($"Placeholder Text: ({hostname}): {archiveURL}\n");
                            }*/
                        }
                    }
                }
            }
            commentsSeenList.Add(commentID);
        }

        static void PostArchiveLinks(UserData config, Dictionary<string, string> ReplyDict, string d_head, string p_head, string footer, string botsrights, Post post, List<string> ArchiveLinks) // not a fan of the params
        {
            string head = post.IsSelfPost ? d_head : p_head;
            string LinksListBody = "";
            foreach (string str in ArchiveLinks)
            {
                LinksListBody += str + "\n";
            }
            string c = head
                + LinksListBody
                + "\n\n" + footer
                + config.FlavorText[random.Next(0, config.FlavorText.Length - 1)]
                + botsrights; //archive for a post or a discussion, archive, footer, flavortext, botsrights link
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
            Comment botComment = post.Comment(c);
            ReplyDict.Add(post.Id, botComment.Id);
            Console.WriteLine(c);
        }

        static void EditArchiveListComment()
        {

        }

        static Dictionary<string,string> ReadReplyTrackingFile(string file)
        {
            Dictionary<string, string> replyDict = new Dictionary<string, string>();
            string fileIn = File.ReadAllText(file);
            string[] elements = fileIn.Split(new char[]{':',','}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < elements.Length; i += 2)
            {
                string postID = elements[i];
                string botCommentID = elements[i + 1];
                replyDict.Add(postID, botCommentID);                
            }

            return replyDict;
        }

        static void WriteReplyTrackingFile(Dictionary<string,string> replyDict)
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string,string> reply in replyDict)
            {
                builder.Append(reply.Key);
                builder.Append(':');
                builder.Append(reply.Value);
                builder.Append(',');
            }

            File.WriteAllText(@".\ReplyTracker.txt", builder.ToString(0, builder.Length - 1));
        }
    }
}
