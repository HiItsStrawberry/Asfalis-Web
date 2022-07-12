namespace asfalis.Client.ViewModels
{
    public interface ILoginViewModel
    {
        int CurrentStep { get; set; }
        int MaximumStep { get; set; }
        List<ImageListDTO> Images { get; set; }
        List<string> SelectedImages { get; set; }
        bool IsLoading { get; set; }
        LoginComplexModel LoginModel { get; set; }
        string InfoMessage { get; set; }
        string ErrorMessage { get; set; }

        Task LoginStepOne();
        Task LoginStepThree();
        Task LoginStepTwo();
        Task SendQRCode();
    }
}