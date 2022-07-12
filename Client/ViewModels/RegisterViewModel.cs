namespace asfalis.Client.ViewModels
{
    public class RegisterViewModel : IRegisterViewModel
    {
        private readonly HttpClient _httpClient;
        private readonly ICustomUserValidator _userValidator;


        public RegisterViewModel(HttpClient httpClient, ICustomUserValidator userValidator)
        {
            this._httpClient = httpClient;
            this._userValidator = userValidator;
        }

        #region state variables
        // Set the starting step of the form
        public int CurrentStep { get; set; } = 1;

        // Set the maximum step of the form
        public int MaximumStep { get; set; } = 3;

        // Set the current loading state
        public bool IsLoading { get; set; } = false;

        // A variable to hold info message returned from api
        public string InfoMessage { get; set; } = string.Empty;

        // A variable to hold error message returned from api
        public string ErrorMessage { get; set; } = string.Empty;

        // A list to hold the random user login images from api
        public List<ImageListDTO> Images { get; set; } = new List<ImageListDTO>();

        // A complex model to hold user inputs for user registration
        public RegisterComplexModel RegisterModel { get; set; } = new RegisterComplexModel();
        #endregion

        // A method for user registration step 1
        public async Task RegisterStepOne()
        {
            try
            {
                // validator for user personal information
                var validator = new UserValidator(_userValidator);

                // validate the user input information 
                var result = await validator.ValidateAsync(RegisterModel.User);

                // Return to page if the user input is invalid
                if (!result.IsValid) return;

                // Get images for registration
                await GetRegistrationImage();

                // Proceed to register step 2
                NextRegisterStep();
            }
            catch (Exception err)
            {
                RedirectError(err.Message);
            }
        }


        // A method for user registration step 1
        public async Task RegisterStepTwo()
        {
            try
            {
                IsLoading = true;

                // Call api to register user
                var resRegister = await _httpClient.PostAsJsonAsync("api/register/user", RegisterModel);

                // Check if the response returned is valid
                if (!CheckIsValid(resRegister)) return;

                // Call api to send activation link and image PDF to user's email
                var resEmail = await _httpClient.PostAsJsonAsync("api/mail/registration", RegisterModel);

                // Check if the response returned is valid
                if (!CheckIsValid(resEmail)) return;

                // Proceed to register step 3 (Result page)
                NextRegisterStep();
            }
            catch (Exception err)
            {
                RedirectError(err.Message);
            }
        }


        // A method to proceed user to next register step
        private void NextRegisterStep()
        {
            // Increment the currentStep by 1
            int step = CurrentStep + 1;

            // Check if step is the maximum step or next step
            CurrentStep = step >= MaximumStep ? MaximumStep : step;

            // Clear info and error message
            ResetMessages();

            IsLoading = false;
        }


        // A method to proceed user to previous register step
        public void PreviousRegisterStep()
        {
            // Decrement the currentStep by 1
            int step = CurrentStep - 1;

            // Check if step is the valid (return 1 as valid, else decrement by 1)
            CurrentStep = step <= 0 ? 1 : step;

            // Clear info and error message
            ResetMessages();
        }


        // A method to get images for registration
        public async Task GetRegistrationImage()
        {
            try
            {
                IsLoading = true;

                // Set image list to a new empty list to store new images 
                RegisterModel.Image = new List<Image>();

                // Call api to get registration login images
                var result = await _httpClient.GetAsync("api/register/images");

                if (!CheckIsValid(result)) return;

                // Get the responsded registration images if is valid
                var imageSet = await result.GetListResponse<List<ImageListDTO>>();

                // Redirect to error page if login images is null
                if (!imageSet!.Any())
                {
                    RedirectError(MessageOption.Image);
                    return;
                }

                // Set the registration images for displaying
                Images = imageSet;

                IsLoading = false;
            }
            catch (Exception err)
            {
                RedirectError(err.Message);
            }
        }


        // A method to check if Http response returned is valid
        private bool CheckIsValid(HttpResponseMessage response)
        {
            // Console.WriteLine(response.IsSuccessStatusCode); True|False
            // Console.WriteLine(response.StatusCode); Ok
            if (response.IsSuccessStatusCode)
            {
                // Set info message to message returned from api
                InfoMessage = response.GetResponse();
                return true;
            }

            // Set info error to message returned from api
            ErrorMessage = response.GetResponse();

            // Check if error message matches some error message
            // Return to error page if matched
            if (ErrorMessage.Contains("Sorry"))
                CurrentStep = MaximumStep;

            IsLoading = false;

            return false;
        }


        // A method to reset info and error messages
        private void ResetMessages()
        {
            InfoMessage = "";
            ErrorMessage = "";
        }


        // A method to redirect to error page if there is any exception or error occurs
        private void RedirectError(string message)
        {
            ErrorMessage = message;
            CurrentStep = MaximumStep;
            IsLoading = false;
        }
    }
}