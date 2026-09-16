using FluentValidation;
using OrderFunctionApp.Models;

namespace OrderFunctionApp.Validation
{
    /// <summary>
    /// Validator for Customer entity using FluentValidation.
    /// Ensures that Customer entities meet business requirements.
    /// </summary>
    public class CustomerValidator : AbstractValidator<Customer>
    {
        public CustomerValidator()
        {
            RuleFor(c => c.CustomerId)
                .NotNull()
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("CustomerId is required and must not exceed 50 characters");

            RuleFor(c => c.Name)
                .NotNull()
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Name is required and must not exceed 100 characters");

            RuleFor(c => c.Email)
                .NotNull()
                .EmailAddress()
                .MaximumLength(100)
                .WithMessage("Email is required and must be a valid email address");

            RuleFor(c => c.Phone)
                .MaximumLength(20)
                .When(c => !string.IsNullOrEmpty(c.Phone))
                .WithMessage("Phone must not exceed 20 characters");

            RuleFor(c => c.RegistrationNumber)
                .MaximumLength(50)
                .When(c => !string.IsNullOrEmpty(c.RegistrationNumber))
                .WithMessage("RegistrationNumber must not exceed 50 characters");

            RuleFor(c => c.BillingAddress)
                .MaximumLength(255)
                .When(c => !string.IsNullOrEmpty(c.BillingAddress))
                .WithMessage("BillingAddress must not exceed 255 characters");

            RuleFor(c => c.ShippingAddress)
                .MaximumLength(255)
                .When(c => !string.IsNullOrEmpty(c.ShippingAddress))
                .WithMessage("ShippingAddress must not exceed 255 characters");
        }
    }
}
