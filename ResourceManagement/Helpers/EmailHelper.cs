using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace ResourceManagement.Helpers
{
    public class EmailHelper
    {
        public static string SendEmailFromHRMS(Models.Email.SendEmail emailModel)
        {
            try
            {
                var SMTPUserName = !string.IsNullOrWhiteSpace(emailModel.SpecificUserName) ? emailModel.SpecificUserName : ConfigurationManager.AppSettings["SMTPUserName"];
                using (MailMessage mm = new MailMessage(SMTPUserName, emailModel.To))
                {
                    mm.Subject = emailModel.Subject;
                    mm.Body = HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(emailModel.EmailBody));

                    if (!string.IsNullOrWhiteSpace(emailModel.CC))
                    {
                        var ccEmails = emailModel.CC.Split(',');
                        foreach (var ccEmail in ccEmails)
                        {
                            mm.CC.Add(ccEmail.Trim());
                        }

                    }

                    if (!string.IsNullOrWhiteSpace(emailModel.BCC))
                    {
                        var bccEmails = emailModel.BCC.Split(',');
                        foreach (var bccEmail in bccEmails)
                        {
                            mm.Bcc.Add(bccEmail.Trim());
                        }
                    }

                    mm.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = ConfigurationManager.AppSettings["SMTPHost"];
                    smtp.EnableSsl = true;
                    NetworkCredential credentials = new NetworkCredential();
                    credentials.UserName = !string.IsNullOrWhiteSpace(emailModel.SpecificUserName) ? emailModel.SpecificUserName : ConfigurationManager.AppSettings["SMTPUserName"];
                    credentials.Password = !string.IsNullOrWhiteSpace(emailModel.SpecificPassword) ? emailModel.SpecificPassword : ConfigurationManager.AppSettings["SMTPPassword"];
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = credentials;
                    smtp.Port = System.Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                    smtp.Send(mm);
                }

                emailModel.JsonResponse.StatusCode = 200;
                emailModel.JsonResponse.Message = "Status Report Remainder Email Sent Successfully!";
                return JsonConvert.SerializeObject(emailModel);
            }
            catch (Exception ex)
            {
                emailModel.JsonResponse.StatusCode = 500;
                if (ex.InnerException != null && ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message))
                {
                    var actuallErrors = ex.InnerException.InnerException.Message.Split('.');
                    foreach (var actuallError in actuallErrors)
                    {
                        if (actuallError.ToLowerInvariant().Contains("duplicate key value is"))
                        {
                            emailModel.JsonResponse.Message = actuallError;
                        }
                    }
                }
                else
                {
                    emailModel.JsonResponse.Message = ex.Message;
                }
            }

            return JsonConvert.SerializeObject(emailModel);
        }

    }
}