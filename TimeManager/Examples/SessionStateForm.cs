using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;
using TimeManager.Models;
using TimeManager.Helpers;

namespace TimeManager.Examples
{

    public partial class SessionStateForm : Form
    {
        WindowsSession session;

        System.Threading.Timer timer;

        public SessionStateForm()
        {
            InitializeComponent();

            //Initialize the WindowsSession instance.
            session = new WindowsSession();

            //Initialize the timer, but not start.
            timer = new System.Threading.Timer(
                new System.Threading.TimerCallback(DetectSessionState),
                null,
                System.Threading.Timeout.Infinite,
                5000);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Register the StateChanged event.
            session.StateChanged += new EventHandler<SessionSwitchEventArgs>(session_StateChanged);
        }

        ///<summary>Handle the StateChanged event of WindowsSession</summary>
        void session_StateChanged(object sender, SessionSwitchEventArgs e)
        {
            //Display the current state.
            lbState.Text = string.Format("Current State: {0}    Detect Time: {1} ",
                                         e.Reason, 
                                         DateTime.Now);

            // Record the StateChanged event and add it to the list box. 
            lstRecord.Items.Add(string.Format("{0}   {1} \tOccurred",
                                DateTime.Now, 
                                e.Reason));

            lstRecord.SelectedIndex = lstRecord.Items.Count - 1;
        }

        private void chkEnableTimer_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEnableTimer.Checked)
                timer.Change(0, 5000);
            else
                timer.Change(0, System.Threading.Timeout.Infinite);
        }

        void DetectSessionState(object obj)
        {
            // Check whether the current session is locked. 
            bool isCurrentLocked = session.IsLocked();

            var state = isCurrentLocked 
                        ? SessionSwitchReason.SessionLock
                        : SessionSwitchReason.SessionUnlock;

            InformationModel info = new InformationModel();

            string line = string.Format("Current State: {0}    Time: {1}    " +
                                        "User Name: {2}    Domain: {3}    ",
                                         state,
                                         DateTime.Now,
                                         info.UserInfo.LoginName,
                                         info.UserInfo.DomainName
                                         );

            StreamWriter file = new StreamWriter(Path.GetTempPath() + "\\Debugfile.log", true);

            file.WriteLine(line);

            file.Close();

        }
    }


}
