using FluentValidation;
using OrderFunctionApp.Models.DTOs;

namespace OrderFunctionApp.Validation
{
    /// <summary>
    /// Validator for ERPOrderLineDTO using FluentValidation.
    /// Ensures that order line items meet business requirements.
    /// </summary>
    public class ERPOrderLineDTOValidator : AbstractValidator<ERPOrderLineDTO>
    {
        public ERPOrderLineDTOValidator()
        {
            RuleFor(ol => ol.LineNumber)
                .GreaterThan(0)
                .WithMessage("LineNumber must be greater than zero");

            RuleFor(ol => ol.ProductId)
                .NotNull()
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("ProductId is required and must not exceed 50 characters");

            RuleFor(ol => ol.Description)
                .NotNull()
                .NotEmpty()
                .MaximumLength(255)
                .WithMessage("Description is required and must not exceed 255 characters");

            RuleFor(ol => ol.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero");

            RuleFor(ol => ol.Unit)
                .NotNull()
                .NotEmpty()
                .MaximumLength(20)
                .WithMessage("Unit is required and must not exceed 20 characters");

            RuleFor(ol => ol.UnitPrice)
                .GreaterThan(0)
                .WithMessage("UnitPrice must be greater than zero");

            RuleFor(ol => ol.LineTotal)
                .GreaterThan(0)
                .WithMessage("LineTotal must be greater than zero");

            RuleFor(ol => ol.DiscountPercentage)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(100)
                .When(ol => ol.DiscountPercentage.HasValue)
                .WithMessage("DiscountPercentage must be between 0 and 100");

            RuleFor(ol => ol.Notes)
                .MaximumLength(255)
                .When(ol => !string.IsNullOrEmpty(ol.Notes))
                .WithMessage("Notes must not exceed 255 characters");
        }
    }
}
