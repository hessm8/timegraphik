using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace Timegraphik.Data {
    public class SQLSystem {
        public static string ConnectionString => @"Data Source=desktop-t7m86ng\sqlexpress;Initial Catalog=TimegraphikBD;Integrated Security=True;";
		public SqlConnection connection;
        public void Start() {
			connection = new SqlConnection(ConnectionString);
			connection.Open();

   //         string query = @"
			//	create table groups ( name varchar(50) );
			//	create table subjects ( name varchar(50) );
			//	create table teachers ( name varchar(50) );
			//	create table rooms ( name varchar(50) );
			//	create table lessons ( 
			//		day date,
			//		number int,
			//		groups varchar(50),
			//		subjects varchar(50),
			//		teachers varchar(50),
			//		rooms varchar(50)
			//	);
			//";
			//SqlCommand command = new SqlCommand(query, connection);
			//command.ExecuteNonQuery();
		}

    }
}
