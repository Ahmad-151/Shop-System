namespace ShopAPI.DTOClasses
{
    // يُستخدم لعرض بيانات شخص موجود بالفعل (Response DTO) - يتضمّن المعرّف
    // حتى يستطيع العميل استخدامه لاحقاً في GetById / Update / Delete.
    public class PersonDTO
    {
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
    }

    // يُستخدم حصراً لتسجيل حساب جديد (عبر AuthController). لا يُستخدم كـ Response DTO أبداً
    // حتى لا تُعاد كلمة المرور - ولو كانت مُشفّرة - ضمن أي استجابة API.
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

    // DTO داخلي فقط (لا يُعاد للعميل) يُستخدم أثناء تدفّق تسجيل الدخول للتحقق من كلمة المرور
    // وبناء الـ Claims الخاصة بالتوكن، لذلك يحمل الحقول الحساسة (PasswordHash) والـ Role.
    public class PersonAuthDTO
    {
        public int Person_ID { get; set; }
        public string PersonName { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
