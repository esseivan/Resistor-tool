using ESNLib.Controls;
using ESNLib.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using static ESN.ResistorCalculator.ResistorCalculator;

namespace ESN.ResistorCalculator.Forms
{
    public partial class WindowRatio : Form
    {
        public Series.SerieName CurrentSerie = Series.SerieName.E3;
        public List<short> Serie;

        public string msg;
        public bool Exact;

        // Result
        public static List<Result> Results;

        // Config
        public int shownResult = 0;
        public static int maxResults = 0;
        public static double desiredRatio = 0;
        public static double minRes = 0;
        public static double maxRes = 0;
        public static double minError = 0;
        public static int buffersize = 0;

        // BW
        public bool Running = false;
        public BackgroundWorker bw;

        public WindowRatio()
        {
            InitializeComponent();
            Serie = Series.GetSerie(CurrentSerie);
            Results = new List<Result>();
        }

        public void GetRatios(double Ratio)
        {
            Serie = Series.GetSerie(CurrentSerie);
            ClearOutput();

            // Get settings
            minRes = MiscTools.EngineerToDecimal(textbox_RL1.Text);
            if (minRes == 0)
            {
                WriteLog("Incorrect minRes", Logger.LogLevels.Error);
                return;
            }

            maxRes = MiscTools.EngineerToDecimal(textbox_RL2.Text);
            if (maxRes == 0)
            {
                WriteLog("Incorrect maxRes", Logger.LogLevels.Error);
                return;
            }

            if (minRes > maxRes)
            {
                WriteLog("Min res is greater than Max res", Logger.LogLevels.Warn);
                MessageBox.Show("Minimum resistor must be greater or equal than maximum resistor", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!double.TryParse(textbox_Error.Text, out minError))
            {
                WriteLog("Min error incorrect", Logger.LogLevels.Error);
                MessageBox.Show("Invalid minimum error value\n" + minError, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!int.TryParse(textbox_Buffer.Text, out buffersize))
            {
                WriteLog("Buffer incorrect", Logger.LogLevels.Error);
                MessageBox.Show("Invalid buffer size value\n" + buffersize, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            maxResults = buffersize;

            desiredRatio = Ratio;

            WriteLog(string.Format("Processing with config : Ratio={0} | Rmin={1} | Rmax={2} | ErrMin={3} | MaxResults={4}", desiredRatio, minRes, maxRes, minError, maxResults), Logger.LogLevels.Debug);

            if (!(Ratio > 0))
            {
                WriteLog("Invalid ratio value", Logger.LogLevels.Warn);
                MessageBox.Show("Invalid ratio value\n" + Ratio, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Cursor = Cursors.WaitCursor;
            Running = true;

            // Backgroundwoker
            WriteLog("Running background worker ", Logger.LogLevels.Debug);
            bw = new BackgroundWorker();
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.RunWorkerAsync();
        }

        // GetResistor background worker
        public void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            FillTable(sender as BackgroundWorker, Serie, minError, desiredRatio);
        }

        public void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            label_status.Text = Results.Count.ToString() + " Press ESC to cancel";
        }

        public void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WriteLog("Background worker complete", Logger.LogLevels.Debug);
            label_status.Text = "Complete";
            progressBar.Value = 100;
            Running = false;
            Cursor = Cursors.Default;

            if (Results == null || Results.Count == 0)
            {
                WriteLog("No result", Logger.LogLevels.Debug);
                MessageBox.Show("No result found\nCheck min and max resistors values", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearOutput();
                return;
            }

            WriteLog(Results.Count + " results found", Logger.LogLevels.Debug);

            Results.Sort();
            DisplayOutputs(CheckboxExact.Checked == true, Results.ElementAtOrDefault(shownResult));
            labelResultCount.Text = $"{shownResult + 1}/{Results.Count}";
        }

        public static double CalculateRatio(Resistors Resistors, double WantedValue, double MinError, bool Inverted)
        {
            double ratio = Resistors.R1 / Resistors.R2;
            // Ger error
            double errorRatio = MiscTools.GetErrorPercent(WantedValue, ratio);

            // Add if serial in range
            if (Math.Abs(errorRatio) <= MinError)
            {
                if (Inverted)
                {
                    Resistors = new Resistors()
                    {
                        R1 = Resistors.R2,
                        R2 = Resistors.R1,
                    };
                    ratio = 1 / ratio;
                }

                Result res = new Result()
                {
                    BaseResistors = Resistors,
                    Ratio = ratio,
                    Parallel = false,
                    Error = errorRatio
                };

                Results.Add(res);
            }

            // Return lowest error
            return Math.Abs(errorRatio);
        }

        public static void FillTable(BackgroundWorker b, List<short> Serie, double MinError, double WantedValue)
        {
            Results.Clear();
            MinError = Math.Abs(MinError);
            double currentPercent = 0;
            double PreviousPercent = 0;
            bool Inverted = false;

            if (!(WantedValue > 0 && WantedValue < 1))
            {
                Inverted = true;
                WantedValue = 1 / WantedValue;
            }

            // For each R1 in the serie
            foreach (var r1 in Serie)
            {
                // Check if cancel asked
                if (b.CancellationPending)
                {
                    return;
                }

                // Update progress
                int progress = ((Results.Count * 100) / maxResults);
                if (progress > 100)
                {
                    progress = 100;
                }
                else if (progress < 0)
                {
                    progress = 0;
                }
                b.ReportProgress(progress);

                // Take the min value
                double r1t = r1;
                while (r1t >= (10 * minRes))
                {
                    r1t /= 10;
                }
                while (r1t < minRes)
                {
                    r1t *= 10;
                }

                // Check only for one occurence of R1, you just multiply left and right by 10 to get the other one...
                if (r1t <= maxRes)
                {
                    // Check overflow
                    if (Results.Count >= maxResults)
                    {
                        WriteLog("Buffer size reached, incomplete results", Logger.LogLevels.Warn);
                        MessageBox.Show("Maximum number of result reached\nNot all results are found, you may want to decrease the minimum error or increase the buffer size !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // With R1 fixed, select R2 in the serie
                    foreach (var r2 in Serie)
                    {
                        double r2t = r2;
                        // Take the min value
                        while (r2t >= (10 * minRes))
                        {
                            r2t /= 10;
                        }
                        while (r2t < minRes)
                        {
                            r2t *= 10;
                        }

                        PreviousPercent = -1;
                        // Check for all powers of R2
                        while (r2t <= maxRes)
                        {
                            Resistors Res = new Resistors()
                            {
                                R1 = r1t,
                                R2 = r2t,
                                Parallel = false,
                            };

                            // Get the error percent and save result if in error range
                            currentPercent = CalculateRatio(Res, WantedValue, MinError, Inverted);

                            // If error is increasing, it will continue, so jump to next R2
                            if (currentPercent > PreviousPercent && PreviousPercent != -1)
                            {
                                break;
                            }

                            PreviousPercent = currentPercent;
                            r2t *= 10;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clear output textboxes
        /// </summary>
        public void ClearOutput()
        {
            Results?.Clear();
            labelResultCount.Text =
                textbox_outR1.Text =
                textbox_outR2.Text =
                textbox_outRatio.Text =
                textbox_outError.Text = "";
        }

        /// <summary>
        /// Display the result
        /// </summary>
        public void DisplayOutputs(bool Exact, Result result)
        {
            if (!((result.BaseResistors.R1 == 0) && (result.BaseResistors.R2 == 0)))
            {
                if (CheckboxExact.Checked == true)
                {
                    // Affichage des valeurs exactes
                    textbox_outR1.Text = result.BaseResistors.R1.ToString() + "Ω";
                    textbox_outR2.Text = result.BaseResistors.R2.ToString() + "Ω";
                    textbox_outRatio.Text = result.Ratio.ToString();
                    textbox_outError.Text = $"{result.Error}%";
                }
                else
                {
                    // Affichage des valeurs arrondies
                    textbox_outR1.Text = MiscTools.DecimalToEngineer(result.BaseResistors.R1);
                    textbox_outR2.Text = MiscTools.DecimalToEngineer(result.BaseResistors.R2);
                    textbox_outRatio.Text = Math.Round(result.Ratio, 3).ToString();
                    textbox_outError.Text = $"{Math.Round(result.Error, 3)}%";
                }
            }
        }

        /// <summary>
        /// Calculate ratios
        /// </summary>
        public void Calculate()
        {
            if (Running)
            {
                WriteLog("Process already running", Logger.LogLevels.Info);
                return;
            }

            shownResult = 0;
            if (double.TryParse(TextBoxRatio.Text, out double Ratio))
            {
                if (Ratio > 0)
                {
                    GetRatios(Ratio);
                }
                else
                {
                    WriteLog("Incorrect value, positive numbers only : " + TextBoxRatio.Text, Logger.LogLevels.Error);
                    MessageBox.Show("Value incorect : Positive numbers only\nFollow this example :\n24.56k", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Ratio = MiscTools.EngineerToDecimal(TextBoxRatio.Text);
                if (Ratio > 0)
                {
                    GetRatios(Ratio);
                }
                else
                {
                    WriteLog("Incorrect value, invalid characters : " + TextBoxRatio.Text, Logger.LogLevels.Error);
                    MessageBox.Show("Value incorect : Positive numbers only\nFollow this example :\n24.56k", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Display text containing all results
        /// </summary>
        public void DisplayList(BackgroundWorker b)
        {
            msg = string.Empty;
            // Use multiple strings to increase speed (wow ! much faster !)
            List<string> msgs = new List<string>();
            msgs.Add(string.Empty);

            int count = 0;
            int max_i = Results.Count;
            int progress = 0;
            Result result;
            int maxlength = 0;
            if (Exact)
            {
                WriteLog("Exact mode ON", Logger.LogLevels.Debug);
                maxlength = maxRes.ToString().Length;
                msgs[0] += $"{"R1".PadRight(maxlength)} {" "} {"R2".PadRight(maxlength)}   {"Ratio".PadRight(16)}  {" Error [%]"}{Environment.NewLine}";
            }
            else
            {
                WriteLog("Exact mode OFF", Logger.LogLevels.Debug);
                msgs[0] += $"{"R1".PadRight(6)} {" "} {"R2".PadRight(6)}   {"Req".PadRight(9)} {"Error [%]"}{Environment.NewLine}";
            }
            for (int i = 0; i < max_i;)
            {
                if (b.CancellationPending)
                {
                    WriteLog("Display List skipped", Logger.LogLevels.Info);
                    break;
                }

                result = Results[i];

                if (Exact)
                {
                    msgs[count] += $"{result.BaseResistors.R1.ToString().PadRight(maxlength)} / {result.BaseResistors.R2.ToString().PadRight(maxlength)} = {result.Ratio.ToString().PadRight(16)}  {((result.Error > 0) ? "+" : ((result.Error == 0) ? " " : ""))}{result.Error}{Environment.NewLine}";
                }
                else
                {
                    msgs[count] += $"{MiscTools.DecimalToEngineer(result.BaseResistors.R1).PadRight(6)} / {MiscTools.DecimalToEngineer(result.BaseResistors.R2).PadRight(6)} = {MiscTools.DecimalToEngineer(Math.Round(result.Ratio, 3)).PadRight(9)}  {((result.Error >= 0) ? " " : "")}{Math.Round(result.Error, 3)}{Environment.NewLine}";
                }

                i++;

                if (i % 1000 == 0)
                {
                    count++;
                    msgs.Add(string.Empty);
                    progress = ((i * 100) / max_i);
                    b.ReportProgress(progress);
                }
            }

            label_status.Text = "Appending text...";
            for (int i = 0; i < msgs.Count; i++)
            {
                msg += msgs[i];
                if (i % 100 == 0)
                {
                    progress = (i * 100) / msgs.Count;
                    b.ReportProgress(progress);
                }
            }
            label_status.Text = "Loading window";
            b.ReportProgress(100);
        }

        /// <summary>
        /// Start the worker to display the list
        /// </summary>
        public void ShowList()
        {
            WriteLog("Displaying list", Logger.LogLevels.Debug);
            Exact = CheckboxExact.Checked;
            Running = true;
            label_status.Text = "Creating text... Press ESC to cancel";
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += Bw_ProgressChanged1;
            bw.DoWork += Bw_DoWork1;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted1;
            bw.RunWorkerAsync();
        }

        // ShowList background worker
        public void Bw_DoWork1(object sender, DoWorkEventArgs e)
        {
            DisplayList(sender as BackgroundWorker);
        }

        public void Bw_ProgressChanged1(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        public void Bw_RunWorkerCompleted1(object sender, RunWorkerCompletedEventArgs e)
        {
            WriteLog("List display complete", Logger.LogLevels.Info);
            label_status.Text = "Complete";
            new frmPreview(msg).Show();
            msg = string.Empty;
            Running = false;
        }

        // Evenements

        public void WindowRatio_Load(object sender, EventArgs e)
        {
            WriteLog("Ratio calculator window shown", Logger.LogLevels.Debug);
            SerieComboBox.SelectedIndex = 2;
        }

        public void SerieComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {   // Valeur série changée
            CurrentSerie = (Series.SerieName)SerieComboBox.SelectedIndex;
            Serie = Series.GetSerie(CurrentSerie);
            WriteLog("Serie selected : " + CurrentSerie, Logger.LogLevels.Debug);
        }

        public void TextBoxRatio_KeyDown(object sender, KeyEventArgs e)
        {   // Touche Enter pressée
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                Calculate();
            }
        }

        public void ButtonSerie_Click(object sender, EventArgs e)
        {   // Affiche la liste des valeurs
            MessageBox.Show($"{string.Join(", ", Serie)}", $"Serie {CurrentSerie}", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        public void ButtonConfirm_Click(object sender, EventArgs e)
        {
            Calculate();
        }

        public void CheckboxExact_CheckedChanged(object sender, EventArgs e)
        {
            DisplayOutputs(CheckboxExact.Checked == true, Results.ElementAtOrDefault(shownResult));
        }

        private void btnShowList_Click(object sender, EventArgs e)
        {
            if (Running)
            {
                WriteLog("Process running, can't show list", Logger.LogLevels.Info);
                return;
            }

            if (Results == null || Results.Count == 0)
            {
                WriteLog("No result, can't show list", Logger.LogLevels.Info);
                return;
            }

            ShowList();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (Running)
            {
                WriteLog("Process running, can't get next entry", Logger.LogLevels.Info);
                return;
            }

            if (Results == null || Results.Count == 0)
            {
                WriteLog("No result, can't get next entry", Logger.LogLevels.Info);
                return;
            }

            if (shownResult + 1 < Results.Count)
            {
                shownResult++;
            }

            DisplayOutputs(CheckboxExact.Checked == true, Results.ElementAtOrDefault(shownResult));
            labelResultCount.Text = $"{shownResult + 1}/{Results.Count}";
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (Running)
            {
                WriteLog("Process running, can't get previous entry", Logger.LogLevels.Info);
                return;
            }

            if (Results == null || Results.Count == 0)
            {
                WriteLog("No result, can't get previous entry", Logger.LogLevels.Info);
                return;
            }

            if (shownResult > 0)
            {
                shownResult--;
            }

            DisplayOutputs(CheckboxExact.Checked == true, Results.ElementAtOrDefault(shownResult));
            labelResultCount.Text = $"{shownResult + 1}/{Results.Count}";
        }

        private void WindowRatio_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && Running)
            {
                if (MessageBox.Show("Are you sure you want to cancel ?", "Cancel requested", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    bw.CancelAsync();
                }
            }
        }

        private static void WriteLog(string data, Logger.LogLevels LogLevels)
        {
            Tools.WriteLog(2, data, LogLevels);
        }
    }
}
