using EsseivaN.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using static EsseivaN.Tools.ResistorCalculator;
using Tools2 = EsseivaN.Tools.Tools;


// Todo : Dictionnaire avec comme clé, chaque valeur de la série
//        Quand une valeur doit être sauvegardée, la mettre sous la clé voulue.
// Avantages : Comparaison plus rapide (si déjà existant)

namespace EsseivaN.Apps.ResistorTool
{
    public partial class WindowParallelReverse : Form
    {
        // The selected serie
        public Series.SerieName CurrentSerie = Series.SerieName.E3;
        // The list of values in the serie
        public List<short> Serie;

        // Are values exact or rounded
        public bool Exact;

        // flag used to display correct text when doing backgorund work
        public int state = 0;

        // Data for the results
        public string msg;

        // Result
        public static List<Result> Results = new List<Result>();

        // Config
        public int shownResult = 0;
        public static int maxResults = 0;
        public static double desiredResistor = 0;
        public static double minRes = 0;
        public static double maxRes = 0;
        public static double minError = 0;
        public static int buffersize = 0;

        // BW
        // Flag to run only one time
        public bool Running = false;
        public BackgroundWorker bw;

        // Preview window
        frmPreview preview = new frmPreview();

        public WindowParallelReverse()
        {
            InitializeComponent();
            preview.FormClosing += Preview_FormClosing;
        }

        private void Preview_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            preview.Hide();
        }

