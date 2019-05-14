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
    }
}
