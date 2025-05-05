namespace SimpleAuthApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
        public string Hobbies { get; set; }
        public string City { get; set; }
        public string Role { get; set; }
    }
}
