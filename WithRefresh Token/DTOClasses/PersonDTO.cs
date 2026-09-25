namespace ShopAPI.DTOClasses
{
    public class PersonDTO
    {
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
    }
    public class RegisterPersonDTO
    {
        public string PersonName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
    }

    public class UpdatePersonDTO
    {
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }

    public class BannedPersonDTO
    {
        public string Reason { get; set; }
        public DateTime BannedAt { get; set; }
    }
    public class PersonAuthDTO
    {
        public int Person_ID { get; set; }
        public string PersonName { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
