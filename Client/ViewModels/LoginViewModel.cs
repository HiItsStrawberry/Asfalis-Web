using System.Text;
using Blazored.Toast.Services;

namespace asfalis.Client.ViewModels
{
    public class LoginViewModel : ILoginViewModel
    {
        private readonly IToastService _toast;
        private readonly HttpClient _httpClient;
        private readonly CustomAuthStateProvider _authState;
        private readonly ILocalStorageService _localStorage;

        public LoginViewModel(IToastService toast, HttpClient HttpClient, ILocalStorageService localStorage, CustomAuthStateProvider authState)
        {
            this._toast = toast;
            this._authState = authState;
            this._httpClient = HttpClient;
            this._localStorage = localStorage;
        }

        #region state variables
        // Set the starting step of the form
        public int CurrentStep { get; set; } = 1;

        // Set the maximum step of the form
        public int MaximumStep { get; set; } = 4;

        // Set the current loading state
        public bool IsLoading { get; set; } = false;

        // A variable to hold info message returned from api
        public string InfoMessage { get; set; } = string.Empty;

        // A variable to hold error message returned from api
        public string ErrorMessage { get; set; } = string.Empty;

        // A list to hold the selected user login images
        public List<string> SelectedImages { get; set; } = new List<string>();

        // A list to hold the random user login images from api
        public List<ImageListDTO> Images { get; set; } = new List<ImageListDTO>();

        // A complex model to hold user inputs for user login
        public LoginComplexModel LoginModel { get; set; } = new LoginComplexModel();
        #endregion

        // A method for user login step 1
        public async Task LoginStepOne()
        {
            try
            {
                IsLoading = true;

                // Call api to validate user personal login
                var resLogin = await _httpClient.PostAsJsonAsync("api/login/personal", LoginModel.LoginPersonal);

                // Check if the response returned is valid
                if (!CheckIsValid(resLogin)) return;

                // Get the response as userId if is valid
                string userId = resLogin.GetResponse();
                LoginModel.UserId = userId.ToInt();
                LoginModel.QRCode.UserId = userId.ToInt();

                // Send QR Code to email
                await SendQRCode();

                // Proceed to user login step 2
                NextLoginStep();
            }
            catch (Exception err)
            {
                RedirectError(err.Message);
            }
        }


        // A method for sending QR Code to email
        public async Task SendQRCode()
        {
            IsLoading = true;

            // Call api for sending a new QR Code to email
            var result = await _httpClient.GetAsync($"api/mail/qrcode/{LoginModel.UserId}");

            // Check if the response returned is valid
            if (!CheckIsValid(result)) return;

            // Render a toast message if is valid to inform the QR Code is sent
            _toast.ShowInfo(InfoMessage, "QR Code Verification");

            IsLoading = false;
        }


        // A method for user login step 2
        public async Task LoginStepTwo()
        {
            try
            {
                IsLoading = true;

                // Call api for validate user personal login
                var resCodeLogin = await _httpClient.PostAsJsonAsync("api/login/qrcode/", LoginModel.QRCode);

                // Check if the response returned is valid
                if (!CheckIsValid(resCodeLogin)) return;

                // Get user login images from api
                await GetLoginImages();

                // Proceed to user login step 3
                NextLoginStep();
            }
            catch (Exception err)
            {
                RedirectError(err.Message);
            }
        }


        // A method to get user login images
        private async Task GetLoginImages()
        {
            // Call api to get the user login images
            var result = await _httpClient.GetAsync($"api/login/images/{LoginModel.UserId}");

            // Check if the response returned is valid
            if (!CheckIsValid(result)) return;

            // Get the responsded user login images if is valid
            var imageSet = await result.GetListResponse<LoginImageListDTO>();

            // Redirect to error page if login images is null
            if (!imageSet.LoginImages!.Any())
            {
                RedirectError(MessageOption.Image);
                return;
            }

            // Set the user login images for displaying
            Images = imageSet.LoginImages!;

            // Set the number of correct images
            LoginModel.ImageCorrectCount = imageSet.CorrectCount;
        }


        // A method for user login step 3
        public async Task LoginStepThree()
        {
            try
            {
                IsLoading = true;

                // Return error message if no image is selected
                if (!SelectedImages.Any())
                {
                    ErrorMessage = MessageOption.ImageAuthFail;
                    IsLoading = false;
                    return;
                }

                // Loop the selected image and bind into Image object
                for (int i = 0; i < SelectedImages.Count; i++)
                {
                    LoginModel.SelectedImages.Add(new Image
                    {
                        Name = SelectedImages[i].EncodeBase64()
                    });
                }

                // Call api to validate the selected user login images
                var resImageLogin = await _httpClient.PostAsJsonAsync(
                    $"api/login/images/{LoginModel.UserId}/{LoginModel.ImageCorrectCount}",
                    LoginModel.SelectedImages);

                // Check if the response returned is valid
                if (!CheckIsValid(resImageLogin))
                {
                    // Call api to return new random login images if not valid
                    await GetLoginImages();

                    // Set the selected images back to null
                    SelectedImages = new List<string>();
                    LoginModel.SelectedImages = new List<Image>();
                    IsLoading = false;
                    return;
                }

                // Generate a new Jwt token if valid
                await GenerateLoginToken();
            }
            catch (Exception err)
            {
                RedirectError(err.Message);
            }
        }


        // A method to proceed user to next login step
        private void NextLoginStep()
        {
            // Increment the currentStep by 1
            int step = CurrentStep + 1;

            // Check if step is the maximum step or next step
            CurrentStep = step >= MaximumStep ? MaximumStep : step;

            // Clear info and error message
            ResetMessages();

            IsLoading = false;
        }


        // A method to generate a new jwt token
        private async Task GenerateLoginToken()
        {
            try
            {
                // Call api to generate a new Jwt token
                var resToken = await _httpClient.GetAsync($"api/login/token/{LoginModel.UserId}");

                // Check if the response returned is valid
                if (!CheckIsValid(resToken)) return;

                // Get the jwt token object if valid
                var token = await resToken.GetListResponse<JwtDTO>();

                // Store the new token into local storage
                await _localStorage.SetItemAsync("auth_token", token.Token);

                // Notify the entire web the authentication state is changed
                await _authState.NotifyStateChanged();
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
            if (ErrorMessage.Contains("Sorry") || ErrorMessage.Equals(MessageOption.LoginFailure))
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
