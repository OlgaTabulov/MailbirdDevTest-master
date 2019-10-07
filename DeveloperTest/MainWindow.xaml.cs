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
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace DeveloperTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public dynamic Manager;
        public bool DontKillManager = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        async Task AsyncDisplayRefreshWorker()
        {
            while (true)
            {
                await Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                {
                    Task.Delay(100);
                    if (Manager!= null && Manager.EmailList != null)
                    {
                        if (Manager.ListChanged)
                        {
                            HeadersGrid.ItemsSource = ((Dictionary<string, EmailView>)Manager.EmailList).Values.ToList();
                            HeadersGrid.Items.Refresh();
                            Manager.ListChanged = false;
                        }
                    }
                }));
                if (IsTimeToKill())
                {
                    Manager.Close();
                    break; //breaks out of display feedback loop
                }
            }
        }

        private bool IsTimeToKill()
        {
            if (Manager != null && Manager.EmailList != null && !DontKillManager)
            {
                foreach(EmailView email in Manager.EmailList.Values) 
                {
                    if (email.Text == null || email.Subject == null)
                        return false;                
                }
                return true;
            }
            return false;
        }

        private void CleanScreen()
        {
            HeadersGrid.ItemsSource = null;
            if (Manager != null)
                Manager.Close();
            Manager = null;
            MessageBody.Text = "";
        }

        private bool ConnectToServer()
        {
            try
            {
                if (ServiceType.Text == "POP3")
                {
                    Manager = new POP3Manager();
                }
                else
                {
                    Manager = new IMAPManager();
                }

                Manager.Connect(Server.Text, Port.Text, Encryption.SelectedValue.ToString());
                Manager.Authenticate(Username.Text, Password.Password);
                Manager.SelectInbox();
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not connect. \n" + e);
                return false;
            }
            return true;
        }
        private async Task Load()
        {
            try
            {
                List<string> uids = Manager.GetAllUids();
                Manager.EmailList = new Dictionary<string, EmailView>();
                for (int i = 0; i < uids.Count; i++)
                {
                    Manager.EmailList.Add(uids[i], new EmailView { Id = uids[i], Order = i });
                }
                var displayWorker = Task.Run(
                async () =>
                {
                    await AsyncDisplayRefreshWorker();
                });

                foreach (var uid in uids) 
                {
                    await Task.Factory.StartNew(() =>
                    {
                        DispatcherOperation op = Dispatcher.InvokeAsync((Action)(() =>
                        {
                            Manager.PopulateHeaderAsync(uid);
                        }));                        
                    });
                }
                
                foreach (string uid in uids)
                {
                    await Task.Factory.StartNew(() =>
                    {
                        DispatcherOperation op = Dispatcher.InvokeAsync((Action)(() =>
                        {
                            Manager.PopulateBodyAsync(uid);
                        }));
                    });
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error accured. \n" + e);
                Manager.Close();
            }
        }
        protected async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            CleanScreen();
            if (ConnectToServer())
                await Load();
        }

        protected void HeadersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource != null)
            {
                var item = (EmailView)((DataGrid)e.OriginalSource).SelectedItem;

                if (item != null)
                {
                    //I simplified the displayed field to Text only, even though both Text and Html are populated.
                    string text = item.Text;
                    if (text is null)
                    {
                        DontKillManager = true;
                        Manager.PopulateBodySync(item.Id);
                        text = ((Dictionary<string, EmailView>)Manager.EmailList)[item.Id].Text;
                        DontKillManager = false;
                    }
                    MessageBody.Text = text;
                }
            }
        }
    }
}
