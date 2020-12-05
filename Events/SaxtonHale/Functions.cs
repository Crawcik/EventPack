using Smod2;
using Smod2.API;

using System;
using System.Reflection;

using UnityEngine;
using Mirror;

namespace SaxtonHale
{
    public static class Functions
    {
        public static void SetHitboxScale(GameObject target,float x, float y, float z)
        {
            try
            {
                NetworkIdentity component = target.GetComponent<NetworkIdentity>();
                target.transform.localScale = new Vector3(1f * x, 1f * y, 1f * z);
                ObjectDestroyMessage objectDestroyMessage = default(ObjectDestroyMessage);
                objectDestroyMessage.netId = component.netId;
                foreach (Player player in PluginManager.Manager.Server.GetPlayers())
                {
                    GameObject gameObject = (GameObject)player.GetGameObject();
                    NetworkConnection connectionToClient = gameObject.GetComponent<NetworkIdentity>().connectionToClient;
                    if (gameObject != target)
                    {
                        connectionToClient.Send<ObjectDestroyMessage>(objectDestroyMessage, 0);
                    }
                    object[] param = new object[]
                    {
                        component,
                        connectionToClient
                    };
                    typeof(NetworkServer).InvokeStaticMethod("SendSpawnMessage", param);
                }
            }
            catch (Exception)
            {

            }
        }
        public static void InvokeStaticMethod(this Type type, string methodName, object[] param)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            MethodInfo method = type.GetMethod(methodName, bindingAttr);
            if (method == null)
            {
                return;
            }
            method.Invoke(null, param);
        }
    }
}
