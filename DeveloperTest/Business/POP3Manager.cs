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
        public Dictionary<string, EmailView> EmailList { get; set; }
        public bool ListChanged { get; set; } = false;

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

        public async void PopulateHeaderAsync(string uid)
        {
            await Task.Run(() => GetHeader(uid));
        }
        private void GetHeader(string uid)
        {
            lock (EmailList)
            {
                var emailView = EmailList[uid.ToString()];
                lock (pop)
                {
                    MailBuilder builder = new MailBuilder();
                    var headers = pop.GetHeadersByUID(uid);
                    IMail email = builder.CreateFromEml(headers);
                    EmailViewManager.SetHeader(ref emailView,
                        email.From.First().Name,
                        email.Subject,
                        email.Date?.ToString("MM/dd/yyyy hh:mm:ss"));
                    ListChanged = true;
                }
            }
        }
        public async Task PopulateBodyAsync(string uid)
        {
            await Task.Run(() => GetBody(uid));
        }
        public void PopulateBodySync(string uid)
        {
            GetBody(uid);
        }

        private void GetBody(string uid)
        {
            lock (EmailList)
            {
                var emailView = EmailList[uid.ToString()];
                if (emailView.Text == null)
                {
                    lock (pop)
                    {
                        MailBuilder builder = new MailBuilder();
                        IMail email = builder.CreateFromEml(
                            pop.GetMessageByUID(uid));
                        EmailViewManager.SetBody(ref emailView, email.Text, email.Html);
                        ListChanged = true;
                    }
                }
            }
        }
        public void Close()
        {
            lock (pop)
            {
                pop.Close();
            }
        }
    }
}
