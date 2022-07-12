using QRCoder;
using QRImage = System.Drawing;

namespace asfalis.Server.Services.QRCodeService
{
    public class QRCodeService : IQRCodeService
    {
        private readonly ApplicationDbContext _db;

        public QRCodeService(ApplicationDbContext db)
        {
            this._db = db;
        }

        // A method to add qr code if there is none
        // A method to update latest qr code if there is one
        public async Task<bool> AddOrUpdateQRCode(QRCode qrcode)
        {
            try
            {
                // Return false if the qrcode data is null
                if (qrcode == null) return false;

                // Get the qr code data from database by UserId
                var codeFound = await this.GetQRCode(userId: qrcode.UserId);

                if (codeFound == null)
                {
                    // Add a new qr code if there is no existing
                    await _db.QRCodes!.AddAsync(qrcode);
                }
                else
                {
                    // Update the existing qr code to the latest
                    qrcode.CodeId = codeFound.CodeId;
                    _db.QRCodes!.Update(qrcode);
                }

                // Return true of false based on successful
                return (await _db.SaveChangesAsync()) > 0;
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return false;
            }
        }

        // A method to set the qr code to expired after it is used
        public async Task<bool> ExpireQRCode(QRCode qrcode)
        {
            try
            {
                // Return false if the qrcode data is null
                if (qrcode == null) return false;

                // Update the code to expired code
                qrcode.CodeExpired = true;
                _db.QRCodes!.Update(qrcode);

                // Return true of false based on successful
                return (await _db.SaveChangesAsync()) > 0;
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return false;
            }
        }

        // A method to get the qr code from database
        public async Task<QRCode> GetQRCode(int codeId = 0, int userId = 0)
        {
            try
            {
                var qrcode = new QRCode();
                if (codeId != 0)
                {
                    // Get the qr code by CodeId
                    qrcode = await _db.QRCodes!.FindAsync(codeId);
                }
                else
                {
                    // Get the qr code by UserId
                    qrcode = await _db.QRCodes!
                        .AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
                }
                return qrcode!;
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return null!;
            }
        }

        // A method to generate a new QR Code
        public async Task<string> GenerateQRCode(string code)
        {
            try
            {
                var qrGenerator = new QRCodeGenerator();

                // Generate a new qr code with the given data and ECC level of medium
                var qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.M);

                // Convert the qr code generated to bitmap
                var qrCode = new BitmapByteQRCode(qrCodeData);

                // Set the size and color of the qr code
                var qrCodeImage = qrCode.GetGraphic(20, "#008000", "#FFFFFF");

                // Generate a random string Id as the name
                var guid = Guid.NewGuid();

                // Set where the qr code generated should be saved
                string filename = Helpers.GetFilePathName(Helpers.PathOptions.QRCode, $"{guid}.jpeg");

                // Saving the qrcode into the QR Code folder
                await File.WriteAllBytesAsync(filename, qrCodeImage);

                return filename;
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return null!;
            }
        }

        // A method to generate a random code
        public string GenerateCode(int length = 6)
        {
            try
            {
                // Only these chars will be chosen for generation
                string allowedChars = "QWERTYUIOPASDFGHJKLZXCVBNM1234567890";
                var random = new Random();

                // Set the length of the code
                char[] code = new char[length];

                // Loop all chars and pick randomly
                for (int i = 0; i < length; i++)
                {
                    code[i] = allowedChars[((allowedChars.Length - 1) * random.NextDouble()).ToInt()];
                }

                // Return the code as a string
                return new string(code);
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return null!;
            }
        }

        // A method to check if the qr code is expired for login
        public bool CheckIsCodeExpired(QRCode qrcode)
        {
            try
            {
                // Return false if the code is expired
                if (qrcode.CodeExpired == true) return false;

                // Return false if the code is expired by time
                if (Helpers.GetCurrentDate() > qrcode.ExpiryTime) return false;

                return true;
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return false;
            }
        }
    }
}
