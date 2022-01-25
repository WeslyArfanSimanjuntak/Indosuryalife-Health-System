using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
namespace Web.MainApplication.WebUtility
{
    public static class EmailHelper
    {

        
        public static bool SendEmail(string To, string Subject, string CC, string Body)
        {
            SmtpSection smtpConfiguration = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
            SmtpNetworkElement config = smtpConfiguration.Network;
            try
            {
                if (config.EnableSsl)
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                    delegate (
                        object s,
                        X509Certificate certificate,
                        X509Chain chain,
                        SslPolicyErrors sslPolicyErrors
                    )
                    {
                        return true;
                    };
                }

                SmtpClient smtp = new SmtpClient();
                MailMessage newMessage = new MailMessage();

                if (To != string.Empty)
                {
                    if (To.Contains(";"))
                    {
                        var toList = To.Split(';');
                        foreach (var item in toList)
                        {
                            newMessage.To.Add(item);
                        }
                    }
                    else
                    {
                        newMessage.To.Add(To);
                    }

                }

                newMessage.From = new MailAddress(smtpConfiguration.From);
                newMessage.Subject = Subject;
                newMessage.IsBodyHtml = true;


                if (CC != null)
                {
                    string[] CCId = CC.Split(';');
                    foreach (string CCEmail in CCId)
                    {
                        newMessage.CC.Add(new MailAddress(CCEmail));
                    }
                }
                newMessage.Body = Body;
                smtp.Send(newMessage);
                newMessage.Dispose();
                smtp.Dispose();
                return true;
            }
            catch (Exception e)
            {
                e.MessageToList();
                return false;
            }
        }

