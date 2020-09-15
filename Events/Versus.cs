using Smod2.API;
using Smod2.Commands;
using Smod2.Events;

using System.Collections.Generic;

namespace EventManager.Events
{
    class Versus : Event
    {
        #region Settings

        public override string[] GetCommands()
        {
            return new string[] { "versus" };
        }

        public override string GetName()
        {
            return "Versus";
        }
        #endregion
        public override void EventStart(RoundStartEvent ev)
        {
            if (!isQueue)
                return;
            ev.Server.Map.Broadcast(20, Translation["game_tutorial"], false);
            bool nowNerd = false;
            List<Door> doors = ev.Server.Map.GetDoors();
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_A").Locked = true;
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_B").Locked = true;
            Player[] players = ev.Server.GetPlayers().ToArray();
            
            foreach (Player player in players)
            {
                if (nowNerd)
                {
                    player.ChangeRole(RoleType.SCIENTIST);
                    player.SetAmmo(AmmoType.AMMO9MM, 30);
                    player.GiveItem(ItemType.USP);
                }
                else
                {
                    player.ChangeRole(RoleType.CLASSD);
                    player.GiveItem(ItemType.MEDKIT);
                    player.SetAmmo(AmmoType.AMMO9MM, 30);
                    player.GiveItem(ItemType.USP);
                }
                nowNerd = !nowNerd;
            }
        }
    }
}
