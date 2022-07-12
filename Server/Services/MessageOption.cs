namespace asfalis.Server.Services
{
    public static class MessageOption
    {
        private const string tail = "Please try again.";

        private const string later = "Please try again later.";

        private const string head = "Sorry, there was an error";

        private enum Tail
        {
            None,
            Later,
            Normal,
        }

        public static class Error
        {
            public static string Image => Message(true, "retrieving images.", Tail.Later);
            public static string QRCode => Message(true, "sending the QR Code.", Tail.Later);
            public static string Login => Message(true, "logging into your account.", Tail.Later);
            public static string Register => Message(true, "registering your account.", Tail.Later);
            public static string LoginCodeNotFound => Message(false, "Code not found.", Tail.Normal);
            public static string Exception(string exception) => Message(false, exception, Tail.Later);
            public static string Email => Message(true, "sending the activiation email.", Tail.Later);
            public static string QRCodeValidation => Message(true, "validating the Code.", Tail.Later);
            public static string Attachment => Message(true, "generating the PDF document.", Tail.Later);
            public static string UserActivation => Message(true, "activiating your account.", Tail.Later);
            public static string JwtTokenGeneration => Message(true, "generating the token.", Tail.Later);
            public static string EmailNotVerifiedMobile => Message(false, "Your email is not verifed.", Tail.Later);
            public static string DatabaseInteraction => Message(true, "interacting with the database.", Tail.Later);
            public static string LoginQRCodeFail => Message(false, "You have entered an invalid code.", Tail.Normal);
            public static string LoginImageFail => Message(false, "You have selected the wrong images.", Tail.Normal);
            public static string MissingDetails => Message(false, "There are missing details to complete the process.", Tail.Normal);
            public static string LoginAttemps => Message(false, "There have been too many login failures from account.", Tail.Later);
            public static string JwtTokenInvalid => Message(false, "Your session token may be invalid or expired. Please sign in again.");
            public static string LoginPersonalFail => Message(false, "You have entered your username or password incorrectly.", Tail.Normal);
            public static string EmailNotVerified => Message(true, "performing this activity with your account because your email is not verified.", Tail.Later);

            private static string Message(bool withHead, string content, Tail option = Tail.None)
            {
                string message;

                if (withHead)
                {
                    message = $"{head} {content}";
                }
                else
                {
                    message = content;
                }


                if (option == Tail.Later)
                {
                    message += " " + later;
                }
                else if (option == Tail.Normal)
                {
                    message += " " + tail;
                }
                return message;
            }
        }

        public static class Success
        {
            public const string QRCode = "The QR code for account verification has been sent to your email.";

            public const string UserActivation = "If the registered email is valid, you will be receiving activiation email from your account";
        }
    }
}
