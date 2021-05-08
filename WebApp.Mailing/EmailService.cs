using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using WebApp.Mailing.Abstractions;

namespace WebApp.Mailing
{
	public class EmailService : IEmailService
	{
		private readonly IMailjetClient Client;

		public EmailService(IMailjetClient mailjetClient)
		{
			Client = mailjetClient;
		}

		public static async Task Send(string from, string to, string subject, string message)
		{
                MailjetClient client = new MailjetClient(Environment.GetEnvironmentVariable("ea755c21df7e0fb986cd751d8e4ee601"), Environment.GetEnvironmentVariable("29c4a8b93e7e9aaeb6a4b3f40136ffb8"));
                MailjetRequest request = new MailjetRequest
                {
                    Resource = Send.Resource,
                }
				 .Property(Send.Messages, new JArray {
     new JObject {
      {
       "From",
       new JObject {
        {"Email", "laragalvanim@gmail.com"},
        {"Name", "Lara"}
       }
      }, {
       "To",
       new JArray {
        new JObject {
         {
          "Email",
          "laragalvanim@gmail.com"
         }, {
          "Name",
          "Lara"
         }
        }
       }
      }, {
       "Subject",
       "Greetings from Mailjet."
      }, {
       "TextPart",
       "My first Mailjet email"
      }, {
       "HTMLPart",
       "<h3>Dear passenger 1, welcome to <a href='https://www.mailjet.com/'>Mailjet</a>!</h3><br />May the delivery force be with you!"
      }, {
       "CustomID",
       "AppGettingStartedTest"
      }
     }
                 });
                MailjetResponse response = await client.PostAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(string.Format("Total: {0}, Count: {1}\n", response.GetTotal(), response.GetCount()));
                    Console.WriteLine(response.GetData());
                }
                else
                {
                    Console.WriteLine(string.Format("StatusCode: {0}\n", response.StatusCode));
                    Console.WriteLine(string.Format("ErrorInfo: {0}\n", response.GetErrorInfo()));
                    Console.WriteLine(response.GetData());
                    Console.WriteLine(string.Format("ErrorMessage: {0}\n", response.GetErrorMessage()));
                }
            }
	}
}
