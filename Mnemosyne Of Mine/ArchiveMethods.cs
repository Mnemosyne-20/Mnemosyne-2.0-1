using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
namespace Mnemosyne_Of_Mine
{
    class ArchiveMethods
    {
        /// <summary>
        /// This pages through the comments of a post and tries to archive them
        /// </summary>
        /// <param name="postID">id for the post that you will get links in the comments and archive</param>
        public static void Archive(string postID)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Gets the url of the Archive, goddamn once this is finished i will have no idea how this works
        /// </summary>
        /// <param name="serviceURL">Archiving service, generally archive.is</param>
        /// <param name="url">The url that we're archiving</param>
        /// <returns>the archive url</returns>
        public static string Archive(string serviceURL, string url)
        {
            string archiveURL = null;
            HttpClientHandler handle = new HttpClientHandler();
            handle.AllowAutoRedirect = true;
            using (var client = new HttpClient(handle))
            {
                var values = new Dictionary<string, string>
                {
                    {"url", url }
                };
                var content = new FormUrlEncodedContent(values);
                serviceURL = "http://" + serviceURL + "/submit/";
                Console.WriteLine("Damnit");
                /// <summary>
                /// This puts a request to the archive site, so yhea...
                /// </summary>
                var response = client.PostAsync(serviceURL, content);
                var loc = response.Result;
                archiveURL = loc.RequestMessage.RequestUri.ToString();
                if (archiveURL == "http://archive.is/submit/")
                {
                    #region fixing it
                    StringReader reader = new StringReader(loc.ToString());
                    for (int i = 0; i < 3; i++)
                    {
                        reader.ReadLine();
                    }
                    string wanted = reader.ReadLine();
                    string[] sides = wanted.Split('=');
                    Console.WriteLine(sides[1]);
                    archiveURL = sides[1];
                    #endregion
                }
            }
            return archiveURL;
        }
    }
}
