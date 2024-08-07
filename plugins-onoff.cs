using System;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;

namespace PluginsOnOff
{
    public partial class PluginsOnOff : BasePlugin
    {
        public override string ModuleName => "PluginsManagement";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "Sergey";
        public override string ModuleDescription => "Plugins Management: enable/disable";
        string pluginFolder = "";
        string disabledFolder = "";
        public override void Load(bool hotReload)
        {
            pluginFolder = Server.GameDirectory + "/csgo/addons/counterstrikesharp/plugins";
            disabledFolder = Server.GameDirectory + "/csgo/addons/counterstrikesharp/plugins/disabled";
        }
        private bool MovePluginToFolder(string pluginName, string pluginFolder, string destinationFolder)
        {
            try
            {
                string folder = destinationFolder + "/" +pluginName;
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                // Move each file to the disabled folder, overwriting if exists
                bool f1 = MoveFileToFolder(pluginName, "dll", pluginFolder, folder, true);
                bool f2 = MoveFileToFolder(pluginName, "pdb", pluginFolder, folder, true);
                bool f3 = MoveFileToFolder(pluginName, "deps.json", pluginFolder, folder, true);
                if (f1 && f2 && f3)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disabling plugin '{pluginName}': {ex.Message}");
                return false;
            }
        }

        private bool MoveFileToFolder(string pluginName, string extension, string sourceFolder, string destinationFolder, bool overwrite)
        {
            string sourcePath = Path.Combine(sourceFolder, $"{pluginName}/{pluginName}.{extension}");
            string destinationPath = Path.Combine(destinationFolder, $"{pluginName}.{extension}");

            if (File.Exists(sourcePath))
            {
                try
                {
                    File.Move(sourcePath, destinationPath, overwrite);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error moving file '{sourcePath}' to '{destinationPath}': {ex.Message}");
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"File '{pluginName}.{extension}' not found in the source folder.");
                Logger.LogInformation($"File '{pluginName}.{extension}' not found in the source folder.");
                return false;
            }
        }
        
        [ConsoleCommand("plugins", "Plugins management")]
        [CommandHelper(whoCanExecute: CommandUsage.SERVER_ONLY)]
        public void PluginsCommand(CCSPlayerController? caller, CommandInfo command)
        {
            if (command.ArgCount < 3) 
            { 
                Console.WriteLine("Usage: plugins <enable/disable> <plugin_name>");
                return; 
            }
            string pluginName = command.GetArg(2);
            bool result;
            if (command.GetArg(1) == "enable")
            {
                result = MovePluginToFolder(pluginName, disabledFolder, pluginFolder);
                if (result)
                {
                    Server.ExecuteCommand($"css_plugins load {pluginName}");
                    Console.WriteLine($"Plugin '{pluginName}' enabled");
                    Logger.LogInformation($"Plugin '{pluginName}' enabled");
                }
                else
                {
                    Console.WriteLine($"Error in enabling the '{pluginName}' plugin");
                    Logger.LogInformation($"Error in enabling the '{pluginName}' plugin");
                }

            }
            else if (command.GetArg(1) == "disable")
            {
                result = MovePluginToFolder(pluginName, pluginFolder, disabledFolder);
                if (result)
                {
                    Console.WriteLine($"Plugin '{pluginName}' disabled");
                    Logger.LogInformation($"Plugin '{pluginName}' disabled");
                }
                else
                {
                    Console.WriteLine($"Error in disabling the '{pluginName}' plugin");
                    Logger.LogInformation($"Error in disabling the '{pluginName}' plugin");
                }
            }
            else
            {
                Console.WriteLine("Usage: plugins <enable/disable> <plugin_name>");
            }
        }
    }
}

 
