using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using Timegraphik.Forms;
using System.Windows.Forms;

namespace Timegraphik.Data {
    public static class SQLSystem {
		public static string ConnectionString { get; set; }
		public static SqlConnection connection;

		private static List<string> tables = new List<string>(){ "groups", "subjects", "teachers", "rooms" };

        public static void Start() {
			if (!MainForm.Storage.Settings.Initialized) {
				string connectionString = @"Data Source=desktop-t7m86ng\sqlexpress;Initial Catalog=master;Integrated Security=True;";
				using (SqlConnection connection = new SqlConnection(connectionString)) {
					connection.Open();
					SqlCommand cmd = new SqlCommand();
					cmd.CommandText = @"
						CREATE DATABASE timegraphik 
						COLLATE Cyrillic_General_CI_AS;
					";
					cmd.Connection = connection;
					cmd.ExecuteNonQuery();
				}
			}

			connection = new SqlConnection(ConnectionString);
			connection.Open();

			if (!MainForm.Storage.Settings.Initialized) {
				string query = @"
					create table groups ( name nvarchar(50) );
					create table subjects ( name nvarchar(50) );
					create table teachers ( name nvarchar(50) );
					create table rooms ( name nvarchar(50) );
					create table lessons ( 
						day date,
						groupname nvarchar(50),
						number int,					
						subject nvarchar(50),
						teacher nvarchar(50),
						room nvarchar(50)
					);
				";
				SqlCommand command = new SqlCommand(query, connection);
				command.ExecuteNonQuery();

				MainForm.Storage.Settings.Initialized = true;
			}
        }

		public static void SchedulesToStorage() {

			var command = connection.CreateCommand();
			command.CommandText = "select * from lessons order by day, groupname, number";
			var reader = command.ExecuteReader();

			var classes = MainForm.Storage.Schedules;
			classes.Clear();

			if (reader.HasRows) {

				while (reader.Read()) {
					var day = reader.GetDateTime(0);					
					var group = reader.GetString(1);
					var number = reader.GetInt32(2);
					var subject = reader.GetString(3);
					var teacher = reader.GetString(4);
					var room = reader.GetString(5);

					if (!classes.ContainsKey(day)) {
						classes[day] = new Dictionary<string, Lesson[]>();
					}

					if (!classes[day].ContainsKey(group)) {
						classes[day][group] = new Lesson[6];
					}

					classes[day][group][number] = new Lesson(subject, teacher, room);
				}
			}
			reader.Close();	
		}

		public static void StorageToSchedules() {
			var command = connection.CreateCommand();

			command.CommandText = "delete from lessons";
			command.ExecuteNonQuery();

			var classes = MainForm.Storage.Schedules;
			foreach (var schedule in classes) {
				var date = schedule.Key;

				foreach (var groupLessons in classes[date]) {
					var group = groupLessons.Key;

					var lessons = classes[date][group];
					for (int num = 0; num < lessons.Length; num++) {
						var lesson = lessons[num];

						command.Parameters.Clear();

						command.CommandText = "insert into lessons values(@day, @groupname, @num, @subject, @teacher, @room)";
						command.Parameters.AddWithValue("@day", date);						
						command.Parameters.AddWithValue("@groupname", group);
						command.Parameters.AddWithValue("@num", num);
						command.Parameters.AddWithValue("@subject", lesson.subject);
						command.Parameters.AddWithValue("@teacher", lesson.teacher);
						command.Parameters.AddWithValue("@room", lesson.room);

						command.ExecuteNonQuery();
					}
				}
			}

		}

		public static void TablesToStorage() {
			var command = connection.CreateCommand();

			foreach (string table in tables) {				
				command.CommandText = $"select * from {table}";
				var reader = command.ExecuteReader();

				var data = MainForm.Storage.Data[table];
				data.Clear();

				if (reader.HasRows) {
					while (reader.Read()) {
						var name = reader.GetString(0);
						data.Add(name);
					}
				}
				reader.Close();
			}
		}

		public static void StorageToTables() {
			QueryTables(t => $"delete from {t}");
			try {
				QueryTables(t => {
					var data = MainForm.Storage.Data[tables.IndexOf(t)];

					string values = "(";
					foreach (var entry in data) {
						values += $"'{entry}'), (";
					}
					values = values.Substring(0, values.Length - 3);

					return $"insert into {t} (name) values {values};";
				});
			} catch {
				return; 
			}
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
