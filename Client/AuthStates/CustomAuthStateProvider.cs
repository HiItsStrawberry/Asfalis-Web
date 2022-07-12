using System.Net.Http.Headers;
using System.Security.Claims;

namespace asfalis.Client.AuthStates
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public CustomAuthStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            this._httpClient = httpClient;
            this._localStorage = localStorage;
        }

        // A method to identify the current login state
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Create a empty claims
            var identity = new ClaimsIdentity();

            // Valid the user with the login jwt token
            var token = await this.ValidateUserWithJwt();

            // Get the claims from the token if the token is not null
            if (!string.IsNullOrEmpty(token))
            {
                identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            }
            
            // Return valid of invalid login state based on the claims
            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }

        // A method to notify the login state to the entire web application
        public async Task NotifyStateChanged()
        {
            await Task.Run(() =>
            {
                NotifyAuthenticationStateChanged(this.GetAuthenticationStateAsync());
            });
        }


        // A method to validate the user login state with the jwt token
        private async Task<string> ValidateUserWithJwt()
        {
            try
            {
                // Check if there is a login token exists in local storage
                if (!await _localStorage.ContainKeyAsync("auth_token")) return null!;

                // Retreive the token from the local storage
                var token = await _localStorage.GetItemAsync<string>("auth_token");

                // Check if the token retrieved is null
                if (string.IsNullOrEmpty(token)) return null!;

                // Bind the token retrieved into a new JwtDTO object
                var jwtToken = new JwtDTO { Token = token };

                // Send the token to the api for validation
                var resValidation = await _httpClient.PostAsJsonAsync("api/login/token/validation", jwtToken);

                // Return null value if the validation failed
                if (!resValidation.IsSuccessStatusCode) return null!;

                // Retrieve the token sent back from API if validation successed
                var validToken = await resValidation.GetListResponse<JwtDTO>();

                // Return null value if the token sent back is null
                if (string.IsNullOrEmpty(validToken.Token)) return null!;

                // Set the validated token into the local storage
                await _localStorage.SetItemAsync("auth_token", validToken.Token);

                // Return the validated token to continue the previous process
                return await Task.FromResult(validToken.Token);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
            return null!;
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            return keyValuePairs!.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!));
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
