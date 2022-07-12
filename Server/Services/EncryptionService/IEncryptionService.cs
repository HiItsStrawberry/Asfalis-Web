namespace asfalis.Server.Services
{
    public interface IEncryptionService
    {
        Task<string> Aes_Decrypt(string encryptedText);
        Task<string> Aes_Encrypt(string originalText);
        Task<string> HashPassword(string password);
        Task<bool> VerifyHashedPassword(string password, string hashedPassword);
    }
}