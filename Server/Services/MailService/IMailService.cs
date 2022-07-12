using System.Net.Mail;

namespace asfalis.Server.Services
{
    public interface IMailService
    {
        string ActivationMessage(string username, string url);
        string GenerateBody(string username = "Mr/Mrs.", string content = "");
        string GenerateImagePDF(List<Image> images, string username);
        AlternateView ImageMessage(string username, string image);
        Task<bool> SendEmail(string toEmail, string subject, string body = "", string attachmentFile = "", AlternateView? image = null);
    }
}