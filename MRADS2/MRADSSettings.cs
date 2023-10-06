using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRADS2
{
    /// <summary>
    /// A static class containing global settings
    /// </summary>
    public class MRADSSettings
    {
        public static double CANTimeout { get; } = 5;

        public static string AppData { get; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\MRADSData";
    }
}
