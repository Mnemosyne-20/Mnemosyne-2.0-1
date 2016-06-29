using ArchiveLibrary;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mnemosyne_Of_Mine
{
    internal static class CommentArchiver
    {
        static Random random = new Random();
        /// <summary>
        /// Archives comments that contain links
        /// </summary>
        /// <param name="config">user config, see <see cref="UserData"/></param>
        /// <param name="ReplyDict">dictionary of replies for each post</param>
        /// <param name="reddit">the reddit<seealso cref="Reddit"/></param>
        /// <param name="comment">the comment to point to<seealso cref="Comment"/></param>
        /// <param name="FoundLinks">links found in the stuff lol</param>
        /// <param name="commentsSeenList">list of seen comments</param>
        /// <!--What the hell is the point of this, i'm adding this line just because, this is a fucking comment for no real reason-->
        internal static Tuple<Dictionary<string, string>, List<string>> ArchiveCommentLinks(UserData config, Dictionary<string, string> ReplyDict, Reddit reddit, Comment comment, List<string> FoundLinks, List<string> commentsSeenList) // not as bad now
        {
            List<string> ArchivedLinks = new List<string>();
            string commentID = comment.Id;
            if (comment.Id.Contains("t1_") || comment.LinkId.Contains("t1_"))
            {
                Console.WriteLine(comment.Id);
                Console.WriteLine(comment.LinkId);
                Console.WriteLine(comment.Id.Substring(3));
                Console.WriteLine(comment.LinkId.Substring(3));
                Console.ReadLine();
            }
            if (commentID.Contains("t1_"))
            {
                commentID = commentID.Split('_')[1];
            }
            string postID = comment.LinkId.Substring(3);
            foreach (string link in FoundLinks)
            {
                // foreach already handles empty collection case
                if (!Program.exclude.IsMatch(link))
                {
                    Console.WriteLine($"Found {link} in comment {commentID}");
                    string hostname = new Uri(link).Host.Replace("www.", "");
                    string commentLink = $"https://www.reddit.com/comments/{postID}/_/{comment.Id}"; // ugly way to get comment link
                    string archiveURL = Archiving.Archive(@"archive.is", link);
                    if (Archiving.VerifyArchiveResult(link, archiveURL))
                    {
                        ArchivedLinks.Add($"* **By [{comment.Author}]({commentLink})** ([{hostname}]({link})): {archiveURL}\n");
                    }
                }
            }
            if (ArchivedLinks.Count >= 1) // ensure bot does not post if list is empty (ex. archiving failed)
            {
                bool bHasPostITT = ReplyDict.ContainsKey(postID);
                if (bHasPostITT)
                {
                    Console.WriteLine($"Already have post in {postID}, getting comment {ReplyDict[postID]}");
                    string botCommentThingID = "t1_" + ReplyDict[postID];
                    Comment botComment = (Comment)reddit.GetThingByFullname(botCommentThingID);
                    EditArchiveListComment(botComment, ArchivedLinks);
                }
                else
                {
                    Console.WriteLine($"No comment in {postID} to edit, making new one");
                    Post post = (Post)reddit.GetThingByFullname(comment.LinkId);
                    ReplyDict = PostArchiveLinks(config, ReplyDict, Program.c_head, post, ArchivedLinks);
                }
                commentsSeenList.Add(commentID);
            }
            return new Tuple<Dictionary<string, string>, List<string>>(ReplyDict, commentsSeenList);
        }
        /// <summary>
        /// posts everything
        /// </summary>
        /// <param name="config">configuration for flavortext</param>
        /// <param name="ReplyDict">dictionary of replies</param>
        /// <param name="head">header</param>
        /// <param name="post">post to edit</param>
        /// <param name="ArchiveLinks">links of archives</param>
        /// <returns>dictoinary of replied to list, so that it updates</returns>
        internal static Dictionary<string, string> PostArchiveLinks(UserData config, Dictionary<string, string> ReplyDict, string head, Post post, List<string> ArchiveLinks)
        {
            //string head = post.IsSelfPost ? Program.d_head : Program.p_head;
            string LinksListBody = "";
            foreach (string str in ArchiveLinks)
            {
                LinksListBody += str;
            }
            string c = head
                + LinksListBody
                + "\n" + Program.footer
                + config.FlavorText[random.Next(0, config.FlavorText.Length - 1)]
                + Program.botsrights; //archive for a post or a discussion, archive, footer, flavortext, botsrights link
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
            Comment botComment = post.Comment(c);
            if (botComment.Id.Contains("t1_"))
            {
                botComment.Id = botComment.Id.Substring(3);
            }
            ReplyDict.Add(post.Id, botComment.Id);
            Console.WriteLine(c);
            return ReplyDict;
        }
        /// <summary>
        /// Edits the archive comment
        /// </summary>
        /// <param name="targetComment">comment to edit</param>
        /// <param name="ArchivesToInsert">archive links to insert</param>
        internal static void EditArchiveListComment(Comment targetComment, List<string> ArchivesToInsert)
        {
            if (ArchivesToInsert.Count > 0)
            {
                Console.Title = $"Editing comment {targetComment.Id}";
                bool bEditGood = false;
                string newCommentText = "";
                string[] oldCommentLines = targetComment.Body.Split("\n".ToArray(), StringSplitOptions.None);
                if (oldCommentLines.Length >= 1) // removed the null check, as that's pointless because if you have an element it's not null
                {
                    string[] head = oldCommentLines.Take(oldCommentLines.Length - 3).ToArray();
                    string[] tail = oldCommentLines.Skip(oldCommentLines.Length - 3).ToArray();
                    newCommentText += string.Join("\n", head);
                    if (head.Length >= 1)
                    {
                        if (head[head.Length - 1].StartsWith("* **By"))
                        {
                            foreach (string str in ArchivesToInsert)
                            {
                                newCommentText += "\n" + str;
                            }
                            bEditGood = true;
                        }
                        else if (head[head.Length - 1].StartsWith("* **Link"))
                        {
                            newCommentText += "\n\n----\nArchives for links in comments: \n\n";
                            foreach (string str in ArchivesToInsert)
                            {
                                newCommentText += str;
                            }
                            bEditGood = true;
                        }
                        else
                        {
                            throw new Exception($"Unexpected end of head: {head[head.Length - 1]}"); // more appropriate, as that's not supposed to happen
                        }
                        newCommentText += string.Join("\n", tail);
                    }
                }
                if (bEditGood)
                {
                    targetComment.EditText(newCommentText);
                }
            }
        }
        /// <summary>
        /// Reads the file where we track who we reply to
        /// </summary>
        /// <param name="file">file that we're using</param>
        /// <returns>dictionary of replys and comment ids</returns>
        internal static Dictionary<string, string> ReadReplyTrackingFile(string file)
        {
            Dictionary<string, string> replyDict = new Dictionary<string, string>();
            string fileIn = File.ReadAllText(file);
            string[] elements = fileIn.Split(new char[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < elements.Length; i += 2)
            {
                string postID = elements[i];
                string botCommentID = elements[i + 1];
                replyDict.Add(postID, botCommentID);
            }

            return replyDict;
        }
        /// <summary>
        /// Writes the reply tracking file
        /// </summary>
        /// <param name="replyDict">Dictionary of replies</param>
        internal static void WriteReplyTrackingFile(Dictionary<string, string> replyDict)
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string> reply in replyDict)
            {
                builder.Append(reply.Key + ':' + reply.Value + ',');
            }

            File.WriteAllText(@".\ReplyTracker.txt", builder.ToString(0, builder.Length - 1));
        }
    }
}
