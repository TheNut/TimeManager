using TimeManager.Helpers;
using System;

namespace TimeManager.Models
{
    /// <summary>Model that contains information about the computer on which this application is running.</summary>
    public class InformationModel
    {
        /// <summary>Constructor - Default</summary>
        public InformationModel()
        {
            MachineInfo = new MachineInformationModel();
            OperatingSystemInfo = new OperatingSystemInformationModel();
            ApplicationInfo = new ApplicationInformationModel();
            UserInfo = new UserInfoModel();
        }
        /// <summary>Clear the values back to defaults</summary>
        public void Clear()
        {
            MachineInfo.Clear();
            OperatingSystemInfo.Clear();
            ApplicationInfo.Clear();
            UserInfo.Clear();
        }
        /// <summary>Infomation about the hardware</summary>
        public MachineInformationModel MachineInfo { get; private set; }
        /// <summary>Information about the operating system</summary>
        public OperatingSystemInformationModel OperatingSystemInfo { get; private set; }
        /// <summary>Information about the application running environment</summary>
        public ApplicationInformationModel ApplicationInfo { get; private set; }
        /// <summary>Information about the user</summary>
        public UserInfoModel UserInfo { get; private set; }


        /// <summary>Information about the machine the user is on</summary>
        public class MachineInformationModel
        {
            /// <summary>Constructor - Default</summary>
            public MachineInformationModel()
            {
                BiosId = ComputerHardwareInfo.GetBiosMfgSerial();
                BiosManufacturer = ComputerHardwareInfo.GetBiosManufacturer();
                BiosSerialNumber = ComputerHardwareInfo.GetBiosSerialNumber();
                MacAddress = ComputerHardwareInfo.GetMACAddressFirst();
                ProcessorCount = ComputerHardwareInfo.GetProcessorCount();
            }
            /// <summary>Clear the values back to defaults</summary>
            public void Clear()
            {
                BiosId = string.Empty;
                BiosManufacturer = string.Empty;
                BiosSerialNumber = string.Empty;
                MacAddress = string.Empty;
                ProcessorCount = 0;
            }
            /// <summary>Get the first ManufacturerName:SerialNumber found in the BIOS.</summary>
            public string BiosId { get; set; }
            /// <summary>The Manufacturer of the bios if available</summary>
            public string BiosManufacturer { get; set; }
            /// <summary>The Serial number found in the bios if available</summary>
            public string BiosSerialNumber { get; set; }
            /// <summary>The first enabled non-empty MAC address from the system</summary>
            public string MacAddress { get; set; }
            /// <summary>The number of processors on the current machine</summary>
            public int ProcessorCount { get; set; }
        }
        /// <summary>The model to hold user information</summary>
        public class UserInfoModel
        {
            /// <summary>Constructor - Default</summary>
            public UserInfoModel()
            {
                LoginName = ComputerUserInfo.GetName();
                DomainName = ComputerUserInfo.GetDomainName();
            }
            /// <summary>Clear the values back to defaults</summary>
            public void Clear()
            {
                LoginName = string.Empty;
                DomainName = string.Empty;
            }
            /// <summary>The user name of the person who is currently logged onto the system.</summary>
            public string LoginName { get; set; }
            /// <summary>Get the network domain name associated with the user</summary>
            public string DomainName { get; set; }
        }
        /// <summary>Information about the operating system</summary>
        public class OperatingSystemInformationModel
        {
            /// <summary>Constructor - Default</summary>
            public OperatingSystemInformationModel()
            {
                Is64Bit = ComputerOsInfo.GetIs64Bit();
                NetBiosName = ComputerOsInfo.GetMachineName();
                OsVersion = ComputerOsInfo.GetOsVersion();
                SystemStart = ComputerOsInfo.GetOsStarted();
                UptimeTickCount = Environment.TickCount;
            }
            /// <summary>Clear the values back to defaults</summary>
            public void Clear()
            {
                Is64Bit = false;
                NetBiosName = string.Empty;
                OsVersion = null;
                SystemStart = new DateTimeOffset();
                UptimeTickCount = 0;
            }
            /// <summary>Determines whether the current operating system is a 64-bit operating system.</summary>
            public bool Is64Bit { get; set; }
            /// <summary>The NetBIOS name of this local computer.</summary>
            public string NetBiosName { get; set; }
            /// <summary>A System.OperatingSystem object that contains the current platform identifier and version number</summary>
            public OperatingSystem OsVersion { get; set; }
            /// <summary>The DateTime of when the OS started</summary>
            public DateTimeOffset SystemStart { get; set; }
            /// <summary>The uptime tick count since the OS started</summary>
            public int UptimeTickCount { get; set; }
            /// <summary>The current uptime tick count since the OS started</summary>
            public int UptimeTickCountNow { get { return Environment.TickCount; } }
        }
        /// <summary>Information about the application itself</summary>
        public class ApplicationInformationModel
        {
            /// <summary>Constructor - Default</summary>
            public ApplicationInformationModel()
            {
                Is64BitProcess = ComputerAppInfo.GetIs64Bit();
                RuntimeVersion = ComputerAppInfo.GetRuntimeVersion();
            }
            /// <summary>Clear the values back to defaults</summary>
            public void Clear()
            {
                Is64BitProcess = false;
                RuntimeVersion = null;

            }
            /// <summary>Is the current process a 64 bit process</summary>
            public bool Is64BitProcess { get; set; }
            /// <summary>Describes the major, minor, build, and revision numbers of the common language runtime.</summary>
            public Version RuntimeVersion { get; set; }
        }
    }

