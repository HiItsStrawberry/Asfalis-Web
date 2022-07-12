using Microsoft.IdentityModel.Tokens;

namespace asfalis.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;
        private readonly IUserService _userService;
        private readonly IQRCodeService _codeService;
        private readonly IEncryptionService _encryptor;

        public MailController(IMailService mailService, IEncryptionService encryptor, IUserService userManager, IQRCodeService codeService)
        {
            this._encryptor = encryptor;
            this._mailService = mailService;
            this._userService = userManager;
            this._codeService = codeService;
        }

        [HttpPost]
        [Route("registration")]
        // Post method -> create a new PDF with user registered images and send to email
        public async Task<ActionResult> SendEmail([FromBody] RegisterComplexModel registerModel)
        {
            try
            {
                // Values for validation                
                bool isValid = new object[] { registerModel.User.Username! }.ValidateValues();

                // Return error message if the values are not valid
                if (!isValid) return BadRequest(MessageOption.Error.MissingDetails);

                // Getting the newly registered user
                var user = await _userService.GetUser(name: registerModel.User.Username!);

                // Return error messsage if the user does not exist
                if (user == null) return BadRequest(MessageOption.Error.Email);

                // Getting the user's registered images
                var images = registerModel.Image;

                // Return error messsage if the images are empty
                if (!images.Any()) return BadRequest(MessageOption.Error.Email);

                // Start generating an attachment (A PDF file with the registered images)
                string attachement = _mailService.GenerateImagePDF(images, user.Username!);

                // Return error messsage if the attachment generation failed
                if (attachement.Contains("Sorry")) return BadRequest(MessageOption.Error.Email);

                // Encrypt the registered user Id before sending via Url
                string userId = await _encryptor.Aes_Encrypt(user.UserId.ToString());

                // Encoding the encrypted userId before sending via Url
                string url = $"https://localhost:5001/activation?token={Base64UrlEncoder.Encode(userId)}";

                // Creating a new email body
                string body = _mailService.ActivationMessage(user.Username!, url);

                // Send email with few required arguments (email to be sent, email's subhect, email body, attachment)
                bool sent = await _mailService.SendEmail(user.Email!, "Account activation", body, attachement);

                // Return error message if the email is not sent
                if (!sent) return BadRequest(MessageOption.Error.Email);

                // Return OK Http message/successful message
                return Ok(MessageOption.Success.UserActivation);
            }
            catch (Exception err)
            {
                // Return error message if the email cannot be sent
                return BadRequest(MessageOption.Error.Exception(err.Message));
            }
        }


        [HttpGet]
        [Route("qrcode/{userId}")]
        // Get method -> send a new QR Code generated to user's email
        public async Task<ActionResult> GetLoginQRCode(int userId)
        {
            try
            {
                // Values for validation
                bool isValid = new object[] { userId }.ValidateValues();

                // Return error message if the values are not valid
                if (!isValid) return BadRequest(MessageOption.Error.MissingDetails);

                // Get the user from database by UserId
                var userFound = await _userService.GetUser(userId);

                // Return error message if the user is not found
                if (userFound == null) return BadRequest(MessageOption.Error.QRCode);

                // Verify if the user'email is confirmed
                if (userFound.EmailConfirmed == false) return BadRequest(MessageOption.Error.EmailNotVerified);

                // Encrypt the code generated with user information
                string code = await _encryptor.Aes_Encrypt(
                    $"{_codeService.GenerateCode()}.{userFound.Username}.{userFound.UserId}");

                // Create a new QRCode object with the required information
                var qrcode = new QRCode
                {
                    OTPCode = code,
                    ExpiryTime = Helpers.GetCurrentDate(5),
                    UserId = userFound.UserId
                };

                // Add or update the QRCode into database
                bool created = await _codeService.AddOrUpdateQRCode(qrcode);

                // Return error message if the QRCode is not added or updated
                if (!created) return BadRequest(MessageOption.Error.QRCode);

                // Generate a new QRCode embedded with the code generated after encoding it
                string generatedQR = await _codeService.GenerateQRCode(Base64UrlEncoder.Encode(code));

                // Return error message if the QR Code is not generated
                if (generatedQR.IsEmpty()) return BadRequest(MessageOption.Error.QRCode);

                // Create a mail body with the QR Code generated as an Image attachment
                var body = _mailService.ImageMessage(userFound.Username!, generatedQR);

                // Send email with few required arguments (email to be sent, email's subhect, email body, attachment)
                bool sent = await _mailService.SendEmail(userFound.Email!, "QR Code Verification", image: body);

                // Return error message if the email is not sent
                if (!sent) return BadRequest(MessageOption.Error.QRCode);

                // Return email sent message to client side
                return Ok(MessageOption.Success.QRCode);
            }
            catch (Exception err)
            {
                return BadRequest(MessageOption.Error.Exception(err.Message));
            }
        }
    }
}
