using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using static ResistorTool.Tools;

// Todo : Dictionnaire avec comme clé, chaque valeur de la série
//        Quand une valeur doit être sauvegardée, la mettre sous la clé voulue.
// Avantages : Comparaison plus rapide (si déjà existant)

namespace ResistorTool
{
    public partial class WindowParallelReverse : Form
    {
        public Series series;
        public Series.CurrentSerie CurrentSerie = Series.CurrentSerie.E3;
        public List<short> Serie;

        public string msg;
        public bool Exact;
        public int state = 0;

        // Result
        public static List<Result> Results;

        // Config
        public int shownResult = 0;
        public static int maxResults = 0;
        public static double desiredResistor = 0;
        public static double minRes = 0;
        public static double maxRes = 0;
        public static double minError = 0;
        public static int buffersize = 0;

        // BW
        public bool Running = false;
        public BackgroundWorker bw;

        public WindowParallelReverse()
        {
            InitializeComponent();
            series = new Series(CurrentSerie);
            Results = new List<Result>();
        }

        /// <summary>
        /// Run the worker to get resistors
        /// </summary>
        public void GetResistors(double Resistor)
        {
            Serie = series.GetSerie(CurrentSerie);
            ClearOutput();

            // Get settings
            minRes = EngineerToDecimal(textbox_RL1.Text);
            if (minRes == 0)
            {
                return;
            }

            maxRes = EngineerToDecimal(textbox_RL2.Text);
            if (maxRes == 0)
            {
                return;
            }

            if (minRes > maxRes)
            {
                MessageBox.Show("Minimum resistor must be greater or equal than maximum resistor", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!double.TryParse(textbox_Error.Text, out minError))
            {
                MessageBox.Show("Invalid minimum error value\n" + minError, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!int.TryParse(textbox_Buffer.Text, out buffersize))
            {
                MessageBox.Show("Invalid buffer size value\n" + buffersize, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            maxResults = buffersize;

            desiredResistor = Resistor;

            if (!(Resistor > 0))
            {
                MessageBox.Show("Invalid resistor value\n" + Resistor, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Cursor = Cursors.WaitCursor;
            Running = true;
            state = 0;

            // Backgroundwoker
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
            FillTable(bw, Serie, desiredResistor, minError, minRes, maxRes, maxResults);
            // Remove all double results
            bw.ReportProgress(100);
            CheckDoubles(bw, Results);
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
            label_status.Text = "Complete";
            Running = false;
            Cursor = Cursors.Default;

            if (Results == null || Results.Count == 0)
            {
                MessageBox.Show("No result found\nCheck min and max resistors values", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearOutput();
                return;
            }

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
                    textbox_outR1.Text = DecimalToEngineer(result.BaseResistors.R1);
                    textbox_outR2.Text = DecimalToEngineer(result.BaseResistors.R2);
                    textbox_outR.Text = DecimalToEngineer(Math.Round(result.Resistor, 3));
                    textbox_outError.Text = $"{Math.Round(result.Error, 3)}%";
                }
                labelSerieParallel.Text = result.Parallel ? "||" : "+";
            }
        }

        /// <summary>
        /// Do the hard work
        /// </summary>
        public static void FillTable(BackgroundWorker b, List<short> Serie, double WantedValue, double MinError, double minRes, double maxRes, int maxResults)
        {
            Results.Clear();
            MinError = Math.Abs(MinError);
            double currentPercent = 0;
            double PreviousPercent = 0;
            int progress;
            double r1t;
            double r2t;

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

                // Take the min value
                r1t = r1;
                while (r1t >= (10 * minRes))
                {
                    r1t /= 10;
                }
                while (r1t < minRes)
                {
                    r1t *= 10;
                }

                // Check for all powers of R1
                while (r1t <= maxRes)
                {
                    // Check overflow
                    if (Results.Count >= maxResults)
                    {
                        MessageBox.Show("Maximum number of result reached\nNot all results are found, you may want to decrease the minimum error or increase the buffer size !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
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
                            currentPercent = CalculateResistors(Res, WantedValue, MinError);

                            // If error is increasing, it will continue, so jump to next R2
                            if (currentPercent > PreviousPercent && PreviousPercent != -1)
                            {
                                break;
                            }

                            PreviousPercent = currentPercent;
                            r2t *= 10;
                        }
                    }
                    r1t *= 10;
                }
            }
        }

        /// <summary>
        /// Calculate equivalent resistor in serial and parallel configuration. Save it if lower than MinError
        /// </summary>
        /// <returns>Lowest error</returns>
        public static double CalculateResistors(Resistors Res, double WantedValue, double MinError)
        {
            // parallel resistor
            double pr = GetParallelResistor(Res);
            // Serial resistor
            double sr = Res.R1 + Res.R2;


            // Ger error
            double pErrorRatio = GetErrorPercent(WantedValue, pr);
            double sErrorRatio = GetErrorPercent(WantedValue, sr);

            // Add if serial in range
            if (Math.Abs(sErrorRatio) <= MinError)
            {
                Result res = new Result()
                {
                    BaseResistors = Res,
                    Resistor = sr,
                    Parallel = false,
                    Error = sErrorRatio
                };

                Results.Add(res);
            }

            // Add if parallel in range
            if (Math.Abs(pErrorRatio) <= MinError)
            {
                Res.Parallel = true;
                Result res = new Result()
                {
                    BaseResistors = Res,
                    Resistor = pr,
                    Parallel = true,
                    Error = pErrorRatio
                };

                Results.Add(res);
            }

            // Return lowest error
            return Math.Min(Math.Abs(pErrorRatio), Math.Abs(sErrorRatio));
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
                maxlength = maxRes.ToString().Length;
                msgs[0] += $"{"R1".PadRight(maxlength)} {"  "} {"R2".PadRight(maxlength)}   {"Req".PadRight(16)} {"Error [%]"}{Environment.NewLine}";
            }
            else
            {
                msgs[0] += $"{"R1".PadRight(6)} {"  "} {"R2".PadRight(6)}   {"Req".PadRight(9)} {"Error [%]"}{Environment.NewLine}";
            }
            for (int i = 0; i < max_i; i++)
            {
                if (b.CancellationPending)
                {
                    break;
                }

                result = Results[i];

                if (Exact)
                {
                    msgs[count] += $"{result.BaseResistors.R1.ToString().PadRight(maxlength)} {(result.Parallel ? "||" : " +")} {result.BaseResistors.R2.ToString().PadRight(maxlength)} = {result.Resistor.ToString().PadRight(16)} {result.Error}{Environment.NewLine}";
                }
                else
                {
                    msgs[count] += $"{DecimalToEngineer(result.BaseResistors.R1).PadRight(6)} {(result.Parallel ? "||" : " +")} {DecimalToEngineer(result.BaseResistors.R2).PadRight(6)} = {DecimalToEngineer(Math.Round(result.Resistor, 3)).PadRight(9)} {Math.Round(result.Error, 3)}{Environment.NewLine}";
                }

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
            Exact = CheckboxExact.Checked;
            Running = true;
            state = 0;
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
            label_status.Text = "Complete";
            new frmPreview(msg).Show();
            msg = string.Empty;
            Running = false;
        }

        // Evenements
        public void SerieComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {   // Valeur série changée
            CurrentSerie = (Series.CurrentSerie)SerieComboBox.SelectedIndex;
            series.UpdateSerie(CurrentSerie);
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
            MessageBox.Show($"{string.Join(", ", series.GetSerie(CurrentSerie))}", $"Serie {series.GetSerieName(CurrentSerie)}", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        public void ButtonConfirm_Click(object sender, EventArgs e)
        {
            if (Running)
            {
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
                    MessageBox.Show("Value incorect : Positive numbers only\nFollow this example :\n24.56k", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Resistor = EngineerToDecimal(TextBoxResistor.Text);
                if (Resistor > 0)
                {
                    GetResistors(Resistor);
                }
                else
                {
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
                return;
            }

            if (Results == null || Results.Count == 0)
            {
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
                return;
            }

            if (Results == null || Results.Count == 0)
            {
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
                return;
            }

            if (Results == null || Results.Count == 0)
            {
                return;
            }

            ShowList();
        }

        public void WindowParallelReverse_Load(object sender, EventArgs e)
        {
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
    }
}
