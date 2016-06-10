using System;
using System.Data.SqlClient;
namespace Mnemosyne_Of_Mine
{
    class Sql
    {
        /// <summary>
        /// TODO: MAKE THIS WORK
        /// </summary>
        internal static void CreateDatabase()
        {
            string str;
            SqlConnection connection = new SqlConnection();
            str = "CREATE DATABASE RepliedTo ON PRIMARY " +
                "(NAME = RepliedTo_Data, " +
                "FILENAME = 'D:\\RepliedToList.mdf' " +
                "SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%) " +
                "FILENAME = 'D:\\RepliedToLog.ldf', " +
                "SIZE = 1MB, " +
                "MAXSIZE = 5MB, " +
                "FILEGROWTH = 10%)";
            SqlCommand command = new SqlCommand(str, connection);
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                Console.WriteLine("Database successfully created");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + '\n' + e.StackTrace);
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
    }
}
