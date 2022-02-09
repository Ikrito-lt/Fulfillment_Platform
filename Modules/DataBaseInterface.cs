using Ikrito_Fulfillment_Platform.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ikrito_Fulfillment_Platform.Modules {
    class DataBaseInterface {
        private readonly MySqlConnection connection;

        private string query;
        private string tableName;
        private string where;

        private readonly bool debug = false;

        public DataBaseInterface(string hostname, string username, string password, string database, int port = 3306) {
            String connectionQuery = "Server=" + hostname + ";Database=" + database
               + ";port=" + port + ";User Id=" + username + ";password=" + password + ";SslMode=none";

            connection = new MySqlConnection(connectionQuery);
            connection.Open();
        }

        public DataBaseInterface() {
            String connectionQuery = "Server=" + Globals.DBServer + ";Database=" + Globals.DefaultDB
               + ";port=" + Globals.DBServerPort + ";User Id=" + Globals.DBServerUsername + ";password=" + Globals.DBServerPassword + ";SslMode=none; default command timeout=150;";

            connection = new MySqlConnection(connectionQuery);

            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        public DataBaseInterface Table(string name) {
            tableName = name;

            return this;
        }

        public DataBaseInterface Where(Dictionary<string, Dictionary<string, string>> condition) {
            where = "WHERE " + string.Join(" AND ", condition.Select(
                x => string.Join(" OR ", x.Value.Select(y => "`" + x.Key + "` " + y.Key + " '" + y.Value + "'").ToArray())
            ).ToArray());

            return this;
        }

        public string Insert(Dictionary<string, string> data) {
            string columns = string.Join(", ", data.Select(x => x.Key).ToArray());
            string values = string.Join(", ", data.Select(x => "'" + x.Value + "'").ToArray());

            query = "INSERT INTO `" + tableName + "`(" + columns + ") VALUES (" + values + "); SELECT LAST_INSERT_ID();";

            var a = ExecuteQuery();
            return a[0]["LAST_INSERT_ID()"];
        }

        public Dictionary<int, Dictionary<string, string>> Get(string fields = "*", int rowLimit = 0) {
            string limit = rowLimit > 0 ? " LIMIT " + rowLimit : "";
            query = "SELECT " + fields + " FROM `" + tableName + "` " + where + limit;

            return ExecuteQuery();
        }

        public void Update(Dictionary<string, string> data) {
            string update = string.Join(", ", data.Select(x => "`" + x.Key + "` = '" + x.Value + "'").ToArray());
            query = "UPDATE `" + tableName + "` SET " + update + " " + where;

            ExecuteQuery();
        }

        public void Delete() {
            query = "DELETE FROM `" + tableName + "` " + where;
            ExecuteQuery();
        }

        private void ResetQuery() {
            query = "";
            where = "";
            tableName = "";
        }

        private Dictionary<int, Dictionary<string, string>> ExecuteQuery() {
            if (debug) {
                Console.WriteLine(query);
            }

            var cmd = new MySqlCommand(query, connection);
            var reader = cmd.ExecuteReader();

            var result = new Dictionary<int, Dictionary<string, string>>();

            if (reader.HasRows) {
                int rowCol = 0;
                while (reader.Read()) {
                    var fieldValue = new Dictionary<string, string>();

                    for (int col = 0; col < reader.FieldCount; col++) {
                        fieldValue.Add(reader.GetName(col).ToString(), reader.GetValue(col).ToString());
                    }

                    result.Add(rowCol, fieldValue);
                    rowCol++;
                }

            }

            reader.Close();
            ResetQuery();

            return result;
        }

        public Dictionary<int, Dictionary<string, string>> ExecuteTextQuery(string q)
        {
            if (debug)
            {
                Console.WriteLine(query);
            }

            var cmd = new MySqlCommand(q, connection);
            var reader = cmd.ExecuteReader();

            var result = new Dictionary<int, Dictionary<string, string>>();

            if (reader.HasRows)
            {
                int rowCol = 0;
                while (reader.Read())
                {
                    var fieldValue = new Dictionary<string, string>();

                    for (int col = 0; col < reader.FieldCount; col++)
                    {
                        fieldValue.Add(reader.GetName(col).ToString(), reader.GetValue(col).ToString());
                    }

                    result.Add(rowCol, fieldValue);
                    rowCol++;
                }

            }

            reader.Close();
            ResetQuery();

            return result;
        }

        ~DataBaseInterface() {
            connection.Close();
        }

    }
}
