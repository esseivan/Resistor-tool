using ESNLib.Controls;
using ESNLib.Tools;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ESN.ResistorCalculator;

namespace ESN.ResistorCalculator.Forms
{
    public partial class frmMain : Form
    {
        internal Logger logger = new Logger();
        readonly WindowRatio wr = new WindowRatio();
        readonly WindowParallelReverse wp = new WindowParallelReverse();

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            var args = Environment.GetCommandLineArgs().ToList();
            // Remove first argument : executable path
            args.RemoveAt(0);

            // If not empty
            if (args.Count > 0)
            {
                Console.WriteLine("Arguments : " + string.Join(" ; ", args));

                // If installer argument, restart
                if (args.Contains("-installer"))
                {
                    // Remove installer argument
                    args.Remove("-installer");

                    // Generate new arguments
                    string args_line = (args.Count > 0) ? string.Join(" ", args) : string.Empty;

                    Console.WriteLine("New arguments : " + args_line);

                    // Restart the exe without the "-installer" argument
                    Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location, args_line);
                    Close();
                    return;
                }
            }

            string userAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"EsseivaN\ResistorTool");
            Logger.Instance.FilenameMode = Logger.FilenamesModes.FileName_LastPrevious;
            Logger.Instance.FilePath = Path.Combine(userAppData, "log.log");

            Logger.Instance.WriteMode = Logger.WriteModes.Write;
            Logger.Instance.PrefixMode = Logger.PrefixModes.RunTime;

            TryEnableLogger();

            wr.Closing += Wr_Closing;
            wp.Closing += Wp_Closing;

            labelVersion.Text = Application.ProductVersion;

            Logger.Instance.Write("Resistor Tool IDLE", Logger.LogLevels.Debug);
        }

        private void TryEnableLogger()
        {
            if (!Logger.Instance.Enable())
            {
                if (Logger.Instance.LastException != null)
                {
                    Dialog.DialogConfig dialogConfig = new Dialog.DialogConfig()
                    {
                        Button1 = Dialog.ButtonType.OK,
                        Button2 = Dialog.ButtonType.Retry,
                        Button3 = Dialog.ButtonType.None,
                        DefaultInput = string.Empty,
                        Message = "Unable to start logger :\n" + Logger.Instance.LastException,
                        Title = "Error"
                    };
                    Dialog.ShowDialogResult result = Dialog.ShowDialog(dialogConfig);

                    if (result.DialogResult == Dialog.DialogResult.Retry)
                    {
                        TryEnableLogger();
                    }
                }
                else
                {
                    MessageBox.Show("Unable to start logger, contact support", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Tools.WriteLog(0, "Openning Ratio Calculator", Logger.LogLevels.Info);
            wr.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Tools.WriteLog(0, "Openning Reverser Equivalent Resistor", Logger.LogLevels.Info);
            wp.Show();
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.WriteLog(0, "Checking for updates", Logger.LogLevels.Debug);
            CheckUpdate();
        }

        private void CheckUpdate()
        {
            MessageBox.Show("Function disabled", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // This function is now osbolete.
            // Maybe will be brought up to date later.
            // todo: Maybe check on github repository for update ?
            return;
/*
            try
            {
                //MessageBox.Show(System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
                UpdateChecker update = new UpdateChecker(@"http://www.esseivan.ch/files/softwares/resistortool/infos.xml", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
                update.CheckUpdates();
                if (update.Result.ErrorOccurred)
                {
                    Tools.WriteLog(0, update.Result.Error.ToString(), Logger.LogLevels.Error);
                    MessageBox.Show(update.Result.Error.ToString(), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (update.NeedUpdate())
                {   // Update available
                    Tools.WriteLog(0, "Update available", Logger.LogLevels.Info);
                    var result = update.Result;

                    Dialog.DialogConfig dialogConfig = new Dialog.DialogConfig()
                    {
                        Message = $"Update is available, do you want to download ?\nCurrent: {result.CurrentVersion}\nLast: {result.LastVersion}",
                        Title = "Update available",
                        Button1 = Dialog.ButtonType.Custom1,
                        CustomButton1Text = "Visit website",
                        Button2 = Dialog.ButtonType.Custom2,
                        CustomButton2Text = "Download and install",
                        Button3 = Dialog.ButtonType.Cancel,
                    };

                    var dr = Dialog.ShowDialog(dialogConfig);

                    if (dr.DialogResult == Dialog.DialogResult.Custom1)
                    {
                        Tools.WriteLog(0, "Openning website", Logger.LogLevels.Debug);
                        // Visit website
                        result.OpenUpdateWebsite();
                    }
                    else if (dr.DialogResult == Dialog.DialogResult.Custom2)
                    {
                        Tools.WriteLog(0, "Downloading and installing update", Logger.LogLevels.Info);
                        // Download and install
                        if (await result.DownloadUpdate())
                        {
                            Tools.WriteLog(0, "Download complete, closing app to let install continue", Logger.LogLevels.Info);
                            Close();
                        }
                        else
                        {
                            Tools.WriteLog(0, "Download failed", Logger.LogLevels.Error);
                            MessageBox.Show("Unable to download update", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    Tools.WriteLog(0, "Up to date ; " + update.Result.LastVersion, Logger.LogLevels.Info);
                    MessageBox.Show("No new release found", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Tools.WriteLog(0, ex.ToString(), Logger.LogLevels.Error);
                MessageBox.Show($"Unknown error :\n{ex.ToString()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
*/
        }

        private void Wp_Closing(object sender, CancelEventArgs e)
        {
            Tools.WriteLog(0, "Reverse Equivalent Resistor Closed", Logger.LogLevels.Debug);
            e.Cancel = true;
            wp.Hide();
            if (!wr.Visible)
            {
                Tools.WriteLog(0, "Both windows closed, quitting...", Logger.LogLevels.Info);
                Close();
            }
        }

        private void Wr_Closing(object sender, CancelEventArgs e)
        {
            Tools.WriteLog(0, "Ratio Calculator Closed", Logger.LogLevels.Debug);
            e.Cancel = true;
            wr.Hide();
            if (!wp.Visible)
            {
                Tools.WriteLog(0, "Both windows closed, quitting...", Logger.LogLevels.Info);
                Close();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (wr.Visible || wp.Visible)
            {
                Hide();
                e.Cancel = true;
            }
            else
            {
                base.OnClosing(e);
                Application.Exit();
            }
        }
    }
}
