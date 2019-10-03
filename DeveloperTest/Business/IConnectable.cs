using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.Business
{
    public interface IConnectable
    {
        void Connect(string server, string port, string encryption);
        void Authenticate(string username, string password);
        void SelectInbox();
        List<string> GetAllUids();
        void PopulateHeaderByUid(string uid, ref EmailView emailView);
        void PopulateBodyByUid(string uid, ref EmailView emailView);
        void Close();
    }
}
