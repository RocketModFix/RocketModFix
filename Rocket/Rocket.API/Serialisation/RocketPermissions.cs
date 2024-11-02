using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Rocket.API.Serialisation
{
    [Serializable]
    public class RocketPermissions : IDefaultable
    {
        public RocketPermissions()
        {
            foreach (RocketPermissionsGroup _Group in Groups) GroupsDict[_Group.Id] = _Group;
        }

        [XmlElement("DefaultGroup")]
        public string DefaultGroup = "default";

        [XmlArray("Groups")]
        [XmlArrayItem(ElementName = "Group")]
        public List<RocketPermissionsGroup> Groups = new List<RocketPermissionsGroup>();
        [XmlIgnore]
        public Dictionary<string,RocketPermissionsGroup> GroupsDict = new Dictionary<string,RocketPermissionsGroup>(StringComparer.OrdinalIgnoreCase);
        public void LoadDefaults()
        {
            DefaultGroup = "default";
            Groups = new List<RocketPermissionsGroup> {
                new RocketPermissionsGroup("default","Guest",null, new List<string>() , new List<Permission>() { new Permission("p"),  new Permission("rocket")},"white"),
                new RocketPermissionsGroup("vip","VIP", "default",new List<string>() { "76561198016438091" }, new List<Permission>() {  new Permission("effect"), new Permission("heal",120), new Permission("v",30) },"FF9900")
            };
            foreach (RocketPermissionsGroup _Group in Groups) GroupsDict[_Group.Id] = _Group;
        }
    }
}
