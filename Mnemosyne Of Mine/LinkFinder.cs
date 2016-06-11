using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mnemosyne_Of_Mine
{
    static class LinkFinder
    {
/// <summary>
/// This finds links in a self post
/// </summary>
/// <param name="PostBody">This is the post body</param>
/// <returns>the list of URLS we're archiving</returns>
        public static List<string> FindLinks(string PostBody)
        {
            List<string> LinksList = new List<string>();

            var match = Regex.Match(PostBody, @"""(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?""");
            while(match.Success)
            {
                string foundLink = match.Value.TrimEnd('"').TrimStart('"');
                LinksList.Add(foundLink);
                match = match.NextMatch();
            }
            return LinksList;
        }
    }
}