    /// <summary>Model that contains information about the computer on which this application is running.</summary>
    public static class InformationModelStatic
    {
        /// <summary>Infomation about the hardware</summary>
        public static class MachineInfo
        {
            /// <summary>Get the first ManufacturerName:SerialNumber found in the BIOS.</summary>
            public static string BiosId           { get { return ComputerHardwareInfo.GetBiosMfgSerial();    } }
            /// <summary>The Manufacturer of the bios if available</summary>
            public static string BiosManufacturer { get { return ComputerHardwareInfo.GetBiosManufacturer(); } }
            /// <summary>The Serial number found in the bios if available</summary>
            public static string BiosSerialNumber { get { return ComputerHardwareInfo.GetBiosSerialNumber(); } }
            /// <summary>The first enabled non-empty MAC address from the system</summary>
            public static string MacAddress       { get { return ComputerHardwareInfo.GetMACAddressFirst();  } }
            /// <summary>The number of processors on the current machine</summary>
            public static int    ProcessorCount   { get { return ComputerHardwareInfo.GetProcessorCount();   } }
        }

        /// <summary>Information about the operating system</summary>
        public static class OperatingSystemInfo
        {
            /// <summary>Determines whether the current operating system is a 64-bit operating system.</summary>
            public static bool Is64Bit               { get { return ComputerOsInfo.GetIs64Bit();     } }
            /// <summary>The NetBIOS name of this local computer.</summary>
            public static string NetBiosName         { get { return ComputerOsInfo.GetMachineName(); } }
            /// <summary>A System.OperatingSystem object that contains the current platform identifier and version number</summary>
            public static OperatingSystem OsVersion  { get { return ComputerOsInfo.GetOsVersion();   } }
            /// <summary>The DateTime of when the OS started</summary>
            public static DateTimeOffset SystemStart { get { return ComputerOsInfo.GetOsStarted();   } }
            /// <summary>The current uptime tick count since the OS started</summary>
            public static int UptimeTickCount        { get { return Environment.TickCount;           } }
        }

        /// <summary>Information about the application itself</summary>
        public static class ApplicationInfo
        {
            /// <summary>Is the current process a 64 bit process</summary>
            public static bool Is64BitProcess { get { return ComputerAppInfo.GetIs64Bit(); } }
            /// <summary>Describes the major, minor, build, and revision numbers of the common language runtime.</summary>
            public static Version RuntimeVersion { get { return ComputerAppInfo.GetRuntimeVersion(); } }
        }

        /// <summary>The model to hold user information</summary>
        public static class UserInfo
        {
            /// <summary>The user name of the person who is currently logged onto the system.</summary>
            public static string LoginName  { get { return ComputerUserInfo.GetName();       } }
            /// <summary>Get the network domain name associated with the user</summary>
            public static string DomainName { get { return ComputerUserInfo.GetDomainName(); } }
        }
    }
}