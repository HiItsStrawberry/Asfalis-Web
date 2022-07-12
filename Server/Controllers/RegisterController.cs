using Microsoft.IdentityModel.Tokens;

namespace asfalis.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEncryptionService _encryptor;

        public RegisterController(IEncryptionService encryptor, IUserService userManager)
        {
            this._encryptor = encryptor;
            this._userService = userManager;
        }

        [HttpGet]
        [Route("images")]
        // Get method -> Get random images for user registration
        public async Task<ActionResult<List<ImageListDTO>>> GetImages()
        {
            List<ImageListDTO>? images;

            try
            {
                // Get 5 images for user registration
                images = await _userService.GetImages(5);

                // Return error message if the image is null
                if (images == null) return BadRequest(MessageOption.Error.Image);
            }
            catch (Exception)
            {
                return BadRequest(MessageOption.Error.Image);
            }

            // Return images
            return Ok(images);
        }


        [HttpPost]
        [Route("user")]
        // Post method -> register user and user images
        public async Task<ActionResult> Register([FromBody] RegisterComplexModel registerModel)
        {
            // Return error message if the model/user input is invalid
            // Return error message if the user model/user input is invalid
            // Return error message if the image model is invalid

            var userInput = registerModel.User;

            // Values for validation           
            bool isValid = new object[] {
                userInput.Username!,
                userInput.Email!,
                userInput.Gender!,
                userInput.Password!,
                userInput.ConfirmPassword!
            }.ValidateValues();

            // Return error message if the values are not valid
            if (!isValid) return BadRequest(MessageOption.Error.MissingDetails);

            // Return error message if the username is existing
            isValid = await _userService.GetUsername(userInput.Username!);
            if (!isValid) return BadRequest(MessageOption.Error.MissingDetails);

            // Return error message if the email is existing
            isValid = await _userService.GetEmail(userInput.Email!);
            if (!isValid) return BadRequest(MessageOption.Error.MissingDetails);

            // Return error message if images for registration is null
            if (!registerModel.Image.Any()) return BadRequest(MessageOption.Error.Register);

            try
            {
                // Filling user input into a new User instance
                var user = new User
                {
                    Username = registerModel.User.Username,
                    Email = registerModel.User.Email,
                    Gender = registerModel.User.Gender,
                    // Hashing the user's plain text password
                    Password = await _encryptor.HashPassword(registerModel.User.Password!)
                };

                // Loop through user's registered images
                // Encrypt all images before saving to database
                foreach (var image in registerModel.Image)
                {
                    image.Name = await _encryptor.Aes_Encrypt(image.Name!);
                }

                // Filling user's registered into a new Image instance
                user.Images = registerModel.Image;

                // Get the result for registring the user
                var success = await _userService.RegisterUser(user);

                // Return error message if the registration is not success
                if (success == null) return BadRequest(MessageOption.Error.Register);
            }
            catch (Exception err)
            {
                return BadRequest(MessageOption.Error.Exception(err.Message));
            }

            // Return OK Http message/successful message
            return Ok();
        }


        [HttpGet]
        [Route("username/{username}")]
        // Get method -> validate existing username from database
        public async Task<ActionResult<bool>> GetUsername(string username)
        {
            try
            {
                // Finding the any user from database that matches the username
                return await _userService.GetUsername(username);
            }
            catch (Exception)
            {
                // Return error message if there is something wrong with interacting the database
                return BadRequest(MessageOption.Error.DatabaseInteraction);
            }
        }


        [HttpGet]
        [Route("email/{email}")]
        // Get method -> validate existing email from database
        public async Task<ActionResult<bool>> GetEmail(string email)
        {
            try
            {
                // Finding the any user from database that matches the username
                return await _userService.GetEmail(email);
            }
            catch (Exception)
            {
                // Return error message if there is something wrong with interacting the database
                return BadRequest(MessageOption.Error.DatabaseInteraction);
            }
        }


        [HttpPost]
        [Route("activation")]
        // Post method -> activiate user account with a valid token
        public async Task<ActionResult> ActivateUser([FromBody] string token)
        {
            try
            {
                // Check if the activation token is there
                if (token.IsEmpty()) return BadRequest(MessageOption.Error.UserActivation);

                // Decode the token from url
                string originalToken = Base64UrlEncoder.Decode(token);

                // Decrypt the activation key to retrieve userId
                string userId = await _encryptor.Aes_Decrypt(originalToken);

                // Get the user from database using the userId
                var success = await _userService.ActivateUser(userId.ToInt());

                if (!success) return BadRequest(MessageOption.Error.UserActivation);

                // Return OK Http message/successful message
                return Ok("Your account has been activated. Enjoy!");
            }
            catch (Exception err)
            {
                // Return error message if there was an error activating the user
                return BadRequest(MessageOption.Error.Exception(err.Message));
            }
        }
    }
}
