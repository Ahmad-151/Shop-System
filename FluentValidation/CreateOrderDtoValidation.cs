using FluentValidation;
using ShopAPI.DTOClasses;

namespace ShopAPI.Validators
{
    public class CreateOrderDtoValidation : AbstractValidator<CreateOrderDTO>
    {
        private const int MaxItemsPerOrder = 50;

        public CreateOrderDtoValidation()
        {
            RuleFor(x => x.Items).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Items are required")
                .NotEmpty().WithMessage("An order must contain at least one item")
                .Must(items => items.Count <= MaxItemsPerOrder)
                    .WithMessage($"An order cannot contain more than {MaxItemsPerOrder} items")
                .Must(items => items.Select(i => i.ProductId).Distinct().Count() == items.Count)
                    .WithMessage("Each product may appear only once per order; increase its quantity instead");

            RuleForEach(x => x.Items).SetValidator(new OrderItemDtoValidation());
        }
    }
}
