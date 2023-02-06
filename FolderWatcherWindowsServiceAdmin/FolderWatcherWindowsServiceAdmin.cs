using System;
using System.ServiceProcess;
using System.Windows.Forms;

namespace FolderWatcherWindowsServiceAdmin
{
    public partial class FolderWatcherWindowsServiceAdmin : Form
    {
        // service management
        private ServiceController serviceController;

        // service status
        private ServiceControllerStatus serviceControllerStatus;

        // button text
        private enum btnStartStopText
        {
            Start,
            Stop
        }

        public FolderWatcherWindowsServiceAdmin()
        {
            // create ServiceController instance for FolderWatcherWindowsService
            serviceController = new ServiceController("FolderWatcherWindowsService");

            InitializeComponent();
        }

        /// <summary>
        /// On form load.
        /// </summary>
        /// <param name="sender">form</param>
        /// <param name="e">event arguments</param>
        private void FolderWatcherWindowsServiceAdmin_Load(object sender, EventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// On form closing.
        /// </summary>
        /// <param name="sender">form</param>
        /// <param name="e">event arguments</param>
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
            // get status
            serviceControllerStatus = serviceController.Status;
            // set lblStatus text
            lblStatus.Text = serviceControllerStatus.ToString();
            // set btnText text
            string btnText = serviceControllerStatus == ServiceControllerStatus.Running ? btnStartStopText.Stop.ToString() : btnStartStopText.Start.ToString();
            btnChangeStatus.Text = btnText;
        }

        /// <summary>
        /// btnChangeStatus click.
        /// </summary>
        /// <param name="sender">btnChangeStatus</param>
        /// <param name="e">event arguments</param>
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