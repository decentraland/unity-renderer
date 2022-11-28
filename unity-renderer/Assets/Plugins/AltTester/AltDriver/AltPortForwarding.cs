using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Altom.AltDriver.Logging;

namespace Altom.AltDriver
{
    /// <summary>
    /// API to interact with adb, iproxy and xcrun programatically
    /// </summary>
    public class AltPortForwarding
    {

#if UNITY_EDITOR
        private static readonly NLog.Logger logger = DriverLogManager.Instance.GetCurrentClassLogger();
#endif
        public static int IdIproxyProcess = 0;

        /// <summary>
        /// Calls `iproxy {localport} {remotePort} -u {deviceId}`
        /// </summary>
        /// <param name="localPort">The local port to forward from</param>
        /// <param name="remotePort">The device port to forward to</param>
        /// <param name="deviceId">The id of the device</param>
        /// <param name="iproxyPath">The path to iProxy. If iproxyPath is not provided, iproxy should be available in PATH</param>
        public static int ForwardIos(int localPort = 13000, int remotePort = 13000, string deviceId = "", string iproxyPath = "")
        {
            iproxyPath = GetIProxyPath(iproxyPath);
            string arguments;
            if (deviceId.Equals(""))
                arguments = localPort + " " + remotePort;
            else
            {
                arguments = localPort + " " + remotePort + " -u " + deviceId;
            }
            try
            {
                var process = startProcess(iproxyPath, arguments);

                IdIproxyProcess = process.Id;

                if (process.HasExited)
                {
                    throw new PortForwardingException("Error while running command `" + iproxyPath + " " + arguments + "`:" + process.StandardError.ReadToEnd());

                }
                return process.Id;
            }
            catch (Exception ex)
            {
                throw new PortForwardingException("Error while running command: " + iproxyPath + " " + arguments, ex);
            }
        }
        /// <summary>
        /// Get connected iOS devices
        /// </summary>
        /// <param name="xcrunPath">The path to xcrun. If xcrunPath is not provided, xcrun should be available in PATH</param>
        public static List<AltDevice> GetConnectediOSDevices(string xcrunPath = "")
        {
            xcrunPath = GetXcrunPath(xcrunPath);
            var arguments = "instruments -s devices";

            var devices = new List<AltDevice>();
            try
            {
                var process = startProcess(xcrunPath, arguments);

                string line = process.StandardOutput.ReadLine();//Known devices: line
                line = process.StandardOutput.ReadLine();//mac's id
                while (!process.StandardOutput.EndOfStream)
                {
                    line = process.StandardOutput.ReadLine();
                    if (line.Length > 0 && !line.Contains("(Simulator)"))
                    {
                        var parts = line.Split('[');
                        string deviceId = parts[1].Split(']')[0];
                        devices.Add(new AltDevice(deviceId, "iOS", 13000, 13000, false));
                    }
                }
                process.WaitForExit();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                throw new PortForwardingException("Error while running command: " + xcrunPath + " " + arguments, ex);
            }
            return devices;
        }

        /// <summary>
        /// Gets the list of forwarded ios devices
        /// </summary>
        public static List<AltDevice> GetForwardediOSDevices()
        {
            var process = startProcess("ps", "aux");

            var devices = new List<AltDevice>();
            while (!process.StandardOutput.EndOfStream)
            {
                var line2 = process.StandardOutput.ReadLine();//mac's id    
                if (line2.Contains("/iproxy"))
                {
                    var splitedString = line2.Split(' ');
                    splitedString = splitedString.Where(a => !string.IsNullOrEmpty(a)).ToArray();
                    var id = splitedString[splitedString.Length - 1];
                    var localPort = int.Parse(splitedString[splitedString.Length - 3]);
                    var remotePort = int.Parse(splitedString[splitedString.Length - 2]);
                    var pid = int.Parse(splitedString[1]);
                    devices.Add(new AltDevice(id, "iOS", localPort, remotePort, true, pid));
                }
            }
            return devices;
        }

        /// <summary>
        /// Kills provided process id
        /// </summary>
        /// <param name="processId"></param>
        public static void KillIProxy(int processId)
        {
            var process = Process.GetProcessById(processId);
            if (process != null)
            {
                process.Kill();
                process.WaitForExit();
            }
        }

        /// <summary>
        /// Kills iproxy process by name.
        /// </summary>
        public static void KillAllIproxyProcess()
        {
            var process = startProcess("killall", "iproxy");
            process.WaitForExit();
        }

        /// <summary>
        /// Calls adb forward [-s {deviceId}] tcp:{localPort} tcp:{remotePort}
        /// </summary>
        /// <param name="localPort">The local port to be forwarded from</param>
        /// <param name="remotePort">The port on the device to forward to</param>
        /// <param name="deviceId">The id of the device</param>
        /// <param name="adbPath">
        /// The adb path.
        /// If no adb path is provided, it tries to use adb from  ${ANDROID_SDK_ROOT}/platform-tools/adb
        /// if ANDROID_SDK_ROOT env varibale is not set, it tries to execute adb from path.
        /// </param>
        public static string ForwardAndroid(int localPort = 13000, int remotePort = 13000, string deviceId = "", string adbPath = "")
        {
            adbPath = GetAdbPath(adbPath);
            string arguments;
            if (deviceId.Equals(""))
                arguments = "forward tcp:" + localPort + " tcp:" + remotePort;
            else
            {
                arguments = "-s " + deviceId + " forward" + " tcp:" + localPort + " tcp:" + remotePort;
            }
            try
            {

                var process = startProcess(adbPath, arguments);

                string stdout = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (stdout.Length > 0)
                {
                    return stdout;
                }
            }
            catch (Exception ex)
            {
                throw new PortForwardingException("Error while running command: " + adbPath + " " + arguments, ex);

            }
            return "Ok";
        }

