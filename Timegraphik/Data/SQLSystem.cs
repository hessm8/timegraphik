using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using Timegraphik.Forms;

namespace Timegraphik.Data {
    public static class SQLSystem {
		public static string ConnectionString => @"Data Source=desktop-t7m86ng\sqlexpress;Initial Catalog=TimegraphikBD;Integrated Security=True;";
		public static SqlConnection connection;

		public static List<string> tables = new List<string>(){ "groups", "subjects", "teachers", "rooms" };

        public static void Start() {
			connection = new SqlConnection(ConnectionString);
			connection.Open();

   //         string query = @"
			//	create table groups ( name nvarchar(50) );
			//	create table subjects ( name nvarchar(50) );
			//	create table teachers ( name nvarchar(50) );
			//	create table rooms ( name nvarchar(50) );
			//	create table lessons ( 
			//		day date,
			//		number int,
			//		groups nvarchar(50),
			//		subjects nvarchar(50),
			//		teachers nvarchar(50),
			//		rooms nvarchar(50)
			//	);
			//";
   //         SqlCommand command = new SqlCommand(query, connection);
   //         command.ExecuteNonQuery();
        }

		public static void Unload() {
			QueryTables(t => $"delete from {t}");
			QueryTables(t => {
				var data = MainForm.Storage.Data[tables.IndexOf(t)];

				string values = "(";
				foreach (var entry in data) {
					values += $"'{entry}'), (";
                }
				values = values.Substring(0, values.Length - 3);

				return $"insert into {t} (name) values {values};";
			});
		}

		public static void QueryTables(Func<string, string> query) {
			foreach (var table in tables) {
				string tableQuery = query(table);
				Query(tableQuery);
			}
		}

		public static int Query(string query) {
			var command = new SqlCommand(query, connection);
			return command.ExecuteNonQuery();
		}

    }
}
