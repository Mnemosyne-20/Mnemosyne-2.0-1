using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace Mnemosyne_Of_Mine
{
    public class SQLBotStateTracker : IBotStateTracker
    {
        string DatabaseFilename;
        SQLiteConnection dbConnection;

        SQLiteCommand SQLCmd_AddBotComment, SQLCmd_AddCheckedComment, SQLCmd_DoesBotCommentExist, SQLCmd_GetBotComment,
            SQLCmd_HasCommentBeenChecked, SQLCmd_IsURLArchived, SQLCmd_GetArchive, SQLCmd_AddArchive;

        public SQLBotStateTracker(string filename = "botstate.sqlite")
        {
            DatabaseFilename = filename;
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
            try
            {
                SQLCmd_AddBotComment.Parameters["@postID"].Value = postID;
                SQLCmd_AddBotComment.Parameters["@botReplyID"].Value = commentID;
                SQLCmd_AddBotComment.ExecuteNonQuery();
            }
            catch(SQLiteException ex)
            {
                if(ex.ResultCode == SQLiteErrorCode.Constraint)
                {
                    Console.WriteLine($"The post {postID} already exists in database");
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
            catch(SQLiteException ex)
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
            string botReplyID = (string)SQLCmd_GetBotComment.ExecuteScalar() ?? "";
            return botReplyID;
        }

        public bool HasCommentBeenChecked(string commentID)
        {
            SQLCmd_HasCommentBeenChecked.Parameters["@commentID"].Value = commentID;
            long count = SQLCmd_HasCommentBeenChecked.ExecuteScalar() as long? ?? 0;
            return count != 0;
        }

        void InitializeDatabase()
        {
            string query = "create table replies (postID text unique, botReplyID text)";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            query = "create table comments (commentID text unique)"; // yes this is a table with one column and eventually along with the reply table won't even be needed at all
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
        }
    }
}
