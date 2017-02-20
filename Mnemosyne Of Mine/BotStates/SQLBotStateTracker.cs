using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Mnemosyne_Of_Mine
{
    public class SQLBotStateTracker : IBotStateTracker, IDisposable
    {

        bool isMono;
        string DatabaseFilename;
        SQLiteConnection dbConnection;
#pragma warning disable 0649
        SQLiteCommand SQLCmd_AddBotComment, SQLCmd_AddCheckedComment, SQLCmd_DoesBotCommentExist, SQLCmd_GetBotComment,
            SQLCmd_HasCommentBeenChecked, SQLCmd_IsURLArchived, SQLCmd_GetArchive, SQLCmd_AddArchive, SQLCmd_GetArchivesCount, SQLCmd_SetArchivesCount;//, SQLCmd_AddTrackingLink, SQLCmd_GetNumArchived;
#pragma warning restore
        public SQLBotStateTracker(string filename = "botstate.sqlite")
        {
            isMono = Type.GetType("Mono.Runtime") != null;
            DatabaseFilename = filename;
            if (!File.Exists(DatabaseFilename))
            {
                SQLiteConnection.CreateFile(DatabaseFilename);
            }
            string assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            AppDomain.CurrentDomain.SetData("DataDirectory", assemblyPath);
            dbConnection = new SQLiteConnection($"Data Source=|DataDirectory|/{DatabaseFilename};Version=3;");
            dbConnection.Open();
            InitializeDatabase();
            InitializeCommands();
        }

        public void AddBotComment(string postID, string commentID)
        {
            try
            {
                SQLCmd_AddBotComment.Parameters["@postID"].Value = postID;
                SQLCmd_AddBotComment.Parameters["@botReplyID"].Value = commentID;
                SQLCmd_AddBotComment.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                if (ex.ResultCode == SQLiteErrorCode.Constraint)
                {
                    throw new InvalidOperationException($"The post {postID} already exists in database");
                }
                else
                {
                    throw;
                }
            }
        }

        public void AddCheckedComment(string commentID)
        {
            try
            {
                SQLCmd_AddCheckedComment.Parameters["@commentID"].Value = commentID;
                SQLCmd_AddCheckedComment.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                if (ex.ResultCode == SQLiteErrorCode.Constraint)
                {
                    Console.WriteLine($"The comment {commentID} already exists in database");
                }
                else
                {
                    throw;
                }
            }
        }

        public bool DoesBotCommentExist(string postID)
        {
            SQLCmd_DoesBotCommentExist.Parameters["@postID"].Value = postID;
            long count = SQLCmd_DoesBotCommentExist.ExecuteScalar() as long? ?? 0;
            return count != 0;
        }

        public string GetBotCommentForPost(string postID)
        {
            SQLCmd_GetBotComment.Parameters["@postID"].Value = postID;
            string botReplyID = (string)SQLCmd_GetBotComment.ExecuteScalar();
            if (string.IsNullOrWhiteSpace(botReplyID))
            {
                throw new InvalidOperationException($"Comment ID for post {postID} is null or empty");
            }
            return botReplyID;
        }

        public bool HasCommentBeenChecked(string commentID)
        {
            SQLCmd_HasCommentBeenChecked.Parameters["@commentID"].Value = commentID;
            long count = SQLCmd_HasCommentBeenChecked.ExecuteScalar() as long? ?? 0;
            return count != 0;
        }

        ///<remarks>TODO:FIXME</remarks>
        public bool IsURLAlreadyArchived(string url)
        {
            /*SQLCmd_IsURLArchived.Parameters["@url"].Value = url;
            long count = SQLCmd_IsURLArchived.ExecuteScalar() as long? ?? 0;
            return count != 0;*/
            return false;
        }

        public string GetArchiveForURL(string url)
        {
            /*SQLCmd_GetArchive.Parameters["@url"].Value = url;
            string archiveURL = (string)SQLCmd_GetArchive.ExecuteScalar() ?? "";
            return archiveURL;*/
            return "";
        }

        public void AddArchiveForURL(string originalURL, string archiveURL)
        {
            /*SQLCmd_AddArchive.Parameters["@originalURL"].Value = originalURL;
            SQLCmd_AddArchive.Parameters["@archiveURL"].Value = archiveURL;
            SQLCmd_AddArchive.ExecuteNonQuery();*/
        }

        public int GetArchiveCount(string url)
        {
            SQLCmd_GetArchivesCount.Parameters["@url"].Value = url;
            return Convert.ToInt32(SQLCmd_GetArchivesCount.ExecuteScalar() ?? 0);
        }

        public void AddArchiveCount(string url)
        {
            SQLCmd_SetArchivesCount.Parameters["@url"].Value = url;
            SQLCmd_SetArchivesCount.Parameters["@numArchives"].Value = GetArchiveCount(url) + 1;
            SQLCmd_SetArchivesCount.ExecuteNonQuery();
        }

        void InitializeDatabase()
        {
            string query = "create table if not exists replies (postID text unique, botReplyID text)";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            query = "create table if not exists comments (commentID text unique)"; // yes this is a table with one column and eventually along with the reply table won't even be needed at all
            cmd = new SQLiteCommand(query, dbConnection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            query = "create table if not exists archives (originalURL text unique, numArchives integer)";
            cmd = new SQLiteCommand(query, dbConnection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        void InitializeCommands()
        {
            SQLCmd_AddBotComment = new SQLiteCommand("insert or abort into replies(postID, botReplyID) values(@postID, @botReplyID)", dbConnection);
            SQLCmd_AddBotComment.Parameters.Add(new SQLiteParameter("@postID"));
            SQLCmd_AddBotComment.Parameters.Add(new SQLiteParameter("@botReplyID"));

            SQLCmd_AddCheckedComment = new SQLiteCommand("insert or abort into comments (commentID) values (@commentID)", dbConnection);
            SQLCmd_AddCheckedComment.Parameters.Add(new SQLiteParameter("@commentID"));

            SQLCmd_DoesBotCommentExist = new SQLiteCommand("select count(*) from replies where postID = @postID", dbConnection);
            SQLCmd_DoesBotCommentExist.Parameters.Add(new SQLiteParameter("@postID"));

            SQLCmd_GetBotComment = new SQLiteCommand("select botReplyID from replies where postID = @postID", dbConnection);
            SQLCmd_GetBotComment.Parameters.Add(new SQLiteParameter("@postID"));

            SQLCmd_HasCommentBeenChecked = new SQLiteCommand("select count(commentID) from comments where commentID = @commentID", dbConnection);
            SQLCmd_HasCommentBeenChecked.Parameters.Add(new SQLiteParameter("@commentID"));

            /*SQLCmd_IsURLArchived = new SQLiteCommand("select count(*) from archives where originalURL = @url", dbConnection);
            SQLCmd_IsURLArchived.Parameters.Add(new SQLiteParameter("@url"));

            SQLCmd_GetArchive = new SQLiteCommand("select archiveURLs from archives where originalURL = @url", dbConnection);
            SQLCmd_GetArchive.Parameters.Add(new SQLiteParameter("@url"));

            SQLCmd_AddArchive = new SQLiteCommand("insert or replace into archives (originalURL, numArchives, archiveURLs) values (@originalURL, @numArchives, @archiveURLs)", dbConnection);
            SQLCmd_AddArchive.Parameters.Add(new SQLiteParameter("@originalURL"));
            SQLCmd_AddArchive.Parameters.Add(new SQLiteParameter("@numArchives"));
            SQLCmd_AddArchive.Parameters.Add(new SQLiteParameter("@archiveURLs"));*/

            SQLCmd_GetArchivesCount = new SQLiteCommand("select numArchives from archives where originalURL = @url", dbConnection);
            SQLCmd_GetArchivesCount.Parameters.Add(new SQLiteParameter("@url"));

            SQLCmd_SetArchivesCount = new SQLiteCommand("insert or replace into archives (originalURL, numArchives) values (@url, @numArchives)", dbConnection);
            SQLCmd_SetArchivesCount.Parameters.Add(new SQLiteParameter("@url"));
            SQLCmd_SetArchivesCount.Parameters.Add(new SQLiteParameter("@numArchives"));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool f)
        {
            dbConnection.Dispose();
            SQLCmd_AddArchive.Dispose();
            SQLCmd_AddBotComment.Dispose();
            SQLCmd_AddCheckedComment.Dispose();
            SQLCmd_DoesBotCommentExist.Dispose();
            SQLCmd_GetArchive.Dispose();
            SQLCmd_GetArchivesCount.Dispose();
            SQLCmd_GetBotComment.Dispose();
            SQLCmd_HasCommentBeenChecked.Dispose();
            SQLCmd_IsURLArchived.Dispose();
            SQLCmd_SetArchivesCount.Dispose();
        }
        public Dictionary<string, int> GetArchiveCountDict()
        {
            throw new NotImplementedException();
        }
    }
}
