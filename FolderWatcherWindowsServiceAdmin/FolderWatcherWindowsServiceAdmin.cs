using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FolderWatcherWindowsServiceAdmin
{
    public partial class FolderWatcherWindowsServiceAdmin : Form
    {

        ServiceController serviceController;

        ServiceControllerStatus serviceControllerStatus;

        enum btnStartStopText
        {
            Start,
            Stop
        }
        
        public FolderWatcherWindowsServiceAdmin()
        {
            serviceController = new ServiceController("FolderWatcherWindowsService");
            InitializeComponent();
        }

        private void FolderWatcherWindowsServiceAdmin_Load(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void FolderWatcherWindowsServiceAdmin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serviceController != null)
            {
                serviceController.Dispose();
            }
        }

        /// <summary>
        /// Update service status.
        /// </summary>
        private void UpdateUI()
        {
            serviceControllerStatus = serviceController.Status;
            lblStatus.Text = serviceControllerStatus.ToString();
            string btnText = serviceControllerStatus == ServiceControllerStatus.Running ? btnStartStopText.Stop.ToString() : btnStartStopText.Start.ToString();
            btnChangeStatus.Text = btnText;
        }

        private void btnChangeStatus_Click(object sender, EventArgs e)
        {
            if (serviceControllerStatus == ServiceControllerStatus.Running)
            {
                serviceController.Stop();
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            else if (serviceControllerStatus == ServiceControllerStatus.Stopped)
            {
                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running);
            }
            else
                MessageBox.Show("Service status must be Running.");

            //  update status.
            UpdateUI();
        }
    }
}
