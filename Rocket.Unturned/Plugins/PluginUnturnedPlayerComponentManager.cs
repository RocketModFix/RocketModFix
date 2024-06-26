﻿using Rocket.API;
using Rocket.API.Extensions;
using Rocket.Core.Logging;
using Rocket.Core.Utils;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Rocket.Unturned.Plugins
{
    public sealed class PluginUnturnedPlayerComponentManager : MonoBehaviour
    {
        private Assembly assembly;
        private List<Type> unturnedPlayerComponents = new List<Type>();

        private void OnDisable()
        {
            try
            {
                U.Events.OnPlayerConnected -= addPlayerComponents;
                unturnedPlayerComponents = unturnedPlayerComponents.Where(p => p.Assembly != assembly).ToList();
                List<Type> playerComponents = RocketHelper.GetTypesFromParentClass(assembly, typeof(UnturnedPlayerComponent));
                for (var i = 0; i < playerComponents.Count; i++)
                {
                    var playerComponent = playerComponents[i];
                    var clients = Provider.clients;
                    for (var j = 0; j < clients.Count; j++)
                    {
                        var steamPlayer = clients[j];
                        steamPlayer.player.gameObject.TryRemoveComponent(playerComponent.GetType());
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Logging.Logger.LogException(ex, $"An error occured while removing {nameof(UnturnedPlayerComponent)}");
            }
        }

        private void OnEnable()
        {
            try
            {
                IRocketPlugin plugin = GetComponent<IRocketPlugin>();
                assembly = plugin.GetType().Assembly;

                U.Events.OnBeforePlayerConnected += addPlayerComponents;
                unturnedPlayerComponents.AddRange(RocketHelper.GetTypesFromParentClass(assembly, typeof(UnturnedPlayerComponent)));

                for (var i = 0; i < unturnedPlayerComponents.Count; i++)
                {
                    var playerComponent = unturnedPlayerComponents[i];
                    Core.Logging.Logger.Log("Adding UnturnedPlayerComponent: " + playerComponent.Name);
                    var clients = Provider.clients;
                    for (var j = 0; j < clients.Count; j++)
                    {
                        var steamPlayer = clients[j];
                        steamPlayer.player.gameObject.TryAddComponent(playerComponent.GetType());
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Logging.Logger.LogException(ex, $"An error occured while adding {nameof(UnturnedPlayerComponent)}");
            }
        }

        private void addPlayerComponents(IRocketPlayer p)
        {
            for (int i = 0; i < unturnedPlayerComponents.Count; i++)
            {
                var component = unturnedPlayerComponents[i];
                ((UnturnedPlayer)p).Player.gameObject.AddComponent(component);
            }
        }
    }
}