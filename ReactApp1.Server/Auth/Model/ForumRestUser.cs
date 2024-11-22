using Microsoft.AspNetCore.Identity;

namespace ReactApp1.Server.Auth.Model
{
	public class ForumRestUser : IdentityUser
	{
		public bool forceRelogin { get; set; }
	}
}
