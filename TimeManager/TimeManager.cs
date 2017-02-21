using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;
using TimeManager.Helpers;
using TimeManager.Models;
using TimeManager.Properties;

namespace TimeManager
{
    public class TimeManager
    {
        /// <summary>Constructor - NotifyIcon initializer</summary>
        /// <param name="notifyIcon"></param>
        public TimeManager(NotifyIcon notifyIcon)
        {
            _notifyIcon = notifyIcon;

            //initialize user state with unlocked since the program started
            _userState = new Dictionary<string, SessionSwitchReason> { { StorageFileManager.GetLoginUserName(), SessionSwitchReason.SessionUnlock } };

            //Read the configuration
            Config = StorageFileManager.ReadConfig();

            //Clear the log
            StorageFileManager.Clear();

            //create a new timer to go for a number of seconds (1000 milliseconds = 1 second)
            timer = new System.Timers.Timer(Config.PollingIntervalInSeconds * 1000D);
            //have the timer start again when it finishes..  (creates an interval timer)
            timer.AutoReset = true;
            //Define what to do when the timer goes off - call timer_Elapsed
            timer.Elapsed += timer_Elapsed;
            //Start the timer
            timer.Start();

            //capture SystemEvent SessionSwitch
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        //The icon object for the tray app
        private readonly NotifyIcon _notifyIcon;

        //The object to store users state
        private Dictionary<string, SessionSwitchReason> _userState;
        private void SetUserState(SessionSwitchReason newState)
        {
            string key = StorageFileManager.GetLoginUserName();
            SessionSwitchReason val;
            if (_userState.TryGetValue(key, out val))
                _userState[key] = newState;
            else
                _userState.Add(key, newState);
        }
        private SessionSwitchReason GetUserState() {
            string key = StorageFileManager.GetLoginUserName();
            SessionSwitchReason val;
            if (!_userState.TryGetValue(key, out val))
            {
                _userState.Add(key, SessionSwitchReason.SessionUnlock);
                val = SessionSwitchReason.SessionUnlock;
            }

            return val;
        }
        private ConfigurationModel Config { get; set; }

        //The maximum length of the tool tip
        private static readonly int MaxTooltipLength = 63;
        //Hold information to help build the context menu
        private Dictionary<string, IEnumerable<ServerGroup>> _projectDict = new Dictionary<string, IEnumerable<ServerGroup>>();
        //The storage file lines of text
        private List<string> _storageFileData;
        //Is the item displayed
        public bool IsDecorated { get; private set; }

        //Holds the timer object
        private System.Timers.Timer timer;

        /// <summary>Handle what happens when the timer fires off(ends)</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            HandleEvent("timer_Elapsed", GetUserState(), Config.TimerLogEntryIncludesComputerDetail);
        }
        /// <summary>Get the System Events</summary>
        /// <param name="sender"></param>
        /// <param name="e">The SessionSwitchEvent arguments</param>
        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            //Collapse reasons to a simple locked / unlocked state
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.SessionLogoff:
                case SessionSwitchReason.ConsoleDisconnect:
                case SessionSwitchReason.RemoteDisconnect:
                    SetUserState(SessionSwitchReason.SessionLock);
                    break;
                case SessionSwitchReason.SessionUnlock:
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.ConsoleConnect:
                case SessionSwitchReason.RemoteConnect:
                    SetUserState(SessionSwitchReason.SessionUnlock);
                    break;
                case SessionSwitchReason.SessionRemoteControl:
                    SetUserState(SessionSwitchReason.SessionRemoteControl);
                    break;
                default:
                    break;
            }

