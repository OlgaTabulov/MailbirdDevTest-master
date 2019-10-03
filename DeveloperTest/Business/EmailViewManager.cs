using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.Business
{
    public class EmailViewManager
    {
        public static void SetHeader(ref EmailView item, string from, string subject, string date)
        {
            item.From = from;
            item.Subject = subject;
            item.Date = date;
        }
        public static void SetBody(ref EmailView item, string text = null, string html = null)
        {
            item.Text = text;
            item.Html = html;
        }
    }
}
