using FluentValidation;
using ShopAPI.DTOClasses;

namespace ShopAPI.FluentValidation
{
    public class RegisterPersonDtoValidation : AbstractValidator<RegisterPersonDTO>
    {
        public RegisterPersonDtoValidation()
        {
            RuleFor(x => x.PersonName).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Person name is required")
                .MinimumLength(3).WithMessage("Person name must be at least 3 characters long")
                .MaximumLength(150).WithMessage("Person name must not exceed 150 characters");

            RuleFor(x => x.Address).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(250).WithMessage("Address must not exceed 250 characters");

            RuleFor(x => x.PhoneNumber).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^09\d{8}$").WithMessage("Phone number must be a valid Syrian number (format: 09xxxxxxxx)");

            RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters");
        }
    }
}
