using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mnemosyne_Of_Mine
{
    public class FlatFileBotStateTracker : IBotStateTracker
    {
        string replyTrackerFilePath;
        string checkedCommentsFilePath;

        static Dictionary<string, string> BotComments = new Dictionary<string, string>();
        static List<string> CheckedComments = new List<string>();

        public FlatFileBotStateTracker(string replyFile = @".\ReplyTracker.txt", string commentFile = @".\Comments_Seen.txt")
        {
            replyTrackerFilePath = replyFile;
            checkedCommentsFilePath = commentFile;
            BotComments = ReadReplyTrackingFile(replyTrackerFilePath);
            CheckedComments = File.ReadAllLines(checkedCommentsFilePath).ToList();
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
            string botReplyID = "";
            if (BotComments.ContainsKey(postID))
            {
                botReplyID = BotComments[postID];
            }
            if (string.IsNullOrWhiteSpace(botReplyID))
            {
                throw new InvalidOperationException($"Comment ID for post {postID} is null or empty");
            }
            return botReplyID;
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
            if (!BotComments.ContainsKey(postID))
            {
                BotComments.Add(postID, commentID);
                AppendReplyTrackingFile(postID, commentID);
            }
            else
            {
                throw new InvalidOperationException($"The post {postID} already exists in the tracking file");
            }
        }

        /// <summary>
        /// Add comment to checked comments tracker
        /// </summary>
        /// <param name="commentID">Comment ID</param>
        public void AddCheckedComment(string commentID)
        {
            if (!CheckedComments.Contains(commentID))
            {
                CheckedComments.Add(commentID);
                File.AppendAllText(checkedCommentsFilePath, $"{commentID}{Environment.NewLine}");
            }
            else
            {
                Console.WriteLine($"The comment {commentID} already exists in the tracking file");
            }
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
    }
}
