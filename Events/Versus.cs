using Smod2.API;
using Smod2.Commands;
using Smod2.Events;
using Smod2.EventHandlers;

using System.Collections.Generic;
using Smod2.EventSystem.Events;
using System.Linq;

namespace EventManager.Events
{
    class Versus : Event, IEventHandlerRoundStart
    {
        #region Settings
        public Versus()
        {
            this.Translation = PluginHandler.Shared.AllTranslations[GetName()];
        }

        public override string[] GetCommands()
        {
            return new string[] { "event_versus" };
        }

        public override ConsoleType GetCommandType()
        {
            return ConsoleType.RA;
        }

        public override string GetName()
        {
            return "Versus";
        }
        #endregion
        public void OnRoundStart(RoundStartEvent ev)
        {
            if (!isQueue)
                return;
            ev.Server.Map.Broadcast(20, Translation["game_tutorial"], false);
            bool nowNerd = false;
            List<Smod2.API.Door> doors = ev.Server.Map.GetDoors();
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_A").Locked = true;
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_B").Locked = true;
            Player[] players = ev.Server.GetPlayers().ToArray();
            
            foreach (Player player in players)
            {
                if (nowNerd)
                {
                    player.ChangeRole(Smod2.API.RoleType.SCIENTIST);
                    player.GiveItem(Smod2.API.ItemType.USP);
                }
                else
                {
                    player.ChangeRole(Smod2.API.RoleType.CLASSD);
                    player.GiveItem(Smod2.API.ItemType.MEDKIT);
                    player.GiveItem(Smod2.API.ItemType.USP);
                }
                nowNerd = !nowNerd;
            }
        }
    }
}
