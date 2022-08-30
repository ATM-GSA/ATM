using FluentEmail.Core;
using FluentEmail.Razor;
using FluentEmail.Smtp;
using System.Net.Mail;
using System.Threading.Tasks;
using TABS.Data.Emails;

namespace TABS.Data
{
    public class EmailService
    {

        /// <summary>
        /// Instantiate an SmtpSender using the SMTP out server
        /// </summary>
        private readonly SmtpSender sender = new SmtpSender(() => new SmtpClient("")
        {
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Port = 25
        });

        /// <summary>
        /// Send an email to the given address using the specified template 
        /// </summary>
        /// <param name="to">To address</param>
        /// <param name="templateName">Name of the .cshtml template from the templates folder</param>
        /// <param name="templateModel">Object that contains the values for the template parameters</param>
        /// <returns></returns>
        public async Task SendTemplatedEmail(string to, string templateName, object templateModel)
        {
            Email.DefaultRenderer = new RazorRenderer();
            Email.DefaultSender = sender;

            // Load the template
            ITemplate Template = EmailTemplateService.GetTemplate(templateName);
            if (Template == null)
            {
                // Template not found, don't send email
                return;
            }

            await Email.From("")
                .To(to)
                .Subject(Template.GetSubject())
                .UsingTemplate(Template.GetTemplate(), templateModel)
                .SendAsync();

        }

        /// <summary>
        /// Send an email to the given address
        /// </summary>
        /// <param name="to">To address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body</param>
        /// <returns></returns>
        public async Task SendEmail(string to, string subject, string body)
        {
            Email.DefaultRenderer = new RazorRenderer();
            Email.DefaultSender = sender;
            await Email.From("")
                 .To(to)
                 .Subject(subject)
                 .Body(body)
                 .SendAsync();
        }
    }
}
