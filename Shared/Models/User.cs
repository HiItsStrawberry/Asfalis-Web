using asfalis.Shared.Services;
using FluentValidation;
using System.Text.Json.Serialization;

namespace asfalis.Shared.Models
{
    // A table for user
    public class User
    {
        public int UserId { get; set; }

        public string? Username { get; set; }

        public string? Email { get; set; }

        public bool EmailConfirmed { get; set; } = false;

        public char? Gender { get; set; }

        public string? Password { get; set; }

        public string? ConfirmPassword { get; set; }

        public Nullable<DateTime> LockoutEnd { get; set; }

        public int AccessFailedTime { get; set; } = 0;

        [JsonIgnore]
        public virtual QRCode? QRCode { get; set; }

        [JsonIgnore]
        public virtual ICollection<Image>? Images { get; set; }

        [JsonIgnore]
        public virtual List<UserImage>? UserImages { get; set; }
    }

    public class UserValidator : AbstractValidator<User>
    {
        private readonly ICustomUserValidator _userValidator;

        // Validation for the selected properties of user
        public UserValidator(ICustomUserValidator userValidator)
        {
            this._userValidator = userValidator;

            // Username validation
            RuleFor(x => x.Username)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The {PropertyName} cannot be empty.")
                .MinimumLength(6).WithMessage("The {PropertyName} cannot less than 6 characters.")
                .MaximumLength(25).WithMessage("The {PropertyName} cannot more than 25 characters.")
                .Matches(@"^[a-zA-Z0-9_.-]+$").WithMessage("The {PropertyName} contains an invalid character.")
                .MustAsync(async (username, cancellation) =>
                {
                    bool exist = await _userValidator.CheckUsername(username, cancellation);
                    return exist;
                })
                .WithMessage("The {PropertyName} '{PropertyValue}' has already been taken.");

            // Email validation
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The {PropertyName} cannot be empty.")
                .EmailAddress().WithMessage("The {PropertyName} must be a valid email.")
                .MaximumLength(50).WithMessage("The {PropertyName} cannot more than 50 characters.")
                .MustAsync(async (email, cancellation) =>
                {
                    bool exist = await _userValidator.CheckEmail(email, cancellation);
                    return exist;
                }).WithMessage("The {PropertyName} '{PropertyValue}' has already been taken.");

            // Gender validation
            RuleFor(x => x.Gender).NotEmpty().WithMessage("The {PropertyName} cannot be empty.");

            // Password validation
            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The {PropertyName} cannot be empty.")
                .MinimumLength(8).WithMessage("The {PropertyName} must contain at least 8 characters.")
                .Matches(@"[0-9]+").WithMessage("The {PropertyName} must contain at least one number.")
                .Matches(@"[A-Z]+").WithMessage("The {PropertyName} must contain at least one uppercase letter.")
                .Matches(@"[a-z]+").WithMessage("The {PropertyName} must contain at least one lowercase letter.")
                .Matches(@"[~!@#$%^&*) (_.-]+").WithMessage("The {PropertyName} must contain at least one special character.");



            // .WithMessage("The {PropertyName} must contain at least 8 characters, including one uppercase, lowercase, numbers and special character");
            // At least one upper case English letter, (?=.*?[A-Z])
            // At least one lower case English letter, (?=.*?[a - z])
            // At least one digit, (?=.*?[0 - 9])
            // At least one special character, (?=.*?[#?!@$%^&*-])
            // Minimum eight in length.{8,} (with the anchors)

            // Confirm Password validation
            RuleFor(x => x.ConfirmPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The {PropertyName} cannot be empty.")
                .Equal(x => x.Password).WithMessage("The {PropertyName} and Password do not match.");
        }
    }
}