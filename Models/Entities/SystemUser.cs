namespace AtlasPremierProperties.Models.Entities
{
    public class SystemUser
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string UserRole { get; set; }
    }
}
