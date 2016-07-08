using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mnemosyne_Of_Mine;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;

namespace BotStateTests
{
    [TestClass]
    [DeploymentItem("x64\\SQLite.Interop.dll", "x64")] //it's half a bit stupid that this is even necessary, and another half a bit stupid that this specifically isn't deleted afterwards
    [DeploymentItem("x86\\SQLite.Interop.dll", "x86")]
    public class SQLTests
    {
        Dictionary<string, string> BotRepliesDataset = new Dictionary<string, string>()
        {
            {"4nn36s", "d4yy5xu"},
            {"4ntdcn", "d4yy63q"},
            {"4ntao4", "d4yy6bj"}
        };
        List<string> CheckedCommentsDataset = new List<string>()
        {
            "d46qosm",
            "d46qnw6",
            "d46q8wt",
            "d46q6ml",
            "d46pwtk",
            "d46k73i",
            "d46ezlc"
        };
        Dictionary<string, string> ArchivesDataset;

        [TestMethod]
        [DeploymentItem("Test.sqlite", "1")]
        public void BotReplyExistsTest_ReplyDoesNotExist()
        {
            IBotStateTracker SQLTracker = new SQLBotStateTracker("1\\Test.sqlite");
            Assert.IsFalse(SQLTracker.DoesBotCommentExist("wololo"));
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "2")]
        public void BotReplyExistsTest_ReplyDoesExist()
        {
            IBotStateTracker SQLTracker = new SQLBotStateTracker("2\\Test.sqlite");
            foreach (KeyValuePair<string, string> pair in BotRepliesDataset)
            {
                Assert.IsTrue(SQLTracker.DoesBotCommentExist(pair.Key));
            }
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "3")]
        public void BotReplyGet_ReplyExists()
        {
            IBotStateTracker SQLTracker = new SQLBotStateTracker("3\\Test.sqlite");
            foreach (KeyValuePair<string, string> pair in BotRepliesDataset)
            {
                Assert.AreEqual(pair.Value, SQLTracker.GetBotCommentForPost(pair.Key));
            }
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "4")]
        public void BotReplyGet_ReplyDoesNotExist()
        {
            IBotStateTracker SQLTracker = new SQLBotStateTracker("4\\Test.sqlite");
            Assert.AreEqual("", SQLTracker.GetBotCommentForPost("nope"));
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "5")]
        public void BotReplyAddTest_UniqueAdd()
        {
            IBotStateTracker SQLTracker = new SQLBotStateTracker("5\\Test.sqlite");
            SQLTracker.AddBotComment("yep", "blaah");
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "6")]
        public void BotReplyAddTest_NotUniqueAdd()
        {
            IBotStateTracker SQLTracker = new SQLBotStateTracker("6\\Test.sqlite");
            SQLTracker.AddBotComment("4nn36s", "d4yy5xu");
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "7")]
        public void CheckedCommentsTest_Unchecked()
        {
            IBotStateTracker SQLTracker = new SQLBotStateTracker("7\\Test.sqlite");
            Assert.IsFalse(SQLTracker.HasCommentBeenChecked("wololo"));
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "8")]
        public void CheckedCommentsTest_Checked()
        {
            IBotStateTracker SQLTracker = new SQLBotStateTracker("8\\Test.sqlite");
            foreach (string str in CheckedCommentsDataset)
            {
                Assert.IsTrue(SQLTracker.HasCommentBeenChecked(str));
            }
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "9")]
        public void CheckedCommentsTest_UniqueAdd()
        {
            IBotStateTracker SQLTracker = new SQLBotStateTracker("9\\Test.sqlite");
            SQLTracker.AddCheckedComment("unique");
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "10")]
        public void CheckedCommentsTest_NotUniqueAdd()
        {
            IBotStateTracker SQLTracker = new SQLBotStateTracker("10\\Test.sqlite");
            SQLTracker.AddCheckedComment("d46qosm");
        }

        [TestMethod]
        public void FreshDatabaseTest()
        {
            IBotStateTracker SQLTracker = new SQLBotStateTracker("newdb.sqlite");
            foreach(KeyValuePair<string, string> pair in BotRepliesDataset)
            {
                SQLTracker.AddBotComment(pair.Key, pair.Value);
            }
            foreach(string str in CheckedCommentsDataset)
            {
                SQLTracker.AddCheckedComment(str);
            }
        }
    }
}
