using FluentValidation;
using ShopAPI.DTOClasses;

namespace ShopAPI.Validators
{
    public class UpdateOrderDtoValidation : AbstractValidator<UpdateOrderDTO>
    {
        private static readonly string[] AllowedStatuses =
            { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

        public UpdateOrderDtoValidation()
        {
            RuleFor(x => x.DeliveredAt).Cascade(CascadeMode.Stop)
                .Must(date => date is null || date <= DateTime.UtcNow).WithMessage("Date Can Not Be In The Future");

            RuleFor(x => x.Status).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Status is required")
                .Must(s => AllowedStatuses.Contains(s))
                .WithMessage("Status must be one of: " + string.Join(", ", AllowedStatuses));
        }
    }
}
