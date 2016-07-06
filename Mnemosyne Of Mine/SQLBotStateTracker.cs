using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
namespace Mnemosyne_Of_Mine
{
    class SQLBotStateTracker : IBotStateTracker
    {
        static Dictionary<string, string> BotComments = new Dictionary<string, string>();
        static List<string> CheckedComments = new List<string>();
        public SQLBotStateTracker()
        {

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
    }
}
