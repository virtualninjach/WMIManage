using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Windows.Forms;
using Sample.WmiSample.App.Properties;

namespace Sample.WmiSample.App
{
    public partial class MainForm : Form
    {
        private DateTime dtBootTime = new DateTime();
        public MainForm()
        {
            InitializeComponent();
        }

        private void ChangeCredentialState(object sender, EventArgs e)
        {
            txtPassword.Enabled = !(chkUseCurrentUser.Checked);
            txtUsername.Enabled = !(chkUseCurrentUser.Checked);
        }

        private void GetServicesClick(object sender, EventArgs e)
        {
            string computerName = txtComputerName.Enabled ? txtComputerName.Text : SystemInformation.ComputerName;
            if (!string.IsNullOrEmpty(computerName))
            {
                GetServicesForComputer(computerName);
            }
            else
            {
                lblErrors.Text = @"Computer Name cannot be empty";
                return;
            }
        }
        private void btnLastReboot_Click(object sender, EventArgs e)
        {
            string computerName = txtComputerName.Enabled ? txtComputerName.Text : SystemInformation.ComputerName;
            if (!string.IsNullOrEmpty(computerName))
            {
                GetLastRebootForComputer(computerName);
               
            }
            else
            {
                lblErrors.Text = @"Computer Name cannot be empty";
                return;
            }
        }

        private void ChangeComputerState(object sender, EventArgs e)
        {
            txtComputerName.Enabled = !(chkUseCurrentComputer.Checked);
        }

        private void GetServicesForComputer(string computerName)
        {
            ManagementScope scope = CreateNewManagementScope(computerName);

            SelectQuery query = new SelectQuery("select * from Win32_Service");

            try
            {
                using (var searcher = new ManagementObjectSearcher(scope, query))
                {
                    ManagementObjectCollection services = searcher.Get();

                    List<string> serviceNames =
                        (from ManagementObject service in services select service["Caption"].ToString()).ToList();

                    lstServices.DataSource = serviceNames;
                }
            }
            catch (Exception exception)
            {
                lstServices.DataSource = null;
                lstServices.Items.Clear();
                lblErrors.Text = exception.Message;
                Console.WriteLine(Resources.MainForm_GetServicesForServer_Error__ + exception.Message);
            }
        }

        private void GetLastRebootForComputer(string computerName)
        {
            ManagementScope scope = CreateNewManagementScope(computerName);

            SelectQuery sq = new SelectQuery("SELECT LastBootUpTime FROM Win32_OperatingSystem WHERE Primary = 'true'");

            try
            {
                    var searcher = new ManagementObjectSearcher(sq);
                foreach (ManagementObject oReturn in searcher.Get())
                {
      dtBootTime  =  ManagementDateTimeConverter.ToDateTime(oReturn.Properties["LastBootUpTime"].Value.ToString());
                    lblLastReboot.Text = Convert.ToString(dtBootTime);
                }
                   
                
            }
            catch (Exception exception)
            {
                lstServices.DataSource = null;
                lstServices.Items.Clear();
                lblErrors.Text = exception.Message;
                lblLastReboot.Text = "oops something went wrong with WMI query";
                Console.WriteLine(Resources.MainForm_GetServicesForServer_Error__ + exception.Message);
            }
        }

        private ManagementScope CreateNewManagementScope(string server)
        {
            string serverString = @"\\" + server + @"\root\cimv2";

            ManagementScope scope = new ManagementScope(serverString);

            if (!chkUseCurrentUser.Checked)
            {
                ConnectionOptions options = new ConnectionOptions
                                  {
                                      Username = txtUsername.Text,
                                      Password = txtPassword.Text,
                                      Impersonation = ImpersonationLevel.Impersonate,
                                      Authentication = AuthenticationLevel.PacketPrivacy
                                  };
                scope.Options = options;
            }

            return scope;
        }

        
    }
}