﻿using RedditSharp;

namespace Mnemosyne_Of_Mine
{
    internal static class Extensions
    {
        /// <summary>
        /// logs you in, simpler for the UserData method
        /// </summary>
        /// <param name="reddit">the reddit object</param>
        /// <param name="user">The user data storage from the config file</param>
        internal static void LogIn(this Reddit reddit, UserData user)
        {
            reddit.LogIn(user.Username, user.Password);
        }
        /// <summary>
        /// % likelyhood match
        /// </summary>
        /// <param name="post">Post from redditsharp see <seealso cref="RedditSharp.Things.Post"/></param>
        /// <returns>a percent match to the annoying post</returns>
        internal static double CheckRepost(this RedditSharp.Things.Post post)
        {
            double perMatch = 0;
            string[] h = "difference between girls and guys code programs".Split(' ');
            foreach (var word in h)
                if (post.Title.Contains(word))
                    perMatch++;
            return (post.Title.Split(' ').Length / perMatch) / 10;
        }
        static string[] types = new string[] { "*", "^", "~~", "[", "]" };
        static string[] replacement = new string[] { "\\*", "\\^", "\\~~", "\\[", "\\]" };
        internal static string DeMarkup(this string str)
        {
            for(int i = 0; i < types.Length; i++)
            {
                if(str.Contains(types[i]))
                {
                    str.Replace(types[i], replacement[i]);
                }
            }
            return str;
        }
    }
}
