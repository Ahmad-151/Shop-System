using FluentValidation;
using ShopAPI.DTOClasses;

namespace ShopAPI.FluentValidation
{
    public class UpdateProductDtoValidation : AbstractValidator<UpdateProductDTO>
    {
        public UpdateProductDtoValidation()
        {
            RuleFor(x => x.Description).Cascade(CascadeMode.Stop)
             .NotEmpty().WithMessage("Description is required")
             .MinimumLength(10).WithMessage("Description must be at least 10 characters long")
             .MaximumLength(1000).WithMessage("Description must not exceed 100 characters");

            RuleFor(x => x.UnitPrice).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("UnitPrice is required")
                .GreaterThanOrEqualTo(1).WithMessage("UnitPrice must be a positive number .");


        }
    }
}
