using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mnemosyne_Of_Mine;
using System;
using System.Collections.Generic;

namespace BotStateTests
{
    [TestClass]
    [DeploymentItem("x64\\SQLite.Interop.dll", "x64")] //it's half a bit stupid that this is even necessary, and another half a bit stupid that this specifically isn't deleted afterwards
    [DeploymentItem("x86\\SQLite.Interop.dll", "x86")]
    public class BotStateTests
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
        [DeploymentItem("ReplyTracker.txt", "1")]
        [DeploymentItem("Comments_Seen.txt", "1")]
        public void BotReplyExistsTest_ReplyDoesNotExist()
        {
            IBotStateTracker BotState = new SQLBotStateTracker("1\\Test.sqlite");
            Assert.IsFalse(BotState.DoesBotCommentExist("wololo"));
            BotState = new FlatFileBotStateTracker("1\\ReplyTracker.txt", "1\\Comments_Seen.txt");
            Assert.IsFalse(BotState.DoesBotCommentExist("wololo"));
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "2")]
        [DeploymentItem("ReplyTracker.txt", "2")]
        [DeploymentItem("Comments_Seen.txt", "2")]
        public void BotReplyExistsTest_ReplyDoesExist()
        {
            IBotStateTracker BotState = new SQLBotStateTracker("2\\Test.sqlite");
            foreach (KeyValuePair<string, string> pair in BotRepliesDataset)
            {
                Assert.IsTrue(BotState.DoesBotCommentExist(pair.Key));
            }
            BotState = new FlatFileBotStateTracker("2\\ReplyTracker.txt", "2\\Comments_Seen.txt");
            foreach (KeyValuePair<string, string> pair in BotRepliesDataset)
            {
                Assert.IsTrue(BotState.DoesBotCommentExist(pair.Key));
            }
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "3")]
        [DeploymentItem("ReplyTracker.txt", "3")]
        [DeploymentItem("Comments_Seen.txt", "3")]
        public void BotReplyGet_ReplyExists()
        {
            IBotStateTracker BotState = new SQLBotStateTracker("3\\Test.sqlite");
            foreach (KeyValuePair<string, string> pair in BotRepliesDataset)
            {
                Assert.AreEqual(pair.Value, BotState.GetBotCommentForPost(pair.Key));
            }
            BotState = new FlatFileBotStateTracker("3\\ReplyTracker.txt", "3\\Comments_Seen.txt");
            foreach (KeyValuePair<string, string> pair in BotRepliesDataset)
            {
                Assert.AreEqual(pair.Value, BotState.GetBotCommentForPost(pair.Key));
            }
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "4")]
        [DeploymentItem("ReplyTracker.txt", "4")]
        [DeploymentItem("Comments_Seen.txt", "4")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BotReplyGet_ReplyDoesNotExist()
        {
            IBotStateTracker BotState = new SQLBotStateTracker("4\\Test.sqlite");
            Assert.AreEqual("", BotState.GetBotCommentForPost("nope"));
            BotState = new FlatFileBotStateTracker("4\\ReplyTracker.txt", "4\\Comments_Seen.txt");
            Assert.AreEqual("", BotState.GetBotCommentForPost("nope"));
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "5")]
        [DeploymentItem("ReplyTracker.txt", "5")]
        [DeploymentItem("Comments_Seen.txt", "5")]
        public void BotReplyAddTest_UniqueAdd()
        {
            IBotStateTracker BotState = new SQLBotStateTracker("5\\Test.sqlite");
            BotState.AddBotComment("yep", "blaah");
            BotState = new FlatFileBotStateTracker("5\\ReplyTracker.txt", "5\\Comments_Seen.txt");
            BotState.AddBotComment("yep", "blaah");
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "6")]
        [DeploymentItem("ReplyTracker.txt", "6")]
        [DeploymentItem("Comments_Seen.txt", "6")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BotReplyAddTest_NotUniqueAdd()
        {
            IBotStateTracker BotState = new SQLBotStateTracker("6\\Test.sqlite");
            BotState.AddBotComment("4nn36s", "d4yy5xu");
            BotState = new FlatFileBotStateTracker("6\\ReplyTracker.txt", "6\\Comments_Seen.txt");
            BotState.AddBotComment("4nn36s", "d4yy5xu");
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "7")]
        [DeploymentItem("ReplyTracker.txt", "7")]
        [DeploymentItem("Comments_Seen.txt", "7")]
        public void CheckedCommentsTest_Unchecked()
        {
            IBotStateTracker BotState = new SQLBotStateTracker("7\\Test.sqlite");
            Assert.IsFalse(BotState.HasCommentBeenChecked("wololo"));
            BotState = new FlatFileBotStateTracker("7\\ReplyTracker.txt", "7\\Comments_Seen.txt");
            Assert.IsFalse(BotState.HasCommentBeenChecked("wololo"));
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "8")]
        [DeploymentItem("ReplyTracker.txt", "8")]
        [DeploymentItem("Comments_Seen.txt", "8")]
        public void CheckedCommentsTest_Checked()
        {
            IBotStateTracker BotState = new SQLBotStateTracker("8\\Test.sqlite");
            foreach (string str in CheckedCommentsDataset)
            {
                Assert.IsTrue(BotState.HasCommentBeenChecked(str));
            }
            BotState = new FlatFileBotStateTracker("8\\ReplyTracker.txt", "8\\Comments_Seen.txt");
            foreach (string str in CheckedCommentsDataset)
            {
                Assert.IsTrue(BotState.HasCommentBeenChecked(str));
            }
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "9")]
        [DeploymentItem("ReplyTracker.txt", "9")]
        [DeploymentItem("Comments_Seen.txt", "9")]
        public void CheckedCommentsTest_UniqueAdd()
        {
            IBotStateTracker BotState = new SQLBotStateTracker("9\\Test.sqlite");
            BotState.AddCheckedComment("unique");
            BotState = new FlatFileBotStateTracker("9\\ReplyTracker.txt", "9\\Comments_Seen.txt");
            BotState.AddCheckedComment("unique");
        }

        [TestMethod]
        [DeploymentItem("Test.sqlite", "10")]
        [DeploymentItem("ReplyTracker.txt", "10")]
        [DeploymentItem("Comments_Seen.txt", "10")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CheckedCommentsTest_NotUniqueAdd()
        {
            IBotStateTracker BotState = new SQLBotStateTracker("10\\Test.sqlite");
            BotState.AddCheckedComment("d46qosm");
            BotState = new FlatFileBotStateTracker("10\\ReplyTracker.txt", "10\\Comments_Seen.txt");
            BotState.AddCheckedComment("d46qosm");
        }

        [TestMethod]
        public void FreshDatabaseTest()
        {
            IBotStateTracker BotState = new SQLBotStateTracker("newdb.sqlite");
            foreach (KeyValuePair<string, string> pair in BotRepliesDataset)
            {
                BotState.AddBotComment(pair.Key, pair.Value);
            }
            foreach (string str in CheckedCommentsDataset)
            {
                BotState.AddCheckedComment(str);
            }
        }
        [TestMethod]
        [DeploymentItem("Test.sqlite", "11")]
        [DeploymentItem("ReplyTracker.txt", "11")]
        [DeploymentItem("Comments_Seen.txt", "11")]
        public void ArchiveCountTest()
        {
            IBotStateTracker botstate = new SQLBotStateTracker("10\\Test.sqlite");
            botstate.AddArchiveCount("hello");
            if(botstate.GetArchiveCount("hello") != 1)
                throw new Exception("Failed to add count");
            botstate = new FlatFileBotStateTracker();
            botstate.AddArchiveCount("hello");
            if (botstate.GetArchiveCount("hello") != 1)
                throw new Exception("Failed to get count");
        }
    }
}
