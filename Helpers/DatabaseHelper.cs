using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// Add comments
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
    }
}