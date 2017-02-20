using RedditSharp;
using RedditSharp.Things;
namespace Mnemosyne_Of_Mine
{
    public class ArchiveSub : Subreddit
    {
        public bool ArchivePosts { get; private set; }
        public bool ArchiveComments { get; private set; }
        private Subreddit Sub { get; set; }
        public new Listing<Post> New => Sub.New;
        public new Listing<Comment> Comments => Sub.Comments;

        public ArchiveSub(Subreddit subreddit, bool archivePosts, bool archiveComments)
        {
            Sub = subreddit;
            ArchivePosts = archivePosts;
            ArchiveComments = archiveComments;
        }
    }
}
