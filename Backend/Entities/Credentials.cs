namespace Backend.Entities
{
    public class Credentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class CredentialsReset : Credentials
    {
        public string NewPassword { get; set; }
    }
}