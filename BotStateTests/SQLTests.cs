using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mnemosyne_Of_Mine;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;

namespace BotStateTests
{
    [TestClass]
    public class SQLTests
    {
        IBotStateTracker SQLTracker;

        Dictionary<string, string> BotRepliesDataset;
        List<string> CheckedCommentsDataset;
        Dictionary<string, string> ArchivesDataset;

        [TestInitialize]
        public void PreTestSetup()
        {
            File.Copy("TestBase.sqlite", "Test.sqlite");
            SQLTracker = new SQLBotStateTracker("Test.sqlite");
            BotRepliesDataset = new Dictionary<string, string>()
            {
                {"4nn36s", "d4yy5xu"},
                {"4ntdcn", "d4yy63q"},
                {"4ntao4", "d4yy6bj"}
            };
            CheckedCommentsDataset = new List<string>()
            {
                "d46qosm",
                "d46qnw6",
                "d46q8wt",
                "d46q6ml",
                "d46pwtk",
                "d46k73i",
                "d46ezlc"
            };
        }

        [TestCleanup]
        public void PostTestCleanup()
        {
            File.Delete("Test.sqlite");
        }

        [TestMethod]
        public void BotReplyExistsTest_ReplyDoesNotExist()
        {
            Assert.IsFalse(SQLTracker.DoesBotCommentExist("wololo"));
        }

        [TestMethod]
        public void BotReplyExistsTest_ReplyDoesExist()
        {
            foreach (KeyValuePair<string, string> pair in BotRepliesDataset)
            {
                Assert.IsTrue(SQLTracker.DoesBotCommentExist(pair.Key));
            }
        }

        [TestMethod]
        public void BotReplyGet_ReplyExists()
        {
            foreach(KeyValuePair<string, string> pair in BotRepliesDataset)
            {
                Assert.AreEqual(pair.Value, SQLTracker.GetBotCommentForPost(pair.Key));
            }
        }

        [TestMethod]
        public void BotReplyGet_ReplyDoesNotExist()
        {
            Assert.IsNull(SQLTracker.GetBotCommentForPost("nope"));
        }

        [TestMethod]
        public void BotReplyAddTest_UniqueAdd()
        {
            SQLTracker.AddBotComment("yep", "blaah");
        }

        [TestMethod]
        [ExpectedException(typeof(SQLiteException))]
        public void BotReplyAddTest_NotUniqueAdd()
        {
            SQLTracker.AddBotComment("4nn36s", "d4yy5xu");
        }

        [TestMethod]
        public void CheckedCommentsTest_Unchecked()
        {
            Assert.IsFalse(SQLTracker.HasCommentBeenChecked("wololo"));
        }

        [TestMethod]
        public void CheckedCommentsTest_Checked()
        {
            foreach(string str in CheckedCommentsDataset)
            {
                Assert.IsTrue(SQLTracker.HasCommentBeenChecked(str));
            }
        }

        [TestMethod]
        public void CheckedCommentsTest_UniqueAdd()
        {
            SQLTracker.AddCheckedComment("unique");
        }

        [TestMethod]
        [ExpectedException(typeof(SQLiteException))]
        public void CheckedCommentsTest_NotUniqueAdd()
        {
            SQLTracker.AddCheckedComment("d46qosm");
        }
    }
}
