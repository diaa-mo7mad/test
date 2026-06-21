using Azure.Core;


using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;



namespace ARGI.BLL.Service
{
    public class EmailSender : IEmailSender
    {
        public EmailSender() { }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {


            var apiToken = "6bc92282d2db1f9579be06ecd477e860";


            var inboxId = 4568008;

            using var client = new HttpClient();


            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);


            var requestBody = new
            {
                to = new[] { new { email = email } },
                from = new { email = "info@dashop.com", name = "ArgiDome Admin" },
                subject = subject,
                html = htmlMessage
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {

                var response = await client.PostAsync($"https://sandbox.api.mailtrap.io/api/send/{inboxId}", content);


                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"Mailtrap API Error: {error}");
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Network Error: {ex.Message}");
            }
        }
    }


}