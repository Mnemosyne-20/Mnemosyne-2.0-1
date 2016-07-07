using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mnemosyne_Of_Mine;
using System.Collections.Generic;

namespace BotStateTests
{
    [TestClass]
    public class SQLTests
    {
        IBotStateTracker SQLTracker = new SQLBotStateTracker("Test.sqlite");

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
        Dictionary<string, string> ArchivesDataset = new Dictionary<string, string>();

        [TestMethod]
        public void BotReplyExistsTest_ShouldPass_ReplyDoesNotExist()
        {
        }

        [TestMethod]
        public void BotReplyExistsTest_ShouldPass_ReplyDoesExist()
        {
        }

        [TestMethod]
        public void BotReplyExistsTest_ShouldFail_PostDoesNotExist()
        {
        }

        [TestMethod]
        public void BotReplyAddTest_ShouldPass_UniqueAdd()
        {
        }

        [TestMethod]
        public void BotReplyAddTest_ShouldFail_PostAlreadyAdded()
        {
        }

        [TestMethod]
        public void CheckedCommentsTest_ShouldPass_Unchecked()
        {
        }

        [TestMethod]
        public void CheckedCommentsTest_ShouldPass_Checked()
        {
        }

        [TestMethod]
        public void CheckedCommentsTest_ShouldPass_AddUnique()
        {
        }

        [TestMethod]
        public void CheckedCommentsTest_ShouldFail_AlreadyAdded()
        {
        }
    }
}
