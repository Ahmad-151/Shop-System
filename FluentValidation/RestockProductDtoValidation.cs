using FluentValidation;
using ShopAPI.DTOClasses;

namespace ShopAPI.Validators;

public sealed class RestockProductDtoValidation : AbstractValidator<RestockProductDTO>
{
    public RestockProductDtoValidation()
    {
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(1_000_000);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(200);
    }
}
