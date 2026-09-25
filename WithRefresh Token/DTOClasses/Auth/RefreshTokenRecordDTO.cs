namespace ShopAPI.DTOClasses.Auth
{
    public class RefreshTokenRecordDTO
    {
        public int Token_ID { get; set; }
        public int Person_ID { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
