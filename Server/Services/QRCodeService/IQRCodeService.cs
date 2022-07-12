namespace asfalis.Server.Services.QRCodeService
{
    public interface IQRCodeService
    {
        Task<QRCode> GetQRCode(int codeId = 0, int userId = 0);
        Task<string> GenerateQRCode(string code);
        Task<bool> AddOrUpdateQRCode(QRCode qrcode);
        Task<bool> ExpireQRCode(QRCode qrcode);
        string GenerateCode(int length = 6);
        bool CheckIsCodeExpired(QRCode qrcode);
    }
}
