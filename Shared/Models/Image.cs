using FluentValidation;
using System.Text.Json.Serialization;

namespace asfalis.Shared.Models
{
    // A table for user image
    public class Image
    {
        public int ImageId { get; set; }

        public string? Name { get; set; }

        [JsonIgnore]
        public virtual ICollection<User>? Users { get; set; }

        [JsonIgnore]
        public virtual List<UserImage>? UserImages { get; set; }
    }

    public class ImageValidator : AbstractValidator<Image>
    {
        // Validation for the selected properties of user image
        public ImageValidator()
        {
            // Image name validation
            RuleFor(x => x.Name)
                .NotEmpty()
                .NotNull()
                .MaximumLength(500);
        }
    }
}