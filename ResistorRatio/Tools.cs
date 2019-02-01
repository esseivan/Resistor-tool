using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistorTool
{
    public class Tools
    {
        /// <summary>
        /// Convert decimal format to engineer format
        /// </summary>
        public static string DecimalToEngineer(double Value)
        {
            string Output = "";
            short PowS = 0;

            if (Value < 0)
            {
                return "Out of bounds";
            }

            if (Value == 0)
            {
                return "0Ω";
            }

            while (Value < 1)
            {
                Value *= 1000;
                PowS--;
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

        /// <summary>
        /// Convert engineer format to decimal
        /// </summary>
        public static double EngineerToDecimal(string Text)
        {
            short PowS = 0;

            if (Text == string.Empty)
            {
                //MessageBox.Show("Missing value", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }

            char PowSString = Text.LastOrDefault();
            if (double.TryParse(Text, out double temp))
            {
                return temp;
            }

            if (!double.TryParse(Text.Remove(Text.Length - 1, 1), out double Value))
            {
                //MessageBox.Show("Invalid resistor value format\n" + Text.Remove(Text.Length - 1, 1), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                case 'm':
                    PowS -= 1;
                    break;
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
                        //MessageBox.Show("Invalid resistor value format.\nAccepted prefixes are 'm', 'k', 'M', 'G'. Use as following :\n24.56k", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return 0;
                    }
            }

            Value *= Math.Pow(10, 3 * PowS);

            return Value;
        }

        /// <summary>
        /// Get error percent
        /// </summary>
        public static double GetErrorPercent(double RealValue, double CalculatedValue)
        {
            return (100 * (CalculatedValue - RealValue) / RealValue);
        }

        /// <summary>
        /// No fucking idea what I did
        /// </summary>
        public static int GetNearestValue(double Value, List<short> Serie)
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

        /// <summary>
        /// Return parallel resistor value
        /// </summary>
        public static double GetParallelResistor(Resistors resistors)
        {
            return (resistors.R1 * resistors.R2) / (resistors.R1 + resistors.R2);
        }

        /// <summary>
        /// Check if result is already existing
        /// </summary>
        public static bool CheckDoubles(List<Resistors> Results, Result result)
        {
            for (int i = 0; i < Results.Count; i++)
            {
                Resistors temp = Results.ElementAtOrDefault(i);
                if (temp.R1 == result.BaseResistors.R1)
                {
                    if (temp.R2 == result.BaseResistors.R2)
                    {
                        if (temp.Parallel == result.Parallel)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Delete all doubles
        /// </summary>
        public static void CheckDoubles(List<Result> Results)
        {
            for (int i = 0; i < Results.Count; i++)
            {
                Result result = Results.ElementAtOrDefault(i);
                Results.RemoveAt(i);

                for (int j = 0; j < Results.Count; j++)
                {
                    Result temp = Results[j];

                    if (temp.BaseResistors.R1 == result.BaseResistors.R1)
                    {
                        if (temp.BaseResistors.R2 == result.BaseResistors.R2)
                        {
                            if (temp.Parallel == result.Parallel)
                            {
                                Results.RemoveAt(j);
                                j--;
                                if (j >= Results.Count)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                Results.Insert(i, result);
            }
        }

        public struct Result : IComparable<Result>
        {
            public Resistors BaseResistors;
            public double Error;
            // Used for reverse parallel 
            public double Resistor;
            public int Pow;
            public bool Parallel;
            // Used for ratio
            public double DesiredRatio;
            public double Ratio;


            public double GetResistor()
            {
                return Math.Pow(10, Pow) * Resistor;
            }

            public Resistors GetBaseResistors() => BaseResistors;

            public double GetError() => Error;

            public bool IsParallel() => Parallel;

            public int CompareTo(Result other)
            {
                if (Math.Abs(other.Error) < Math.Abs(Error))
                {
                    return 1;
                }

                if (Math.Abs(other.Error) == Math.Abs(Error))
                {
                    return 0;
                }

                if (Math.Abs(other.Error) > Math.Abs(Error))
                {
                    return -1;
                }

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
                InitSeries(serie);
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
    }
}
