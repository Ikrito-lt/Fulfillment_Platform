using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Ikrito_Fulfillment_Platform.Utils;

namespace Ikrito_Fulfillment_Platform.Modules {
    class DataBaseInterface {

        //todo: add db source
        private readonly string dataSource;

        public DataBaseInterface(string source) {
            dataSource = source;
        }

        public void ExecNonQuery(string CommandText) {
            using SQLiteConnection c = new(dataSource); c.Open();
            using (SQLiteCommand cmd = new(CommandText, c)) {
                cmd.ExecuteNonQuery();
            }
            c.Close();
        }

        public SQLiteDataReader ExecQuery(string CommandText) {

            SQLiteConnection connection = new(dataSource);
            connection.Open();
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = CommandText;
            var QueryReader = cmd.ExecuteReader();

            return QueryReader;
        }

       
    }
}
