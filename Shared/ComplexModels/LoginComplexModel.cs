using asfalis.Shared.DTOs;
using FluentValidation;

namespace asfalis.Shared.Models
{
    // A complex model for user login
    // Including user, qrcode and image information
    public class LoginComplexModel
    {
        public int UserId { get; set; }

        public int ImageCorrectCount { get; set; }

        public LoginPersonalDTO LoginPersonal { get; set; }

        public List<Image> SelectedImages { get; set; }

        public QRCode QRCode { get; set; }


        public LoginComplexModel()
        {
            LoginPersonal = new();
            SelectedImages = new();
            QRCode = new();
        }

        public class LoginComplexModelValidator : AbstractValidator<LoginComplexModel>
        {
            // Validation for the selected properties of user login
            public LoginComplexModelValidator()
            {
                RuleFor(x => x.LoginPersonal).SetValidator(new LoginPersonalValidator());
                RuleForEach(x => x.SelectedImages).SetValidator(new ImageValidator());
                RuleFor(x => x.QRCode).SetValidator(new QRCodeValidator());
            }
        }
    }
}
