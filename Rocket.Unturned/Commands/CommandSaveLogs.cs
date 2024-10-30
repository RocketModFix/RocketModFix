using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using UnityEngine;
using RocketLogger = Rocket.Core.Logging.Logger;

namespace Rocket.Unturned.Commands
{

    public class CommandSaveLogs : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Both;
            }
        }

        public string Name
        {
            get { return "savelogs"; }
        }

        public string Help
        {
            get { return "Saves Rocket and other logs for faster and easier problem solving"; }
        }

        public string Syntax
        {
            get { return ""; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "rocket.savelogs" }; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var workingDirectory = ReadWrite.PATH;
            var serverId = Dedicator.serverID;
            var serverDirectory = Path.Combine(workingDirectory, "Servers", serverId);

            var saveLogsDirectory = Path.Combine(serverDirectory, "SaveLogs");

            var logId = $"savelogs-{((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString()}";
            var saveLogDirectory = Path.Combine(saveLogsDirectory, logId);

            var logFiles = GetLogFiles();
            if (logFiles.Count == 0)
            {
                UnturnedChat.Say(caller, "No one log were found.", Color.red);
                return;
            }

            Directory.CreateDirectory(saveLogDirectory);
            if (Directory.Exists(saveLogsDirectory) == false)
            {
                Directory.CreateDirectory(saveLogsDirectory);
            }

            foreach (var logFilePath in logFiles)
            {
                if (File.Exists(logFilePath) == false)
                {
                    RocketLogger.LogError("Log file not found: " + logFilePath);
                    continue;
                }

                try
                {
                    File.Copy(logFilePath, Path.Combine(saveLogDirectory, Path.GetFileName(logFilePath)));
                    UnturnedChat.Say($"Saved {logFilePath} to {saveLogDirectory}", Color.yellow);
                }
                catch (Exception ex)
                {
                    RocketLogger.LogException(ex, $"An error occured while saving {logFilePath} to {saveLogDirectory}");
                }
            }

            var logArchivePath = Path.Combine(saveLogsDirectory, $"{logId}.zip");
            ZipFile.CreateFromDirectory(saveLogDirectory, logArchivePath);

            UnturnedChat.Say(caller, $"Done! Your logs have been saved to: {logArchivePath}", Color.green);
        }

        private static IReadOnlyCollection<string> GetLogFiles()
        {
            var files = new List<string>();

            var logFilePath = Logs.getLogFilePath();
            files.Add(logFilePath);

            var prevLogFilePath = Path.Combine(Path.GetDirectoryName(logFilePath)!, Path.GetFileNameWithoutExtension(logFilePath) + "_Prev.log");
            files.Add(prevLogFilePath);

            return files;
        }
    }
}