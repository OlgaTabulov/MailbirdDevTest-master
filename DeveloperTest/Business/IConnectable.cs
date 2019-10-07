using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.Business
{
    public interface IConnectable
    {
        Dictionary<string, EmailView> EmailList { get; set; }
        bool ListChanged { get; set; }
        void Connect(string server, string port, string encryption);
        void Authenticate(string username, string password);
        void SelectInbox();
        List<string> GetAllUids();
        void PopulateHeaderAsync(string uid);
        Task PopulateBodyAsync(string uid);
        void PopulateBodySync(string uid);
        void Close();
    }
}
