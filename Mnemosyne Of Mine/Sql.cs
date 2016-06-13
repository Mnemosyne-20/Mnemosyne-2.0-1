using System;
using System.Data.SqlClient;
namespace Mnemosyne_Of_Mine
{
    static class Sql
    {
        static void UpdateDatabase()
        {
            SqlConnection connection = new SqlConnection(@"..\ ..\ArchiveLinkDatabase.mdf");
            SqlCommand command = new SqlCommand("SELECT FROM TABLE1 *");
        }
    }
}
