using System;
using System.Management;
using System.Text;

namespace TimeManager.Helpers
{
    /// <summary>Information about the operating system</summary>
    static class ComputerOsInfo
    {
        /// <summary>Get the Environment.MachineName value</summary>
        /// <returns>The string of the Environment.MachineName value</returns>
        public static string GetMachineName() { try { return Environment.MachineName; } catch { return ""; } }

        /// <summary>Get the Environment.Is64BitOperatingSystem value</summary>
        /// <returns>The bool of the Environment.Is64BitOperatingSystem value</returns>
        public static bool GetIs64Bit() { try { return Environment.Is64BitOperatingSystem; } catch { return false; } }

        /// <summary>Get the Environment.Is64BitOperatingSystem value</summary>
        /// <returns>The bool of the Environment.Is64BitOperatingSystem value. NULL if not available</returns>
        public static OperatingSystem GetOsVersion() { try { return Environment.OSVersion; } catch { return null; } }

        /// <summary>Get the Date and time when the system started</summary>
        /// <returns>The DateTime of when the OS started</returns>
        public static DateTime GetOsStarted() { return DateTime.UtcNow.AddTicks(-Environment.TickCount); }

    }
}