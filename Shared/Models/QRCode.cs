using FluentValidation;
using System.Text.Json.Serialization;

namespace asfalis.Shared.Models
{
    // A table for QRCode
    public class QRCode
    {
        public int CodeId { get; set; }

        public string? OTPCode { get; set; }

        public DateTime ExpiryTime { get; set; }

        public bool CodeExpired { get; set; } = false;

        public int UserId { get; set; }

        [JsonIgnore]
        public virtual User? User { get; set; }
    }

    public class QRCodeValidator : AbstractValidator<QRCode>
    {
        // Validation for the selected properties of QRCode
        public QRCodeValidator()
        {
            // Code validation
            RuleFor(x => x.OTPCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The {PropertyName} cannot be empty.")
                .Matches(@"^[a-zA-Z0-9]+$").WithMessage("The {PropertyName} contains an invalid character.");

            // UserId validation
            RuleFor(x => x.UserId).NotEmpty().NotNull().WithMessage("The {PropertyName} cannot be empty.");
        }
    }
}