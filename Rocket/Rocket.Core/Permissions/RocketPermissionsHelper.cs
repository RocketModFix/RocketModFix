using Rocket.API;
using Rocket.API.Serialisation;
using Rocket.Core.Assets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rocket.Core.Permissions
{
    internal class RocketPermissionsHelper
    {
        internal Asset<RocketPermissions> permissions;

        public RocketPermissionsHelper(Asset<RocketPermissions> permissions)
        {
            this.permissions = permissions;
            foreach (RocketPermissionsGroup _Group in this.permissions.Instance.Groups)
            {
                _Group._Members = new HashSet<string>(_Group.Members);
                foreach (Permission perm in _Group.Permissions)
                {
                    _Group._Permissions[perm.Name] = perm;
                }
                this.permissions.Instance.GroupsDict[_Group.Id] = _Group;
            }
        }

        public List<RocketPermissionsGroup> GetGroupsByIds(List<string> ids)
        {
            var groups = new List<RocketPermissionsGroup>();
            foreach (var id in ids)
            {
                var group = GetGroup(id);
                if (group != null)
                {
                    groups.Add(group);
                }
            }
            return groups.OrderBy(x => x.Priority).ToList();
        }

        public List<string> GetParentGroups(string parentGroup, string currentGroup)
        {
            var allGroups = new List<string>();
            RocketPermissionsGroup group = this.GetGroup(parentGroup);

            if (group == null || string.Equals(group.Id, currentGroup, StringComparison.CurrentCultureIgnoreCase)) { return allGroups; }

            allGroups.Add(group.Id);
            allGroups.AddRange(this.GetParentGroups(group.ParentGroup, currentGroup));

            return allGroups;
        }

        /// <summary>
        /// UnturnedPlayer requires Player, so this version of the method does check if you are admined.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="requestedPermissions"></param>
        /// <returns></returns>
        public bool HasPermission(IRocketPlayer player, List<string> requestedPermissions)
        {
            if (player.IsAdmin) { return true; }

            List<Permission> applyingPermissions = this.GetPermissions(player, requestedPermissions);

            return applyingPermissions.Count != 0;
        }

        public bool HasPermission(IRocketPlayer player, HashSet<string> requestedPermissions)
        {
            if (player.IsAdmin) { return true; }

            HashSet<Permission> applyingPermissions = this.GetPermissions(player, requestedPermissions);

            return applyingPermissions.Count != 0;
        }

        /// <summary>
        /// Separates from the usage of IRocketPlayer because it was causing obvious issues due to UnturnedPlayer reliance on Player. Does not check if you are admined.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="requestedPermissions"></param>
        /// <returns></returns>
        public bool HasPermission(string playerId, List<string> requestedPermissions)
        {
            List<Permission> applyingPermissions = this.GetPermissions(playerId, requestedPermissions);

            return applyingPermissions.Count != 0;
        }
        public bool HasPermission(string playerId, HashSet<string> requestedPermissions)
        {
            HashSet<Permission> applyingPermissions = this.GetPermissions(playerId, requestedPermissions);

            return applyingPermissions.Count != 0;
        }

        internal RocketPermissionsGroup GetGroup(string groupId)
        {
            RocketPermissionsGroup Group = null;
            if(!string.IsNullOrEmpty(groupId)) this.permissions.Instance.GroupsDict.TryGetValue(groupId, out Group);
            return Group;
        }

        internal RocketPermissionsProviderResult RemovePlayerFromGroup(string groupId, IRocketPlayer player)
        {
            return RemovePlayerFromGroup(groupId, player.Id);
        }

        internal RocketPermissionsProviderResult RemovePlayerFromGroup(string groupId, string playerId)
        {
            RocketPermissionsGroup g = GetGroup(groupId);
            if (g == null) return RocketPermissionsProviderResult.GroupNotFound;

            if (!g._Members.Contains(playerId)) return RocketPermissionsProviderResult.PlayerNotFound;

            g.Members.Remove(playerId);
            g._Members.Remove(playerId);
            SaveGroup(g);
            return RocketPermissionsProviderResult.Success;
        }

        internal RocketPermissionsProviderResult AddPlayerToGroup(string groupId, IRocketPlayer player)
        {
            return AddPlayerToGroup(groupId, player.Id);
        }

        internal RocketPermissionsProviderResult AddPlayerToGroup(string groupId, string playerId)
        {
            RocketPermissionsGroup g = GetGroup(groupId);
            if (g == null) return RocketPermissionsProviderResult.GroupNotFound;

            if (g._Members.Contains(playerId)) return RocketPermissionsProviderResult.DuplicateEntry;

            g.Members.Add(playerId);
            g._Members.Add(playerId);
            SaveGroup(g);
            return RocketPermissionsProviderResult.Success;
        }

        internal RocketPermissionsProviderResult DeleteGroup(string groupId)
        {
            RocketPermissionsGroup g = GetGroup(groupId);
            if (g == null) return RocketPermissionsProviderResult.GroupNotFound;

            permissions.Instance.Groups.Remove(g);
            permissions.Instance.GroupsDict.Remove(groupId);
            permissions.Save();
            return RocketPermissionsProviderResult.Success;
        }

        internal RocketPermissionsProviderResult SaveGroup(RocketPermissionsGroup group)
        {
            int i = permissions.Instance.Groups.FindIndex(gr => gr.Id == group.Id);
            if (i < 0) return RocketPermissionsProviderResult.GroupNotFound;
            permissions.Instance.Groups[i] = group;
            permissions.Instance.GroupsDict[group.Id] = group;
            permissions.Save();
            return RocketPermissionsProviderResult.Success;
        }

        internal RocketPermissionsProviderResult AddGroup(RocketPermissionsGroup group)
        {
            int i = permissions.Instance.Groups.FindIndex(gr => gr.Id == group.Id);
            if (i != -1) return RocketPermissionsProviderResult.DuplicateEntry;
            permissions.Instance.Groups.Add(group);
            permissions.Instance.GroupsDict[group.Id] = group;
            permissions.Save();
            return RocketPermissionsProviderResult.Success;
        }


        public List<RocketPermissionsGroup> GetGroups(IRocketPlayer player, bool includeParentGroups)
        {
            return GetGroups(player.Id, includeParentGroups);
        }

        public List<RocketPermissionsGroup> GetGroups(string playerId, bool includeParentGroups)
        {
            // get player groups
            List<RocketPermissionsGroup> groups = this.permissions.Instance?.Groups?.OrderBy(x => x.Priority)
                                                      .Where(g => g._Members.Contains(playerId))
                                                      .ToList() ?? new List<RocketPermissionsGroup>();

            // get first default group
            RocketPermissionsGroup defaultGroup = this.GetGroup(this.permissions.Instance.DefaultGroup);

            // if exists, add to player groups
            if (defaultGroup != null) { groups.Add(defaultGroup); }

            // if requested, return list without parent groups
            if (!includeParentGroups) { return groups.Distinct().OrderBy(x => x.Priority).ToList(); }

            // add parent groups
            var parentGroups = new List<RocketPermissionsGroup>();
            groups.ForEach(g => parentGroups.AddRange(this.GetGroupsByIds(this.GetParentGroups(g.ParentGroup, g.Id))));
            groups.AddRange(parentGroups);

            return groups.Distinct().OrderBy(x => x.Priority).ToList();
        }

        public List<Permission> GetPermissions(IRocketPlayer player)
        {
            return GetPermissions(player.Id);
        }

        public List<Permission> GetPermissions(string playerId)
        {
            var result = new List<Permission>();

            List<RocketPermissionsGroup> playerGroups = this.GetGroups(playerId, true);
            playerGroups.Reverse(); // because we need desc ordering

            playerGroups.ForEach(group =>
            {
                group.Permissions.ForEach(permission =>
                {

                    if (permission.Name.StartsWith("-"))
                    {
                        result.RemoveAll(x => string.Equals(x.Name, permission.Name.Substring(1), StringComparison.InvariantCultureIgnoreCase));
                    } 
                    else 
                    {
                        result.RemoveAll(x => x.Name == permission.Name);
                        result.Add(permission);
                    }

                });
            });

            return result.Distinct().ToList();
        }
        public HashSet<Permission> GetPermissionHash(string playerId)
        {
            Dictionary<string, Permission> result = new Dictionary<string, Permission>(StringComparer.OrdinalIgnoreCase);

            List<RocketPermissionsGroup> playerGroups = this.GetGroups(playerId, true);
            playerGroups.Reverse(); // because we need desc ordering

            playerGroups.ForEach(group =>
            {
                group.Permissions.ForEach(permission =>
                {

                    if (permission.Name.StartsWith("-"))
                    {
                        string perm_key = permission.Name.Substring(1);
                        if (result.ContainsKey(perm_key))
                            result.Remove(perm_key);
                    }
                    else
                    {
                        result[permission.Name] = permission;
                    }

                });
            });
            HashSet<Permission> perms = new HashSet<Permission>();
            foreach (string Perm in result.Keys) perms.Add(result[Perm]);
            return perms;
        }
        public Dictionary<string, Permission> GetPermissionDict(string playerId)
        {
            Dictionary<string, Permission> result = new Dictionary<string, Permission>(StringComparer.OrdinalIgnoreCase);

            List<RocketPermissionsGroup> playerGroups = this.GetGroups(playerId, true);
            playerGroups.Reverse(); // because we need desc ordering

            playerGroups.ForEach(group =>
            {
                group.Permissions.ForEach(permission =>
                {

                    if (permission.Name.StartsWith("-"))
                    {
                        string perm_key = permission.Name.Substring(1);
                        if (result.ContainsKey(perm_key))
                            result.Remove(perm_key);
                    }
                    else
                    {
                        result[permission.Name] = permission;
                    }

                });
            });
            return result;
        }

        public List<Permission> GetPermissions(IRocketPlayer player, List<string> requestedPermissions)
        {
            return GetPermissions(player.Id, requestedPermissions);
        }
        public HashSet<Permission> GetPermissions(IRocketPlayer player, HashSet<string> requestedPermissions)
        {
            return GetPermissions(player.Id, requestedPermissions);
        }

        public List<Permission> GetPermissions(string playerId, List<string> requestedPermissions)
        {
            List<Permission> playerPermissions = this.GetPermissions(playerId);

            List<Permission> applyingPermissions = playerPermissions
                .Where(p => requestedPermissions.Exists(x => string.Equals(x, p.Name, StringComparison.InvariantCultureIgnoreCase)))
                .ToList();

            if (playerPermissions.Exists(p => p.Name == "*")) { applyingPermissions.Add(new Permission("*")); }

            playerPermissions.ForEach(p =>
            {

                if (!p.Name.EndsWith(".*")) { return; }

                requestedPermissions.ForEach(requestedPermission =>
                {

                    int dotIndex = requestedPermission.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                    string baseRequested = dotIndex > 0 ? requestedPermission.Substring(0, dotIndex) : requestedPermission;

                    dotIndex = p.Name.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                    string basePlayer = dotIndex > 0 ? p.Name.Substring(0, dotIndex) : p.Name;

                    if (string.Equals(basePlayer, baseRequested, StringComparison.InvariantCultureIgnoreCase)) { applyingPermissions.Add(p); }

                });

            });

            return applyingPermissions.Distinct().ToList();
        }

        public HashSet<Permission> GetPermissions(string playerId, HashSet<string> requestedPermissions)
        {
            // Get player permissions as a HashSet for fast lookups
            HashSet<Permission> playerPermissions = this.GetPermissionHash(playerId);
            HashSet<Permission> applyingPermissions = new HashSet<Permission>();

            // Check if any wildcard permission is present
            if (playerPermissions.Any(p => p.Name == "*"))
            {
                applyingPermissions.Add(new Permission("*"));
            }
            foreach (var permission in playerPermissions)
            {
                if (requestedPermissions.Contains(permission.Name.ToLower()))
                {
                    applyingPermissions.Add(permission);
                }

                // Check for wildcard permissions (e.g., "command.*")
                if (permission.Name.EndsWith(".*", StringComparison.InvariantCultureIgnoreCase))
                {
                    string basePermission = permission.Name.Substring(0, permission.Name.Length - 2); // Remove ".*"

                    foreach (var requestedPermission in requestedPermissions)
                    {
                        // If the base permission matches the requested permission
                        if (requestedPermission.StartsWith(basePermission, StringComparison.InvariantCultureIgnoreCase))
                        {
                            applyingPermissions.Add(permission);
                        }
                    }
                }
            }

            return applyingPermissions; // Return the HashSet of matching permissions
        }

    }

}