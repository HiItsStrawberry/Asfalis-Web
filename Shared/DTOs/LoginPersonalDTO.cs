using FluentValidation;

namespace asfalis.Shared.DTOs
{
    // A data transfer object for personal information login
    public class LoginPersonalDTO
    {
        public string? Name { get; set; }

        public string? Password { get; set; }
    }

    public class LoginPersonalValidator : AbstractValidator<LoginPersonalDTO>
    {
        // Validation for the selected properties of personal information login
        public LoginPersonalValidator()
        {
            // Username validation
            RuleFor(x => x.Name).NotEmpty().WithMessage("The {PropertyName} cannot be empty.");
            // Password validation
            RuleFor(x => x.Password).NotEmpty().WithMessage("The {PropertyName} cannot be empty.");
        }
    }
}
