using ArchiveLibrary;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mnemosyne_Of_Mine
{
    internal static class CommentArchiver
    {
        static Random random = new Random();
        /// <summary>
        /// Archives comments that contain links
        /// </summary>
        /// <param name="config">user config, see <see cref="UserData"/></param>
        /// <param name="reddit">the reddit<seealso cref="Reddit"/></param>
        /// <param name="comment">the comment to point to<seealso cref="Comment"/></param>
        /// <param name="FoundLinks">links found in the stuff lol</param>
        /// <!--What the hell is the point of this, i'm adding this line just because, this is a fucking comment for no real reason--> //  that thing to the left is here because it's pointless 
        internal static void ArchiveCommentLinks(UserData config, IBotStateTracker BotState, Reddit reddit, Comment comment, List<string> FoundLinks)
        {
            List<string> ArchivedLinks = new List<string>();
            string commentID = comment.Id;
            if (comment.Id.Contains("t1_") || comment.LinkId.Contains("t1_")) // Should NEVER ever EVER happen, hasn't yet
            {
                Console.WriteLine(comment.Id);
                Console.WriteLine(comment.LinkId);
                Console.WriteLine(comment.Id.Substring(3));
                Console.WriteLine(comment.LinkId.Substring(3));
                Console.ReadLine();
            }
            if (commentID.Contains("t1_"))
            {
                commentID = Regex.Replace(commentID, "t1_", "");
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
                    string archiveURL = Archiving.Archive(@"archive.is", link).Result;
                    if (Archiving.VerifyArchiveResult(link, archiveURL))
                    {
                        ArchivedLinks.Add($"* **By [{comment.Author}]({commentLink})** ([{hostname}]({link})): {archiveURL}\n");
                    }
                }
            }
            if (ArchivedLinks.Count >= 1) // ensure bot does not post if list is empty (ex. archiving failed)
            {
                bool bHasPostITT = BotState.DoesBotCommentExist(postID);
                if (bHasPostITT)
                {
                    string botCommentThingID = "t1_" + BotState.GetBotCommentForPost(postID);
                    Console.WriteLine($"Already have post in {postID}, getting comment {botCommentThingID.Substring(3)}");
                    Comment botComment = (Comment)reddit.GetThingByFullname(botCommentThingID);
                    EditArchiveListComment(botComment, ArchivedLinks);
                }
                else
                {
                    Console.WriteLine($"No comment in {postID} to edit, making new one");
                    Post post = (Post)reddit.GetThingByFullname(comment.LinkId);
                    PostArchiveLinks(config, BotState, Program.c_head, post, ArchivedLinks);
                }
                BotState.AddCheckedComment(commentID);
            }
        }
        /// <summary>
        /// posts everything
        /// </summary>
        /// <param name="config">configuration for flavortext</param>
        /// <param name="head">header</param>
        /// <param name="post">post to edit</param>
        /// <param name="ArchiveLinks">links of archives</param>
        internal static void PostArchiveLinks(UserData config, IBotStateTracker BotState, string head, Post post, List<string> ArchiveLinks)
        {
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
            string commentID = botComment.Id;
            if (commentID.Contains("t1_"))
            {
                commentID = commentID.Substring(3);
            }
            try
            {
                BotState.AddBotComment(post.Id, Regex.Replace(botComment.Id,"t1_",""));
                Console.WriteLine(c);
            }
            catch(InvalidOperationException ex)
            {
                Console.WriteLine($"Caught exception replying to {post.Id} with new comment {Regex.Replace(botComment.Id, "t1_", "")}: {ex.Message}");
                botComment.Del(); // likely will never happen but this should ensure things don't get dupey if things get dupey
            }
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
                if (oldCommentLines.Length >= 1)
                {
                    string[] head = oldCommentLines.Take(oldCommentLines.Length - 3).ToArray();
                    string[] tail = oldCommentLines.Skip(oldCommentLines.Length - 3).ToArray();
                    newCommentText += string.Join("\n", head);
                    if (head.Length >= 1)
                    {
                        if (head[head.Length - 1].StartsWith("* **By")) // a comment
                        {
                            foreach (string str in ArchivesToInsert)
                                newCommentText += "\n" + str;
                            bEditGood = true;
                        }
                        else if (head[head.Length - 1].StartsWith("* **Link")) // links in a post
                        {
                            newCommentText += "\n\n----\nArchives for links in comments: \n\n";
                            foreach (string str in ArchivesToInsert)
                                newCommentText += str;
                            bEditGood = true;
                        }
                        else if(head[head.Length - 1].StartsWith("* **Post")) // POST
                        {
                            newCommentText += "\n\n----\nArchives for links in comments: \n\n";
                            foreach (string str in ArchivesToInsert)
                                newCommentText += str;
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
    }
}
