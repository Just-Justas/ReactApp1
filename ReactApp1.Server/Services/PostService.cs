using MySql.Data.MySqlClient;

namespace ReactApp1.Server.Services
{
	public class PostService
	{
		private readonly string _connectionString;

		public PostService(string connectionString)
		{
			_connectionString = connectionString;
		}
		public List<PostDto> GetPosts(int DormId, int PosterId)
		{
			var dorms = new List<PostDto>();

			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "SELECT Title, Text, PosterID, DormId, ID, posted_timestamp FROM post WHERE posterID = @posterID AND dormId = @dormId;";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@posterID", PosterId);
			command.Parameters.AddWithValue("@dormId", DormId);
			using var reader = command.ExecuteReader();

			while (reader.Read())
			{
				var dorm = new PostDto(
					reader.GetString("Title"),
					reader.GetString("Text"),
					reader.GetInt32("PosterID"),
					reader.GetInt32("DormId"),
					reader.GetInt32("ID"),
					reader.GetDateTime("posted_timestamp")
				);
				dorms.Add(dorm);
			}

			return dorms;
		}

		public PostDto GetPostById(int dormId, int posterId, int id)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "SELECT Title, Text, PosterID, DormId, ID, posted_timestamp FROM Post WHERE Id = @Id AND DormId = @did AND PosterID=@posterid";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@did", dormId);
			command.Parameters.AddWithValue("@Id", id);
			command.Parameters.AddWithValue("@posterid", posterId);

			using var reader = command.ExecuteReader();

			if (reader.Read())
			{
				return new PostDto(
					reader.GetString("Title"),
					reader.GetString("Text"),
					reader.GetInt32("PosterID"),
					reader.GetInt32("DormId"),
					reader.GetInt32("ID"),
					reader.GetDateTime("posted_timestamp")
				);
			}

			return null;
		}

		public PostDto CreatePost(CreatePostDto dto, int dormId, int posterId)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "INSERT INTO post (Title, Text, PosterID, DormId, posted_timestamp) VALUES (@Title, @Text, @PosterID, @DormId, @posted_timestamp); SELECT LAST_INSERT_ID();";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@Title", dto.Title);
			command.Parameters.AddWithValue("@Text", dto.Text);
			command.Parameters.AddWithValue("@PosterID", posterId);
			command.Parameters.AddWithValue("@DormId", dormId);
			DateTime created = DateTime.Now;
			command.Parameters.AddWithValue("@posted_timestamp", created);

			int newId = Convert.ToInt32(command.ExecuteScalar());
			return new PostDto(dto.Title, dto.Text, posterId, dormId, newId, created);
		}
		public void UpdatePost(UpdatePostDto dto, int dormId, int posterId, int Id)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "UPDATE post SET Title = @Title, Text = @Text WHERE dormId = @dormId AND posterId = @posterId AND Id = @Id";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@Title", dto.Title);
			command.Parameters.AddWithValue("@Text", dto.Text);
			command.Parameters.AddWithValue("@dormId", dormId);
			command.Parameters.AddWithValue("@posterId", posterId);
			command.Parameters.AddWithValue("@Id", Id);

			command.ExecuteNonQuery();
		}

		public void DeletePost(int dormId, int resId, int Id)
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.Open();

			string query = "DELETE FROM post WHERE Id = @Id AND posterId = @resId AND DormId = @DormId";
			using var command = new MySqlCommand(query, connection);
			command.Parameters.AddWithValue("@DormId", dormId);
			command.Parameters.AddWithValue("@resId", resId);
			command.Parameters.AddWithValue("@Id", Id);

			command.ExecuteNonQuery();
		}


	}
}
