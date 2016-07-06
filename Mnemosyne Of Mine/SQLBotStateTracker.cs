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
        static Dictionary<string, string> BotComments = new Dictionary<string, string>();
        static List<string> CheckedComments = new List<string>();

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

            if(bFreshStart)
            {
                InitializeDatabase();
            }
        }
        public void AddBotComment(string postID, string commentID)
        {
            BotComments.Add(postID, commentID);
            
        }

        public void AddCheckedComment(string commentID)
        {
            
        }

        public bool DoesBotCommentExist(string commentID)
        {
            return true;
        }

        public string GetBotCommentForPost(string postID)
        {
            return null;
        }

        public bool HasCommentBeenChecked(string commentID)
        {
            return true;
        }

        void InitializeDatabase()
        {
            string query = "create table replies (postID text, botReplyID text)";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.ExecuteNonQuery();
            query = "create table comments (commentID text)"; // yes this is a table with one column and eventually along with the reply table won't even be needed at all
            cmd = new SQLiteCommand(query, dbConnection);
            cmd.ExecuteNonQuery();
            query = "create table archives (originalURL text, archiveURL text)";
            cmd = new SQLiteCommand(query, dbConnection);
            cmd.ExecuteNonQuery();
        }
    }
}
