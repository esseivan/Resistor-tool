using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ResistorTool.Tools;

namespace ResistorTool
{
    public partial class WindowRatio : Form
    {
        private Series series;

        private Series.CurrentSerie CurrentSerie = Series.CurrentSerie.E3;

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
                int Value01 = Tools.GetNearestValue((Value02 * Ratio), Serie);
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
                    textbox_outR1.Text = Tools.DecimalToEngineer(R2);
                    textbox_outR2.Text = Tools.DecimalToEngineer(R1);
                    textbox_outRatio.Text = Math.Round(ActualRatio, 3).ToString();
                    textbox_outError.Text = $"{Math.Round(RatioError, 3)}%";
                }
            }
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

        private void WindowRatio_Load(object sender, EventArgs e)
        {
            SerieComboBox.SelectedIndex = 2;
        }

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
