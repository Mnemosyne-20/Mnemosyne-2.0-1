using System.Collections.Generic;
using HtmlAgilityPack;

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
            HtmlDocument body = new HtmlDocument();
            body.LoadHtml(PostBody);

            HtmlNodeCollection links = body.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                foreach (HtmlNode link in links)
                {
                    LinksList.Add(link.Attributes["href"].Value);
                }
            }
            return LinksList;
        }
    }
}
