namespace asfalis.Client.ViewModels
{
    public interface IRegisterViewModel
    {
        int CurrentStep { get; set; }
        int MaximumStep { get; set; }
        List<ImageListDTO> Images { get; set; }
        bool IsLoading { get; set; }
        string InfoMessage { get; set; }
        string ErrorMessage { get; set; }
        RegisterComplexModel RegisterModel { get; set; }

        Task GetRegistrationImage();
        void PreviousRegisterStep();
        Task RegisterStepOne();
        Task RegisterStepTwo();
    }
}