using System;

namespace TimeManager.Helpers
{
    /// <summary>Information about the Application environment</summary>
    static class ComputerAppInfo
    {
        /// <summary>Get the Environment.Is64BitOperatingSystem value</summary>
        /// <returns>The bool of the Environment.Is64BitOperatingSystem value. NULL if not available</returns>
        public static bool GetIs64Bit() { try { return Environment.Is64BitProcess; } catch { return false; } }

        /// <summary>Gets a System.Version object that describes the major, minor, build, and revision numbers of the common language runtime.</summary>
        /// <returns>The Version of the Environment.Version value. NULL if not available</returns>
        public static Version GetRuntimeVersion() { try { return Environment.Version; } catch { return null; } }
        
    }
}