using System.Collections.Generic;
using System.Linq;
using System.Text;
using ValheimRcon.Commands.Modification;
using ValheimRcon.Commands.Search;
using ValheimRcon.ZDOInfo;

namespace ValheimRcon.Commands
{
    internal class ModifyObject : RconCommand
    {
        public override string Command => "modifyObject";

        public override string Description => "Modify properties of an object. " +
            "Usage (with required and optional arguments): modifyObject " +
            "<id:userid> " +
            "-position <x> <y> <z> " +
            "-rotation <x> <y> <z> " +
            "-health <value> " +
            "-tag <tag> " +
            "-prefab <prefab name> " +
            "-force (bypass security checks)";

        private readonly List<IZdoModification> _modifications = new List<IZdoModification>();
        private readonly StringBuilder builder = new StringBuilder();

        protected override string OnHandle(CommandArgs args)
        {
            var id = args.GetObjectId(0);
            var idCriteria = new IdCriteria(id);
            var zdo = ZDOMan.instance.m_objectsByID.Values.FirstOrDefault(idCriteria.IsMatch);

            if (zdo == null)
            {
                return "No object found";
            }

            if (!zdo.Persistent)
            {
                return "Object is not persistent and cannot be modified.";
            }

            var optionalArgs = args.GetOptionalArguments();
            var force = false;
            var targetPrefabName = string.Empty;
            _modifications.Clear();
            foreach (var (index, argument) in optionalArgs)
            {
                switch (argument)
                {
                    case "-position":
                        _modifications.Add(new PositionModification(args.GetVector3(index + 1)));
                        break;
                    case "-rotation":
                        _modifications.Add(new RotationModification(args.GetVector3(index + 1)));
                        break;
                    case "-health":
                        _modifications.Add(new HealthModification(args.GetFloat(index + 1)));
                        break;
                    case "-tag":
                        _modifications.Add(new TagModification(args.GetString(index + 1)));
                        break;
                    case "-prefab":
                        targetPrefabName = args.GetString(index + 1);
                        _modifications.Add(new PrefabModification(targetPrefabName));
                        break;
                    case "-force":
                        force = true;
                        break;
                    default:
                        return $"Unknown argument: {args.GetString(index)}";
                }
            }

            if (!_modifications.Any())
            {
                return "At least one valid modification argument must be provided.";
            }

            if (!force && !ZdoUtils.CanModifyZdo(zdo))
            {
                return "Object cannot be modified.";
            }

            var owner = zdo.GetOwner();
            var ownerPlayer = ZNet.instance.GetPeer(owner);

            if (!force && ownerPlayer != null)
            {
                return $"Object is owned by an online player {ownerPlayer.GetPlayerInfo()} and cannot be modified.";
            }

            if (!force
                && !string.IsNullOrEmpty(targetPrefabName)
                && !ZNetScene.instance.HasPrefab(targetPrefabName.GetStableHashCode()))
            {
                return $"Cannot find prefab with name {targetPrefabName}. If you know what you are doing, use -force option.";
            }

            foreach (var modification in _modifications)
            {
                modification.Apply(zdo);
            }
            zdo.SetZdoModified();

            builder.Clear();
            builder.AppendLine("Object modified successfully");
            builder.AppendFormat("{0} ", zdo.GetPrefabName());
            ZDOInfoUtil.AppendInfo(zdo, builder);
            return builder.ToString().TrimEnd();
        }
    }
}
