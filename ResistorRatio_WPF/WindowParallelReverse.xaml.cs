using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    public partial class WindowParallel : Window
    {
        public struct Series
        {
            public List<short> SerieE192;
            public List<short> SerieE96;
            public List<short> SerieE48;
            public List<short> SerieE24;
            public List<short> SerieE12;
            public List<short> SerieE6;
            public List<short> SerieE3;

            public Series(CurrentSerie serie)
            {
                SerieE192 = new List<short>();
                SerieE96 = new List<short>();
                SerieE48 = new List<short>();
                SerieE24 = new List<short>();
                SerieE12 = new List<short>();
                SerieE6 = new List<short>();
                SerieE3 = new List<short>();
                this.InitSeries(serie);
            }

            public enum CurrentSerie
            {
                E3,
                E6,
                E12,
                E24,
                E48,
                E96,
                E192,
                None
            }

            public void UpdateSerie(CurrentSerie serie)
            {
                InitSeries(serie);
            }

            internal void InitSeries(CurrentSerie serie)
            {
                SerieE192 = new List<short>();
                SerieE96 = new List<short>();
                SerieE48 = new List<short>();
                SerieE24 = new List<short>();
                SerieE12 = new List<short>();
                SerieE6 = new List<short>();
                SerieE3 = new List<short>();

                switch (serie)
                {
                    case CurrentSerie.E3:
                        SerieE3 = new List<short>()
                        {
                            100,220,470
                        };
                        break;
                    case CurrentSerie.E6:
                        SerieE6 = new List<short>()
                        {
                            100,150,220,330,470,680
                        };
                        break;
                    case CurrentSerie.E12:
                        SerieE12 = new List<short>()
                        {
                            100,120,150,180,220,270,330,390,470,560,680,820
                        };
                        break;
                    case CurrentSerie.E24:
                        SerieE24 = new List<short>()
                        {
                            100,110,120,130,150,160,180,200,220,240,270,300,330,360,390,430,470,510,560,620,680,750,820,910
                        };
                        break;
                    case CurrentSerie.E48:
                        SerieE48 = new List<short>()
                        {
                            100,105,110,115,121,127,133,140,147,154,162,169,178,187,196,205,215,226,237,249,261,274,287,301,316,332,348,365,383,402,422,442,
                            464,487,511,536,562,590,619,649,681,715,750,787,825,866,909,953
                        };
                        break;
                    case CurrentSerie.E96:
                        SerieE96 = new List<short>()
                        {
                            100,102,105,107,110,113,115,118,121,124,127,130,133,137,140,143,147,150,154,158,162,165,169,174,178,182,187,191,196,200,205,210,
                            215,221,226,232,237,243,249,255,261,267,274,280,287,294,301,309,316,324,332,340,348,357,365,374,383,392,402,412,422,432,442,453,
                            464,475,487,499,511,523,536,549,562,576,590,604,619,634,649,665,681,698,715,732,750,768,787,806,825,845,866,887,909,931,953,976
                        };
                        break;
                    case CurrentSerie.E192:
                        SerieE192 = new List<short>()
                        {
                            100,101,102,104,105,106,107,109,110,111,113,114,115,117,118,120,121,123,124,126,127,129,130,132,133,135,137,138,140,142,143,145,
                            147,149,150,152,154,156,158,160,162,164,165,167,169,172,174,176,178,180,182,184,187,189,191,193,196,198,200,203,205,208,210,213,
                            215,218,221,223,226,229,232,234,237,240,243,246,249,252,255,258,261,264,267,271,274,277,280,284,287,291,294,298,301,305,309,312,
                            316,320,324,328,332,336,340,344,348,352,357,361,365,370,374,379,383,388,392,397,402,407,412,417,422,427,432,437,442,448,453,459,
                            464,470,475,481,487,493,499,505,511,517,523,530,536,542,549,556,562,569,576,583,590,597,604,612,619,626,634,642,649,657,665,673,
                            681,690,698,706,715,723,732,741,750,759,768,777,787,796,806,816,825,835,845,856,866,876,887,898,909,920,931,942,953,965,976,988
                        };
                        break;
                    default:
                        break;
                }
            }

            public List<short> GetSerie(CurrentSerie serie)
            {
                switch (serie)
                {
                    case CurrentSerie.E3:
                        return SerieE3;
                    case CurrentSerie.E6:
                        return SerieE6;
                    case CurrentSerie.E12:
                        return SerieE12;
                    case CurrentSerie.E24:
                        return SerieE24;
                    case CurrentSerie.E48:
                        return SerieE48;
                    case CurrentSerie.E96:
                        return SerieE96;
                    case CurrentSerie.E192:
                        return SerieE192;
                    default:
                        return null;
                }
            }

            public string GetSerieName(CurrentSerie serie)
            {
                switch (serie)
                {
                    case CurrentSerie.E3:
                        return "E3";
                    case CurrentSerie.E6:
                        return "E6";
                    case CurrentSerie.E12:
                        return "E12";
                    case CurrentSerie.E24:
                        return "E24";
                    case CurrentSerie.E48:
                        return "E48";
                    case CurrentSerie.E96:
                        return "E96";
                    case CurrentSerie.E192:
                        return "E192";
                    default:
                        return "-";
                }
            }

        }

        public struct Result : IComparable<Result>
        {
            public Resistors BaseResistors;
            public double Resistor;
            public int Pow;
            public double Error;
            public bool Parallel;

            public double GetResistor()
            {
                return Math.Pow(10, Pow) * Resistor;
            }

            public Resistors GetBaseResistors() => BaseResistors;

            public double GetError() => Error;

            public bool IsParallel() => Parallel;

            public int CompareTo(Result other)
            {
                if (Math.Abs(other.Error) < Math.Abs(this.Error))
                    return 1;
                if (Math.Abs(other.Error) == Math.Abs(this.Error))
                    return 0;
                if (Math.Abs(other.Error) > Math.Abs(this.Error))
                    return -1;
                return 0;
            }
        }

        public struct Resistors
        {
            public double R1;
            public double R2;
            public bool Parallel;

            public override bool Equals(object obj)
            {
                if (!(obj is Resistors))
                {
                    return false;
                }

                var resistors = (Resistors)obj;
                return R1 == resistors.R1 &&
                       R2 == resistors.R2 &&
                       Parallel == resistors.Parallel;
            }

            public override int GetHashCode()
            {
                var hashCode = 1802964779;
                hashCode = hashCode * -1521134295 + R1.GetHashCode();
                hashCode = hashCode * -1521134295 + R2.GetHashCode();
                hashCode = hashCode * -1521134295 + Parallel.GetHashCode();
                return hashCode;
            }
        }

        Series series;

        Series.CurrentSerie CurrentSerie = Series.CurrentSerie.E3;

        // Result
        //static List<Result> Results;
        static List<Result> Results;
        static List<Resistors> CheckExisting;

        private int ShownResult = 0;
        static int MaxResult = 1024;
        static double DesiredResistor = 0;
        static double minRes = 0;
        static double maxRes = 0;
        static double minError = 0;
        static int buffersize = 0;
        static List<short> Serie;
        static Label progressLabel;

        private bool Running = false;

        public WindowParallel()
        {
            InitializeComponent();
            series = new Series(CurrentSerie);
        }

        BackgroundWorker bw;
        private void GetResistors(double Resistor)
        {
            Serie = series.GetSerie(CurrentSerie);
            ClearOutput();

            // Get settings
            minRes = EngineerToDecimal(S1.Text);
            if (minRes == 0)
                return;
            maxRes = EngineerToDecimal(S2.Text);
            if (maxRes == 0)
                return;
            if (minRes > maxRes)
            {
                MessageBox.Show("Minimum resistor must be greater or equal than maximum resistor", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!double.TryParse(S3.Text, out minError))
            {
                MessageBox.Show("Invalid minimum error value\n" + minError, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!int.TryParse(S4.Text, out buffersize))
            {
                MessageBox.Show("Invalid buffer size value\n" + buffersize, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MaxResult = buffersize;

            DesiredResistor = Resistor;

            if (!(Resistor > 0))
            {
                MessageBox.Show("Invalid resistor value\n" + Resistor, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Mouse.OverrideCursor = Cursors.Wait;
            Running = true;

            // Backgroundwoker
            bw = new BackgroundWorker();
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.RunWorkerAsync();
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Content = Results.Count.ToString() + " Press ESC to cancel";
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progress.Content = "Completed";
            Running = false;
            Mouse.OverrideCursor = null;

            if (Results == null || Results.Count == 0)
            {
                MessageBox.Show("No result found\nCheck min and max resistors values", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearOutput();
                return;
            }

            //CheckDoubles(Results);

            Results.Sort();
            DisplayOutputs(CheckboxExact.IsChecked == true, Results.ElementAtOrDefault(ShownResult));
            labelResultCount.Content = $"{ShownResult + 1}/{Results.Count}";
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            FillTable(sender as BackgroundWorker, Serie, DesiredResistor, minError, minRes, maxRes, MaxResult);
        }

        private void ClearOutput()
        {
            Results?.Clear();
            labelSerieParallel.Content =
                labelResultCount.Content =
                TextBoxR1.Text =
                TextBoxR2.Text =
                TextBoxRatioOut.Text =
                TextBoxRatioError.Text = "";
        }

        private void DisplayOutputs(bool Exact, Result result)
        {
            if (!((result.BaseResistors.R1 == 0) && (result.BaseResistors.R2 == 0)))
            {
                if (CheckboxExact.IsChecked == true)
                {
                    // Affichage des valeurs exactes
                    TextBoxR1.Text = result.BaseResistors.R1.ToString() + "Ω";
                    TextBoxR2.Text = result.BaseResistors.R2.ToString() + "Ω";
                    TextBoxRatioOut.Text = result.Resistor.ToString();
                    TextBoxRatioError.Text = $"{result.Error}%";
                }
                else
                {
                    // Affichage des valeurs arrondies
                    TextBoxR1.Text = DecimalToEngineer(result.BaseResistors.R1);
                    TextBoxR2.Text = DecimalToEngineer(result.BaseResistors.R2);
                    TextBoxRatioOut.Text = Math.Round(result.Resistor, 3).ToString();
                    TextBoxRatioError.Text = $"{Math.Round(result.Error, 3)}%";
                }
                labelSerieParallel.Content = result.Parallel ? "||" : "+";
            }
        }

        private static string DecimalToEngineer(double Value)
        {
            string Output = "";
            short PowS = 0;

            if (Value < 0)
                return "Out of bounds";

            if (Value == 0)
                return "0Ω";

            while (Value < 1)
            {
                Value *= 1000;
                PowS++;
            }

            while (Value >= 1000)
            {
                Value /= 1000;
                PowS++;
            }

            Value = Math.Round(Value, 3);

            switch (PowS)
            {
                case -3:
                    Output = $"{Value}nΩ";
                    break;
                case -2:
                    Output = $"{Value}μΩ";
                    break;
                case -1:
                    Output = $"{Value}mΩ";
                    break;
                case 0:
                    Output = $"{Value}Ω";
                    break;
                case 1:
                    Output = $"{Value}kΩ";
                    break;
                case 2:
                    Output = $"{Value}MΩ";
                    break;
                case 3:
                    Output = $"{Value}GΩ";
                    break;
                default:
                    Output = $"{Value * Math.Pow(10, 3 * PowS)}Ω";
                    break;
            }

            return Output;
        }

        private double EngineerToDecimal(string Text)
        {
            short PowS = 0;

            if (Text == string.Empty)
            {
                MessageBox.Show("Missing value", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }

            char PowSString = Text.LastOrDefault();
            if (double.TryParse(Text, out double temp))
                return temp;

            if (!double.TryParse(Text.Remove(Text.Length - 1, 1), out double Value))
            {
                MessageBox.Show("Invalid resistor value format\n" + Text.Remove(Text.Length - 1, 1), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }

            while (Value < 1)
            {
                Value *= 1000;
                PowS++;
            }

            while (Value >= 1000)
            {
                Value /= 1000;
                PowS++;
            }

            switch (PowSString)
            {
                case 'k':
                    PowS += 1;
                    break;
                case 'M':
                    PowS += 2;
                    break;
                case 'G':
                    PowS += 3;
                    break;
                default:
                    {
                        MessageBox.Show("Invalid resistor value format. Minimum value is 1ohm.\nAccepted prefixes are 'k', 'M', 'G'. Use as following :\n1.2M", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        return 0;
                    }
            }

            Value *= Math.Pow(10, 3 * PowS);

            return Value;
        }

        // Calculs

        private static double GetErrorPercent(double RealValue, double CalculatedValue)
        {
            return (100 * (CalculatedValue - RealValue) / RealValue);
        }

        private static void FillTable(BackgroundWorker b, List<short> Serie, double WantedValue, double MinError, double minRes, double maxRes, int maxResults)
        {
            Results = new List<Result>();
            CheckExisting = new List<Resistors>();
            MinError = Math.Abs(MinError);
            double currentPercent = 0;
            double PreviousPercent = 0;

            foreach (var r1 in Serie)
            {
                if (b.CancellationPending)
                    return;

                b.ReportProgress(Results.Count / MaxResult);

                double r1t = r1;

                while (r1t <= maxRes)
                {
                    if (r1t >= minRes)
                    {
                        //CheckDoubles();
                        if (Results.Count >= maxResults)
                        {
                            MessageBox.Show("Maximum number of result reached", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        if (r1t == WantedValue)
                        {
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

                            // Check if existing
                            if (CheckDoubles(CheckExisting, res))
                            {
                                CheckExisting.Add(resistors);
                                Results.Add(res);
                            }
                        }

                        foreach (var r2 in Serie)
                        {
                            double r2t = r2;
                            if (r2t >= minRes)
                            {
                                PreviousPercent = 0;
                                while (r2t <= maxRes)
                                {
                                    Resistors Res = new Resistors()
                                    {
                                        R1 = Math.Min(r1t, r2t),
                                        R2 = Math.Max(r1t, r2t),
                                        Parallel = false,
                                    };

                                    currentPercent = CalculateResistors(Res, WantedValue, MinError);

                                    if (currentPercent >= PreviousPercent)
                                        break;
                                    PreviousPercent = currentPercent;

                                    r2t *= 10;
                                }
                            }

                            r2t = r2;

                            if (r2t <= maxRes)
                            {
                                PreviousPercent = 0;
                                while (r2t >= minRes)
                                {
                                    Resistors Res = new Resistors()
                                    {
                                        R1 = Math.Min(r1t, r2t),
                                        R2 = Math.Max(r1t, r2t),
                                        Parallel = false,
                                    };

                                    currentPercent = CalculateResistors(Res, WantedValue, MinError);

                                    if (currentPercent >= PreviousPercent)
                                        break;
                                    PreviousPercent = currentPercent;

                                    r2t /= 10;
                                }
                            }
                        }
                    }
                    r1t *= 10;
                }

                r1t = r1;

                while (r1t >= minRes)
                {
                    if (r1t <= maxRes)
                    {
                        //CheckDoubles();
                        if (Results.Count >= maxResults)
                        {
                            MessageBox.Show("Maximum number of result reached", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        if (r1t == WantedValue)
                        {
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

                            // Check if existing
                            if (CheckDoubles(CheckExisting, res))
                            {
                                CheckExisting.Add(resistors);
                                Results.Add(res);
                            }
                        }

                        foreach (var r2 in Serie)
                        {
                            double r2t = r2;

                            if (r2t >= minRes)
                            {
                                PreviousPercent = 0;
                                while (r2t <= maxRes)
                                {
                                    Resistors Res = new Resistors()
                                    {
                                        R1 = Math.Min(r1t, r2t),
                                        R2 = Math.Max(r1t, r2t),
                                        Parallel = false,
                                    };

                                    currentPercent = CalculateResistors(Res, WantedValue, MinError);

                                    if (currentPercent >= PreviousPercent)
                                        break;
                                    PreviousPercent = currentPercent;

                                    r2t *= 10;
                                }
                            }

                            r2t = r2;
                            if (r2t <= maxRes)
                            {
                                PreviousPercent = 0;
                                while (r2t >= minRes)
                                {
                                    Resistors Res = new Resistors()
                                    {
                                        R1 = Math.Min(r1t, r2t),
                                        R2 = Math.Max(r1t, r2t),
                                        Parallel = false,
                                    };

                                    currentPercent = CalculateResistors(Res, WantedValue, MinError);

                                    if (currentPercent >= PreviousPercent)
                                        break;
                                    PreviousPercent = currentPercent;

                                    r2t /= 10;
                                }
                            }
                        }
                    }
                    r1t /= 10;
                }
            }
        }

        private static double CalculateResistors(Resistors Res, double WantedValue, double MinError)
        {
            // parallel resistor
            double pr = GetParallelResistor(Res);
            // Serial resistor
            double sr = Res.R1 + Res.R2;

            // Ger error
            double pErrorRatio = GetErrorPercent(WantedValue, pr);
            double sErrorRatio = GetErrorPercent(WantedValue, sr);

            // Add if error in range
            if (Math.Abs(sErrorRatio) <= MinError)
            {
                Result res = new Result()
                {
                    BaseResistors = Res,
                    Resistor = sr,
                    Parallel = false,
                    Error = sErrorRatio
                };

                // Check if existing
                if (CheckDoubles(CheckExisting, res))
                {
                    CheckExisting.Add(Res);
                    Results.Add(res);
                }
            }

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

                // Check if existing
                if (CheckDoubles(CheckExisting, res))
                {
                    CheckExisting.Add(Res);
                    Results.Add(res);
                }
            }

            return Math.Min(Math.Abs(pErrorRatio), Math.Abs(sErrorRatio));
        }

        private static bool CheckDoubles(List<Resistors> Results, Result result)
        {
            for (int i = 0; i < Results.Count; i++)
            {
                Resistors temp = Results.ElementAtOrDefault(i);
                if (temp.R1 == result.BaseResistors.R1)
                    if (temp.R2 == result.BaseResistors.R2)
                        if (temp.Parallel == result.Parallel)
                            return false;
            }
            return true;
        }

        private bool[] CheckDoubles(Result[] results)
        {
            bool[] output = new bool[results.Length];

            for (int i = 0; i < Results.Count; i++)
            {
                Result temp = Results.ElementAtOrDefault(i);

                for (int j = 0; j < results.Length; j++)
                {
                    Result result = results.ElementAtOrDefault(j);
                    if (temp.BaseResistors.R1 == result.BaseResistors.R1)
                        if (temp.BaseResistors.R2 == result.BaseResistors.R2)
                            if (temp.Parallel == result.Parallel)
                                output[j] = false;
                }

            }
            return output;
        }

        private static void CheckDoubles(List<Result> Results)
        {
            for (int i = 0; i < Results.Count; i++)
            {
                Result result = Results.ElementAtOrDefault(i);
                Results.RemoveAt(i);

                for (int j = 0; j < Results.Count; j++)
                {
                    Result temp = Results[j];

                    if (temp.BaseResistors.R1 == result.BaseResistors.R1)
                        if (temp.BaseResistors.R2 == result.BaseResistors.R2)
                            if (temp.Parallel == result.Parallel)
                            {
                                Results.RemoveAt(j);
                                j--;
                                if (j >= Results.Count)
                                    break;
                            }
                }
                Results.Insert(i, result);
            }
        }

        private static double GetParallelResistor(Resistors resistors)
        {
            return (resistors.R1 * resistors.R2) / ((double)resistors.R1 + resistors.R2);
        }

        // Evenements sur contrôles

        private void SerieComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {   // Valeur série changée
            CurrentSerie = (Series.CurrentSerie)SerieComboBox.SelectedIndex;
            series.UpdateSerie(CurrentSerie);
        }

        private void TextBoxRatio_TextChanged(object sender, TextChangedEventArgs e)
        {   // Valeur texte changée

        }

        private void TextBoxRatio_KeyDown(object sender, KeyEventArgs e)
        {   // Touche Enter pressée
            if (e.Key == Key.Enter)
            {
                if (double.TryParse(TextBoxRatioIn.Text, out double Ratio))
                {
                    GetResistors(Ratio);
                }
                else
                {
                    MessageBox.Show("Value incorect. Follow this example :\n47.00", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ButtonSerie_Click(object sender, RoutedEventArgs e)
        {   // Affiche la liste des valeurs
            MessageBox.Show($"{string.Join(", ", series.GetSerie(CurrentSerie))}", $"Serie {series.GetSerieName(CurrentSerie)}", MessageBoxButton.OK, MessageBoxImage.None);
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (Running)
                return;

            ShownResult = 0;
            if (double.TryParse(TextBoxRatioIn.Text, out double Ratio))
            {
                if (Ratio > 0)
                {
                    GetResistors(Ratio);
                }
                else
                    MessageBox.Show("Value incorect : Positive numbers only\nFollow this example :\n24.56", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
                MessageBox.Show("Value incorect\nFollow this example :\n24.56", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DisplayOutputs(CheckboxExact.IsChecked == true, Results.ElementAtOrDefault(ShownResult));
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (Running)
                return;

            if (Results == null || Results.Count == 0)
                return;

            if (ShownResult > 0)
                ShownResult--;
            DisplayOutputs(CheckboxExact.IsChecked == true, Results.ElementAtOrDefault(ShownResult));
            labelResultCount.Content = $"{ShownResult + 1}/{Results.Count}";
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (Running)
                return;

            if (Results == null || Results.Count == 0)
                return;

            if (ShownResult + 1 < Results.Count)
                ShownResult++;
            DisplayOutputs(CheckboxExact.IsChecked == true, Results.ElementAtOrDefault(ShownResult));
            labelResultCount.Content = $"{ShownResult + 1}/{Results.Count}";
        }

        private void ShowList_Click(object sender, RoutedEventArgs e)
        {
            if (Running)
                return;

            if (Results == null || Results.Count == 0)
                return;
            Exact = CheckboxExact.IsChecked == true;
            Running = true;
            progress.Content = "Creating text... Press ESC to cancel";
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += Bw_DoWork1;
            bw.RunWorkerAsync();
        }

        private void Bw_DoWork1(object sender, DoWorkEventArgs e)
        {
            DisplayList(sender as BackgroundWorker);
            Running = false;
            Application.Current.Dispatcher.Invoke(new Action(() => { new frmPreview(msg).Show(); progress.Content = "Complete"; }));
            msg = string.Empty;
        }

        static string msg;
        static bool Exact;
        private static void DisplayList(BackgroundWorker b)
        {
            msg = string.Empty;
            foreach (var result in Results)
            {
                if (b.CancellationPending)
                    break;

                if (Exact)
                    msg += $"{result.BaseResistors.R1} {(result.Parallel ? "||" : "+")} {result.BaseResistors.R2} = {result.Resistor} ({result.Error}%)";
                else
                    msg += $"{DecimalToEngineer(result.BaseResistors.R1)} {(result.Parallel ? "||" : "+")} {DecimalToEngineer(result.BaseResistors.R2)} = {Math.Round(result.Resistor, 3)} ({Math.Round(result.Error, 3)}%)";
                msg += Environment.NewLine;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxRatioIn.Focus();
            progressLabel = progress;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Running)
                if (MessageBox.Show("Are you sure you want to cancel ?", "Cancel requested", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    bw.CancelAsync();
        }
    }
}
