using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace asfalis.Server
{
    public static class Helpers
    {
        // To check if the request is sent from a mobile side
        static readonly Regex MobileCheck = new(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        static readonly Regex MobileVersionCheck = new(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        // A method to validate all empty values
        public static bool ValidateValues(this object[] values)
        {
            // Loop all values from the object
            for (int i = 0; i < values.Length; i++)
            {
                // Return invalid if value is null
                if (values[i] == null) return false;

                // Get the value type - string / int
                var type = values[i].GetType();

                if (type == typeof(string) || type == typeof(char))
                {
                    // Validate empty string
                    if (values[i].ToString()!.IsEmpty()) return false;
                }
                else if (type == typeof(int))
                {
                    // Validate empty integer
                    if (values[i] == null) return false;
                }
            }

            return true;
        }

        // A method to shuffle a list
        public static async Task<IList<T>> ShuffleImagesAsync<T>(IList<T> images)
        {
            // Random shuffling the list using the Random class
            return await Task.Run(() =>
            {
                var rdm = new Random();
                var shuffledImages = images.OrderBy(x => rdm.Next()).ToList();
                return shuffledImages;
            });
        }

        // A method to transform image data to byte data before sending to client side
        public static async Task<List<ImageListDTO>> TransformImageData(HashSet<string> images)
        {
            var imageList = new List<ImageListDTO>();

            // Loop all images
            foreach (var image in images)
            {
                // Read the byte data from the image
                var bytes = await File.ReadAllBytesAsync(image);

                // Add the transffered image into ImageList object
                imageList.Add(new ImageListDTO
                {
                    Name = Path.GetFileName(image),
                    ImageData = Convert.ToBase64String(bytes, 0, bytes.Length)
                });
            }

            return imageList;
        }

        // A method to get random images from a given list
        public static async Task<HashSet<string>> GetRandomImages<T>(IList<T> imageList, int count)
        {
            return await Task.Run(() =>
            {
                // Creating a new random and hashset instance
                var random = new Random();
                var imageSet = new HashSet<string>();

                // Getting number of images
                while (imageSet.Count < count)
                {
                    // Use random class to get a random image among the images
                    var index = random.Next(imageList.Count);
                    imageSet.Add(imageList[index]!.ToString()!);
                }

                return imageSet;
            });
        }

        // A method to get current date of additional minutes
        public static DateTime GetCurrentDate(int minutes = 0)
        {
            if (minutes == 0) return DateTime.Now;

            return DateTime.Now.AddMinutes(minutes);
        }

        // A method to convert any value to integer
        public static int ToInt<T>(this T value)
        {
            return Convert.ToInt32(value);
        }

        // A method to validate empty string
        public static bool IsEmpty(this string? value)
        {
            return string.IsNullOrEmpty(value);
        }

        // A method to convert char value to string value by Gender
        public static string ToGender(this char? gender)
        {
            if (gender == 'M')
            {
                return "Male";
            }
            return "Female";
        }

        // A method to get the particular filepath
        public static string GetFilePathName(PathOptions path = PathOptions.Image, string filename = "")
        {
            string filepath = string.Empty;

            // Three filepath options
            switch (path)
            {
                case PathOptions.Image:
                    filepath = "Images";
                    break;
                case PathOptions.PDF:
                    filepath = "PDFs";
                    break;
                case PathOptions.QRCode:
                    filepath = "QRCodes";
                    break;
            }

            // Return only the path
            if (filename.IsEmpty()) return Path.Combine(Directory.GetCurrentDirectory(), filepath);

            // Return the path with the requested file
            return Path.Combine(Directory.GetCurrentDirectory(), filepath + "\\" + filename);
        }

        // An enum for choosing the path
        public enum PathOptions
        {
            Image = 0,
            PDF = 1,
            QRCode = 2,
        }

        // A method to check if request sent from Desktop or Mobile
        // Return false if request come from Desktop else Mobile
        public static bool CheckIsMobile(this HttpContext context)
        {
            // Check if there is an user agent
            if (!context.Request.Headers.TryGetValue(HeaderNames.UserAgent, out var userAgent))
            {
                return false;
            }

            // Convert the user agent to string
            string agent = userAgent.ToString();

            // Check if request send from mobile (okhttp)
            if (agent.Contains("okhttp") || agent.Contains("Postman")) return true;

            if (agent.Length < 4) return false;

            if (MobileCheck.IsMatch(userAgent) || MobileVersionCheck.IsMatch(agent[..4]))
                return true;

            return false;
        }

        // A method to convert user data based on mobile user class
        public static MobileUserDTO TransformMobile(this User user)
        {
            return new MobileUserDTO()
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email
            };
        }

        // A method to get the claims from a given jwt token
        public static IEnumerable<Claim> ParseClaimsFromJwt(this string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = payload.ParseBase64WithoutPadding();
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            return keyValuePairs!.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!));
        }

        // A method to converting the token to a string
        public static byte[] ParseBase64WithoutPadding(this string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

        public static string DecodeBase64(this string value)
        {
            byte[] byteData = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(byteData);
        }
    }
}
