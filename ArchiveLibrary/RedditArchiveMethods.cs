using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArchiveLibrary
{
    public class RedditArchiveMethods
    {
#pragma warning disable IDE1006
        /// <summary>
        /// returns string for your archive links
        /// </summary>
        /// <param name="FoundLinks">Links found</param>
        /// <param name="exclusions">regex of excluded links</param>
        /// <returns>links archived!</returns>
        public static async Task<List<string>> ArchivePostLinks(List<string> FoundLinks, Regex exclusions)
        {
            List<string> ArchiveLinks = new List<string>();
            int counter = 1;
            foreach (string link in FoundLinks)
            {
                if (!exclusions.IsMatch(link))
                {
                    string archiveURL = await Archiving.Archive(@"archive.is", link);
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
#pragma warning restore
    }
}
