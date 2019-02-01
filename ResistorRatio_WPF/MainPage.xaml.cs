using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ResistorCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WindowMain : Window
    {
        WindowRatio wr = new WindowRatio();
        WindowParallel wp = new WindowParallel();

        private void ButtonRatio_Click(object sender, RoutedEventArgs e)
        {
            wr.Show();
        }

        private void ButtonParallel_Click(object sender, RoutedEventArgs e)
        {
            wp.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (wr.Visibility == Visibility.Visible || wp.Visibility == Visibility.Visible)
            {
                this.Hide();
                e.Cancel = true;
            }
            else
            {
                base.OnClosing(e);
                Application.Current.Shutdown();
            }
        }

        private async void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                //MessageBox.Show(System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
                EsseivaN.Controls.UpdateChecker update = new EsseivaN.Controls.UpdateChecker(@"http://www.esseivan.ch/files/softwares/resistortool/version.xml", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
                update.CheckUpdates();
                if (update.Result.ErrorOccured)
                {
                    MessageBox.Show(update.Result.Error.ToString(), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (update.NeedUpdate())
                {   // Update available
                    var result = update.Result;

                    EsseivaN.Controls.Dialog.SetButton(EsseivaN.Controls.Dialog.Button.Button1, "Visit website");
                    EsseivaN.Controls.Dialog.SetButton(EsseivaN.Controls.Dialog.Button.Button2, "Download and install");
                    EsseivaN.Controls.Dialog.DialogResult dr = EsseivaN.Controls.Dialog.ShowDialog($"Update is available, do you want to download ?\nCurrent : {result.CurrentVersion}\nLast : {result.LastVersion}",
                        "Update available",
                        EsseivaN.Controls.Dialog.ButtonType.Custom1,
                        EsseivaN.Controls.Dialog.ButtonType.Custom2,
                        EsseivaN.Controls.Dialog.ButtonType.None);

                    if (dr == EsseivaN.Controls.Dialog.DialogResult.Custom1)
                    {
                        // Visit website
                        result.OpenUpdateWebsite();
                    }
                    else if (dr == EsseivaN.Controls.Dialog.DialogResult.Custom2)
                    {
                        // Download and install
                        if (await result.DownloadUpdate())
                        {
                            this.Close();
                        }
                        else
                            MessageBox.Show("Unable to download update", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No new release found", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unknown error :\n{ex}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private async void DownloadAndClose(EsseivaN.Controls.UpdateChecker.CheckUpdateResult result)
        //{
        //	await result.DownloadUpdate();
        //	this.Close();
        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Contains("INSTALLED"))
            {
                Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                this.Close();
                return;
            }

            this.labelVersion.Content = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            wr.Closing += Wr_Closing;
            wp.Closing += Wp_Closing;
        }

        private void Wp_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            wp.Hide();
            if (wr.Visibility != Visibility.Visible)
                this.Close();
        }

        private void Wr_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            wr.Hide();
            if (wp.Visibility != Visibility.Visible)
                this.Close();
        }
    }
}
