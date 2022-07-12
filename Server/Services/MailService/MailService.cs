using Spire.Pdf;
using Spire.Pdf.Graphics;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace asfalis.Server.Services
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _config;

        public MailService(IConfiguration config)
        {
            this._config = config;
        }

        // A method to generate a PDF file for user images
        public string GenerateImagePDF(List<Image> images, string username)
        {
            try
            {
                // Getting the image folder path
                string imagePath = Helpers.GetFilePathName();

                // Return error message if the image folder does not exist
                if (!Directory.Exists(imagePath)) return MessageOption.Error.Attachment;

                // Create a new PDF document
                var document = new PdfDocument();
                var page = document.Pages.Add();

                // Inserting a title in the PDf document
                // Font: Helvetica, size 20
                // Color: Black
                page.Canvas.DrawString(
                    "\nYour registered images\n\n",
                    new PdfFont(PdfFontFamily.Helvetica, 20f),
                    new PdfSolidBrush(System.Drawing.Color.Black), 10, 10);


                // Return error message if the registered image list is empty
                if (images == null || images.Count <= 0) return MessageOption.Error.Attachment;

                // Variable for styling the image in the PDF document
                int count = 0;
                float x, y = 70f, width = 150f, height = 150f, position = 0f;


                foreach (var image in images)
                {
                    // Find the current image from the image folder
                    var curImage = PdfImage.FromFile(Path.Combine(imagePath, image.Name!));

                    // Return error message if the image cannot be found
                    if (curImage == null) return MessageOption.Error.Attachment;

                    // Positioning first 3 images
                    if (count < 3)
                    {
                        x = position * count;
                        count += 1;
                        position = 160f;
                        // Insert image into the PDF document
                        page.Canvas.DrawImage(curImage, x, y, width, height);
                        continue;
                    }

                    // Positioning last 2 images
                    count = 1;
                    x = position * count;
                    position += 160f;
                    y = 230f;
                    page.Canvas.DrawImage(curImage, x, y, width, height);
                }

                // Save the PDF document
                document.SaveToFile(Helpers.GetFilePathName(Helpers.PathOptions.PDF, $"{username}_registered_image.pdf"));

                // Close and dispose the PDF document
                document.Close();
                document.Dispose();

                // Return the file name as a successful message
                return Helpers.GetFilePathName(Helpers.PathOptions.PDF, $"{username}_registered_image.pdf");
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return MessageOption.Error.Attachment;
            }
        }


        // A method to send email to the given email
        public Task<bool> SendEmail(string toEmail, string subject, string? body = null, string? attachmentFile = null, AlternateView? image = null)
        {
            try
            {
                // Create a new mail and smtp object
                // Setting up the mail properties
                // The mail being sent from
                var mail = new MailMessage
                {
                    From = new MailAddress(_config["MailSettings:FromEmail"])
                };
                // The mail being sent to
                mail.To.Add(new MailAddress(toEmail));
                // The subject of the mail
                mail.Subject = subject;
                // The body or content of the mail
                mail.IsBodyHtml = true;

                #region ImageAttachment
                if (image == null)
                {
                    // Set the mail body to the given body
                    mail.Body = body;
                }
                else
                {
                    // Else set the body with an image
                    mail.AlternateViews.Add(image);
                }
                #endregion

                #region NormalAttachment
                if (!attachmentFile!.IsEmpty())
                {
                    // Set the attachment to email if not empty
                    mail.Attachments.Add(new Attachment(attachmentFile!));
                }
                #endregion

                #region SmtpConfiguration
                var smtp = new SmtpClient
                {
                    // Network host
                    Host = _config["MailSettings:Host"],

                    // Smtp server port
                    Port = Convert.ToInt32(_config["MailSettings:Port"]),

                    // Set to true if the host accepts SSL
                    EnableSsl = true,

                    // Do not use any default credentials
                    UseDefaultCredentials = false,

                    // Use own email credentials
                    Credentials = new NetworkCredential(_config["MailSettings:FromEmail"], _config["MailSettings:AppPassword"]),

                    // Set the delivery method
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
                #endregion

                // Create a thread to run on background
                Thread email = new(delegate ()
                {
                    // Send email as background task
                    // So the code will continue running without blocking by this
                    Console.WriteLine("Sending Email");
                    smtp.Send(mail);
                    mail.Dispose();
                    Console.WriteLine("Email Sent");
                })
                {
                    IsBackground = true
                };
                email.Start();

                return Task.FromResult(true);
            }
            catch (Exception err)
            {
                Console.WriteLine(MessageOption.Error.Exception(err.Message));
                return Task.FromResult(false);
            }
        }


        // A method to setup the header and footer of the mail body
        public string GenerateBody(string username = "user", string content = "")
        {
            var str = new StringBuilder();

            if (!string.IsNullOrEmpty(content))
            {
                str.Append($"Dear {username},<br><br>");
                str.Append(content);
                str.Append("Best Ragards,<br>");
                str.Append("Asfalis Community");
            }
            return str.ToString();
        }


        // A method to generate a normal mail body for account activation
        public string ActivationMessage(string username, string url)
        {
            var str = new StringBuilder();
            str.Append("Thank you for joining us.<br><br>");
            str.Append("Please click the below link to activate your account.<br>");
            str.Append($"<a href={url}>Activiation link</a><br><br>");
            str.Append("Besides that, the attached PDF is your registered images.<br>");
            str.Append("You can opt to download and save it as you might forget these images.<br><br>");

            // Combine this mail body with the header and footer
            string message = this.GenerateBody(username, str.ToString());

            return message;
        }


        // A method to generate a normal mail body
        public AlternateView ImageMessage(string username, string image)
        {
            var str = new StringBuilder();
            var inline = new LinkedResource(image, MediaTypeNames.Image.Jpeg);

            str.Append("Please kindly scan the QR Code to get the login code.<br><br>");
            //str.Append("Please click the box to view the QR Code if it is blank.<br><br>");
            str.Append($@"<img src='cid:{inline.ContentId}' id='img' alt='QRCode' width='200px' height='200px'/><br><br>");
            str.Append("<h3>Note: This QR Code will be expired after a period of time.</h3><br><br>");

            // Combine this mail body with the header and footer
            string message = this.GenerateBody(username, str.ToString());

            // Setup the alternate view with the mail body generated in order to send a body with image
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(message, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(inline);

            return alternateView;
        }
    }
}