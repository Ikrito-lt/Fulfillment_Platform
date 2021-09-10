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
        static private string dataSource = Globals.DBsource;
        public SQLiteConnection SQLiteConnection;
        public SQLiteCommand SQLiteCommand;
        public SQLiteDataReader SQLiteDataReader;

        public string CommandText { get; set; }

        public DataBaseInterface() {

            SQLiteConnection = new SQLiteConnection(dataSource);
            SQLiteConnection.Open();
        }

        public void execCommand() {

            SQLiteCommand = SQLiteConnection.CreateCommand();
            SQLiteCommand.CommandText = CommandText;
            SQLiteDataReader = SQLiteCommand.ExecuteReader();

        }

        public void closeConnection() {
            this.SQLiteConnection.Close();
        }



    }
}
