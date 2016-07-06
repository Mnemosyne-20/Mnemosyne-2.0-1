using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace Mnemosyne_Of_Mine
{
    class SQLBotStateTracker : IBotStateTracker
    {
        string DatabaseFilename = "botstate.sqlite";
        SQLiteConnection dbConnection;

        SQLiteCommand SQLCmd_AddBotComment, SQLCmd_AddCheckedComment, SQLCmd_DoesBotCommentExist, SQLCmd_GetBotComment,
            SQLCmd_HasCommentBeenChecked, SQLCmd_IsURLArchived, SQLCmd_GetArchive, SQLCmd_AddArchive;

        public SQLBotStateTracker()
        {
            bool bFreshStart = false;
            if (!File.Exists($".\\{DatabaseFilename}"))
            {
                SQLiteConnection.CreateFile($".\\{DatabaseFilename}");
                bFreshStart = true;
            }

            string assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            AppDomain.CurrentDomain.SetData("DataDirectory", assemblyPath);
            dbConnection = new SQLiteConnection($"Data Source=|DataDirectory|\\{DatabaseFilename};Version=3;");
            dbConnection.Open();

            if (bFreshStart)
            {
                InitializeDatabase();
            }

            InitializeCommands();
        }

        public void AddBotComment(string postID, string commentID)
        {
            SQLCmd_AddBotComment.Parameters["@postID"].Value = postID;
            SQLCmd_AddBotComment.Parameters["@commentID"].Value = commentID;
            SQLCmd_AddBotComment.ExecuteNonQuery();
        }

        public void AddCheckedComment(string commentID)
        {
            SQLCmd_AddCheckedComment.Parameters["@commentID"].Value = commentID;
            SQLCmd_AddCheckedComment.ExecuteNonQuery();
        }

        public bool DoesBotCommentExist(string commentID)
        {
            SQLCmd_DoesBotCommentExist.Parameters["@commentID"].Value = commentID;
            int count = (int)SQLCmd_DoesBotCommentExist.ExecuteScalar();
            return count != 0;
        }

        public string GetBotCommentForPost(string postID)
        {
            SQLCmd_GetBotComment.Parameters["@postID"].Value = postID;
            string commentID = (string)SQLCmd_GetBotComment.ExecuteScalar();
            return commentID;
        }

        public bool HasCommentBeenChecked(string commentID)
        {
            SQLCmd_HasCommentBeenChecked.Parameters["@commentID"].Value = commentID;
            int count = (int)SQLCmd_HasCommentBeenChecked.ExecuteScalar();
            return count != 0;
        }

        public bool IsURLAlreadyArchived(string url)
        {
            SQLCmd_IsURLArchived.Parameters["@url"].Value = url;
            int count = (int)SQLCmd_IsURLArchived.ExecuteScalar();
            return count != 0;
        }

        public string GetArchiveForURL(string url)
        {
            SQLCmd_GetArchive.Parameters["@url"].Value = url;
            string archiveURL = (string)SQLCmd_GetArchive.ExecuteScalar();
            return archiveURL;
        }

        public void AddArchiveForURL(string originalURL, string archiveURL)
        {
            SQLCmd_AddArchive.Parameters["@originalURL"].Value = originalURL;
            SQLCmd_AddArchive.Parameters["@archiveURL"].Value = archiveURL;
            SQLCmd_AddArchive.ExecuteNonQuery();
        }

        void InitializeDatabase()
        {
            string query = "create table replies (postID text, botReplyID text)";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            query = "create table comments (commentID text)"; // yes this is a table with one column and eventually along with the reply table won't even be needed at all
            cmd = new SQLiteCommand(query, dbConnection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            query = "create table archives (originalURL text, archiveURL text)";
            cmd = new SQLiteCommand(query, dbConnection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        void InitializeCommands()
        {
            SQLCmd_AddBotComment = new SQLiteCommand("insert into replies(postID, botReplyID) values(@postID, @commentID)", dbConnection);
            SQLCmd_AddBotComment.Parameters.Add(new SQLiteParameter("@postID"));
            SQLCmd_AddBotComment.Parameters.Add(new SQLiteParameter("@commentID"));
            
            SQLCmd_AddCheckedComment = new SQLiteCommand("insert into comments (commentID) values (@commentID)", dbConnection);
            SQLCmd_AddCheckedComment.Parameters.Add(new SQLiteParameter("@commentID"));
            
            SQLCmd_DoesBotCommentExist = new SQLiteCommand("select count(*) from replies where commentID = @commentID", dbConnection);
            SQLCmd_DoesBotCommentExist.Parameters.Add(new SQLiteParameter("@commentID"));
            
            SQLCmd_GetBotComment = new SQLiteCommand("select commentID from replies where postID = @postID", dbConnection);
            SQLCmd_GetBotComment.Parameters.Add(new SQLiteParameter("@postID"));
            
            SQLCmd_HasCommentBeenChecked = new SQLiteCommand("select count(commentID) from comments where commentID = @commentID", dbConnection);
            SQLCmd_HasCommentBeenChecked.Parameters.Add(new SQLiteParameter("@commentID"));
            
            SQLCmd_IsURLArchived = new SQLiteCommand("select count(*) from archives where originalURL = @url", dbConnection);
            SQLCmd_IsURLArchived.Parameters.Add(new SQLiteParameter("@url"));
            
            SQLCmd_GetArchive = new SQLiteCommand("select archiveURL from archives where originalURL = @url", dbConnection);
            SQLCmd_GetArchive.Parameters.Add(new SQLiteParameter("@url"));
            
            SQLCmd_AddArchive = new SQLiteCommand("insert into archives (originalURL, archiveURL) values (@originalURL, @archiveURL)", dbConnection);
            SQLCmd_AddArchive.Parameters.Add(new SQLiteParameter("@originalURL"));
            SQLCmd_AddArchive.Parameters.Add(new SQLiteParameter("@archiveURL"));
        }
    }
}
