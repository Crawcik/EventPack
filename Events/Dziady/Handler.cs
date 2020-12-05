using EventManager;

using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dziady
{
    public class Handler : GameEvent, IEventHandlerTeamRespawn
    {
        #region Settings
        public override void Register()
        {
            DefaultTranslation = new Dictionary<string, string> {
                { "start", "Event 'Dziady' starts" },
                { "scp049_start", "You're a priest" },
                { "zombie_spawn", "The Dead rose from their graves!" }
            };
        }

        public override string[] GetCommands() => new[] { "dziady" };

        public override string GetName() => "Dziady";
        #endregion

        public override void EventStart(RoundStartEvent ev)
        {
            ev.Server.Map.Broadcast(5, Translation("start"), false);
            Door gate_a = ev.Server.Map.GetDoors().Find(x => x.Name == "GATE_A");
            gate_a.Open = true;
            gate_a.Locked = true;
            Player[] scps = ev.Server.GetPlayers().Where(x => x.TeamRole.Team == Smod2.API.TeamType.SCP).ToArray();
            foreach (Player scp in scps)
            {
                scp.ChangeRole(RoleType.SCP_049);
                scp.PersonalBroadcast(30, Translation("scp049_start"), false);
            }
        }

        public override void EventEnd(RoundEndEvent ev) { }

        public void OnTeamRespawn(TeamRespawnEvent ev)
        {
            ev.SpawnChaos = true;
            SpawnZombie(ev.PlayerList.ToArray()).GetAwaiter();
        }

        public async Task SpawnZombie(Player[] players)
        {
            await Task.Delay(500);
            foreach(Player zombie in players)
            {
                zombie.ChangeRole(RoleType.SCP_049_2);
                zombie.Teleport(PluginManager.Manager.Server.Map.GetSpawnPoints(RoleType.CHAOS_INSURGENCY)[0]);
            }
            PluginManager.Manager.Server.Map.Broadcast(10, Translation("zombie_spawn"), false);
            PluginManager.Manager.Server.Map.AnnounceCustomMessage("Dead Is Alive . . Destroy Every 1");
        }
    }
}
