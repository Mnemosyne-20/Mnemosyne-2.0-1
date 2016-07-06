using System.Data.SQLite;
namespace Mnemosyne_Of_Mine
{
    static class Sql
    {
        static void UpdateDatabase(string postID, string commentID, string link, string archiveLink)
        {
            var myConnection = new SQLiteConnection("Data Source=Data.sqlite;Version=3");
            myConnection.Open();
        }
        static void CreateDatabsae()
        {
            SQLiteConnection.CreateFile("Data.sqlite");
            var myConnection = new SQLiteConnection("Data Source=Data.sqlite;Version=3");
            myConnection.Open();
            string sql = "CREATE TABLE postArchiveList (name ";
        }
    }
}
