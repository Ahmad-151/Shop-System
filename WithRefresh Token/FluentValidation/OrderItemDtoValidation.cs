using FluentValidation;
using ShopAPI.DTOClasses;

namespace ShopAPI.FluentValidation
{
    public class OrderItemDtoValidation : AbstractValidator<CreateOrderItemsDTO>
    {
       public OrderItemDtoValidation()
       {
            RuleFor(X => X.Quantity).Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Quantity Must Be a Positive Number");

            RuleFor(X => X.ProductId).Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("ProductId Must Be a Positive Number");
       }
    }
}
