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
		public static string ConnectionString => @"Data Source=desktop-t7m86ng\sqlexpress;Initial Catalog=TimegraphikBD;Integrated Security=True;";
		public static SqlConnection connection;

		private static List<string> tables = new List<string>(){ "groups", "subjects", "teachers", "rooms" };

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
			//		group nvarchar(50),
			//		number int,					
			//		subjects nvarchar(50),
			//		teachers nvarchar(50),
			//		rooms nvarchar(50)
			//	);
			//";
   //         SqlCommand command = new SqlCommand(query, connection);
   //         command.ExecuteNonQuery();
        }

		public static void SchedulesToStorage() {

			var command = connection.CreateCommand();
			command.CommandText = "select * from lessons order by day, groups, number";
			var reader = command.ExecuteReader();

			var classes = MainForm.Storage.Schedules;
			classes.Clear();

			if (reader.HasRows) {

				//DateTime datePrev;

				// Change whenever tested (order & names! groups -> group)

				while (reader.Read()) {
					var day = reader.GetDateTime(0);
					var number = reader.GetInt32(1);
					var group = reader.GetString(2);
					var subjects = reader.GetString(3);
					var teachers = reader.GetString(4);
					var rooms = reader.GetString(5);


					//var lessons = new List<Lesson>();
					//var fieldsBlank = new List<bool>();

					//// Read all lessons
					//for (int l = 0; l < groupCount; l++) {
					//	var lesson = new Lesson();
					//	bool blank = true;

					//	// Read all type fields
					//	for (int t = 0; t < fieldCount; t++) {
					//		var text = AllFields[d, l, t].Text;
					//		lesson[t] = text;
					//		// If all fields are blank
					//		blank = blank && text == "";
					//	}

					//	lessons.Add(lesson);
					//	fieldsBlank.Add(blank);
					//}

					//// Remove empty lessons at end
					//for (int l = groupCount - 1; l >= 0; l--) {
					//	if (fieldsBlank[l]) lessons.RemoveAt(l);
					//	else break;
					//}

					//if (lessons.Count == 0) continue;


					if (!classes.ContainsKey(day)) {
						classes[day] = new Dictionary<string, Lesson[]>();
					}

					if (!classes[day].ContainsKey(group)) {
						classes[day][group] = new Lesson[6];
					}

					classes[day][group][number] = new Lesson(subjects, teachers, rooms);
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

						command.CommandText = "insert into lessons values(@day, @num, @group, @subject, @teacher, @room)";
						command.Parameters.AddWithValue("@day", date);
						command.Parameters.AddWithValue("@num", num);
						command.Parameters.AddWithValue("@group", group);
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

		//private static void TestParameters() {
		//	string query = "INSERT INTO chitateli (familia,imia,otchestvo) VALUES (@familia,@imia,@otchestvo);";
		//	SqlCommand com = new SqlCommand(query, connection);
		//	com.Parameters.AddWithValue("@familia", "abc"); // textBox1.Text (понятно откуда поди) вместо abc
		//	com.Parameters.AddWithValue("@imia", "abc");
		//	com.Parameters.AddWithValue("@otchestvo", "abc");
		//	int chitatel = (int)com.ExecuteScalar();

		//	com.Parameters.Clear();
		//	com.CommandText = "INSERT INTO literatura (nazvanie,kategoria,avtor,izdatelstvo) VALUES (@nazvanie,@kategoria,@avtor,@izdatelstvo);";
		//	com.Parameters.AddWithValue("@nazvanie", "abc");
		//	com.Parameters.AddWithValue("@kategoria", "abc");
		//	com.Parameters.AddWithValue("@avtor", "abc");
		//	com.Parameters.AddWithValue("@izdatelstvo", "abc");
		//	int literatura = (int)com.ExecuteScalar();

		//	com.Parameters.Clear();
		//	com.CommandText = "INSERT INTO vidacha_literaturi (chitatel,literatura,data_vidachi,srok_vidachi) VALUES (@chitatel,@literatura,@data_vidachi,@srok_vidachi);";
		//	com.Parameters.AddWithValue("@chitatel", chitatel);
		//	com.Parameters.AddWithValue("@literatura", literatura);
		//	com.Parameters.AddWithValue("@data_vidachi", "abc");
		//	com.Parameters.AddWithValue("@srok_vidachi", "abc");
		//	com.ExecuteNonQuery();
		//}
    }
}
