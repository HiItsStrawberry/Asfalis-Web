using System.Security.Cryptography;
using Bcrypt = BCrypt.Net.BCrypt;

namespace asfalis.Server.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly IConfiguration _config;

        public EncryptionService(IConfiguration config)
        {
            _config = config;
        }

        // A method to hash a password
        public async Task<string> HashPassword(string password)
        {
            return await Task.Run(() => Bcrypt.EnhancedHashPassword(password, workFactor: 10)); // workFactor = salt iteration 2^10
        }


        // A method to verify a hased password and a plain password
        public async Task<bool> VerifyHashedPassword(string password, string hashedPassword)
        {
            return await Task.Run(() => Bcrypt.EnhancedVerify(password, hashedPassword));
        }


        // A method to encrypt a string with AES
        public async Task<string> Aes_Encrypt(string originalText)
        {
            // Aes block is always the size of 16bytes = 128bits
            using var cipher = Aes.Create(); //Defaults Key: 32bytes/256bits, Mode: CBC, Padding: PKCS7
                                             // (CBC) Cipher-Block-Chaining: Each block only fits 16bytes/1 character
                                             // (Defaults) Padding is helps to fill the values to meets its 128bits block value for the blocks

            // Get the secret key from Hex to byte
            cipher.Key = Convert.FromHexString(_config.GetValue<string>("AES:SecKey"));

            byte[] encrypted;
            var iv = cipher.IV; // Generating random Initializing Vector

            // Create an encryptor with the key and IV 
            var encryptor = cipher.CreateEncryptor(cipher.Key, iv);

            // Encrypt the data with the encryptor created
            await using (var stream = new MemoryStream())
            {
                await using (var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
                {
                    await using (var writer = new StreamWriter(cryptoStream))
                    {
                        await writer.WriteAsync(originalText);
                        writer.Close();
                    }
                }
                encrypted = stream.ToArray();
                await stream.DisposeAsync();
            }

            return Convert.ToBase64String(iv) + "." + Convert.ToBase64String(encrypted);
        }


        // A method to decrypt a string with AES
        public async Task<string> Aes_Decrypt(string encryptedText)
        {
            // Aes block is always the size of 16bytes = 128bits
            using var cipher = Aes.Create(); //Defaults Key: 32bytes/256bits, Mode: CBC, Padding: PKCS7
                                             // (CBC) Cipher-Block-Chaining: Each block only fits 16bytes/1 character
                                             // (Defaults) Padding is helps to fill the values to meets its 128bits block value for the blocks

            // Get the secret key from Hex to byte
            cipher.Key = Convert.FromHexString(_config.GetValue<string>("AES:SecKey"));

            string originalText;

            // Separate the IV and the key from the encryptedText
            var ivKey = encryptedText.Split(".")[0];
            encryptedText = encryptedText.Split(".")[1];
            var encrypted = Convert.FromBase64String(encryptedText);

            // Create an decryptor with the key and IV 
            var decryptor = cipher.CreateDecryptor(cipher.Key, Convert.FromBase64String(ivKey));

            // Decrypt the data with the decryptor created
            await using (var stream = new MemoryStream(encrypted))
            {
                await using (var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
                {
                    using (var reader = new StreamReader(cryptoStream))
                    {
                        originalText = await reader.ReadToEndAsync();
                        reader.Close();
                    }
                }
                await stream.DisposeAsync();
            }
            return originalText;
        }
    }
}
