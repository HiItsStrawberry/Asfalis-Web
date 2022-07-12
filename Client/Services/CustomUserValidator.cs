namespace asfalis.Client.Services
{
    public class CustomUserValidator : ICustomUserValidator
    {
        private readonly HttpClient _httpClient;

        public CustomUserValidator(HttpClient httpClient)
        {
            this._httpClient = httpClient;
        }

        public async Task<bool> CheckEmail(string email, CancellationToken token)
        {
            var requestUri = $"api/register/email/{email}";
            var isValid = await _httpClient.GetFromJsonAsync<bool>(requestUri, token);
            return isValid;
        }

        public async Task<bool> CheckUsername(string username, CancellationToken token)
        {
            var requestUri = $"api/register/username/{username}";
            var isValid = await _httpClient.GetFromJsonAsync<bool>(requestUri, token);
            return isValid;
        }
    }
}
