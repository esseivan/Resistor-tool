using EsseivaN.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace EsseivaN.Apps.ResistorTool
{
    public class Tools
    {
        public static Logger logger { get; set; }

        public static void WriteLog(int caller, string data)
        {
            WriteLog(caller, data, Logger.Log_level.Debug);
        }

        /// <summary>
        /// Write to logger
        /// </summary>
        public static void WriteLog(int caller, string data, Logger.Log_level level)
        {
            if (caller != 0)
            {
                data = $"[{(caller == 1 ? "RER" : (caller == 2 ? "RC" : "UNK"))}] {data}";
            }

            logger.WriteLog(data, level);
        }

        /// <summary>
        /// Delete all doubles
        /// </summary>
        public static void CheckDoubles(BackgroundWorker bw, List<ResistorCalculator.Result> Results)
        {
            var t = Results.Distinct();

            int progress;
            ResistorCalculator.Result result, temp;
            int j;
            // For each in results
            for (int i = 0; i < Results.Count; i++)
            {
                // Retrieve the element
                result = Results.ElementAtOrDefault(i);
                Results.RemoveAt(i);

                // Update progress
                progress = ((i * 100) / Results.Count);
                if (progress > 100)
                {
                    progress = 100;
                }
                else if (progress < 0)
                {
                    progress = 0;
                }
                bw.ReportProgress(progress);

                // Check for all others in results
                for (j = 0; j < Results.Count; j++)
                {
                    // Check for cancel
                    if (bw.CancellationPending)
                    {
                        // reinsert the deleted one
                        Results.Insert(i, result);
                        return;
                    }

                    temp = Results[j];

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

    }
}
