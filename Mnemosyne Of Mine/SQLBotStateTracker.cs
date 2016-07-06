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
        }

        public void AddBotComment(string postID, string commentID)
        {
            string query = "insert into replies (postID, botReplyID) values (@postID, @commentID)";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.Parameters.Add(new SQLiteParameter("@postID", postID));
            cmd.Parameters.Add(new SQLiteParameter("@commentID", commentID));
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        public void AddCheckedComment(string commentID)
        {
            string query = "insert into comments (commentID) values (@commentID)";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.Parameters.Add(new SQLiteParameter("@commentID", commentID));
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        public bool DoesBotCommentExist(string commentID)
        {
            string query = "select count(*) from replies where commentID = @commentID";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.Parameters.Add(new SQLiteParameter("@commentID", commentID));
            int count = (int)cmd.ExecuteScalar();
            cmd.Dispose();
            return count != 0;
        }

        public string GetBotCommentForPost(string postID)
        {
            string query = "select commentID from replies where postID = @postID";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.Parameters.Add(new SQLiteParameter("@postID", postID));
            string commentID = (string)cmd.ExecuteScalar();
            cmd.Dispose();
            return commentID;
        }

        public bool HasCommentBeenChecked(string commentID)
        {
            string query = "select count(commentID) from comments where commentID = @commentID";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.Parameters.Add(new SQLiteParameter("@commentID", commentID));
            int count = (int)cmd.ExecuteScalar();
            cmd.Dispose();
            return count != 0;
        }

        public bool IsURLAlreadyArchived(string url)
        {
            string query = "select count(*) from archives where originalURL = @url";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.Parameters.Add(new SQLiteParameter("@url", url));
            int count = (int)cmd.ExecuteScalar();
            cmd.Dispose();
            return count != 0;
        }

        public string GetArchiveForURL(string url)
        {
            string query = "select archiveURL from archives where originalURL = @url";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.Parameters.Add(new SQLiteParameter("@url", url));
            string archiveURL = (string)cmd.ExecuteScalar();
            cmd.Dispose();
            return archiveURL;
        }

        public void AddArchiveForURL(string originalURL, string archiveURL)
        {
            string query = "insert into archives (originalURL, archiveURL) values (@originalURL, @archiveURL)";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.Parameters.Add(new SQLiteParameter("@originalURL", originalURL));
            cmd.Parameters.Add(new SQLiteParameter("@archiveURL", archiveURL));
            cmd.ExecuteNonQuery();
            cmd.Dispose();
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
    }
}