            HandleEvent("systemEvent_SessionSwitch", GetUserState(), Config.StateChangeLogEntryIncludesComputerDetail);
        }

        /// <summary>Handle the event to log</summary>
        /// <param name="source"></param>
        /// <param name="state"></param>
        /// <param name="includeInformationModel"></param>
        private void HandleEvent(string source, SessionSwitchReason state, bool includeInformationModel)
        {

            //make and Write the log entry
            string line = string.Format("{0} ({1}) - Git:{2}", state, source, GitRepositoryCurrentBranch());
            if (includeInformationModel)
                line += " " + JsonConvert.SerializeObject(new InformationModel());
            StorageFileManager.Write(line);
        }

        private string GitRepositoryCurrentBranch()
        {
            string returnValue = string.Empty;

            try
            {
                if (Config.GitRepositoryPath != null && LibGit2Sharp.Repository.IsValid(Config.GitRepositoryPath))
                {
                    returnValue = new LibGit2Sharp.Repository(Config.GitRepositoryPath).Head.FriendlyName;
                }
            }
            catch (Exception) { }

            return returnValue;
        }

        #region context menu creation

        /// <summary>Build the context menu</summary>
        /// <param name="contextMenuStrip"></param>
        public void BuildContextMenu(ContextMenuStrip contextMenuStrip)
        {
            //Clear the menu strip so we can rebuild it
            contextMenuStrip.Items
                            .Clear();

            //Add the dynamic entries
                //Add the git repositories at the selected path

            contextMenuStrip.Items
                            .AddRange(_projectDict.Keys
                                                 .OrderBy(project => project)
                                                 .Select(project => BuildSubMenu(project))
                                                 .ToArray());

            //Add the fixed entries at the end
            contextMenuStrip.Items
                            .AddRange(new ToolStripItem[]
                                        {
                                            new ToolStripSeparator(),
                                            ToolStripMenuItemWithHandler("&Open log file", openFileItemForEditing_Click),
                                            ToolStripMenuItemWithHandler("Open log &folder", openFolderItem_Click)
                                        });
        }

        /// <summary>Build the submenu based on the project dictionary</summary>
        /// <param name="project">The project dictionary</param>
        /// <returns></returns>
        private ToolStripMenuItem BuildSubMenu(string project)
        {
            var menuItem = new ToolStripMenuItem(project);
                menuItem.DropDownItems
                        .AddRange(_projectDict[project]
                                    .OrderBy(serverGroup => serverGroup.Name)
                                    .Select(serverGroup => ToolStripMenuItemWithHandler(serverGroup.Name, 
                                                                                        serverGroup.EnabledCount, 
                                                                                        serverGroup.DisabledCount, 
                                                                                        serverGroupItem_Click))
                                    .ToArray());
            return menuItem;
        }

        # endregion context menu creation

        # region hosts file analysis

        private static readonly string HostsCommentMarker = "#";
        private static readonly string FilteringPattern = HostsCommentMarker + @"\s*\[([^/]+)/([^]]+)\]";
        private static readonly Regex FilteringRegex = new Regex(FilteringPattern);
        // Each host line must have this suffix format to be considered:
        //  #  [ ProjectName / ServerGroupName ]
        // This regex has 2 subgroups capturing this information:
        private static readonly int ProjectSubGroupIndex = 1;
        private static readonly int ServerGroupSubGroupIndex = 2;
        
        private class ServerGroup
        {
            public string Name { get; set; }
            public int EnabledCount { get; set; }
            public int DisabledCount { get; set; }
            //public ServerGroup(string name) { Name = name; }
        }

        /// <summary>Builds the server associations into a dictionary.</summary>
        public void BuildServerAssociations()
        {
            try
            {
                _storageFileData = StorageFileManager.Read();
            }
            catch (Exception ex)
            {
                _storageFileData = new List<string>(0);
                MessageBox.Show(ex.Message, "HOSTS file error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            CreateMap();
            SetNotifyIconToolTip();
        }

        private void CreateMap()
        {
            _projectDict = 
            _storageFileData
            .Select(line => new { line, m = FilteringRegex.Match(line) })
            .Where(item => item.m.Success)
            .Select(item => new
            {
                item.line,
                item.m,
                project = item.m.Groups[ProjectSubGroupIndex].ToString().Trim(),
                serverGroup = item.m.Groups[ServerGroupSubGroupIndex].ToString().Trim()
            })
            .GroupBy(item => new { item.project, item.serverGroup }, item => item.line)
            .GroupBy(projAndServer => projAndServer.Key.project)
            .ToDictionary(
                project => project.Key,
                project => project.Select(item =>
                    new ServerGroup
                    {
                        Name = item.Key.serverGroup,
                        EnabledCount  = item.Where(line => !IsCommented(line)).Count(),
                        DisabledCount = item.Where(line =>  IsCommented(line)).Count()
                    }
                )
            );
        }

        private void SetNotifyIconToolTip()
        {
            string activeServerGroupsText =
                string.Join("\n",
                     _projectDict.Keys
                         .Select(project => new
                                 {
                                     project,
                                     activeServerGroups = string.Join(", ",
                                         _projectDict[project]
                                             .Where(
                                                 serverGroup =>
                                                 serverGroup.EnabledCount > 0 &&
                                                 serverGroup.DisabledCount == 0)
                                             .Select(serverGroup => serverGroup.Name)
                                     )
                                 }
                         )
                         .Where(item => item.activeServerGroups.Length > 0)
                         .Select(item => item.project + ": " + item.activeServerGroups));

            IsDecorated = activeServerGroupsText.Length > 0;

            int activeLineCount = _projectDict.Keys
                .SelectMany(p => _projectDict[p], (project, serverGroup) => serverGroup.EnabledCount).Sum();

            string toolTipText = string.Format("{0}\n{1} of {2} lines",
                (IsDecorated ? activeServerGroupsText : "No Active Servers!"), activeLineCount, _storageFileData.Count);

            _notifyIcon.Text = toolTipText.Length >= MaxTooltipLength ?
                toolTipText.Substring(0, MaxTooltipLength - 3) + "..." : toolTipText;
        }

        private static bool IsCommented(string line)
        {
            return Regex.IsMatch(line, @"^\s*" + HostsCommentMarker);
        }

        private static string Comment(string line)
        {   // when adding, always add marker + space to start of line
            return IsCommented(line) ? line : (HostsCommentMarker + " " + line);
        }

        private static string Uncomment(string line)
        {   // when removing, remove marker + space if present, otherwise just marker.
            return !IsCommented(line) ? line : Regex.Replace(line, @"^(\s*)" + HostsCommentMarker + " ?", @"$1");
        }

        # endregion hosts file analysis

        # region details form support

        private static readonly string ServerPattern =
            "(" + HostsCommentMarker + @"?)\s*((?:\d+\.){3}\d+)\s+(\S+)\s+" + HostsCommentMarker + @"\s*\[([^/]+)/([^]]+)\]";
        private static readonly Regex ServerRegex = new Regex(ServerPattern);
        // Each host line must have this format to be considered, where the initial comment marker is optional:
        //  #  IpAddress   HostName  #  [ ProjectName / ServerGroupName ]
        // This regex has 5 subgroups capturing this information:
        private static readonly int StatusIndex = 1; // presence or absence of a comment marker
        private static readonly int IpAddressIndex = 2;
        private static readonly int HostNameIndex = 3;
        private static readonly int ProjectNameIndex = 4;
        private static readonly int ServerGroupNameIndex = 5;

        private class Server
        {   // NB: the order here determines the order of the table in the details view.
            public string IpAddress { get; set; }
            public string HostName { get; set; }
            public string ProjectName { get; set; }
            public string ServerGroupName { get; set; }
            public string Status { get; set; }
        }

        public static readonly string EnabledLabel = "enabled";
        public static readonly string DisabledLabel = "disabled";
        public static readonly int EnabledColumnNumber = 4;
        private static readonly int BalloonTimeout = 3000; // preferred timeout (msecs) though .NET enforces 10-sec minimum

        public bool IsEnabled(string cellValue)
        {
            return EnabledLabel.Equals(cellValue);
        }

        public void GenerateHostsDetails(DataGridView dgv)
        {
            var servers = 
                _storageFileData
                .Select(line => ServerRegex.Match(line))
                .Where(match => match.Success)
                .Select(match => new Server
                {
                    Status = match.Groups[StatusIndex].ToString().Trim().Equals(HostsCommentMarker) ? DisabledLabel : EnabledLabel,
                    HostName = match.Groups[HostNameIndex].ToString().Trim(),
                    IpAddress = match.Groups[IpAddressIndex].ToString().Trim(),
                    ProjectName = match.Groups[ProjectNameIndex].ToString().Trim(),
                    ServerGroupName = match.Groups[ServerGroupNameIndex].ToString().Trim()
                });

            dgv.DataSource = new BindingSource { DataSource = new SortableBindingList<Server>(servers.ToList()) };
        }

        # endregion details form support
        
        # region Menu / form event handlers

        /// <summary>Open the storage file in notepad for editing</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFileItemForEditing_Click(object sender, EventArgs e)
        {
            try { Process.Start("notepad.exe", StorageFileManager.GetPathAndFileName()); }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, 
                              "Cannot Open " + StorageFileManager.GetFileName() + " File", 
                              MessageBoxButtons.OK, 
                              MessageBoxIcon.Error);
            }
        }
        /// <summary>Open a folder in the default file manager</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFolderItem_Click(object sender, EventArgs e)
        {
            try { Process.Start(StorageFileManager.GetPath()); }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, 
                                "Cannot Open Folder", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Error);
            }
        }

        private void serverGroupItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem itemClicked = ((ToolStripMenuItem)sender);
            var targetServerGroup = itemClicked.Text;
            var targetProject = itemClicked.OwnerItem.Text;

            for (int i = 0; i < _storageFileData.Count; i++)
            {
                string line = _storageFileData[i];
                Match m = FilteringRegex.Match(line);
                if (m.Success)
                {
                    string project = m.Groups[ProjectSubGroupIndex].ToString().Trim();
                    string serverGroup = m.Groups[ServerGroupSubGroupIndex].ToString().Trim();
                    if (project.Equals(targetProject))
                    {
                        _storageFileData[i] = serverGroup.Equals(targetServerGroup) ? Uncomment(line) : Comment(line);
                    }
                }
            }

            string msg = null;
            try { StorageFileManager.Write(_storageFileData); }
            catch (Exception ex) { msg = ex.Message; }

            CreateMap(); // regen the map to reflect this update (successful or not) for tooltip processing
            SetNotifyIconToolTip();

            if (msg == null) // no error
            {
                _notifyIcon.ShowBalloonTip(BalloonTimeout, "Hosts Switched!",
                                          string.Format("{0} => {1}", targetProject, targetServerGroup), ToolTipIcon.Info);
            }
            else { MessageBox.Show(msg, "Cannot Update HOSTS File", MessageBoxButtons.OK, MessageBoxIcon.Error); }

        }

        # endregion menu / form event handlers

        # region support methods

        /// <summary>create a new Context menu item with a handler without image or tool tip. The menu item.</summary>
        /// <param name="displayText">The text to display on the menu item</param>
        /// <param name="eventHandler">the handler to assign to the menu click</param>
        /// <returns></returns>
        public ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText, EventHandler eventHandler)
        {
            return ToolStripMenuItemWithHandler(displayText, 0, 0, eventHandler);
        }
        /// <summary>create a new Context menu item with a handler and image/tool tip if either enabledCount or disabledCount are > 0.
        ///     This will place an image to the left of the menu item and set a tool tip for the menu item.
        /// </summary>
        /// <param name="displayText">The text to display on the menu item</param>
        /// <param name="enabledCount">The number of items enabled - helps to set the image and tool tip if > 0</param>
        /// <param name="disabledCount">The number of items disabled - helps to set the image and tool tip if > 0</param>
        /// <param name="eventHandler">the handler to assign to the menu click</param>
        /// <returns></returns>
        private ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText, int enabledCount, int disabledCount, EventHandler eventHandler)
        {
            var item = new ToolStripMenuItem(displayText);
            if (eventHandler != null) { item.Click += eventHandler; }

            //set the display text of the item
            item.Text = displayText;

            //TODO: find different way to assign the image
            //Assign the image that will show up to the left of the menu item
            item.Image = (enabledCount > 0 && disabledCount > 0) 
                            ? Resources.signal_yellow
                            : (enabledCount > 0) 
                                ? Resources.signal_green
                                : (disabledCount > 0) 
                                    ? Resources.signal_red
                                    : null;
            //TODO: find a different way to assign the tooltip
            //Assign the tool tip for the menu item
            item.ToolTipText = (enabledCount > 0 && disabledCount > 0)
                                ? string.Format("{0} enabled, {1} disabled", enabledCount, disabledCount)
                                : (enabledCount > 0) 
                                    ? string.Format("{0} enabled", enabledCount)
                                    : (disabledCount > 0) 
                                        ? string.Format("{0} disabled", disabledCount)
                                        : "";
            //return the menu item that was created
            return item;
        }

        #endregion support methods
    }
}
