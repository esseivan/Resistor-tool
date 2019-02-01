using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResistorTool
{
    public partial class WindowRatio : Form
    {
        struct Series
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

        Series series;

        Series.CurrentSerie CurrentSerie = Series.CurrentSerie.E3;

        // Result
        int R1 = 0;
        int R2 = 0;
        double DesiredRatio = 0;
        double ActualRatio = 0;
        double RatioError = 0;

        public WindowRatio()
        {
            InitializeComponent();
            series = new Series(CurrentSerie);
        }

        private void WindowRatio_Load(object sender, EventArgs e)
        {
            SerieComboBox.SelectedIndex = 2;
        }

        // Calculs

        private void GetResistorFromRatio(double Ratio)
        {
            double ErrorPercent = double.MaxValue;
            double ErrorPercentTemp = 0;
            int Value1 = 0;
            short Value2 = 0;
            bool Inverted = false;

            List<short> Serie = series.GetSerie(CurrentSerie);

            DesiredRatio = Ratio;

            if ((Ratio < 1) && (Ratio > 0))
            {
                Ratio = 1 / Ratio;
                Inverted = true;
            }

            foreach (short Value02 in Serie)
            {
                int Value01 = GetNearestValue((Value02 * Ratio), Serie);
                ErrorPercentTemp = Math.Abs(GetErrorPercent(((double)Value01 / (double)Value02), Ratio));
                if (ErrorPercentTemp < ErrorPercent)
                {
                    ErrorPercent = ErrorPercentTemp;
                    Value1 = Value01;
                    Value2 = Value02;
                    if (ErrorPercent == 0)
                        break;
                }
            }

            // Sauvegarde des valeurs
            if (!Inverted)
            {
                R1 = Value1;
                R2 = Value2;
            }
            else
            {
                R1 = Value2;
                R2 = Value1;
            }
            ActualRatio = ((double)R1 / (double)R2);
            RatioError = ErrorPercent;

            DisplayOutputs(CheckboxExact.Checked == true);

            //MessageBox.Show($"Desied Ratio = {Ratio}\nActual Ratio (R1/R2) = {Value2 / Value1}\nResistor 1 = {Value2}\nResistor 2 = {Value1}\nError = {Math.Round(ErrorPercent, 3)}%");
        }

        private void DisplayOutputs(bool Exact)
        {
            if ((R1 != 0) && (R2 != 0))
            {
                if (CheckboxExact.Checked == true)
                {
                    // Affichage des valeurs exactes
                    textbox_outR1.Text = R2.ToString() + "Ω";
                    textbox_outR2.Text = R1.ToString() + "Ω";
                    textbox_outRatio.Text = ActualRatio.ToString();
                    textbox_outError.Text = $"{RatioError}%";
                }
                else
                {
                    // Affichage des valeurs arrondies
                    textbox_outR1.Text = DecimalToEngineer(R2);
                    textbox_outR2.Text = DecimalToEngineer(R1);
                    textbox_outRatio.Text = Math.Round(ActualRatio, 3).ToString();
                    textbox_outError.Text = $"{Math.Round(RatioError, 3)}%";
                }
            }
        }

        private string DecimalToEngineer(double Value)
        {
            string Output = "";
            short PowS = 0;

            if (Value < 0)
                return "Out of bounds";

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

        private double GetErrorPercent(double RealValue, double CalculatedValue)
        {
            return (100 * (CalculatedValue - RealValue) / RealValue);
        }

        private int GetNearestValue(double Value, List<short> Serie)
        {
            // Réduire la valeur entre 100 et 1000 non compris
            short Pow = 0;
            while (Value < 100)
            {
                Value *= 10;
                Pow--;
            }

            while (Value >= 1000)
            {
                Value /= 10;
                Pow++;
            }

            // Calcul de la valeur la plus proche
            double Delta = double.MaxValue;
            int NearestValue = 0;

            // Valeurs pour les calculs
            short ValueTemp = 0;
            double DeltaTemp = 0;

            // Flag indiquant que la valeur la plus proche est trouvée
            bool ValueFound = false;

            // Boucle cherchant la première fois que l'erreur est plus petite
            byte Counter = 0;
            while ((Counter < Serie.Count) && !ValueFound)
            {
                ValueTemp = Serie[Counter];
                DeltaTemp = Math.Abs(ValueTemp - Value);

                if (DeltaTemp < Delta)
                {   // Première valeur plus proche trouvée
                    if (DeltaTemp != 0)
                    {
                        while ((DeltaTemp < Delta) && ((Counter + 1) < Serie.Count))
                        {   // Tant que la valeur se rapproche
                            Counter++;
                            Delta = DeltaTemp;
                            ValueTemp = Serie[Counter];
                            DeltaTemp = Math.Abs(ValueTemp - Value);
                        }

                        if (DeltaTemp >= Delta)
                            Counter--;
                        //else if ((Counter + 1) < Serie.Count)
                        //    Counter--;
                    }
                    else
                    {
                        Delta = DeltaTemp;
                    }

                    // Sortie de la boucle quand la valeur s'éloigne
                    NearestValue = Serie[Counter];
                    ValueFound = true;
                }

                Counter++;
            }

            NearestValue = (int)(NearestValue * Math.Pow(10, Pow));

            return NearestValue;
        }

        private void Calculate()
        {
            if (double.TryParse(TextBoxRatio.Text, out double Ratio))
            {
                if (Ratio > 0)
                {
                    GetResistorFromRatio(Ratio);
                }
                else
                    MessageBox.Show("Value incorect : Positive numbers only\nFollow this example :\n24.56", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                MessageBox.Show("Value incorect\nFollow this example :\n24.56", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Evenements

        private void SerieComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {   // Valeur série changée
            CurrentSerie = (Series.CurrentSerie)SerieComboBox.SelectedIndex;
            series.UpdateSerie(CurrentSerie);
        }

        private void TextBoxRatio_KeyDown(object sender, KeyEventArgs e)
        {   // Touche Enter pressée
            if (e.KeyCode == Keys.Enter)
            {
                Calculate();
            }
        }

        private void ButtonSerie_Click(object sender, EventArgs e)
        {   // Affiche la liste des valeurs
            MessageBox.Show($"{string.Join(", ", series.GetSerie(CurrentSerie))}", $"Serie {series.GetSerieName(CurrentSerie)}", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        private void ButtonConfirm_Click(object sender, EventArgs e)
        {
            Calculate();
        }

        private void CheckboxExact_CheckedChanged(object sender, EventArgs e)
        {
            DisplayOutputs(CheckboxExact.Checked);
        }
    }
}
