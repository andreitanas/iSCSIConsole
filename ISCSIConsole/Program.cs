using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Reflection;
using System.Text;
using DiskAccessLibrary.LogicalDiskManager;
using DiskAccessLibrary;
using Utilities;
using Newtonsoft.Json;
using ISCSI.Server;
using Newtonsoft.Json.Linq;
using ISCSIDisk;

namespace ISCSIConsole
{
    partial class Program
    {
        public static bool m_debug = false;
        public const string CONFIG_FILE = "config.json";

        static void Main(string[] args)
        {
            Console.WriteLine("iSCSI Console v" + Assembly.GetEntryAssembly().GetName().Version);

            ReadConfig();
            MainLoop();
        }

        private static void ReadConfig()
        {
            if (!File.Exists(CONFIG_FILE))
            {
                Console.WriteLine($"{CONFIG_FILE} not found, starting in interactive mode");
                return;
            }
            
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(CONFIG_FILE));

            if (config.Targets == null || config.Targets.Length == 0)
            {
                Console.WriteLine($"No targets configured, starting in interactive mode");
                return;
            }

            var targets = new List<ISCSITarget>();
            foreach (var targetConfig in config.Targets)
            {
                Console.WriteLine("Adding target '{0}'", targetConfig.Name);
                var disks = new List<Disk>();

                foreach (var diskConfig in targetConfig.Disks)
                {
                    Disk disk;
                    var parameters = diskConfig.Parameters as JObject;
                    switch (diskConfig.Kind)
                    {
                        case DiskKind.Raw:
                            Console.WriteLine("  Adding Raw disk image '{0}'. Filename: '{1}'",
                                diskConfig.Name, parameters["File"].Value<string>());
                            disk = DiskImage.GetDiskImage(parameters["File"].Value<string>());
                            break;
                        case DiskKind.External:
                            Console.WriteLine("  Adding External disk '{0}'", diskConfig.Name);
                            var type = Type.GetType(parameters["Type"].Value<string>());
                            var extDisk = Activator.CreateInstance(type) as ExternalDisk;
                            extDisk.SetParameters(parameters);
                            disk = extDisk;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    if (disk is DiskImage)
                    {
                        bool isLocked = ((DiskImage)disk).ExclusiveLock();
                        if (!isLocked)
                        {
                            Console.WriteLine("Error: Cannot lock the disk image for exclusive access");
                            return;
                        }
                    }
                    disks.Add(disk);
                }
                targets.Add(new ISCSITarget(targetConfig.Name, disks));
            }
            m_server = new ISCSIServer(targets, config.Port, config.Logging?.File);
            ISCSIServer.LogLevel = config.Logging.Level;
            ISCSIServer.LogToConsole = config.Logging.LogToConsole;
            m_server.Start();
            Console.WriteLine("Server started, listening on port {0}", config.Port);
        }

        public static void MainLoop()
        {
            bool exit = false;
            while (true)
            {
                if (m_debug)
                {
                    exit = ProcessCommand();
                }
                else
                {
                    try
                    {
                        exit = ProcessCommand();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unhandled exception: " + ex.ToString());
                    }
                }

                if (exit)
                {
                    break;
                }
            }
        }

        /// <returns>true to exit</returns>
        public static bool ProcessCommand()
        {
            Console.WriteLine();
            Console.Write("iSCSI> ");
            string command = Console.ReadLine();
            string[] args = GetCommandArgsIgnoreEscape(command);
            bool exit = false;
            if (args.Length > 0)
            {
                string commandName = args[0];
                switch (commandName.ToLower())
                {
                    case "attach":
                        AttachCommand(args);
                        break;
                    case "create":
                        CreateCommand(args);
                        break;
                    case "detail":
                        DetailCommand(args);
                        break;
                    case "exit":
                        exit = true;
                        if (m_server != null)
                        {
                            m_server.Stop();
                            m_server = null;
                        }
                        break;
                    case "help":
                        {
                            HelpCommand(args);
                            break;
                        }
                    case "list":
                        ListCommand(args);
                        break;
                    case "offline":
                        OfflineCommand(args);
                        break;
                    case "online":
                        OnlineCommand(args);
                        break;
                    case "select":
                        SelectCommand(args);
                        break;
                    case "set":
                        SetCommand(args);
                        break;
                    case "start":
                        StartCommand(args);
                        break;
                    case "stop":
                        StopCommand(args);
                        break;
                    default:
                        Console.WriteLine("Invalid command. use the 'HELP' command to see the list of commands.");
                        break;
                }
            }
            return exit;
        }

        public static KeyValuePairList<string, string> ParseParameters(string[] args, int start)
        {
            KeyValuePairList<string, string> result = new KeyValuePairList<string, string>();
            for (int index = start; index < args.Length; index++)
            {
                string[] pair = args[index].Split('=');
                if (pair.Length >= 2)
                {
                    string key = pair[0].ToLower(); // we search by the key, so it should be set to lowercase
                    string value = pair[1];
                    value = Unquote(value);
                    result.Add(key, value);
                }
                else
                {
                    result.Add(pair[0].ToLower(), String.Empty);
                }
            }
            return result;
        }

        /// <summary>
        /// Make sure all given parameters are allowed
        /// </summary>
        public static bool VerifyParameters(KeyValuePairList<string, string> parameters, params string[] allowedKeys)
        {
            List<string> allowedList = new List<string>(allowedKeys);
            List<string> keys = parameters.Keys;
            foreach(string key in keys)
            {
                if (!allowedList.Contains(key))
                {
                    return false;
                }
            }
            return true;
        }

        private static int IndexOfUnquotedSpace(string str)
        {
            return IndexOfUnquotedSpace(str, 0);
        }

        private static int IndexOfUnquotedSpace(string str, int startIndex)
        {
            return QuotedStringUtils.IndexOfUnquotedChar(str, ' ', startIndex);
        }

        public static string Unquote(string str)
        {
            string quote = '"'.ToString();
            if (str.StartsWith(quote) && str.EndsWith(quote))
            {
                return str.Substring(1, str.Length - 2);
            }
            else
            {
                return str;
            }
        }

        private static string[] GetCommandArgsIgnoreEscape(string commandLine)
        {
            List<string> argsList = new List<string>();
            int endIndex = IndexOfUnquotedSpace(commandLine);
            int startIndex = 0;
            while (endIndex != -1)
            {
                int length = endIndex - startIndex;
                string nextArg = commandLine.Substring(startIndex, length);
                nextArg = Unquote(nextArg);
                argsList.Add(nextArg);
                startIndex = endIndex + 1;
                endIndex = IndexOfUnquotedSpace(commandLine, startIndex);
            }

            string lastArg = commandLine.Substring(startIndex);
            lastArg = Unquote(lastArg);
            if (lastArg != String.Empty)
            {
                argsList.Add(lastArg);
            }

            return argsList.ToArray();
        }
    }
}
