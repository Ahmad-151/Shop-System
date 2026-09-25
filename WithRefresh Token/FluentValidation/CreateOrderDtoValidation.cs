using FluentValidation;
using ShopAPI.DTOClasses;
using System.Data;

namespace ShopAPI.FluentValidation
{
    public class CreateOrderDtoValidation : AbstractValidator<CreateOrderDTO>
    {
        public CreateOrderDtoValidation()
        {

            RuleFor(X => X.CreatedAt).Cascade(CascadeMode.Stop)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Date Can Not Be In The Future");

            RuleForEach(x => x.Items).Cascade(CascadeMode.Stop).SetValidator(new OrderItemDtoValidation());
        }
    }
}
