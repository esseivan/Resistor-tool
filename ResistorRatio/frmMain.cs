using EsseivaN.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResistorTool
{
    public partial class frmMain : Form
    {
        WindowRatio wr = new WindowRatio();
        WindowParallelReverse wp = new WindowParallelReverse();

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            
            string[] args = Environment.GetCommandLineArgs();

            if (args.Contains("INSTALLED"))
            {
				// Keep precedent arguments
                string args_line = string.Empty;
                if (args.Length > 1)
                {
                    args_line = string.Join(" ", args).Replace(" INSTALLED", "");
                }

                // Restart the exe without the "INSTALLED" argument
                Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location, args_line);
                Close();
                return;
            }

            this.labelVersion.Text = Application.ProductVersion;

            wr.Closing += Wr_Closing;
            wp.Closing += Wp_Closing;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            wr.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            wp.Show();
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckUpdate();
        }

        private async void CheckUpdate()
        {
            try
            {
                //MessageBox.Show(System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
                UpdateChecker update = new UpdateChecker(@"http://www.esseivan.ch/files/softwares/resistortool/version.xml", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
                update.CheckUpdates();
                if (update.Result.ErrorOccurred)
                {
                    MessageBox.Show(update.Result.Error.ToString(), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (update.NeedUpdate())
                {   // Update available
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

                    var dr = MessageDialog.ShowDialog(dialogConfig);

                    if (dr == Dialog.DialogResult.Custom1)
                    {
                        // Visit website
                        result.OpenUpdateWebsite();
                    }
                    else if (dr == Dialog.DialogResult.Custom2)
                    {
                        // Download and install
                        if (await result.DownloadUpdate())
                        {
                            this.Close();
                        }
                        else
                            MessageBox.Show("Unable to download update", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No new release found", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unknown error :\n{ex}\n\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void Wp_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            wp.Hide();
            if (!wr.Visible)
                this.Close();
        }

        private void Wr_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            wr.Hide();
            if (!wp.Visible)
                this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (wr.Visible || wp.Visible)
            {
                this.Hide();
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
