using MySql.Data.MySqlClient;

namespace ReactApp1.Server.Services
{
	public class DormService
	{
		private readonly string _connectionString;

		public DormService(string connectionString)
		{
			_connectionString = connectionString;
		}

		public void PrintDorms()
		{
			/*
			// Define the query to get the data from Dorm table
			string query = "SELECT Id, Name FROM Dorm";

			// Create a new MySqlConnection
			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();

				// Create a MySqlCommand to execute the query
				using (var command = new MySqlCommand(query, connection))
				{
					// Execute the query and get the results using a MySqlDataReader
					using (var reader = command.ExecuteReader())
					{
						// Read each record
						while (reader.Read())
						{
							// Get Id and Name from the current row
							int id = reader.GetInt32("Id");
							string name = reader.GetString("Name");

							// Print the data
							Console.WriteLine($"Id: {id}, Name: {name}");
						}
					}
				}
			}*/
			var dorms = GetDorms();
			foreach (var dorm in dorms)
			{
				Console.WriteLine($"Dorm: {dorm.Name}, Id: {dorm.Id}");
			}
		}
		public List<DormDto> GetDorms()
		{
			var dorms = new List<DormDto>();

			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "SELECT Id, Name FROM Dorm";
			using var command = new MySqlCommand(query, connection);
			using var reader = command.ExecuteReader();

			while (reader.Read())
			{
				var dorm = new DormDto(
					reader.GetInt32("Id"),
					reader.GetString("Name")
				);
				dorms.Add(dorm);
			}

			return dorms;
		}

		public DormDto GetDormById(int dormId)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "SELECT Id, Name FROM Dorm WHERE Id = @Id";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@Id", dormId);

			using var reader = command.ExecuteReader();

			if (reader.Read())
			{
				return new DormDto(
					reader.GetInt32("Id"),
					reader.GetString("Name")
				);
			}

			return null;
		}

		public DormDto CreateDorm(CreateDormDto dto)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "INSERT INTO Dorm (Name) VALUES (@Name); SELECT LAST_INSERT_ID();";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@Name", dto.Name);
			int newId = Convert.ToInt32(command.ExecuteScalar());
			return new DormDto(newId, dto.Name);
		}
		public void UpdateDorm(UpdateDormDto dto, int dormId)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "UPDATE Dorm SET Name = @Name WHERE Id = @Id";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@Name", dto.Name);
			command.Parameters.AddWithValue("@Id", dormId);

			command.ExecuteNonQuery();
		}

		public void DeleteDorm(int dormId)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "DELETE FROM Dorm WHERE Id = @Id";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@Id", dormId);

			command.ExecuteNonQuery();
		}


	}
}
