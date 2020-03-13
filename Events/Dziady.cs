using System;
using System.Linq;
using System.Collections.Generic;

using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;

namespace EventManager.Events
{
    public class Dziady : Event, IEventHandlerRoundStart, IEventHandlerTeamRespawn
    {
        private PluginHandler plugin;

        #region Settings

        public Dziady(PluginHandler plugin)
        {
            this.Translation = plugin.AllTranslations[GetName()];
            this.plugin = plugin;
        }
        public override string[] GetCommands()
        {
            return new string[] { "event_dziady" };
        }

        public override ConsoleType GetCommandType()
        {
            return ConsoleType.RA;
        }

        public override string GetName()
        {
            return "Dziady";
        }
        #endregion

        public void OnRoundStart(RoundStartEvent ev)
        {
            if (!isQueue)
                return;
            

            plugin.CommandManager.CallCommand(plugin.Server, "bc", new string[] { "5", Translation["start"] });
            Smod2.API.Door gate_a = plugin.Server.Map.GetDoors().Find(x => x.Name == "GATE_A");
            gate_a.Open = true;
            gate_a.Locked = true;
            Player[] scps = ev.Server.GetPlayers().Where(x => x.TeamRole.Team == Smod2.API.Team.SCP).ToArray();
            foreach (Player scp in scps)
            {
                scp.ChangeRole(Smod2.API.Role.SCP_049);
                scp.PersonalBroadcast(30, Translation["scp049_start"], false);
            }
        }

        public void OnTeamRespawn(TeamRespawnEvent ev)
        {
            if (!isQueue)
                return;
            ev.SpawnChaos = true;
            Player[] players = ev.PlayerList.ToArray();
            ev.PlayerList.Clear();
            foreach (Player zombie in players)
            {
                var rand = new Random();
                plugin.Info(zombie.PlayerId.ToString());
                zombie.ChangeRole(Smod2.API.Role.SCP_049_2);
                zombie.Teleport(plugin.Server.Map.GetSpawnPoints(Smod2.API.Role.CHAOS_INSURGENCY)[rand.Next(0, 5)]);
            }

            plugin.Server.Map.Broadcast(10, Translation["zombie_spawn"], false);
            plugin.Server.Map.AnnounceCustomMessage("Dead Is Alive . . Destroy Every 1", false);
        }
    }
}
