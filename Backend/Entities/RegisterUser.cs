using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class RegisterUser: IUser
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool ResetPassword { get; set; }
    }
}