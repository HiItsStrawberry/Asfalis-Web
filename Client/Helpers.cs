using System.Net.Http.Json;
using System.Text;

namespace asfalis.Client
{
    public static class Helpers
    {
        // A method to get the string rsesponse sent from api
        public static string GetResponse(this HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync().Result;
        }

        // A method to get the response sent from api
        public static async Task<T> GetListResponse<T>(this HttpResponseMessage response)
        {
            var res = await response.Content.ReadFromJsonAsync<T>();

            return await Task.FromResult(res!);
        }

        // A method to convert any value to integer
        public static int ToInt<T>(this T value)
        {
            return Convert.ToInt32(value);
        }

        public static string EncodeBase64(this string value)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(byteData);
        }

        //public static string RemoveChars(this string value)
        //{
        //    char[] chars = new char[] { '\'', '"', '{', '}' };

        //    string newVal = chars.Aggregate(value, (c1, c2) => c1.Replace(c2, ' '));

        //    return newVal.Trim();
        //}
    }
}
