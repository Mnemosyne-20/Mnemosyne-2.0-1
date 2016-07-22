using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mnemosyne_Of_Mine
{
    public class Archiving
    {
        /// <summary>
        /// This gets a post/comment and archives it
        /// </summary>
        /// <param name="postID">id for the post that you want archived</param>
        public static async Task<string> ArchivePost(string postID, string Subreddit)
        {
            return await Archive(@"archive.is", $"https://www.reddit.com/r/{Subreddit}/comments/{postID}");
        }
        /// <summary>
        /// Gets the url of the Archive, goddamn once this is finished i will have no idea how this works
        /// </summary>
        /// <param name="serviceURL">Archiving service, generally archive.is</param>
        /// <param name="url">The url that we're archiving</param>
        /// <returns>the archive url</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
        public static async Task<string> Archive(string serviceURL, string url)
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
                /// <summary>
                /// This puts a request to the archive site, so yhea...
                /// </summary>
                var response =  await client.PostAsync(serviceURL, content);
                archiveURL = response.RequestMessage.RequestUri.ToString();
                /// <remarks>
                /// Fixes the bug where archive.is returns a json file that has a url tag
                /// </remarks>
                if (archiveURL == "http://archive.is/submit/")
                {
                    #region fixing it
                    StringReader reader = new StringReader(response.ToString());
                    for (int i = 0; i < 3; i++)
                    {
                        reader.ReadLine();
                    }
                    string wanted = reader.ReadLine();
                    reader.Dispose();
                    string[] sides = wanted.Split('=');
                    try
                    {
                    archiveURL = sides[1];
                    }
                    catch(System.Exception e)
                    {
                        System.Console.WriteLine(response.ToString());
                        System.Console.ReadLine();
                    }
                    #endregion
                }
            }
            handle.Dispose();
            return archiveURL;
        }
        /// <summary>
        /// Making sure that we got the correct archive
        /// </summary>
        /// <param name="originalURL">Original URL of archive</param>
        /// <param name="archiveURL">the url of the archive we recived</param>
        /// <returns>wether or not it succeded</returns>
        public static bool VerifyArchiveResult(string originalURL, string archiveURL)
        {
            if (archiveURL == null || archiveURL == "http://archive.is/submit/")
            {
                throw new ArchiveLibrary.FailureToArchiveException($"Failed to archive: {originalURL} \n");
            }
            return true;
        }
    }
}
