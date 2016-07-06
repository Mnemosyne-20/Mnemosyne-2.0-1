using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mnemosyne_Of_Mine
{
    class FlatFileBotStateTracker : IBotStateTracker
    {
        string replyTrackerFilePath = @".\ReplyTracker.txt";
        string checkedCommentsFilePath = @".\Comments_Seen.txt";
        string archivesTrackerFilePath = @".\ArchiveTracker.txt";

        static Dictionary<string, string> BotComments = new Dictionary<string, string>();
        static List<string> CheckedComments = new List<string>();
        static Dictionary<string, string> Archives = new Dictionary<string, string>();

        public FlatFileBotStateTracker()
        {
            BotComments = ReadReplyTrackingFile(replyTrackerFilePath);
            CheckedComments = File.ReadAllLines(checkedCommentsFilePath).ToList();
            //Archives = ReadArchivesTrackingFile(archivesTrackerFilePath);
        }

        /// <summary>
        /// Checks if bot already has a comment in specified post
        /// </summary>
        /// <param name="postID">Post ID</param>
        public bool DoesBotCommentExist(string postID)
        {
            return BotComments.ContainsKey(postID);
        }

        /// <summary>
        /// Gets ID of bot's comment in specified post
        /// </summary>
        /// <param name="postID">Post ID</param>
        /// <returns>The bot's comment ID</returns>
        public string GetBotCommentForPost(string postID)
        {
            return BotComments[postID];
        }

        /// <summary>
        /// Checks if bot has already seen and parsed the specified comment
        /// </summary>
        /// <param name="commentID">Comment ID</param>
        public bool HasCommentBeenChecked(string commentID)
        {
            return CheckedComments.Contains(commentID);
        }

        /// <summary>
        /// Add comment to bot's reply tracker
        /// </summary>
        /// <param name="postID">ID of post bot has commented on</param>
        /// <param name="commentID">Bot comment ID</param>
        public void AddBotComment(string postID, string commentID)
        {
            BotComments.Add(postID, commentID);
            AppendReplyTrackingFile(postID, commentID);
        }

        /// <summary>
        /// Add comment to checked comments tracker
        /// </summary>
        /// <param name="commentID">Comment ID</param>
        public void AddCheckedComment(string commentID)
        {
            CheckedComments.Add(commentID);
            File.AppendAllText(checkedCommentsFilePath, $"{commentID}{Environment.NewLine}");
        }

        public bool IsURLAlreadyArchived(string url)
        {
            return Archives.ContainsKey(url);
        }

        public string GetArchiveForURL(string url)
        {
            if (Archives.ContainsKey(url))
            {
                return Archives[url];
            }
            else return "";
        }

        public void AddArchiveForURL(string originalURL, string archiveURL)
        {
            Archives.Add(originalURL, archiveURL);
            AppendArchiveTrackingFile(originalURL, archiveURL);
        }

        /// <summary>
        /// Reads the file where we track who we reply to
        /// </summary>
        /// <param name="file">file that we're using</param>
        /// <returns>dictionary of replys and comment ids</returns>
        Dictionary<string, string> ReadReplyTrackingFile(string file)
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
        /// Appends to reply tracking file
        /// </summary>
        /// <param name="postID">ID of post bot has commented on</param>
        /// <param name="commentID">Bot comment ID</param>
        void AppendReplyTrackingFile(string postID, string commentID)
        {
            string appendStr = postID + ":" + commentID;
            if (new FileInfo(replyTrackerFilePath).Length > 0)
            {
                appendStr = "," + appendStr;
            }
            File.AppendAllText(replyTrackerFilePath, appendStr);
        }

        Dictionary<string, string> ReadArchivesTrackingFile(string file)
        {
            Dictionary<string, string> archives = new Dictionary<string, string>();
            string fileIn = File.ReadAllText(file);
            string[] elements = fileIn.Split(new char[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < elements.Length; i += 2)
            {   
                string originalURL = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(elements[i]));
                string archiveURL = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(elements[i + 1]));
                archives.Add(originalURL, archiveURL);
            }

            return archives;
        }

        void AppendArchiveTrackingFile(string originalURL, string archiveURL)
        {
            string encodedOriginalURL = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(originalURL));
            string encodedArchiveURL = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(archiveURL));
            string appendStr = encodedOriginalURL + ":" + encodedArchiveURL;
            if(new FileInfo(archivesTrackerFilePath).Length > 0)
            {
                appendStr = "," + appendStr;
            }
            File.AppendAllText(archivesTrackerFilePath, appendStr);
        }
    }
}