        /// <summary>
        /// Run the worker to get resistors
        /// </summary>
        public void GetResistors(double Resistor)
        {
            // Retrieve the current serie
            Serie = Series.GetSerie(CurrentSerie);
            // Clear output text
            ClearOutput();

            // Get settings
            // Min resistor
            minRes = Tools2.EngineerToDecimal(textbox_RL1.Text);
            if (minRes == 0)
            {
                WriteLog("Incorrect minRes", Logger.Log_level.Error);
                return;
            }

            // Max resistor
            maxRes = Tools2.EngineerToDecimal(textbox_RL2.Text);
            if (maxRes == 0)
            {
                WriteLog("Incorrect maxRes", Logger.Log_level.Error);
                return;
            }

            // If min is > than max
            if (minRes > maxRes)
            {
                WriteLog("Min res is greater than Max res", Logger.Log_level.Warn);
                MessageBox.Show("Minimum resistor must be greater or equal than maximum resistor", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // If error is not decimal
            if (!double.TryParse(textbox_Error.Text, out minError))
            {
                WriteLog("Min error incorrect", Logger.Log_level.Error);
                MessageBox.Show("Invalid minimum error value\n" + minError, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // If buffer is not decimal
            if (!int.TryParse(textbox_Buffer.Text, out buffersize))
            {
                WriteLog("Buffer incorrect", Logger.Log_level.Error);
                MessageBox.Show("Invalid buffer size value\n" + buffersize, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Set maxresults to buffer size
            maxResults = buffersize;

            // Write to log
            WriteLog(string.Format("Processing with config : R={0} | Rmin={1} | Rmax={2} | ErrMin={3} | MaxResults={4}", Resistor, minRes, maxRes, minError, maxResults), Logger.Log_level.Debug);

            // Set desired resistor
            desiredResistor = Resistor;

            // If reistor less or equal to 0
            if (Resistor <= 0)
            {
                WriteLog("Invalid resistor value", Logger.Log_level.Warn);
                MessageBox.Show("Invalid resistor value\n" + Resistor, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Set cursor and flags
            Cursor = Cursors.WaitCursor;
            Running = true;
            state = 0;

            // Run the Backgroundwoker (Bw_DoWork)
            WriteLog("Running background worker ", Logger.Log_level.Debug);
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
            BackgroundWorker bw = sender as BackgroundWorker;
            GetResults(bw, Serie, desiredResistor, minError, minRes, maxRes, maxResults);
            // Remove all double results
            bw.ReportProgress(100);
            Results = CheckDoubles(Results);
            bw.ReportProgress(100);
        }

        public void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            if (e.ProgressPercentage == 100 && state == 0)
            {
                label_status.Text = "Removing duplicates... Press ESC to cancel. You may want cancel it";
                state = 1;
            }
            else if (state == 0)
            {
                label_status.Text = Results.Count.ToString() + " Press ESC to cancel";
            }
            else if (e.ProgressPercentage == 100)
            {
                label_status.Text = "Complete";
            }
        }

        public void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WriteLog("Background worker complete", Logger.Log_level.Debug);
            label_status.Text = "Complete";
            Running = false;
            Cursor = Cursors.Default;

            if (Results == null || Results.Count == 0)
            {
                WriteLog("No result", Logger.Log_level.Debug);
                MessageBox.Show("No result found\nCheck min and max resistors values", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearOutput();
                return;
            }

            WriteLog(Results.Count + " results found", Logger.Log_level.Debug);

            //CheckDoubles(Results);

            Results.Sort();
            DisplayOutputs(CheckboxExact.Checked == true, Results.ElementAtOrDefault(shownResult));
            labelResultCount.Text = $"{shownResult + 1}/{Results.Count}";
        }

        /// <summary>
        /// Clear output textboxes
        /// </summary>
        public void ClearOutput()
        {
            Results?.Clear();
            labelSerieParallel.Text =
                labelResultCount.Text =
                textbox_outR1.Text =
                textbox_outR2.Text =
                textbox_outR.Text =
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
                    textbox_outR.Text = result.Resistor.ToString();
                    textbox_outError.Text = $"{result.Error}%";
                }
                else
                {
                    // Affichage des valeurs arrondies
                    textbox_outR1.Text = Tools2.DecimalToEngineer(result.BaseResistors.R1);
                    textbox_outR2.Text = Tools2.DecimalToEngineer(result.BaseResistors.R2);
                    textbox_outR.Text = Tools2.DecimalToEngineer(Math.Round(result.Resistor, 3));
                    textbox_outError.Text = $"{Math.Round(result.Error, 3)}%";
                }
                labelSerieParallel.Text = result.Parallel ? "||" : "+";
            }
        }

        /// <summary>
        /// Do the hard work, fill the result list
        /// </summary>
        public static void GetResults(BackgroundWorker b, List<short> Serie, double WantedValue, double MinError, double minRes, double maxRes, int maxResults)
        {
            // Clear previous results
            Results.Clear();
            // Get absolute error
            MinError = Math.Abs(MinError);
            // Error percents
            double PreviousPercent = 0;
            // Progress value
            int progress;
            // Temp values r1 and r2
            double r1t;
            double r2t;

            // No log in this area to maximize speed
            // For each R1 in the serie
            foreach (var r1 in Serie)
            {
                // Check if cancel asked
                if (b.CancellationPending)
                {
                    return;
                }

                // Update progress
                progress = ((Results.Count * 100) / WindowParallelReverse.maxResults);
                if (progress > 100)
                {
                    progress = 100;
                }
                else if (progress < 0)
                {
                    progress = 0;
                }
                b.ReportProgress(progress);

                // Take the value nearest and greater than minRes
                r1t = r1;
                while (r1t >= (10 * minRes))
                {
                    r1t /= 10;
                }
                while (r1t < minRes)
                {
                    r1t *= 10;
                }

                // Check for all powers of R1 until PowS or error increasing
                while (r1t <= maxRes)
                {
                    // Check overflow (with duplicates)
                    if (Results.Count >= maxResults)
                    {
                        // Clear duplicates
                        Results = Results.Distinct().ToList();
                        // Check overflow without dplicates
                        if (Results.Count >= maxResults)
                        {
                            WriteLog("Buffer size reached, incomplete results", Logger.Log_level.Warn);
                            MessageBox.Show("Maximum number of result reached\nResults are incomplete and not all low error are found, you may want to decrease the minimum error or increase the buffer size !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // If resistor is the wanted resistor
                    if (r1t == WantedValue)
                    {
                        // Add result
                        Resistors resistors = new Resistors()
                        {
                            R1 = r1t,
                            R2 = 0,
                            Parallel = false,
                        };
                        Result res = new Result()
                        {
                            BaseResistors = resistors,
                            Resistor = r1t,
                            Parallel = false,
                            Error = 0
                        };

                        Results.Add(res);
                    }

                    // With R1 fixed, select R2 in the serie
                    foreach (var r2 in Serie)
                    {
                        r2t = r2;
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
                                R1 = Math.Min(r1t, r2t),
                                R2 = Math.Max(r1t, r2t),
                                Parallel = false,
                            };

                            // Get the error percent and save result if in error range
                            double currentPercent = Res.CalculateResistors(WantedValue, MinError);

                            if (Res.SerialValid)
                            {
                                Results.Add(Res.SerialResult);
                            }

                            if (Res.ParallelValid)
                            {
                                Results.Add(Res.ParallelResult);
                            }

                            // If error is increasing, it will continue, so jump to next R2
                            if (currentPercent > PreviousPercent && PreviousPercent != -1 && PreviousPercent > MinError)
                            {
                                break;
                            }

                            PreviousPercent = currentPercent;
                            // Next pow
                            r2t *= 10;
                        }
                    }
                    // Next pow
                    r1t *= 10;
                }
            }
        }

        /// <summary>
        /// Display text containing all results
        /// </summary>
        public void DisplayList(BackgroundWorker b)
        {
            msg = string.Empty;
            // Use multiple strings to increase speed
            List<string> msgs = new List<string>();
            // Add header line
            msgs.Add(string.Empty);

            // Progress
            int progress;
            // Line counter
            int count = 0;
            // Number of lines
            int max_i = Results.Count;
            // Result to display
            Result result;
            // Fixed length of the longest double value to be displayed (to have a nice formatted text)
            int maxlength = 0;

            // Is exact mode enabled
            if (Exact)
            {
                WriteLog("Exact mode ON", Logger.Log_level.Debug);
                maxlength = maxRes.ToString().Length;
                msgs[0] += $"{"R1".PadRight(maxlength)} {"  "} {"R2".PadRight(maxlength)}   {"Req".PadRight(16)} {"Error [%]"}{Environment.NewLine}";
            }
            else
            {
                WriteLog("Exact mode OFF", Logger.Log_level.Debug);
                msgs[0] += $"{"R1".PadRight(6)} {"  "} {"R2".PadRight(6)}   {"Req".PadRight(9)} {"Error [%]"}{Environment.NewLine}";
            }

            // Loop every results
            for (int i = 0; i < max_i; i++)
            {
                // If cancel requested
                if (b.CancellationPending)
                {
                    WriteLog("Display List skipped", Logger.Log_level.Info);
                    break;
                }

                // Retrieve result
                result = Results[i];

                // If exact mode enabled
                if (Exact)
                {
                    msgs[count] += $"{result.BaseResistors.R1.ToString().PadRight(maxlength)} {(result.Parallel ? "||" : " +")} {result.BaseResistors.R2.ToString().PadRight(maxlength)} = {result.Resistor.ToString().PadRight(16)} {((result.Error >= 0) ? (" ") : (""))}{result.Error}{Environment.NewLine}";
                }
                else
                {
                    msgs[count] += $"{Tools2.DecimalToEngineer(result.BaseResistors.R1).PadRight(6)} {(result.Parallel ? "||" : " +")} {Tools2.DecimalToEngineer(result.BaseResistors.R2).PadRight(6)} = {Tools2.DecimalToEngineer(Math.Round(result.Resistor, 3)).PadRight(9)} {((result.Error >= 0) ? (" ") : (""))}{Math.Round(result.Error, 3)}{Environment.NewLine}";
                }

                // Switch to next slot every 5k lines
                if (i % 5000 == 0)
                {
                    // Update to preview Window
                    count++;
                    // Add next slot
                    msgs.Add(string.Empty);
                    // Report progress
                    progress = ((i * 100) / max_i);
                    b.ReportProgress(progress);
                }
            }

            // Text generated, appending
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
            // Write log
            WriteLog("Displaying list", Logger.Log_level.Debug);
            // Is exact
            Exact = CheckboxExact.Checked;
            // Reset flags
            Running = true;
            state = 0;
            // Display status
            label_status.Text = "Creating text... Press ESC to cancel";

            preview.Data = "Loading...";
            preview.Show();

            // Run BackgroundWorker
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

        // Display list prodress changed
        public void Bw_ProgressChanged1(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        public void Bw_RunWorkerCompleted1(object sender, RunWorkerCompletedEventArgs e)
        {
            WriteLog("List display complete", Logger.Log_level.Info);
            label_status.Text = "Complete";
            preview.Data = msg;
            msg = string.Empty;
            Running = false;
        }

        // Evenements
        public void SerieComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {   // Valeur série changée
            CurrentSerie = (Series.SerieName)SerieComboBox.SelectedIndex;
            Serie = Series.GetSerie(CurrentSerie);
            WriteLog("Serie selected : " + CurrentSerie, Logger.Log_level.Debug);
        }

        public void TextBoxResistor_KeyDown(object sender, KeyEventArgs e)
        {   // Touche Enter pressée
            if (e.KeyCode == Keys.Enter)
            {
                ButtonConfirm.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        public void ButtonSerie_Click(object sender, EventArgs e)
        {   // Affiche la liste des valeurs
            MessageBox.Show($"{string.Join(", ", Serie)}", $"Serie {CurrentSerie}", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        public void ButtonConfirm_Click(object sender, EventArgs e)
        {
            if (Running)
            {
                WriteLog("Process already running", Logger.Log_level.Info);
                return;
            }

            shownResult = 0;
            if (double.TryParse(TextBoxResistor.Text, out double Resistor))
            {
                if (Resistor > 0)
                {
                    GetResistors(Resistor);
                }
                else
                {
                    WriteLog("Incorrect value, positive numbers only : " + TextBoxResistor.Text, Logger.Log_level.Error);
                    MessageBox.Show("Value incorect : Positive numbers only\nFollow this example :\n24.56k", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Resistor = Tools2.EngineerToDecimal(TextBoxResistor.Text);
                if (Resistor > 0)
                {
                    GetResistors(Resistor);
                }
                else
                {
                    WriteLog("Incorrect value, invalid characters : " + TextBoxResistor.Text, Logger.Log_level.Error);
                    MessageBox.Show("Value incorect : Positive numbers only\nFollow this example :\n24.56k", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void CheckboxExact_CheckedChanged(object sender, EventArgs e)
        {
            DisplayOutputs(CheckboxExact.Checked == true, Results.ElementAtOrDefault(shownResult));
        }

        public void btnPrevious_Click(object sender, EventArgs e)
        {
            if (Running)
            {
                WriteLog("Process running, can't get previous entry", Logger.Log_level.Info);
                return;
            }

            if (Results == null || Results.Count == 0)
            {
                WriteLog("No result, can't get previous entry", Logger.Log_level.Info);
                return;
            }

            if (shownResult > 0)
            {
                shownResult--;
            }
            else
            {
                shownResult = Results.Count - 1;
            }

            DisplayOutputs(CheckboxExact.Checked == true, Results.ElementAtOrDefault(shownResult));
            labelResultCount.Text = $"{shownResult + 1}/{Results.Count}";
        }

        public void btnNext_Click(object sender, EventArgs e)
        {
            if (Running)
            {
                WriteLog("Process running, can't get next entry", Logger.Log_level.Info);
                return;
            }

            if (Results == null || Results.Count == 0)
            {
                WriteLog("No result, can't get next entry", Logger.Log_level.Info);
                return;
            }

            if (shownResult < Results.Count - 1)
            {
                shownResult++;
            }
            else
            {
                shownResult = 0;
            }

            DisplayOutputs(CheckboxExact.Checked == true, Results.ElementAtOrDefault(shownResult));
            labelResultCount.Text = $"{shownResult + 1}/{Results.Count}";
        }

        public void btnShowList_Click(object sender, EventArgs e)
        {
            if (Running)
            {
                WriteLog("Process running, can't show list", Logger.Log_level.Info);
                return;
            }

            if (Results == null || Results.Count == 0)
            {
                WriteLog("No result, can't show list", Logger.Log_level.Info);
                return;
            }

            ShowList();
        }

        public void WindowParallelReverse_Load(object sender, EventArgs e)
        {
            FormInitialize();
        }

        private void WindowParallelReverse_CultureChanged(object sender, EventArgs e)
        {
            Controls.Clear();
            InitializeComponent();
            FormInitialize();
        }

        private void FormInitialize()
        {
            WriteLog("Reverse Equivalent Resistor window shown", Logger.Log_level.Debug);
            TextBoxResistor.Focus();
            SerieComboBox.SelectedIndex = 2;
        }

        public void WindowParallelReverse_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && Running)
            {
                if (MessageBox.Show("Are you sure you want to cancel ?", "Cancel requested", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    bw.CancelAsync();
                }
            }
        }

        private static void WriteLog(string data, Logger.Log_level log_Level)
        {
            Tools.WriteLog(1, data, log_Level);
        }
    }
}
