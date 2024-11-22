using MySql.Data.MySqlClient;
using ReactApp1.Server.Entities;

namespace ReactApp1.Server.Services
{
	public class ResidentService
	{
		private readonly string _connectionString;

		public ResidentService(string connectionString)
		{
			_connectionString = connectionString;
		}

		public void PrintResidents(int dormId)
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
			var residents = GetResidents(dormId);
			foreach (var resident in residents)
			{
				Console.WriteLine($"Firstname: {resident.Firstname}, Lastname: {resident.Lastname}, RoomNumber: {resident.RoomNumber}, ID:{resident.Id}");
			}
		}
		public List<ResidentDto> GetResidents(int DormId)
		{
			var dorms = new List<ResidentDto>();

			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "SELECT Id, FirstName, LastName, RoomNumber, DormId FROM residents WHERE DormId = @did;";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@did", DormId);
			using var reader = command.ExecuteReader();

			while (reader.Read())
			{
				var dorm = new ResidentDto(
					reader.GetInt32("Id"),
					reader.GetString("FirstName"),
					reader.GetString("LastName"),
					reader.GetInt32("RoomNumber"),
					reader.GetInt32("DormId")
				);
				dorms.Add(dorm);
			}

			return dorms;
		}

		public ResidentDto GetResidentsById(int dormId, int id)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "SELECT Id, FirstName, LastName, RoomNumber, DormId FROM Residents WHERE Id = @Id AND DormId = @did";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@did", dormId);
			command.Parameters.AddWithValue("@Id", id);

			using var reader = command.ExecuteReader();

			if (reader.Read())
			{
				return new ResidentDto(
					reader.GetInt32("Id"),
					reader.GetString("FirstName"),
					reader.GetString("LastName"),
					reader.GetInt32("RoomNumber"),
					reader.GetInt32("DormId")
				);
			}

			return null;
		}

		public ResidentDto CreateResidents(CreateResidentDto dto, int dormId)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "INSERT INTO Residents (FirstName, LastName, RoomNumber, DormId) VALUES (@FirstName, @LastName, @RoomNumber, @DormId); SELECT LAST_INSERT_ID();";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@FirstName", dto.Firstname);
			command.Parameters.AddWithValue("@LastName", dto.Lastname);
			command.Parameters.AddWithValue("@RoomNumber", dto.RoomNumber);
			command.Parameters.AddWithValue("@DormId",dormId);
			int newId = Convert.ToInt32(command.ExecuteScalar());
			return new ResidentDto(newId, dto.Firstname, dto.Lastname, dto.RoomNumber, dormId);

		}
		public void UpdateResidents(UpdateResidentDto dto, int dormId, int Id)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "UPDATE Residents SET FirstName = @FirstName, LastName = @LastName, RoomNumber = @RoomNumber WHERE Id = @Id";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@FirstName", dto.Firstname);
			command.Parameters.AddWithValue("@LastName", dto.Lastname);
			command.Parameters.AddWithValue("@RoomNumber", dto.RoomNumber);
			command.Parameters.AddWithValue("@Id", Id);

			command.ExecuteNonQuery();
		}
		public void SetUserIdOfResident(int residentId, string userId)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "UPDATE Residents SET UserId = @UserId WHERE Id = @Id";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@UserId", userId);
			command.Parameters.AddWithValue("@Id", residentId);

			command.ExecuteNonQuery();
		}
		public string FindUserIdOfResident(int id)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "SELECT UserId FROM Residents WHERE Id = @Id";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@Id", id);

			using var reader = command.ExecuteReader();

			if (reader.Read())
			{
				return reader.GetString("UserId");
			}

			return null;
		}
		public void DeleteResidents(int dormId,int resId)
		{
			DeletePostsOfResident(dormId, resId);
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "DELETE FROM Residents WHERE Id = @Id AND DormId = @DormId";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@DormId", dormId);
			command.Parameters.AddWithValue("@Id", resId);

			command.ExecuteNonQuery();
		}
		public void DeletePostsOfResident(int dormId, int resId)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "DELETE FROM post WHERE PosterID = @Id AND DormId = @DormId";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@DormId", dormId);
			command.Parameters.AddWithValue("@Id", resId);

			command.ExecuteNonQuery();
		}
	}
}
