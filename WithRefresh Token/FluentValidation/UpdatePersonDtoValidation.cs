using FluentValidation;
using ShopAPI.DTOClasses;

namespace ShopAPI.FluentValidation
{
    public class UpdatePersonDtoValidation : AbstractValidator<UpdatePersonDTO>
    {
        public UpdatePersonDtoValidation()
        {

            RuleFor(x => x.Address).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(250).WithMessage("Address must not exceed 200 characters");

            RuleFor(x => x.PhoneNumber).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^09\d{8}$").WithMessage("Phone number must be a valid Syrian number (format: 09xxxxxxxx)");
        }
    }
}
