using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace asfalis.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IQRCodeService _codeService;
        private readonly IEncryptionService _encryptor;
        private readonly IJwtTokenService _tokenService;

        public LoginController(IEncryptionService encryptor, IUserService userManager, IQRCodeService codeService, IJwtTokenService tokenService)
        {
            this._encryptor = encryptor;
            this._userService = userManager;
            this._codeService = codeService;
            this._tokenService = tokenService;
        }

        [HttpPost]
        [Route("personal")]
        // Post method -> validate step 1 of user personal login (Desktop & Mobile)
        public async Task<ActionResult> LoginPersonal(LoginPersonalDTO user)
        {
            try
            {
                // Values for validation
                bool isValid = new object[] {
                    user.Name!,
                    user.Password!
                }.ValidateValues();

                // Return error message if the values are not valid
                if (!isValid) return BadRequest(MessageOption.Error.MissingDetails);

                // Find users that matches the entered username from the database
                var userFound = await _userService.GetUser(name: user.Name!);

                // Return invalid credential message if the entered crendential is not found
                if (userFound == null) return NotFound(MessageOption.Error.LoginPersonalFail);

                // Check the request sent from PC or Mobile
                bool isMobile = HttpContext.CheckIsMobile();

                // Verify if the user'email is confirmed
                if (userFound.EmailConfirmed == false)
                {
                    if (!isMobile)
                    {
                        return BadRequest(MessageOption.Error.EmailNotVerified);
                    }
                    else
                    {
                        return BadRequest(MessageOption.Error.EmailNotVerifiedMobile);
                    }
                }

                // Return error message if the account still being lockout
                // Set lockout time of 15 minutes if the user has reached the maximum login attemps
                isValid = await _userService.CheckIsValidLogin(userFound);
                if (!isValid) return BadRequest(MessageOption.Error.LoginAttemps);

                // Check if the entered password is valid
                isValid = await _encryptor.VerifyHashedPassword(user.Password!, userFound.Password!);

                // Increment the fail attemp by 1 if the password is not valid
                if (!isValid)
                {
                    await _userService.CheckIsValidLogin(userFound, true);
                    return NotFound(MessageOption.Error.LoginPersonalFail);
                }

                // Jump to mobile login method if the request is sent from mobile
                if (isMobile) return await this.LoginPersonalMobile(userFound);

                // Return the userId as Http Response
                return Ok(userFound.UserId);
            }
            catch (Exception err)
            {
                return BadRequest(MessageOption.Error.Exception(err.Message));
            }
        }


        // A method for mobile user login only
        public async Task<ActionResult> LoginPersonalMobile(User user)
        {
            try
            {
                // Return error message if user is null
                if (user == null) return BadRequest(MessageOption.Error.JwtTokenGeneration);

                // Generate a token for mobile user
                string token = _tokenService.GenerateToken(user, true);

                // Return error message if token is null
                if (token.IsEmpty()) return BadRequest(MessageOption.Error.JwtTokenGeneration);

                // Transform user data to mobile user DTO
                var userDTO = user.TransformMobile();

                // Set the the token into the user DTO
                userDTO.Token = token;

                // Return the user DTO as Http response
                return Ok(await Task.FromResult(userDTO));
            }
            catch (Exception err)
            {
                return BadRequest(MessageOption.Error.Exception(err.Message));
            }
        }


        [HttpGet]
        [Route("qrcode/{userId}/{code}")]
        [Authorize(Policy = "OnlyMobileUser")]
        // Post method -> get the original code by scanning the QR (Only for mobile user)
        public async Task<ActionResult> GetLoginCode(int userId, string code)
        {
            try
            {
                // Values for validation
                bool isValid = new object[] { userId, code }.ValidateValues();

                // Return error message if the values are not valid
                if (!isValid) return BadRequest(MessageOption.Error.MissingDetails);

                // Get user from database by UserId
                var userFound = await _userService.GetUser(userId);

                // Return error message if user is not found
                if (userFound == null) return BadRequest(MessageOption.Error.LoginCodeNotFound);

                // Verify if the user'email is confirmed
                if (userFound.EmailConfirmed == false) return BadRequest(MessageOption.Error.EmailNotVerified);

                // Decode the from the encoded code sent from mobile
                string scannedCode = Base64UrlEncoder.Decode(code);

                // Return error message if the code sent from mobile is null
                if (scannedCode.IsEmpty()) return BadRequest(MessageOption.Error.LoginCodeNotFound);

                // Decrpyt the code sent from mobile side
                string originalCode = await _encryptor.Aes_Decrypt(scannedCode);

                // Return error message if the code sent is not belong to the user
                if (userId != originalCode.Split('.')[2].ToInt()) return BadRequest(MessageOption.Error.LoginCodeNotFound);

                // Return the decrypted code to mobile side
                return Ok(originalCode.Split('.')[0]);
            }
            catch (Exception err)
            {
                return BadRequest(MessageOption.Error.Exception(err.Message));
            }
        }


        [HttpPost]
        [Route("qrcode")]
        // Post method -> validate step 2 of user qr code login
        public async Task<ActionResult> LoginQRCode(QRCode qrcode)
        {
            try
            {
                // Values for validation
                bool isValid = new object[] { qrcode.UserId, qrcode.OTPCode! }.ValidateValues();

                // Return error message if the values are not valid
                if (!isValid) return BadRequest(MessageOption.Error.MissingDetails);

                // Get the user from database by UserId
                var userFound = await _userService.GetUser(qrcode.UserId);

                // Return error message if the user is not found
                if (userFound == null) return NotFound(MessageOption.Error.QRCodeValidation);

                // Verify if the user'email is confirmed
                if (userFound.EmailConfirmed == false) return BadRequest(MessageOption.Error.EmailNotVerified);

                // Return error message if user login is not currently valid
                isValid = await _userService.CheckIsValidLogin(userFound);
                if (!isValid) return BadRequest(MessageOption.Error.LoginAttemps);

                // Get the original code from database by UserId
                var codeFound = await _codeService.GetQRCode(userId: qrcode.UserId);

                // Return error message if the original code is not found
                if (codeFound == null) return BadRequest(MessageOption.Error.QRCodeValidation);

                // Check if the original code from database is expired
                isValid = _codeService.CheckIsCodeExpired(codeFound);

                // Return error message and increment login failure by 1
                if (!isValid)
                {
                    await _userService.CheckIsValidLogin(userFound, true);
                    return NotFound(MessageOption.Error.LoginQRCodeFail);
                }

                // Decrypt the original code if is not expired
                string originalCode = await _encryptor.Aes_Decrypt(codeFound.OTPCode!);

                // Check if the user entered code is same as the original code
                if (originalCode.Split('.')[0] != qrcode.OTPCode)
                {
                    // Return error message and increment login failure by 1 if not same
                    await _userService.CheckIsValidLogin(userFound, true);
                    return BadRequest(MessageOption.Error.LoginQRCodeFail);
                }

                // Expire the current orginal code after the user has entered a valid code
                isValid = await _codeService.ExpireQRCode(codeFound);

                // Return error message if the orginal code is not set to expired
                if (!isValid) return NotFound(MessageOption.Error.QRCodeValidation);

                return Ok();
            }
            catch (Exception err)
            {
                return BadRequest(MessageOption.Error.Exception(err.Message));
            }
        }


        [HttpGet]
        [Route("images/{userId}")]
        // Get user images for user login
        public async Task<ActionResult<LoginImageListDTO>> GetUserLoginImages(int userId)
        {
            try
            {
                // Values for validation
                bool isValid = new object[] { userId }.ValidateValues();

                // Return error message if the values are not valid
                if (!isValid) return BadRequest(MessageOption.Error.MissingDetails);

                // Get the images for user login with UserId
                var images = await _userService.GetLoginImages(userId);

                // Return error message if there is no correct count or images
                if ((images.CorrectCount <= 0 || images.CorrectCount > 4) || !images.LoginImages!.Any())
                    return BadRequest(MessageOption.Error.Image);

                // Return images
                return Ok(images);
            }
            catch (Exception err)
            {
                return BadRequest(MessageOption.Error.Exception(err.Message));
            }
        }


        [HttpPost]
        [Route("images/{userId}/{correctCount}")]
        // Post method -> validate step 3 of user images login
        public async Task<ActionResult> LoginUserImage([FromRoute] int userId, [FromRoute] int correctCount, [FromBody] List<Image> images)
        {
            try
            {
                // Values for validation
                bool isValid = new object[] { userId, correctCount }.ValidateValues();

                // Return error message if the values are not valid
                if (!isValid) return BadRequest(MessageOption.Error.MissingDetails);

                // Return error message if there is no images selected
                if (!images.Any()) return BadRequest(MessageOption.Error.Login);

                // Get user from database by UserId
                var userFound = await _userService.GetUser(userId);

                // Return error message if user is not found
                if (userFound == null) return NotFound(MessageOption.Error.Login);

                // Verify if the user'email is confirmed
                if (userFound.EmailConfirmed == false) return BadRequest(MessageOption.Error.EmailNotVerified);

                // Return error message if the account still being lockout
                // Set lockout time of 15 minutes if the user has reached the maximum login attemps
                isValid = await _userService.CheckIsValidLogin(userFound);
                if (!isValid) return BadRequest(MessageOption.Error.LoginAttemps);

                // Check If the number of selected images equals to correct count
                if (correctCount != images.Count)
                {
                    // Return error message and increment login failure by 1 if not same
                    await _userService.CheckIsValidLogin(userFound, true);
                    return NotFound(MessageOption.Error.LoginImageFail);
                }

                // A list to store correct images
                var correctImages = new List<string>();

                // Get the user images from database by UserId
                var userImages = await _userService.GetUserImages(userFound.UserId);

                // Variables for converting image information
                string selectedImage;

                // Loop selected images and user images
                for (int i = 0; i < images.Count; i++)
                {
                    for (int j = 0; j < userImages.Count; j++)
                    {
                        // Decode the selected image
                        selectedImage = images[i].Name!.DecodeBase64();

                        // Check if selected image is the user image
                        if (selectedImage == userImages[j].Name!)
                        {
                            // Add to the list if the selected image is correct
                            correctImages.Add(images[i].Name!);
                            break;
                        }
                    }
                }

                // Check if the correct images in the list same as correct count
                if (correctImages.Count != correctCount)
                {
                    // Return error message and increment login failure by 1 if not same
                    await _userService.CheckIsValidLogin(userFound, true);
                    return NotFound(MessageOption.Error.LoginImageFail);
                }

                return Ok();
            }
            catch (Exception err)
            {
                return BadRequest(MessageOption.Error.Exception(err.Message));
            }
        }


        [HttpGet]
        [Route("token/{userId}")]
        // Get method -> Generate a jwt token for user login
        public async Task<ActionResult<JwtDTO>> LoginToken(int userId)
        {
            try
            {
                // Values for validation
                bool isValid = new object[] { userId }.ValidateValues();

                // Return error message if the values are not valid
                if (!isValid) return BadRequest(MessageOption.Error.MissingDetails);

                // Get user from database by UserId
                var userFound = await _userService.GetUser(userId);

                // Return error message if user is not found
                if (userFound == null) return BadRequest(MessageOption.Error.JwtTokenGeneration);

                // Verify if the user'email is confirmed
                if (userFound.EmailConfirmed == false) return BadRequest(MessageOption.Error.EmailNotVerified);

                // Generate a new jwt token
                var token = await GenerateNewToken(userFound);

                // Return error message if the token is null
                if (token.Token.IsEmpty()) return BadRequest(MessageOption.Error.JwtTokenGeneration);

                return Ok(token);
            }
            catch (Exception err)
            {
                return BadRequest(MessageOption.Error.Exception(err.Message));
            }
        }


        [HttpPost]
        [Route("token/validation")]
        // Post method -> Validate token before signing user into the system
        public async Task<ActionResult<JwtDTO>> TokenValidation([FromBody] JwtDTO token)
        {
            try
            {
                // Set the sent token as old token
                string oldToken = token.Token!;

                // Return error message if the oldtoken is null
                if (oldToken.IsEmpty()) return BadRequest(MessageOption.Error.JwtTokenInvalid);

                // Check if the request sent from mobile site
                bool isMobile = HttpContext.CheckIsMobile();

                // Return the user object after validating the token
                var user = await _tokenService.ValidateToken(oldToken, isMobile);

                // Return invalid token if user object is null and not from mobile side
                if (user == null && !isMobile) return BadRequest(MessageOption.Error.JwtTokenInvalid);

                // Verify if the user'email is confirmed
                if (user!.EmailConfirmed == false && isMobile) return BadRequest(MessageOption.Error.EmailNotVerifiedMobile);

                // Generate a new token for mobile site
                var newToken = await GenerateNewToken(user!);

                return Ok(newToken);
            }
            catch (Exception err)
            {
                return BadRequest(MessageOption.Error.Exception(err.Message));
            }
        }


        // A method to generate a new jwt token
        private async Task<JwtDTO> GenerateNewToken(User user)
        {
            // Check if the request sent from mobile site
            bool isMobile = HttpContext.CheckIsMobile();

            // Return the user object after validating the token
            string token = _tokenService.GenerateToken(user, isMobile);

            // Set the new token into the Jwt object
            var jwtToken = new JwtDTO { Token = token };

            // Return the new token
            return await Task.FromResult(jwtToken);
        }
    }
}