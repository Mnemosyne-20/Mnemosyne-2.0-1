using RedditSharp.Things;
namespace Mnemosyne_Of_Mine
{
    public class ArchiveSub : Subreddit
    {
        public bool ArchivePosts { get; private set; }
        public bool ArchiveComments { get; private set; }
        public ArchiveSub(bool archivePosts, bool archiveComments)
        {
            ArchivePosts = archivePosts;
            ArchiveComments = archiveComments;
        }
    }
}
