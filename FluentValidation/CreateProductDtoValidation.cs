using FluentValidation;
using ShopAPI.DTOClasses;

namespace ShopAPI.FluentValidation
{
    public class CreateProductDtoValidation : AbstractValidator<CreateProductDTO>
    {
        public CreateProductDtoValidation()
        {
            RuleFor(x => x.Description).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

            RuleFor(x => x.UnitPrice).Cascade(CascadeMode.Stop)
                .GreaterThanOrEqualTo(1).WithMessage("UnitPrice must be a positive number.");

            RuleFor(x => x.ProductName).Cascade(CascadeMode.Stop)
           .NotEmpty().WithMessage("ProductName is required")
           .MinimumLength(5).WithMessage("ProductName must be at least 5 characters long")
           .MaximumLength(150).WithMessage("ProductName must not exceed 150 characters");

            RuleFor(x => x.CategoryId).Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Category Id must be a positive number");
        }
    }
}
