using ShopAPI.DTOClasses;

using FluentValidation;

namespace ShopAPI.FluentValidation
{
    public class BannedPersonDtoValidation : AbstractValidator<BannedPersonDTO>
    {
        public BannedPersonDtoValidation()
        {
            RuleFor(x => x.Reason).Cascade(CascadeMode.Stop)
             .NotEmpty().WithMessage("Reason is required")
             .MinimumLength(10).WithMessage("Reason must be at least 10 characters long")
             .MaximumLength(200).WithMessage("Reason must not exceed 200 characters");

            RuleFor(X => X.BannedAt).Cascade(CascadeMode.Stop)
              .LessThanOrEqualTo(DateTime.Now).WithMessage("Date Can Not Be In The Future");
        }
    }
}
