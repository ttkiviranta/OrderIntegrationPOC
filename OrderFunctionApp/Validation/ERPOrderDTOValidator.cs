using FluentValidation;
using OrderFunctionApp.Models.DTOs;

namespace OrderFunctionApp.Validation
{
    /// <summary>
    /// Validator for ERPOrderDTO using FluentValidation.
    /// Ensures that ERP order data meets business requirements before processing.
    /// </summary>
    public class ERPOrderDTOValidator : AbstractValidator<ERPOrderDTO>
    {
        public ERPOrderDTOValidator()
        {
            RuleFor(o => o.ExternalOrderId)
                .NotNull()
                .NotEmpty()
                .WithMessage("ExternalOrderId is required");

            RuleFor(o => o.ExternalCustomerId)
                .NotNull()
                .NotEmpty()
                .WithMessage("ExternalCustomerId is required");

            RuleFor(o => o.CustomerName)
                .NotNull()
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("CustomerName is required and must not exceed 100 characters");

            RuleFor(o => o.CustomerEmail)
                .EmailAddress()
                .WithMessage("CustomerEmail must be a valid email address");

            RuleFor(o => o.Total)
                .GreaterThan(0)
                .WithMessage("Total must be greater than zero");

            RuleFor(o => o.OrderDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("OrderDate cannot be in the future");

            RuleFor(o => o.Description)
                .MaximumLength(255)
                .When(o => !string.IsNullOrEmpty(o.Description))
                .WithMessage("Description must not exceed 255 characters");

            RuleFor(o => o.LineItems)
                .NotNull()
                .NotEmpty()
                .WithMessage("LineItems collection is required and must contain at least one item");

            RuleForEach(o => o.LineItems)
                .SetValidator(new ERPOrderLineDTOValidator());
        }
    }
}
