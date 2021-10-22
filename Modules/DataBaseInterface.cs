using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Ikrito_Fulfillment_Platform.Utils;
using MySql.Data.MySqlClient;

namespace Ikrito_Fulfillment_Platform.Modules {
    class DataBaseInterface {

        //todo: add db source
        //private readonly string dataSource;
        
        private string serverEndpoint = "YourServer";
        private string databaseName = "YourDatabase";
        private string userName = "YourUsername";
        private string password = "YourPassword";

        private readonly MySqlConnection DBcon;

        public DataBaseInterface(string source) {
            string connStr = $"server={Globals.endpoint};user={Globals.user};database={Globals.db};port={Globals.port};password={Globals.pass}";
            DBcon = new MySqlConnection(connStr);
        }

        //public void ExecNonQuery(string CommandText) {
        //    using (SQLiteCommand cmd = new(CommandText, c)) {
        //        cmd.ExecuteNonQuery();
        //    }
        //    c.Close();
        //}

        //public SQLiteDataReader ExecQuery(string CommandText) {
        //    SQLiteConnection connection = new(dataSource);
        //    connection.Open();
        //    SQLiteCommand cmd = connection.CreateCommand();
        //    var QueryReader = cmd.ExecuteReader();
        //    cmd.CommandText = CommandText;
        //    return QueryReader;
        //}

        private void connectMYSQL() {

            var dbCon = new DbConnection;
            dbCon.Server = "YourServer";
            dbCon.DatabaseName = "YourDatabase";
            dbCon.UserName = "YourUsername";
            dbCon.Password = "YourPassword";
            if (dbCon.IsConnect()) {
                //suppose col0 and col1 are defined as VARCHAR in the DB
                string query = "SELECT col0,col1 FROM YourTable";
                var cmd = new MySqlCommand(query, dbCon.Connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    string someStringFromColumnZero = reader.GetString(0);
                    string someStringFromColumnOne = reader.GetString(1);
                    Console.WriteLine(someStringFromColumnZero + "," + someStringFromColumnOne);
                }
                dbCon.Close();
            }
        }


    }
}
