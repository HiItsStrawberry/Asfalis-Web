namespace asfalis.Shared.DTOs
{
    // A data transfer object for user login images
    public class LoginImageListDTO
    {
        public int CorrectCount { get; set; }
        public List<ImageListDTO>? LoginImages { get; set; }
    }
}
