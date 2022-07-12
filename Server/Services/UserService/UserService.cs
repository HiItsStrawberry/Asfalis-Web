namespace asfalis.Server.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEncryptionService _encryptor;

        public UserService(ApplicationDbContext db, IEncryptionService encryptor)
        {
            this._db = db;
            this._encryptor = encryptor;
        }

        // A method to get random images
        public async Task<List<ImageListDTO>> GetImages(int count, List<string>? filteredImages = null)
        {
            try
            {
                // Getting directory of image folder
                var imageDir = new DirectoryInfo(Helpers.GetFilePathName());

                // Return null if the image cannot be retrieved
                if (!imageDir.Exists) return null!;

                // Retrieving all images with JPG format
                var images = imageDir.GetFiles("*.*").Where(x => x.Extension.Contains("jpg")).ToList();

                // Return null if there is no image
                if (images == null) return null!;

                // Remove only registered images from the original image list
                if (filteredImages != null) images.RemoveAll(x => filteredImages.Contains(x.FullName));

                // Shuffling all retrieved images
                var shuffledImages = await Helpers.ShuffleImagesAsync(images!);

                // Getting random images
                var imageSet = await Helpers.GetRandomImages(shuffledImages, count);

                // Transform image data into byte data
                var imageList = await Helpers.TransformImageData(imageSet);

                // Return images
                return await Task.FromResult(imageList);
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return null!;
            }
        }


        // A method to get user from database
        public async Task<User> GetUser(int userId = 0, string? name = null)
        {
            try
            {
                var user = new User();

                if (userId != 0 && !name.IsEmpty())
                {
                    // Get the first user by userId and username
                    user = await _db.Users!.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.Username == name);
                }
                else if (userId == 0)
                {
                    // Get the user by username
                    user = await _db.Users!.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Username == name || x.Email == name);
                }
                else
                {
                    // Get the user by userId
                    user = await _db.Users!.FindAsync(userId);
                }

                return user!;
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return null!;
            }
        }


        // A method to get the images for user login
        public async Task<LoginImageListDTO> GetLoginImages(int userId)
        {
            try
            {
                // Get the images of a user
                var images = await this.GetUserImages(userId);

                // Get all user images from the file
                var imageList = new List<string>();
                for (int i = 0; i < images.Count; i++)
                {
                    imageList.Add(Helpers.GetFilePathName(filename: images[i].Name!));
                }

                // Get a random number from 1 to 4
                var count = new Random().Next(1, 5);

                // Get number of images excluding user images based on (9 - random number)
                var randomImages = await this.GetImages(9 - count, imageList);

                // Shuffle the user image list
                imageList = (List<string>)await Helpers.ShuffleImagesAsync(imageList!);

                // Get random images from user images based on the random number
                var randomUserImages = await Helpers.GetRandomImages(imageList, count);

                // Transform user image data into byte data
                var userImages = await Helpers.TransformImageData(randomUserImages);

                // Combine random images and user images
                var loginImages = randomImages.Union(userImages).ToList();

                // Shuffle the combined images
                loginImages = (List<ImageListDTO>)await Helpers.ShuffleImagesAsync(loginImages!);

                // Set the total of correct images to CorrectCount
                // Set the combined images to LoginImages
                var loginImageList = new LoginImageListDTO
                {
                    CorrectCount = userImages.Count,
                    LoginImages = loginImages
                };

                // Return the login images
                return await Task.FromResult(loginImageList);
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return null!;
            }
        }


        // A method to get user image
        public async Task<List<Image>> GetUserImages(int userId)
        {
            try
            {
                // Get all user images by UserId
                var images = await _db.Users!
                    .Where(x => x.UserId == userId)
                    .SelectMany(x => x.Images!).ToListAsync();

                // Decrypt the user image
                var userImages = new List<Image>();

                for (int i = 0; i < images.Count; i++)
                {
                    userImages.Add(new Image
                    {
                        ImageId = images[i].ImageId,
                        Name = await _encryptor.Aes_Decrypt(images[i].Name!)
                    });
                }

                return await Task.FromResult(userImages);
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return null!;
            }
        }


        // A method to register user
        public async Task<User> RegisterUser(User user)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // Adding user and image data into database
                await _db.Users!.AddAsync(user);

                // Saving the database changes
                await _db.SaveChangesAsync();

                // Commit the changes if there is no error
                await transaction.CommitAsync();

                return user;
            }
            catch (Exception err)
            {
                await transaction.RollbackAsync();
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return null!;
            }
        }


        // A method to activivate the user account
        public async Task<bool> ActivateUser(int userId)
        {
            try
            {
                // Get the user from database using the userId
                var user = await _db.Users!.FindAsync(userId);

                if (user == null) return false;

                // Update the email has been confirmed by user into the database
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await _db.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return false;
            }
        }


        // A method to check existing username
        public async Task<bool> GetUsername(string username)
        {
            try
            {
                // Finding the any user from database that matches the username
                var found = await _db.Users!.AsNoTracking().FirstOrDefaultAsync(x => x.Username == username);
                // Return found result as boolean
                return found == null;
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return false;
            }
        }


        // A method to check existing email
        public async Task<bool> GetEmail(string email)
        {
            try
            {
                // Finding the any user from database that matches the username
                var found = await _db.Users!.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
                // Return found result as boolean
                return found == null;
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return false;
            }
        }


        // A method to check if the login is still valid
        public async Task<bool> CheckIsValidLogin(User user, bool increment = false)
        {
            bool isValid;
            try
            {
                // Return true if the account is not being lockout
                if (user.LockoutEnd == null)
                {
                    isValid = true;
                }
                else
                {
                    // Check if the lockout time is expired
                    isValid = user.LockoutEnd <= Helpers.GetCurrentDate();

                    // Reset user login is valid if lockout time is expired
                    if (isValid)
                    {
                        user.LockoutEnd = null;
                        user.AccessFailedTime = 0;
                        _db.Users!.Update(user);
                        await _db.SaveChangesAsync();
                        return isValid;
                    }
                }

                if (user.AccessFailedTime >= 3 && !increment && user.LockoutEnd == null)
                {
                    // Set lockout time of 15 minutes if the user has reached the maximum login attemps
                    user.LockoutEnd = Helpers.GetCurrentDate(15);
                    isValid = false;
                }
                else if (user.AccessFailedTime <= 3 && increment)
                {
                    // Increment user's login failure attemp by 1
                    user.AccessFailedTime += 1;
                    isValid = false;
                }

                // Save the latest info into database
                _db.Users!.Update(user);
                await _db.SaveChangesAsync();
                return isValid;
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return false;
            }
        }
    }
}
