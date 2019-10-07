using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.Business
{
    public class EmailView
    {
        public string Id { get; set; }

        public int Order { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Date { get; set; }
        public string Text { get; set; }
        public string Html { get; set; }
    }
}
