using System;
using System.Configuration;
using System.Data.OleDb;

namespace AtlasPremierProperties.Helpers
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper()
        {
            string dbPath = ConfigurationManager.AppSettings["DatabasePath"];

            _connectionString =
                "Provider=Microsoft.ACE.OLEDB.12.0;" +
                "Data Source=" + dbPath + ";" +
                "Persist Security Info=False;";
        }

        public OleDbConnection GetConnection()
        {
            return new OleDbConnection(_connectionString);
        }

        // OleDb rejects a null parameter ("has no default value"), so optional fields must be sent as DBNull.
        public static object ToDbValue(object value)
        {
            return value ?? DBNull.Value;
        }
    }
}
