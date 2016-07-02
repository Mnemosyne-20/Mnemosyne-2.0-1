using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ArchiveLibrary
{
    public class RedditArchiveMethods
    {
        /// <summary>
        /// Archives all links in a post
        /// </summary>
        /// <param name="config">userconfig</param>
        /// <param name="FoundLinks">links found by the linkfinder</param>
        /// <param name="exclusions">exclusions from archiving</param>
        /// <returns>archives</returns>
        public static List<string> ArchivePostLinks(UserData config, List<string> FoundLinks, Regex exclusions)
        {
            List<string> ArchiveLinks = new List<string>();
            int counter = 1;
            foreach (string link in FoundLinks)
            {
                if (!exclusions.IsMatch(link))
                {
                    string archiveURL = Archiving.Archive(@"archive.is", link);
                    if (Archiving.VerifyArchiveResult(link, archiveURL))
                    {
                        string hostname = new Uri(link).Host.Replace("www.", "");
                        ArchiveLinks.Add($"* **Link: {counter.ToString()}** ([{hostname}]({link})): {archiveURL}\n");
                    }
                }
                ++counter;
            }
            return ArchiveLinks;
        }
    }
}
