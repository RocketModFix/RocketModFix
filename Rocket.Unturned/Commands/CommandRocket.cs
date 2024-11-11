using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Rocket.API;
using Rocket.Core.Plugins;
using Rocket.Core;
using Rocket.Unturned.Chat;
using Rocket.Core.Logging;

namespace Rocket.Unturned.Commands
{
    public class CommandRocket : IRocketCommand
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
            get { return "rocket"; }
        }

        public string Help
        {
            get { return "Reloading Rocket or individual plugins"; }
        }

        public string Syntax
        {
            get { return "<plugins | reload> | <reload | unload | load> <plugin>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "rocket.info", "rocket.rocket" }; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller,
                    $"RocketModFix v{Assembly.GetExecutingAssembly().GetName().Version} for Unturned v{Provider.APP_VERSION}");
                UnturnedChat.Say(caller, "https://github.com/RocketModFix/RocketModFix/");
                UnturnedChat.Say(caller, "https://discord.gg/z6VM7taWeG");
                return;
            }

            if (command.Length == 1)
            {
                switch (command[0].ToLower())
                {
                    case "plugins":
                        if (caller != null && !caller.HasPermission("rocket.plugins")) return;
                        List<IRocketPlugin> plugins = R.Plugins.GetPlugins();
                        UnturnedChat.Say(caller,
                            U.Translate("command_rocket_plugins_loaded",
                                String.Join(", ",
                                    plugins.Where(p => p.State == PluginState.Loaded)
                                        .Select(p => p.GetType().Assembly.GetName().Name).ToArray())));
                        UnturnedChat.Say(caller,
                            U.Translate("command_rocket_plugins_unloaded",
                                String.Join(", ",
                                    plugins.Where(p => p.State == PluginState.Unloaded)
                                        .Select(p => p.GetType().Assembly.GetName().Name).ToArray())));
                        UnturnedChat.Say(caller,
                            U.Translate("command_rocket_plugins_failure",
                                String.Join(", ",
                                    plugins.Where(p => p.State == PluginState.Failure)
                                        .Select(p => p.GetType().Assembly.GetName().Name).ToArray())));
                        UnturnedChat.Say(caller,
                            U.Translate("command_rocket_plugins_cancelled",
                                String.Join(", ",
                                    plugins.Where(p => p.State == PluginState.Cancelled)
                                        .Select(p => p.GetType().Assembly.GetName().Name).ToArray())));
                        break;
                    case "reload":
                        if (caller != null && !caller.HasPermission("rocket.reload")) return;
                        // Many plugins do not support reloading properly, so this command which reloaded all plugins
                        // at once has been disabled by popular request. Reloading individual plugins is still enabled.
                        // https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/1794
                        UnturnedChat.Say(caller, U.Translate("command_rocket_reload_disabled"));
                        R.ReloadCmds();
                        UnturnedChat.Say(caller, U.Translate("command_rocket_reloaded_commands"));
                        break;
                    case "gc":
                        if (caller != null && !caller.HasPermission("rocket.gc")) return;
                        R.Commands.ClearInactiveCooldowns();
                        UnturnedChat.Say(caller, U.Translate("command_rocket_gc"));
                        break;
                }
            }

            if (command.Length == 2)
            {
                var plugin = R.Plugins
                    .GetPlugins()
                    .FirstOrDefault(pl => pl.Name.ToLower().Contains(command[1].ToLower()));
                if (plugin == null)
                {
                    UnturnedChat.Say(caller, U.Translate("command_rocket_plugin_not_found", command[1]));
                    return;
                }
                if (plugin is not RocketPlugin rocketPlugin)
                {
                    // Probably plugin don't want to allow reload/unload/load plugin
                    return;
                }

                var pluginName = rocketPlugin.GetType().Assembly.GetName().Name;
                switch (command[0].ToLower())
                {
                    case "reload":
                        if (caller != null && !caller.HasPermission("rocket.reloadplugin")) return;
                        if (rocketPlugin.State == PluginState.Loaded)
                        {
                            UnturnedChat.Say(caller, U.Translate("command_rocket_reload_plugin", pluginName));
                            try
                            {
                                rocketPlugin.ReloadPlugin();
                                UnturnedChat.Say(caller, U.Translate("command_rocket_reloaded_plugin", pluginName));
                            }
                            catch (Exception ex)
                            {
                                UnturnedChat.Say(caller, U.Translate("command_rocket_reload_plugin_error", pluginName));
                                Logger.LogException(ex, $"An error occured while reloading plugin {pluginName}");
                            }
                        }
                        else
                        {
                            UnturnedChat.Say(caller, U.Translate("command_rocket_not_loaded", pluginName));
                        }

                        break;
                    case "unload":
                        if (caller != null && !caller.HasPermission("rocket.unloadplugin")) return;
                        if (rocketPlugin.State == PluginState.Loaded)
                        {
                            UnturnedChat.Say(caller, U.Translate("command_rocket_unloading_plugin", pluginName));
                            try
                            {
                                rocketPlugin.UnloadPlugin();

                                UnturnedChat.Say(caller, U.Translate("command_rocket_unloaded_plugin", pluginName));
                            }
                            catch (Exception ex)
                            {
                                UnturnedChat.Say(caller, U.Translate("command_rocket_unloading_plugin_error", pluginName));
                                Logger.LogException(ex, $"An error occured while unloading plugin {pluginName}");
                            }
                        }
                        else
                        {
                            UnturnedChat.Say(caller, U.Translate("command_rocket_not_loaded", pluginName));
                        }

                        break;
                    case "load":
                        if (caller != null && !caller.HasPermission("rocket.loadplugin")) return;
                        if (rocketPlugin.State != PluginState.Loaded)
                        {
                            UnturnedChat.Say(caller, U.Translate("command_rocket_load_plugin", pluginName));
                            try
                            {
                                rocketPlugin.LoadPlugin();

                                UnturnedChat.Say(caller, U.Translate("command_rocket_loaded_plugin", pluginName));
                            }
                            catch (Exception ex)
                            {
                                UnturnedChat.Say(caller, U.Translate("command_rocket_loaded_plugin_error", pluginName));
                                Logger.LogException(ex, $"An error occured while loading plugin {pluginName}");
                            }
                        }
                        else
                        {
                            UnturnedChat.Say(caller, U.Translate("command_rocket_already_loaded", pluginName));
                        }

                        break;
                }
            }
        }
    }
}
