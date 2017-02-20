using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace Mnemosyne_Of_Mine
{
    public class FlatFileBotStateTracker : IBotStateTracker
    {
        public string ReplyTrackerFilePath { get; private set; }
        public string CheckedCommentsFilePath { get; private set; }
        public string ArchivesTrackerFilePath { get; private set; }
        public string ArchiveCountFilePath { get; private set; }
        static Dictionary<string, string> BotComments = new Dictionary<string, string>();
        static List<string> CheckedComments = new List<string>();
        static Dictionary<string, string> Archives = new Dictionary<string, string>();
        static Dictionary<string, int> ArchiveCount = new Dictionary<string, int>(); // added for later, this is how the flatfile will see how much it's been done

        public FlatFileBotStateTracker(string replyFile = "ReplyTracker.txt", string commentFile = "Comments_Seen.txt", string archivesFile = "ArchiveTracker.txt", string archiveCountFile = "ArchiveCount.txt")
        {
            ReplyTrackerFilePath = replyFile;
            CheckedCommentsFilePath = commentFile;
            ArchivesTrackerFilePath = archivesFile;
            ArchiveCountFilePath = archiveCountFile;
            BotComments = ReadReplyTrackingFile(ReplyTrackerFilePath);
            CheckedComments = File.ReadAllLines(CheckedCommentsFilePath).ToList();
            ArchiveCount = ReadArchiveCountTrackingFile(archiveCountFile);
            //Archives = ReadArchivesTrackingFile(archivesTrackerFilePath);
        }

        /// <summary>
        /// Checks if bot already has a comment in specified post
        /// </summary>
        /// <param name="postID">Post ID</param>
        public bool DoesBotCommentExist(string postID) => BotComments.ContainsKey(postID);

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
        public bool HasCommentBeenChecked(string commentID) => CheckedComments.Contains(commentID);

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
                File.AppendAllText(CheckedCommentsFilePath, $"{commentID}{Environment.NewLine}");
            }
            else
            {
                throw new InvalidOperationException($"The comment {commentID} already exists in the tracking file");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool IsURLAlreadyArchived(string url) => Archives.ContainsKey(url);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetArchiveForURL(string url) => IsURLAlreadyArchived(url) ? Archives[url] : "";
        /// <summary>
        /// FIXME REEEEEEEEEEEEEEEEEEEEE
        /// </summary>
        /// <param name="originalURL"></param>
        /// <param name="archiveURL"></param>
        public void AddArchiveForURL(string originalURL, string archiveURL)
        {
            /*Archives.Add(originalURL, archiveURL);
            AppendArchiveTrackingFile(originalURL, archiveURL);*/
        }

        public int GetArchiveCount(string url) => ArchiveCount[url];
        public void AddArchiveCount(string url)
        {
            if (ArchiveCount.ContainsKey(url))
            {
                ArchiveCount[url]++;
            }
            else
            {
                ArchiveCount.Add(url, 1);
            }
            WriteArchiveCountFile(ArchiveCountFilePath);
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
            if (new FileInfo(ReplyTrackerFilePath).Length > 0)
            {
                appendStr = "," + appendStr;
            }
            File.AppendAllText(ReplyTrackerFilePath, appendStr);
        }

        Dictionary<string, string> ReadArchivesTrackingFile(string file)
        {
            Dictionary<string, string> archives = new Dictionary<string, string>();
            string fileIn = File.ReadAllText(file);
            string[] elements = fileIn.Split(new char[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < elements.Length; i += 2)
            {
                string originalURL = Encoding.UTF8.GetString(Convert.FromBase64String(elements[i]));
                string archiveURL = Encoding.UTF8.GetString(Convert.FromBase64String(elements[i + 1]));
                archives.Add(originalURL, archiveURL);
            }

            return archives;
        }
        Dictionary<string, int> ReadArchiveCountTrackingFile(string file)
        {
            string fileIn = File.ReadAllText(file);
            string[] elements = fileIn.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < elements.Length; i += 2)
            {
                string originalURL = Encoding.UTF8.GetString(Convert.FromBase64String(elements[i]));
                int count = Convert.ToInt32(Encoding.UTF8.GetString(Convert.FromBase64String(elements[i + 1])));
                ArchiveCount.Add(originalURL, count);
            }
            return ArchiveCount;
        }
        void WriteArchiveCountFile(string file)
        {
            string fileWriter = "";
            foreach (KeyValuePair<string, int> keypair in ArchiveCount)
            {
                fileWriter += Convert.ToBase64String(Encoding.UTF8.GetBytes(keypair.Key)) + ';' + Convert.ToBase64String(Encoding.UTF8.GetBytes(keypair.Value.ToString())) + ",";
            }
            File.WriteAllText(file, fileWriter);
        }
        void AppendArchiveTrackingFile(string originalURL, string archiveURL)
        {
            string encodedOriginalURL = Convert.ToBase64String(Encoding.UTF8.GetBytes(originalURL));
            string encodedArchiveURL = Convert.ToBase64String(Encoding.UTF8.GetBytes(archiveURL));
            string appendStr = encodedOriginalURL + ":" + encodedArchiveURL;
            if (new FileInfo(ArchivesTrackerFilePath).Length > 0)
            {
                appendStr = "," + appendStr;
            }
            File.AppendAllText(ArchivesTrackerFilePath, appendStr);
        }
        /// <summary>
        /// <see cref="Dictionary{string, int}"/>
        /// </summary>
        /// <returns>internally used dictionary of archive count</returns>
        public Dictionary<string, int> GetArchiveCountDict() => ArchiveCount;
    }
}
