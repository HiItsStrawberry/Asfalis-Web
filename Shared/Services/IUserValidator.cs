namespace asfalis.Shared.Services
{
    public interface ICustomUserValidator
    {
        // A method to validate existing email
        Task<bool> CheckEmail(string email, CancellationToken token);

        // A method to validate existing username
        Task<bool> CheckUsername(string username, CancellationToken token);
    }
}
