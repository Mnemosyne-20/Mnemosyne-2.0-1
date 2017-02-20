using RedditSharp;

namespace Mnemosyne_Of_Mine
{
    internal static class Extensions
    {
        /// <summary>
        /// 
        /// logs you in, simpler for the UserData method
        /// </summary>
        /// <param name="reddit">the reddit object</param>
        /// <param name="user">The user data storage from the config file</param>
        internal static void LogIn(this Reddit reddit, UserData user) => reddit.LogIn(user.Username, user.Password);
        static string[] types = new string[] { "*", "^", "~~", "[", "]", "_" };
        static string[] replacement = new string[] { "\\*", "\\^", "\\~~", "\\[", "\\]", "\\_" };
        /// <summary>
        /// Removes markup names
        /// </summary>
        /// <param name="str">String to remove markup from</param>
        /// <returns>anti-markup backslashes</returns>
        internal static string DeMarkup(this string str)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (str.Contains(types[i]))
                {
                    str.Replace(types[i], replacement[i]);
                }
            }
            return str;
        }
    }
}
