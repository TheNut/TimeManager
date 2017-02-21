using System;
using System.Collections.Generic;
using System.Management;
using System.Text;

namespace TimeManager.Helpers
{
    /// <summary>Information about the hardware the system is running on</summary>
    static class ComputerHardwareInfo
    {
        /// <summary>Get the Environment.ProcessorCount value</summary>
        /// <returns>The int of the Environment.ProcessorCount value</returns>
        public static int GetProcessorCount() { try { return Environment.ProcessorCount; } catch { return -1; } }

        /// <summary>Get the first non empty MAC address from the system</summary>
        /// <returns></returns>
        public static string GetMACAddressFirst()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            string MACAddress = String.Empty;

            foreach (ManagementObject mo in moc)
            {
                if (MACAddress == String.Empty)
                { // only return MAC Address from first card
                    if ((bool)mo["IPEnabled"] == true) MACAddress = mo["MacAddress"].ToString();
                }
                mo.Dispose();
            }

            return MACAddress;
        }

        /// <summary>Get the first ManufacturerName found in the BIOS.
        ///     https://msdn.microsoft.com/en-us/library/aa394077.aspx
        /// </summary>
        /// <returns>a string consisting of ManufacturerName</returns>
        public static string GetBiosManufacturer()
        {
            StringBuilder builder = new StringBuilder();

            String query = "SELECT * FROM Win32_BIOS";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            //  This should only find one
            foreach (ManagementObject item in searcher.Get())
            {
                Object obj = item["Manufacturer"];
                builder.Append(Convert.ToString(obj).Trim());

                return builder.ToString();
            }

            //return an empty string if none found
            return string.Empty;
        }

        /// <summary>Get the first SerialNumber found in the BIOS.
        ///     https://msdn.microsoft.com/en-us/library/aa394077.aspx
        /// </summary>
        /// <returns>a string consisting of SerialNumber</returns>
        public static string GetBiosSerialNumber()
        {
            StringBuilder builder = new StringBuilder();

            String query = "SELECT * FROM Win32_BIOS";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            //  This should only find one
            foreach (ManagementObject item in searcher.Get())
            {
                Object obj = item["SerialNumber"];
                builder.Append(Convert.ToString(obj).Trim());

                return builder.ToString();
            }

            //return an empty string if none found
            return string.Empty;
        }

        /// <summary>Get the first ManufacturerName:SerialNumber found in the BIOS.
        ///     https://msdn.microsoft.com/en-us/library/aa394077.aspx
        /// </summary>
        /// <returns>a string consisting of ManufacturerName:SerialNumber</returns>
        public static string GetBiosMfgSerial()
        {
            StringBuilder builder = new StringBuilder();

            String query = "SELECT * FROM Win32_BIOS";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            //  This should only find one
            foreach (ManagementObject item in searcher.Get())
            {
                Object obj = item["Manufacturer"];
                builder.Append(Convert.ToString(obj).Trim());
                builder.Append(':');
                obj = item["SerialNumber"];
                builder.Append(Convert.ToString(obj).Trim());

                return builder.ToString();
            }

            //return an empty string if none found
            return string.Empty;
        }
    }
}
