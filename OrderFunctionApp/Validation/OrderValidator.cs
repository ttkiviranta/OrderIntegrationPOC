using FluentValidation;
using OrderFunctionApp.Models;

namespace OrderFunctionApp.Validation
{
    /// <summary>
    /// Validator for Order entity using FluentValidation.
    /// Ensures that Order entities meet business requirements.
    /// </summary>
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(o => o.OrderId)
                .NotNull()
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("OrderId is required and must not exceed 50 characters");

            RuleFor(o => o.CustomerId)
                .NotNull()
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("CustomerId is required and must not exceed 50 characters");

            RuleFor(o => o.Total)
                .GreaterThan(0)
                .WithMessage("Total must be greater than zero");

            RuleFor(o => o.Description)
                .MaximumLength(255)
                .When(o => !string.IsNullOrEmpty(o.Description))
                .WithMessage("Description must not exceed 255 characters");

            RuleFor(o => o.OrderDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("OrderDate cannot be in the future");

            RuleFor(o => o.Status)
                .IsInEnum()
                .WithMessage("Status must be a valid OrderStatus");
        }
    }
}
