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

            // I love how htmlagilitypack has essentially zero documentation
            var match = Regex.Match(PostBody, @"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?");
            while(match.Success)
            {
                LinksList.Add(match.Value);
                match = match.NextMatch();
            }
            System.Console.WriteLine(string.Join("\n", LinksList.ToArray()));
            return LinksList;
        }
    }
}
