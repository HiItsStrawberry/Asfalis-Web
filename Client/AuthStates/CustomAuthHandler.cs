using System.Net.Http.Headers;

namespace asfalis.Client.AuthStates
{
    public class CustomAuthHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;

        public CustomAuthHandler(ILocalStorageService localStorage)
        {
            this._localStorage = localStorage;
        }

        // This handler is to include the token found in local storage into the Http Header 
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // This came without quote
            var token = await _localStorage.GetItemAsync<string>("auth_token");

            // This came with quote
            //var token2 = await _localStorage.GetItemAsStringAsync("auth_token");

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