        /// <summary>
        /// Calls `adb forward --remove-all` 
        /// </summary>
        /// <param name="adbPath">
        /// The adb path.
        /// If no adb path is provided, it tries to use adb from  ${ANDROID_SDK_ROOT}/platform-tools/adb
        /// if ANDROID_SDK_ROOT env varibale is not set, it tries to execute adb from path.
        /// </param>
        public static void RemoveAllForwardAndroid(string adbPath = "")
        {
            adbPath = GetAdbPath(adbPath);
            string arguments = "forward --remove-all";
            try
            {
                var process = startProcess(adbPath, arguments);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new PortForwardingException("Error while running command: " + adbPath + " " + arguments, ex);
            }
        }

        /// <summary>
        /// Calls `adb forward --remove [-s {deviceId}] tcp:{localPort}`  or `adb forward --remove-all` if no localport provided
        /// </summary>
        /// <param name="localPort">The local port to be removed </param>
        /// <param name="deviceId">The id of the device to be removed</param>
        /// <param name="adbPath">
        /// The adb path.
        /// If no adb path is provided, it tries to use adb from  ${ANDROID_SDK_ROOT}/platform-tools/adb
        /// if ANDROID_SDK_ROOT env varibale is not set, it tries to execute adb from path.
        /// </param>
        public static void RemoveForwardAndroid(int localPort = 13000, string deviceId = "", string adbPath = "")
        {
            adbPath = GetAdbPath(adbPath);
            string arguments = "forward --remove tcp:" + localPort;

            if (!string.IsNullOrEmpty(deviceId))
            {
                arguments = "-s " + deviceId + " " + arguments;
            }
            try
            {
                var process = startProcess(adbPath, arguments);
                process.WaitForExit();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                throw new PortForwardingException("Error while running command: " + adbPath + " " + arguments, ex);
            }
        }

        /// <summary>
        /// Runs `adb devices`
        /// </summary>
        /// <param name="adbPath">
        /// The adb path.
        /// If no adb path is provided, it tries to use adb from  ${ANDROID_SDK_ROOT}/platform-tools/adb
        /// if ANDROID_SDK_ROOT env varibale is not set, it tries to execute adb from path.
        /// </param>
        public static List<AltDevice> GetDevicesAndroid(string adbPath = "")
        {
            adbPath = GetAdbPath(adbPath);
            var arguments = "devices";
            try
            {
                var process = startProcess(adbPath, arguments);
                var devices = new List<AltDevice>();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    if (line.Length > 0 && !line.StartsWith("List "))
                    {
                        var parts = line.Split('\t');
                        string deviceId = parts[0];
                        devices.Add(new AltDevice(deviceId, "Android"));
                    }
                }
                process.WaitForExit();
                process.StandardError.ReadToEnd();
                return devices;
            }
            catch (Exception ex)
            {
                throw new PortForwardingException("Error while running command: " + adbPath + " " + arguments, ex);
            }
        }
        public static List<AltDevice> GetForwardedDevicesAndroid(string adbPath = "")
        {
            adbPath = GetAdbPath(adbPath);
            var arguments = "forward --list";
            try
            {
                var process = startProcess(adbPath, arguments);
                var devices = new List<AltDevice>();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    if (line.Length > 0)
                    {
                        try
                        {
                            var parts = line.Split(' ');
                            string deviceId = parts[0];
                            int localPort = int.Parse(parts[1].Split(':')[1]);
                            int remotePort = int.Parse(parts[2].Split(':')[1]);
                            devices.Add(new AltDevice(deviceId, "Android", localPort, remotePort, true));
                        }
                        catch (System.FormatException)
                        {
#if UNITY_EDITOR

                            logger.Warn("adb forward also has: " + line + "; which was not included in the list of devices");
#endif
                        }
                    }
                }
                process.WaitForExit();
                return devices;
            }
            catch (Exception ex)
            {
                throw new PortForwardingException("Error while running command: " + adbPath + " " + arguments, ex);
            }
        }

        public static string GetAdbPath(string adbPath)
        {
            if (!string.IsNullOrEmpty(adbPath)) return adbPath;
            var androidSdkRoot = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
            if (!string.IsNullOrEmpty(androidSdkRoot))
            {
                return Path.Combine(androidSdkRoot, "platform-tools", "adb");
            }
            return "adb";
        }

        public static string GetIProxyPath(string iproxyPath)
        {
            if (!string.IsNullOrEmpty(iproxyPath)) return iproxyPath;
            return "iproxy";
        }
        public static string GetXcrunPath(string xcrunPath)
        {
            if (!string.IsNullOrEmpty(xcrunPath)) return xcrunPath;
            return "xcrun";
        }

        private static Process startProcess(string processPath, string arguments)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Minimized,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = processPath,
                Arguments = arguments
            };
            process.StartInfo = startInfo;
            process.Start();

            return process;
        }
    }
}

