using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsseivaN.Tools
{
    public class ResistorCalculator
    {

        /// <summary>
        /// Return parallel resistor value
        /// </summary>
        public static double GetParallelResistor(Resistors resistors)
        {
            return (resistors.R1 * resistors.R2) / (resistors.R1 + resistors.R2);
        }

        /// <summary>
        /// Delete all doubles
        /// </summary>
        public static List<Result> CheckDoubles(List<Result> Results)
        {
            return Results.Distinct().ToList();
        }

        public class Result : IComparable<Result>
        {
            /// <summary>
            /// Input resistors
            /// </summary>
            public Resistors BaseResistors { get; set; }
            /// <summary>
            /// Output resistor
            /// </summary>
            public double Resistor { get; set; }
            /// <summary>
            /// Power of the output resistor
            /// </summary>
            public int Pow { get; set; }
            /// <summary>
            /// Output ratio
            /// </summary>
            public double Ratio { get; set; }
            /// <summary>
            /// Error percent
            /// </summary>
            public double Error { get; set; }
            /// <summary>
            /// True for parallel, false for serial
            /// </summary>
            public bool Parallel { get; set; }

            /// <summary>
            /// Return the value of the resistor (with pow applied)
            /// </summary>
            /// <returns></returns>
            public double GetResistor()
            {
                return Math.Pow(10, Pow) * Resistor;
            }

            /// <summary>
            /// Compare results with absolut error percent
            /// </summary>
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

        public class Resistors
        {
            /// <summary>
            /// Input resistor 1
            /// </summary>
            public double R1 { get; set; }
            /// <summary>
            /// Input resistor 2
            /// </summary>
            public double R2 { get; set; }
            /// <summary>
            /// Is parallel or serial
            /// </summary>
            public bool Parallel { get; set; }

            /// <summary>
            /// Compare with another Resistors class
            /// </summary>
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
                var hashCode = 0;
                hashCode = hashCode * 31 + R1.GetHashCode();
                hashCode = hashCode * 31 + R2.GetHashCode();
                hashCode = hashCode * 31 + Parallel.GetHashCode();
                return hashCode;
            }
        }

        public class Series
        {
            // Series to prevent creating them multiple times
            private static List<short> SerieE192;
            private static List<short> SerieE96;
            private static List<short> SerieE48;
            private static List<short> SerieE24;
            private static List<short> SerieE12;
            private static List<short> SerieE6;
            private static List<short> SerieE3;

            /// <summary>
            /// Serie list
            /// </summary>
            public enum SerieName
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

            /// <summary>
            /// Get the specified serie
            /// </summary>
            /// <param name="serie"></param>
            /// <returns></returns>
            public static List<short> GetSerie(SerieName serie)
            {
                return InitSeries(serie);
            }

            /// <summary>
            /// Initialize the specified serie
            /// </summary>
            private static List<short> InitSeries(SerieName serie)
            {
                List<short> result = null;

                switch (serie)
                {
                    case SerieName.E3:
                        // If not initialized, initialize
                        if (SerieE3 == null)
                        {
                            SerieE3 = new List<short>() { 100, 220, 470 };
                        }
                        result = SerieE3;
                        break;
                    case SerieName.E6:
                        // If not initialized, initialize
                        if (SerieE6 == null)
                        {
                            SerieE6 = new List<short>() { 100, 150, 220, 330, 470, 680 };
                        }
                        result = SerieE6;
                        break;
                    case SerieName.E12:
                        // If not initialized, initialize
                        if (SerieE12 == null)
                        {
                            SerieE12 = new List<short>() { 100, 120, 150, 180, 220, 270, 330, 390, 470, 560, 680, 820 };
                        }
                        result = SerieE12;
                        break;
                    case SerieName.E24:
                        // If not initialized, initialize
                        if (SerieE24 == null)
                        {
                            SerieE24 = new List<short>() { 100, 110, 120, 130, 150, 160, 180, 200, 220, 240, 270, 300, 330, 360, 390, 430, 470, 510, 560, 620, 680, 750, 820, 910 };
                        }
                        result = SerieE24;
                        break;
                    case SerieName.E48:
                        // If not initialized, initialize
                        if (SerieE48 == null)
                        {
                            SerieE48 = new List<short>() { 100, 105, 110, 115, 121, 127, 133, 140, 147, 154, 162, 169, 178, 187, 196, 205, 215, 226, 237, 249, 261, 274, 287, 301, 316, 332, 348, 365, 383, 402, 422, 442, 464, 487, 511, 536, 562, 590, 619, 649, 681, 715, 750, 787, 825, 866, 909, 953 };
                        }
                        result = SerieE48;
                        break;
                    case SerieName.E96:
                        // If not initialized, initialize
                        if (SerieE96 == null)
                        {
                            SerieE96 = new List<short>() { 100, 102, 105, 107, 110, 113, 115, 118, 121, 124, 127, 130, 133, 137, 140, 143, 147, 150, 154, 158, 162, 165, 169, 174, 178, 182, 187, 191, 196, 200, 205, 210, 215, 221, 226, 232, 237, 243, 249, 255, 261, 267, 274, 280, 287, 294, 301, 309, 316, 324, 332, 340, 348, 357, 365, 374, 383, 392, 402, 412, 422, 432, 442, 453, 464, 475, 487, 499, 511, 523, 536, 549, 562, 576, 590, 604, 619, 634, 649, 665, 681, 698, 715, 732, 750, 768, 787, 806, 825, 845, 866, 887, 909, 931, 953, 976 };
                        }
                        result = SerieE96;
                        break;
                    case SerieName.E192:
                        // If not initialized, initialize
                        if (SerieE192 == null)
                        {
                            SerieE192 = new List<short>() { 100, 101, 102, 104, 105, 106, 107, 109, 110, 111, 113, 114, 115, 117, 118, 120, 121, 123, 124, 126, 127, 129, 130, 132, 133, 135, 137, 138, 140, 142, 143, 145, 147, 149, 150, 152, 154, 156, 158, 160, 162, 164, 165, 167, 169, 172, 174, 176, 178, 180, 182, 184, 187, 189, 191, 193, 196, 198, 200, 203, 205, 208, 210, 213, 215, 218, 221, 223, 226, 229, 232, 234, 237, 240, 243, 246, 249, 252, 255, 258, 261, 264, 267, 271, 274, 277, 280, 284, 287, 291, 294, 298, 301, 305, 309, 312, 316, 320, 324, 328, 332, 336, 340, 344, 348, 352, 357, 361, 365, 370, 374, 379, 383, 388, 392, 397, 402, 407, 412, 417, 422, 427, 432, 437, 442, 448, 453, 459, 464, 470, 475, 481, 487, 493, 499, 505, 511, 517, 523, 530, 536, 542, 549, 556, 562, 569, 576, 583, 590, 597, 604, 612, 619, 626, 634, 642, 649, 657, 665, 673, 681, 690, 698, 706, 715, 723, 732, 741, 750, 759, 768, 777, 787, 796, 806, 816, 825, 835, 845, 856, 866, 876, 887, 898, 909, 920, 931, 942, 953, 965, 976, 988 };
                        }
                        result = SerieE192;
                        break;
                    default:
                        break;
                }

                return result;
            }
        }
    }

    public interface ResistorCalculatorProcess
    {
        void run();
    }
}
