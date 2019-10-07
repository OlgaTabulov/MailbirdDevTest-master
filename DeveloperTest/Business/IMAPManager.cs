using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Limilabs.Client.IMAP;

namespace DeveloperTest.Business
{
    public class IMAPManager: IConnectable
    {
        Imap imap;
        public Dictionary<string, EmailView> EmailList { get; set; }
        public bool ListChanged { get; set; } = false;
        public IMAPManager()
        {
            this.imap = new Imap();
        }
        public void Connect(string server, string port, string encryption)
        {
            int portInt;
            if (int.TryParse(port, out portInt))
            {
                if ((string)encryption == "STARTTLS")
                {
                    imap.Connect(server, portInt);
                    imap.StartTLS();
                }
                // I am defaulting to safe connection scenario, in reality this would include Unencryped connection / error messaging if not available
                else
                {
                    imap.ConnectSSL(server, portInt);
                }
            }
            else
            {
                if ((string)encryption == "STARTTLS")
                {
                    imap.Connect(server);
                    imap.StartTLS();
                }
                else
                {
                    imap.ConnectSSL(server);
                }
            }
        }
        public void Authenticate(string username, string password)
        {
            imap.UseBestLogin(username, password);
        }
        public void SelectInbox()
        {
            imap.SelectInbox();
        }
        public List<string> GetAllUids()
        {
            List<string> list;
            lock (imap)
            {
                list = imap.Search(Flag.All).Select(c => c.ToString()).ToList();
            }
            return list;
        }
        public async void PopulateHeaderAsync(string uid)
        {
            long uidLong = long.Parse(uid);
            await Task.Run(() => GetHeader(uidLong));
        }
        private void GetHeader(long uid)
        {
            lock (EmailList)
            {
                var emailView = EmailList[uid.ToString()];
                lock (imap)
                {
                    MessageInfo info = imap.GetMessageInfoByUID(uid);
                    EmailViewManager.SetHeader(ref emailView, info.Envelope.From.First().Name, info.Envelope.Subject,
                        info.Envelope.Date?.ToString("MM/dd/yyyy hh:mm:ss"));
                    ListChanged = true;
                }
            }
        }
        public async Task PopulateBodyAsync(string uid)
        {
            long uidLong = long.Parse(uid);
            await Task.Run(() => GetBody(uidLong));
        }
        public void PopulateBodySync(string uid)
        {
            long uidLong = long.Parse(uid);
            GetBody(uidLong);
        }

        private void GetBody(long uid) {
            lock (EmailList)
            {
                var emailView = EmailList[uid.ToString()];
                if (emailView.Text == null)
                {
                    lock (imap)
                    {
                        BodyStructure structure = imap.GetBodyStructureByUID(uid);
                        string text = null, html = null;
                        if (structure.Text != null)
                            text = imap.GetTextByUID(structure.Text);
                        if (structure.Html != null)
                            html = imap.GetTextByUID(structure.Html);
                        EmailViewManager.SetBody(ref emailView, text, html);
                        ListChanged = true; 
                    }
                }
            }
        }
        public void Close()
        {
            lock (imap)
            {
                imap.Close();
            }
        }
    }
}
