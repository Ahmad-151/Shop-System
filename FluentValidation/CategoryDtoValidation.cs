using FluentValidation;
using ShopAPI.DTOClasses;

namespace ShopAPI.Validators;

public class CategoryDtoValidation : AbstractValidator<CategoryDTO>
{
    public CategoryDtoValidation()
    {
        RuleFor(x => x.CategoryName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Category name is required")
            .MaximumLength(100)
            .WithMessage("Category name must not exceed 100 characters");
    }
}
