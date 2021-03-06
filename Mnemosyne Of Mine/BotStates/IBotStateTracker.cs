﻿using System.Collections.Generic;

namespace Mnemosyne_Of_Mine
{
    public interface IBotStateTracker
    {
        bool DoesBotCommentExist(string commentID);
        string GetBotCommentForPost(string postID);
        bool HasCommentBeenChecked(string commentID);
        void AddBotComment(string postID, string commentID);
        void AddCheckedComment(string commentID);
        bool IsURLAlreadyArchived(string url);
        string GetArchiveForURL(string url);
        void AddArchiveForURL(string originalURL, string archiveURL);
        int GetArchiveCount(string url);
        void AddArchiveCount(string url);
        Dictionary<string, int> GetArchiveCountDict();
    }
}
