using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ValheimRcon.Commands
{
    internal class MoveObjectById : RconCommand
    {
        public override string Command => "moveObjectById";

        public override string Description => "[WIP: Currently requires scene reload] Move an object by its user and object ids (which together make up a ZDO.m_uid). Usage: moveObjectById <userId> <objectId> <x> <y> <x> <rx> <ry> <rz>";

        protected override string OnHandle(CommandArgs args)
        {
            // TODO: Not working client side until scene is reloaded
            var userId = args.GetLong(0);
            var objectId = args.GetLong(1);
            var objects = ZDOMan.instance.m_objectsByID.Values
                .Where(obj => obj.m_uid.UserID == userId && obj.m_uid.ID == objectId)
                .ToArray();

            if (objects.Length == 0)
            {
                return $"No objects found for m_uid {userId}:{objectId}";
            }
            var zdo = objects[0];

            var position = new Vector3(
                args.GetFloat(2),
                args.GetFloat(3),
                args.GetFloat(4)
            );

            var rotation = Quaternion.Euler(
                args.GetFloat(5),
                args.GetFloat(6),
                args.GetFloat(7)
            );

            var sb = new StringBuilder();
            sb.AppendLine($"Moving object with m_uid {userId}:{objectId}:");
            sb.Append($"- Prefab: {ZdoUtils.GetPrefabName(zdo.GetPrefab())}");
            ZdoUtils.AppendZdoStats(zdo, sb);
            sb.AppendLine();
            /*
            GameObject go = ZNetScene.instance.FindInstance(zdo.m_uid);
            if (go != null)
            {
                go.transform.SetPositionAndRotation(position, rotation);
                var sync = go.GetComponent<ZSyncTransform>();
                if (sync != null)
                {
                    sb.AppendLine("- Using ZSyncTransform to: ");
                    sync.SyncNow();
                }
                else
                {
                    // Fallback if no ZSyncTransform — force reload
                    sb.AppendLine("- Falling back to destroy then create at: ");
                    ZNetScene.instance.Destroy(go);
                    ZNetScene.instance.CreateObject(zdo);
                }
            }
            else
            {
                sb.AppendLine("- Game object not found, setting position and rotation directly on ZDO then recreating: ");
                zdo.SetPosition(position);
                zdo.SetRotation(rotation);
                ZNetScene.instance.Destroy(go);
                ZNetScene.instance.CreateObject(zdo);
            }*/
            //ZDOMan.instance.ForceSendZDO(zdo.m_uid);
            //ZDOMan.instance.ClientChanged(zdo.m_uid);
            //ZDOMan.instance.AddForceSendZdos()
            //foreach (ZDOMan.ZDOPeer peer in ZDOMan.instance.m_peers) {
            //    ZDOMan.instance.AddForceSendZdos(peer, new List<ZDO> { zdo });
            //}
            zdo.SetPosition(position);
            zdo.SetRotation(rotation);
            zdo.IncreaseDataRevision();
            zdo.DataRevision += 120;
            ZDOMan.instance.ClientChanged(zdo.m_uid);
            ZDOMan.instance.ForceSendZDO(zdo.m_uid);
            sb.Append("- To new location: ");
            ZdoUtils.AppendZdoStats(zdo, sb);
            sb.AppendLine();
            return sb.ToString().Trim();
        }
    }
}
