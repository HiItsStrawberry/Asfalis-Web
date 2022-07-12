using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace asfalis.Shared.Models
{
    // A table for user and image as a many to many ralationship
    public class UserImage
    {
        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }

        [Required]
        [Column("image_id")]
        public int ImageId { get; set; }

        [JsonIgnore]
        public Image? Image { get; set; }
    }
}