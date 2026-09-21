using FluentValidation;
using ShopAPI.DTOClasses;

namespace ShopAPI.FluentValidation
{
    public class LoginDTOValidator : AbstractValidator<LoginDTO>
    {
        public LoginDTOValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^09\d{8}$").WithMessage("Phone number must be a valid Syrian number (format: 09xxxxxxxx)");

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters");
        }
    }
}
