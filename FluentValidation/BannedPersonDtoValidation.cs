using FluentValidation;
using ShopAPI.DTOClasses;

namespace ShopAPI.Validators
{
    public class BannedPersonDtoValidation : AbstractValidator<BannedPersonDTO>
    {
        public BannedPersonDtoValidation()
        {
            RuleFor(x => x.Reason).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Reason is required")
                .MinimumLength(10).WithMessage("Reason must be at least 10 characters long")
                .MaximumLength(200).WithMessage("Reason must not exceed 200 characters");
        }
    }
}
