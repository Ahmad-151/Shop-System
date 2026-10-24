using FluentValidation;
using ShopAPI.DTOClasses;

namespace ShopAPI.Validators
{
    public class OrderItemDtoValidation : AbstractValidator<CreateOrderItemsDTO>
    {
        private const int MaxQuantityPerItem = 1000;

        public OrderItemDtoValidation()
        {
            RuleFor(x => x.Quantity).Cascade(CascadeMode.Stop)
                .InclusiveBetween(1, MaxQuantityPerItem)
                .WithMessage($"Quantity must be between 1 and {MaxQuantityPerItem}");

            RuleFor(x => x.ProductId).Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("ProductId Must Be a Positive Number");
        }
    }
}