        public static bool SendEmail(string To, string Subject, string CC, string Body, string username, string custName, string custCode)
        {
            SmtpSection smtpConfiguration = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
            SmtpNetworkElement config = smtpConfiguration.Network;
            if (string.IsNullOrEmpty(username))
                username = smtpConfiguration.From;
            try
            {
                if (config.EnableSsl)
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                    delegate (
                        object s,
                        X509Certificate certificate,
                        X509Chain chain,
                        SslPolicyErrors sslPolicyErrors
                    )
                    {
                        return true;
                    };
                }

                SmtpClient smtp = new SmtpClient();
                MailMessage newMessage = new MailMessage();
                newMessage.From = new MailAddress(smtpConfiguration.From);
                if (To != string.Empty)
                {
                    if (To.Contains(";"))
                    {
                        var toList = To.Split(';');
                        foreach (var item in toList)
                        {
                            newMessage.To.Add(item);
                        }
                    }
                    else
                    {
                        newMessage.To.Add(To);
                    }
                }

                newMessage.Subject = Subject;
                newMessage.IsBodyHtml = true;


                if (CC != null)
                {
                    string[] CCId = CC.Split(';');
                    foreach (string CCEmail in CCId)
                    {
                        newMessage.CC.Add(new MailAddress(CCEmail));
                    }
                }
                newMessage.Body = Body;
                smtp.Send(newMessage);


                newMessage.Dispose();
                smtp.Dispose();
                return true;
            }
            catch (Exception e)
            {
                e.MessageToList();
                return false;
            }
        }

        public static bool SendEmail(string To, string Subject, string CC, string Body, string AttachmentFile)
        {
            try
            {
                SmtpSection smtpConfiguration = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
                SmtpNetworkElement config = smtpConfiguration.Network;

                ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                )
                {
                    return true;
                };

                SmtpClient smtp = new SmtpClient();
                MailMessage newMessage = new MailMessage();
                newMessage.From = new MailAddress(smtpConfiguration.From);
                foreach (var item in To.Split(';'))
                {
                    newMessage.To.Add(new MailAddress(item));
                }

                newMessage.Subject = Subject;
                newMessage.Body = Body;
                if (CC != null)
                {
                    string[] CCId = CC.Split(';');
                    foreach (string CCEmail in CCId)
                    {
                        newMessage.CC.Add(new MailAddress(CCEmail));
                    }
                }
                newMessage.IsBodyHtml = true;
                Attachment attachment;

                if (!string.IsNullOrEmpty(AttachmentFile))
                {
                    if (File.Exists(AttachmentFile))
                    {
                        attachment = new Attachment(AttachmentFile);
                        newMessage.Attachments.Add(attachment);
                    }
                }

                smtp.Send(newMessage);
                newMessage.Attachments.Dispose();
                return true;
            }
            catch (Exception e)
            {
                e.MessageToList();
                return false;
            }
        }

        public static bool SendEmail(string To, string Subject, string CC, string Body, IList<AttachmentFile> AttachmentFiles)
        {
            try
            {
                SmtpSection smtpConfiguration = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
                SmtpNetworkElement config = smtpConfiguration.Network;

                ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                )
                {
                    return true;
                };

                SmtpClient smtp = new SmtpClient();
                MailMessage newMessage = new MailMessage();
                newMessage.From = new MailAddress(smtpConfiguration.From);
                foreach (var item in To.Split(';'))
                {
                    newMessage.To.Add(new MailAddress(item));
                }

                newMessage.Subject = Subject;
                newMessage.Body = Body;
                if (CC != null)
                {
                    string[] CCId = CC.Split(';');
                    foreach (string CCEmail in CCId)
                    {
                        newMessage.CC.Add(new MailAddress(CCEmail));
                    }
                }
                newMessage.IsBodyHtml = true;
                if (AttachmentFiles != null)
                {
                    foreach (var item in AttachmentFiles)
                    {
                        Stream fileStream = new MemoryStream(item.FileByte);

                        Attachment attachment;
                        attachment = new Attachment(fileStream, item.FileName);

                        newMessage.Attachments.Add(attachment);
                    }
                }

                smtp.Send(newMessage);
                newMessage.Attachments.Dispose();
                return true;
            }
            catch (Exception e)
            {

                e.MessageToList();

                return false;
            }
        }

        public static async Task<bool> SendEmailAsync(string To, string Subject, string CC, string Body, IList<AttachmentFile> AttachmentFiles)
        {
            try
            {
                SmtpSection smtpConfiguration = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
                SmtpNetworkElement config = smtpConfiguration.Network;

                ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                )
                {
                    return true;
                };

                SmtpClient smtp = new SmtpClient();
                MailMessage newMessage = new MailMessage(smtpConfiguration.From, To);

                newMessage.Subject = Subject;
                newMessage.Body = Body;
                if (CC != null)
                {
                    string[] CCId = CC.Split(';');
                    foreach (string CCEmail in CCId)
                    {
                        newMessage.CC.Add(new MailAddress(CCEmail));
                    }
                }
                newMessage.IsBodyHtml = true;
                if (AttachmentFiles != null)
                {
                    foreach (var item in AttachmentFiles)
                    {
                        Stream fileStream = new MemoryStream(item.FileByte);

                        Attachment attachment;
                        attachment = new Attachment(fileStream, item.FileName);

                        newMessage.Attachments.Add(attachment);
                    }
                }

                await smtp.SendMailAsync(newMessage);
                return true;
            }
            catch (Exception e)
            {

                e.MessageToList();

                return false;
            }
        }

        public static async Task<bool> SendEmailAsync(MailMessage newMessage)
        {
            try
            {
                SmtpSection smtpConfiguration = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
                SmtpNetworkElement config = smtpConfiguration.Network;

                ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                )
                {
                    return true;
                };

                SmtpClient smtp = new SmtpClient();
                newMessage.From = new MailAddress(smtpConfiguration.From);

                await smtp.SendMailAsync(newMessage);
                return true;
            }
            catch (Exception e)
            {

                e.MessageToList();

                return false;
            }
        }


        public class ObjAttach
        {
            public byte[] AttachFileByte { get; set; }
            public string AttachName { get; set; }
        }

        public class ObjMailAddress
        {
            public string MailAddress { get; set; }
            public string MailName { get; set; }
        }
        /**
         * SEND EMAIL WITH MULTIPLE ATTACHMENTS
         */
    }

    public class AttachmentFile
    {
        public byte[] FileByte { get; set; }
        public string FileName { get; set; }
    }

    public class ObjAttach
    {
        public byte[] AttachFileByte { get; set; }
        public string AttachName { get; set; }
    }

    public class ObjMailAddress
    {
        public string MailAddress { get; set; }
        public string MailName { get; set; }
    }

    public class ObjResult
    {
        public ObjResult(bool status, string msg = "", object obj = null)
        {
            Status = status;
            Msg = msg;
            Obj = obj;
        }

        public string Msg { get; set; }
        public object Obj { get; set; }
        public bool Status { get; set; }
    }

    public class ObjSendMail
    {
        public ObjSendMail()
        {
            MailCC = new List<ObjMailAddress>();
            MailBCC = new List<ObjMailAddress>();
            Attachments = new List<ObjAttach>();
        }

        public List<ObjAttach> Attachments { get; set; }

        [Required(ErrorMessage = "Convert Body Email to Base64")]
        public string Body { get; set; }

        [Required]
        public int BrokerId { get; set; }

        public List<ObjMailAddress> MailBCC { get; set; }

        public List<ObjMailAddress> MailCC { get; set; }

        [Required]
        [EmailAddress]
        public string MailTo { get; set; }

        public Guid ReferenceId { get; set; }

        public String ReferenceName { get; set; }

        [Required]
        public string Subject { get; set; }
        [Required]
        public String Type { get; set; }
    }
}
