
namespace asfalis.Server.Services.UserService
{
    public interface IUserService
    {
        Task<bool> ActivateUser(int userId);
        Task<bool> CheckIsValidLogin(User user, bool increment = false);
        Task<bool> GetEmail(string email);
        Task<List<ImageListDTO>> GetImages(int count, List<string>? filteredImages = null);
        Task<LoginImageListDTO> GetLoginImages(int userId);
        Task<List<Image>> GetUserImages(int userId);
        Task<bool> GetUsername(string username);
        Task<User> RegisterUser(User user);
        Task<User> GetUser(int userId = 0, string? name = null);
    }
}