using System;
using System.Management;
using System.Text;

namespace TimeManager.Helpers
{
    /// <summary>Information about the user</summary>
    static class ComputerUserInfo
    {
        /// <summary>Gets the user name of the person who is currently logged on to the operating system.</summary>
        /// <returns>The string of the Environment.UserName value</returns>
        public static string GetName() { try { return Environment.UserName; } catch { return ""; } }

        /// <summary>Get the network domain name associated with the user</summary>
        /// <returns>The string of the Environment.UserDomainName value</returns>
        public static string GetDomainName() { try { return Environment.UserDomainName; } catch { return string.Empty; } }
    }
}