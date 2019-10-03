using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Limilabs.Client.POP3;
using Limilabs.Mail;

namespace DeveloperTest.Business
{
    public class POP3Manager: IConnectable
    {
        Pop3 pop;
        public POP3Manager()
        {
            this.pop = new Pop3();
        }
        public void Connect(string server, string port, string encryption)
        {
            int portInt;
            if (int.TryParse(port, out portInt))
            {
                if ((string)encryption == "STARTTLS")
                {
                    pop.Connect(server, portInt);
                    pop.StartTLS();
                }
                else
                {
                    pop.ConnectSSL(server, portInt);
                }
            }
            else
            {
                if ((string)encryption == "STARTTLS")
                {
                    pop.Connect(server);
                    pop.StartTLS();
                }
                else
                {
                    pop.ConnectSSL(server);
                }
            }
        }
        public void Authenticate(string username, string password)
        {
            pop.Login(username, password);
        }
        public void SelectInbox()
        {
        }
        public List<string> GetAllUids()
        {
            return pop.GetAll();
        }
        public void PopulateHeaderByUid(string uid, ref EmailView emailView)
        {
            MailBuilder builder = new MailBuilder();
            var headers = pop.GetHeadersByUID(uid);
            IMail email = builder.CreateFromEml(headers);
            EmailViewManager.SetHeader(ref emailView, 
                email.From.First().Name, 
                email.Subject,
                email.Date?.ToString("MM/dd/yyyy hh:mm:ss"));
        }
        public void PopulateBodyByUid(string uid, ref EmailView emailView)
        {
            MailBuilder builder = new MailBuilder();
            IMail email = builder.CreateFromEml(
                pop.GetMessageByUID(uid));
            EmailViewManager.SetBody(ref emailView, email.Text, email.Html);
        }
        public void Close()
        {
            pop.Close();
        }
    }
}
