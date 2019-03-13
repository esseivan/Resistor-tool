using EsseivaN.Tools;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ResistorTool
{
    public partial class frmMain : Form
    {
        internal Logger logger = new Logger();
        WindowRatio wr = new WindowRatio();
        WindowParallelReverse wp = new WindowParallelReverse();

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
            logger.LogToFile_FilePath = Path.Combine(userAppData, "log.log");
            logger.LogToFile_Mode = Logger.SaveFileMode.FileName_LastPrevious;
            logger.LogToFile_WriteMode = Logger.WriteMode.Write;
            logger.LogToFile_SuffixMode = Logger.Suffix_mode.RunTime;
            TryEnableLogger();

            Tools.logger = logger;

            labelVersion.Text = Application.ProductVersion;

            wr.Closing += Wr_Closing;
            wp.Closing += Wp_Closing;

            Tools.WriteLog(0, "Resistor Tool IDLE", Logger.Log_level.Info);
        }

        private void TryEnableLogger()
        {
            if (!logger.Enable())
            {
                if (logger.LastException != null)
                {
                    Dialog.DialogConfig dialogConfig = new Dialog.DialogConfig()
                    {
                        Button1 = Dialog.ButtonType.OK,
                        Button2 = Dialog.ButtonType.Retry,
                        Button3 = Dialog.ButtonType.None,
                        DefaultInput = string.Empty,
                        Message = "Unable to start logger :\n" + logger.LastException,
                        Title = "Error"
                    };
                    Dialog.DialogInputResult result = Dialog.ShowDialog(dialogConfig);

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
            Tools.WriteLog(0, "Openning Ratio Calculator", Logger.Log_level.Info);
            wr.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Tools.WriteLog(0, "Openning Reverser Equivalent Resistor", Logger.Log_level.Info);
            wp.Show();
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.WriteLog(0, "Checking for updates", Logger.Log_level.Debug);
            CheckUpdate();
        }

        private async void CheckUpdate()
        {
            try
            {
                //MessageBox.Show(System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
                UpdateChecker update = new UpdateChecker(@"http://www.esseivan.ch/files/softwares/resistortool/infos.xml", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
                update.CheckUpdates();
                if (update.Result.ErrorOccurred)
                {
                    Tools.WriteLog(0, update.Result.Error.ToString(), Logger.Log_level.Error);
                    MessageBox.Show(update.Result.Error.ToString(), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (update.NeedUpdate())
                {   // Update available
                    Tools.WriteLog(0, "Update available", Logger.Log_level.Info);
                    var result = update.Result;

                    Dialog.DialogConfig dialogConfig = new Dialog.DialogConfig()
                    {
                        Message = $"Update is available, do you want to download ?\nCurrent: { result.CurrentVersion}\nLast: { result.LastVersion}",
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
                        Tools.WriteLog(0, "Openning website", Logger.Log_level.Debug);
                        // Visit website
                        result.OpenUpdateWebsite();
                    }
                    else if (dr.DialogResult == Dialog.DialogResult.Custom2)
                    {
                        Tools.WriteLog(0, "Downloading and installing update", Logger.Log_level.Info);
                        // Download and install
                        if (await result.DownloadUpdate())
                        {
                            Tools.WriteLog(0, "Download complete, closing app to let install continue", Logger.Log_level.Info);
                            Close();
                        }
                        else
                        {
                            Tools.WriteLog(0, "Download failed", Logger.Log_level.Error);
                            MessageBox.Show("Unable to download update", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    Tools.WriteLog(0, "Up to date ; " + update.Result.LastVersion, Logger.Log_level.Info);
                    MessageBox.Show("No new release found", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Tools.WriteLog(0, ex.ToString(), Logger.Log_level.Error);
                MessageBox.Show($"Unknown error :\n{ex.ToString()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Wp_Closing(object sender, CancelEventArgs e)
        {
            Tools.WriteLog(0, "Reverse Equivalent Resistor Closed", Logger.Log_level.Debug);
            e.Cancel = true;
            wp.Hide();
            if (!wr.Visible)
            {
                Tools.WriteLog(0, "Both windows closed, quitting...", Logger.Log_level.Info);
                Close();
            }
        }

        private void Wr_Closing(object sender, CancelEventArgs e)
        {
            Tools.WriteLog(0, "Ratio Calculator Closed", Logger.Log_level.Debug);
            e.Cancel = true;
            wr.Hide();
            if (!wp.Visible)
            {
                Tools.WriteLog(0, "Both windows closed, quitting...", Logger.Log_level.Info);
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
