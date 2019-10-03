using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Limilabs.Client.IMAP;

namespace DeveloperTest.Business
{
    public class IMAPManager: IConnectable
    {
        Imap imap;
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
            return imap.Search(Flag.All).Select(c => c.ToString()).ToList();
        }
        public void PopulateHeaderByUid(string uid, ref EmailView emailView)
        {
            long uidLong = long.Parse(uid);
            MessageInfo info = imap.GetMessageInfoByUID(uidLong);
            EmailViewManager.SetHeader(ref emailView, info.Envelope.From.First().Name, info.Envelope.Subject,
                        info.Envelope.Date?.ToString("MM/dd/yyyy hh:mm:ss"));

        }
        public void PopulateBodyByUid(string uid, ref EmailView emailView)
        {
            long uidLong = long.Parse(uid);
            BodyStructure structure = imap.GetBodyStructureByUID(uidLong);
            string text = null, html = null;
            if (structure.Text != null)
                text = imap.GetTextByUID(structure.Text);
            if (structure.Html != null)
                html = imap.GetTextByUID(structure.Html);
            EmailViewManager.SetBody(ref emailView, text, html);
        }
        public void Close()
        {
            imap.Close();
        }
    }
}
