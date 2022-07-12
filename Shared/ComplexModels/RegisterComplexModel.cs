using asfalis.Shared.Services;
using FluentValidation;

namespace asfalis.Shared.Models
{
    // A complex model for user registrantion
    // Including user and image information
    public class RegisterComplexModel
    {
        public User User { get; set; }

        public List<Image> Image { get; set; }

        public RegisterComplexModel()
        {
            User = new();
            Image = new();
        }

        public class RegisterComplexModelValidator : AbstractValidator<RegisterComplexModel>
        {
            private readonly ICustomUserValidator _userValidator;

            // Validation for the selected properties of user registrantion
            public RegisterComplexModelValidator(ICustomUserValidator userValidator)
            {
                this._userValidator = userValidator;

                // Telling the user object to use validation setup from User
                RuleFor(x => x.User).SetValidator(new UserValidator(this._userValidator));

                // Telling the user image object to use validation setup from Image
                RuleForEach(x => x.Image).SetValidator(new ImageValidator());
            }
        }
    }
}