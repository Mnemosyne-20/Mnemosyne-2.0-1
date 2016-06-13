using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mnemosyne_Of_Mine
{
    class CommentArchiver
    {
        static Random random = new Random();
        // this isn't anywhere near complete, usable, ready, or sanitary. do not ingest.
        internal static void ArchiveCommentLinks(UserData config, Dictionary<string, string> ReplyDict, Reddit reddit, Comment comment, Regex exclusions, List<string> commentsSeenList)
        {
            List<string> FoundLinks = LinkFinder.FindLinks(comment.BodyHtml);
            List<string> ArchivedLinks = new List<string>();
            string commentID = comment.Id;
            foreach (string link in FoundLinks)
            {
                // foreach already handles empty collection case
                if (!exclusions.IsMatch(link))
                {
                    Console.WriteLine($"Found {link} in comment {commentID}");
                    /*string archiveURL = Archiving.Archive(@"archive.is", link);
                    if (Archiving.VerifyArchiveResult(link, archiveURL))
                    {
                        string hostname = new Uri(link).Host;
                        ArchivedLinks.Add($"Placeholder Text: ({hostname}): {archiveURL}\n");
                    }*/
                    string hostname = new Uri(link).Host.Replace("www.", "");
                    ArchivedLinks.Add($"* **By [{comment.Author}]({comment.Shortlink})** ([{hostname}]({link})): Placeholder Text.\n"); //FIXME: comment.Shortlink is wrong
                }
            }
            string actualLinkID = comment.LinkId.Substring(3); // because apparently the link id starting with t3_ is intended for reasons but it's useless here
            bool bHasPostITT = ReplyDict.ContainsKey(actualLinkID);
            if (bHasPostITT)
            {
                Console.WriteLine($"Already have post in {actualLinkID}, getting comment {ReplyDict[actualLinkID]}");
                //Comment botComment = reddit.GetComment(config.SubReddit, actualLinkID, ReplyDict[actualLinkID]);
                string botCommentThingID = "t1_" + ReplyDict[actualLinkID]; // thanks redditsharp for having broken GetComment methods
                Comment botComment = (Comment)reddit.GetThingByFullname(botCommentThingID);
                EditArchiveListComment(botComment, ArchivedLinks);
            }
            else
            {
                Console.WriteLine($"No comment in {actualLinkID} to edit, making new one");
                PostArchiveLinks(config, ReplyDict, "Archives for links in comments:\n\n", "Archives for links in comments:\n\n", "", "", null, ArchivedLinks); // TODO: get post object from actualLinkID
            }
            commentsSeenList.Add(commentID);
        }

        internal static void PostArchiveLinks(UserData config, Dictionary<string, string> ReplyDict, string d_head, string p_head, string footer, string botsrights, Post post, List<string> ArchiveLinks) // not a fan of the params
        {
            string head = post.IsSelfPost ? d_head : p_head;
            string LinksListBody = "";
            foreach (string str in ArchiveLinks)
            {
                LinksListBody += str;
            }
            string c = head
                + LinksListBody
                + "\n" + footer
                + config.FlavorText[random.Next(0, config.FlavorText.Length - 1)]
                + botsrights; //archive for a post or a discussion, archive, footer, flavortext, botsrights link
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
            Comment botComment = post.Comment(c);
            ReplyDict.Add(post.Id, botComment.Id);
            Console.WriteLine(c);
        }

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
                    Console.WriteLine($"=====HEAD=====\n{ string.Join("\n",head) }\n=====TAIL=====");
                    string[] tail = oldCommentLines.Skip(oldCommentLines.Length - 3).ToArray();
                    Console.WriteLine($"{ string.Join("\n", tail) }\n==============");
                    newCommentText += string.Join("\n", head);
                    if (head.Length >= 1)
                    {
                        if (head[head.Length - 1].StartsWith("* **By"))
                        {
                            Console.WriteLine("Adding to comment archive list");
                            foreach (string str in ArchivesToInsert)
                            {
                                newCommentText += "\n" + str;
                            }
                            bEditGood = true;
                        }
                        else if (head[head.Length - 1].StartsWith("* **Link"))
                        {
                            Console.WriteLine("No existing comment archive list to add to, starting one");
                            newCommentText += "\n\n----\nArchives for links in comments: \n\n";
                            foreach (string str in ArchivesToInsert)
                            {
                                newCommentText += str;
                            }
                            bEditGood = true;
                        }
                        else
                        {
                            Console.WriteLine("Good job you can't even count right");
                        }
                        newCommentText += string.Join("\n", tail);
                    }
                    else
                    {
                        Console.WriteLine("Comment head somehow empty, ya blew it");
                    }
                }
                else
                {
                    Console.WriteLine("You can't even split the comment right what the shit");
                }
                if (bEditGood)
                {
                    targetComment.EditText(newCommentText);
                    Console.WriteLine(newCommentText);
                }
            }
            else
            {
                Console.WriteLine("Called to edit comment but got empty insertion list");
            }
        }

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
