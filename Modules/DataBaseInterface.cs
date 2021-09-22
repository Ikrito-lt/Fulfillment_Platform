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
        public SQLiteConnection SQLiteConnection;
        public SQLiteCommand SQLiteCommand;

        public DataBaseInterface(string source) {
            dataSource = source;
            SQLiteConnection = new SQLiteConnection(dataSource);
            SQLiteConnection.Open();
        }

        public int ExecNonQuery(string CommandText) {

            SQLiteCommand = SQLiteConnection.CreateCommand();
            SQLiteCommand.CommandText = CommandText;
            return SQLiteCommand.ExecuteNonQuery();
        }

        public SQLiteDataReader ExecQuery(string CommandText) {

            SQLiteCommand = SQLiteConnection.CreateCommand();
            SQLiteCommand.CommandText = CommandText;
            var QueryReader = SQLiteCommand.ExecuteReader();

            return QueryReader;
        }


        public void CloseConnection() {
            this.SQLiteConnection.Close();
        }

        ~DataBaseInterface() {
            this.SQLiteConnection.Close();
        }

    }
}
