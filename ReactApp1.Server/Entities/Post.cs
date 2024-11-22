namespace ReactApp1.Server.Entities
{
	public class Post
	{
		public int ID { get;set; }
		string Title { get; set; }
		string Text { get; set; }
		int PosterId { get; set; }
		int DormId { get; set; }
		int Id { get; set; }
		DateTime posted_timestamp { get; set; }
	}
}
