using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
namespace ArchiveLibrary
{
    public class Archiving
    {
#pragma warning disable IDE1006
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
        public static async Task<string> Archive(string serviceURL, string url)
        {
            string archiveURL = null;
            HttpClientHandler handle = new HttpClientHandler()
            {
                AllowAutoRedirect = true
            };
            using (var client = new HttpClient(handle))
            {
                serviceURL = "http://" + serviceURL + "/submit/";
                /// <summary>
                /// This puts a request to the archive site, so yhea...
                /// </summary>
                var request = new HttpRequestMessage(HttpMethod.Post, serviceURL);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36");
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", url }
                });
                var response = await client.SendAsync(request);
                Task.Delay(8000).Wait(); // elementary test to make it wait 8 seconds to get around, doesn't work
                archiveURL = response.RequestMessage.RequestUri.ToString();
                /// <remarks>
                /// Fixes the bug where archive.is returns a json file that has a url tag
                /// </remarks>
                if (archiveURL == $"http://{serviceURL}/submit/" && !response.IsSuccessStatusCode)
                {
                    #region fixing it
                    using (StringReader reader = new StringReader(response.ToString()))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            reader.ReadLine();
                        }
                        string wanted = reader.ReadLine();
                        reader.Dispose();
                        string[] sides = wanted.Split('=');
                        archiveURL = sides[1];
                    }
                    #endregion
                }
            }
            handle.Dispose();
            return archiveURL;
        }
#pragma warning restore IDE1006
        /// <summary>
        /// Making sure that we got the correct archive
        /// </summary>
        /// <param name="originalURL">Original URL of archive</param>
        /// <param name="archiveURL">the url of the archive we recived</param>
        /// <returns>wether or not it succeded</returns>
        public static bool VerifyArchiveResult(string originalURL, string archiveURL)
        {
            if (archiveURL == null || archiveURL == "http://archive.is/submit/" || archiveURL == "http://archive.fo/submit/")
            {
                throw new FailureToArchiveException($"Failed to archive: {originalURL} \n");
            }
            return true;
        }
    }
}
