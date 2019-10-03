using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Limilabs.Mail;
using Limilabs.Client.IMAP;
using Limilabs.Client.POP3;
using DeveloperTest.Business;

namespace DeveloperTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dictionary<string, EmailView> EmailList = new Dictionary<string, EmailView>();
        public dynamic manager;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CleanEmails()
        {
            HeadersGrid.Items.Clear();
            HeadersGrid.Columns.Clear();
            EmailList = new Dictionary<string, EmailView>();
            manager = null;
            MessageBody.Text = "";
        }
        private void DisplayColumnNames()
        {
            if (HeadersGrid.Columns.Count == 0)
            {
                var column = new DataGridTextColumn();
                column.Header = "From";
                column.Binding = new Binding("From");
                HeadersGrid.Columns.Add(column);
                column = new DataGridTextColumn();
                column.Header = "Subject";
                column.Binding = new Binding("Subject");
                HeadersGrid.Columns.Add(column);
                column = new DataGridTextColumn();
                column.Header = "Date";
                column.Binding = new Binding("Date");
                HeadersGrid.Columns.Add(column);
            }
        }

        private void ConnectAndLoad()
        {
            if (ServiceType.Text == "POP3")
            {
                manager = new POP3Manager();
            }
            else
            {
                manager = new IMAPManager();
            }
            
            manager.Connect(Server.Text, Port.Text, Encryption.SelectedValue.ToString());
            
            try
            {
                manager.Authenticate(Username.Text, Password.Text);
                manager.SelectInbox();
                List<string> uids = manager.GetAllUids();
                foreach (var uid in uids)
                {
                    EmailList.Add(uid, new EmailView { Id = uid });
                }

                foreach (var uid in uids) 
                { 
                    var emailView = EmailList[uid];
                    manager.PopulateHeaderByUid(uid, ref emailView);
                    AddRowToHeadersGrid(emailView);
                }

                foreach (string uid in uids)
                {
                    var emailView = EmailList[uid];
                    manager.PopulateBodyByUid(uid, ref emailView);
                }

                manager.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                manager.Close();
            }
        }

        
        private void AddRowToHeadersGrid(EmailView item)
        {
            HeadersGrid.Items.Add(item);
        }
        protected void StartButton_Click(object sender, RoutedEventArgs e)
        {
            CleanEmails();
            DisplayColumnNames(); //move this on display first row
            ConnectAndLoad();
        }

        protected void HeadersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource != null)
            {
                var grid = (DataGrid)e.OriginalSource;
                var item = (EmailView)grid.SelectedItem;

                if (item != null)
                {
                    if (item.Text is null)
                    {
                        manager.PopulateBodyByUid(item.Id, ref item);
                    }

                    MessageBody.Text = item.Text ?? "The email has not been loaded yet";
                }
            }
        }


    }
}
