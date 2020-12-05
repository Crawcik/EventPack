using EventManager;

using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Collections.Generic;

namespace SaxtonHale
{
    class Handler : GameEvent, IEventHandlerCheckRoundEnd, IEventHandlerSpawn
    {
        public Dictionary<string, int> QueuePoints;
        private Boss boss;
        public int boss_type_num;

        #region Settings
        public override void Register()
        {
            QueuePoints = new Dictionary<string, int>();
            DefaultTranslation = new Dictionary<string, string>()
            {
                { "tutorial", "<size=20>Janitor card - RAGE, MTF card - Taunt, O5 card - SPECJAL</size>" },
                { "hale_spawn", "%nick% became %class% | %hp%HP" }
            };
        }

        public override string[] GetCommands() => new[] { "saxton", "hale", "sh" };

        public override string GetName() => "Saxton Hale";
        #endregion

        public override void EventStart(RoundStartEvent ev)
        {
            ev.Server.Map.GetElevators().ForEach(x => x.Locked = true);
            //Selecting player
            Player most_player = null;
            int most_points = 0;
            ev.Server.GetPlayers().ForEach(player =>
            {
                if (!QueuePoints.ContainsKey(player.UserId))
                    QueuePoints.Add(player.UserId, 0);
                if (QueuePoints[player.UserId] >= most_points)
                {
                    most_player = player;
                    most_points = QueuePoints[player.UserId];
                }
                QueuePoints[player.UserId]++;
                player.ChangeRole(Smod2.API.RoleType.NTF_LIEUTENANT);
                player.GetInventory().ForEach(x => x.Remove());
                player.GiveItem(Smod2.API.ItemType.GUN_E11_SR);
                player.GiveItem(Smod2.API.ItemType.ADRENALINE);
                player.SetAmmo(AmmoType.DROPPED_5, 1000);
            });
            QueuePoints[most_player.UserId] = 0;

            //Setting boss
            if (boss_type_num == Enum.GetNames(typeof(Class)).Length)
                boss_type_num = 0;
            this.boss = new Boss((Class)boss_type_num, most_player, Translation("tutorial"));
            boss_type_num++;
            int count = ev.Server.GetPlayers().Count;
            this.boss.player.HP = count * (400 + (count *20));
            most_player = null;
        }

        public override void EventEnd(RoundEndEvent ev)
        {
            boss = null;
        }

        public void OnCheckRoundEnd(CheckRoundEndEvent ev)
        {
            if (boss == null)
                return;
            if (ev.Status == ROUND_END_STATUS.ON_GOING)
            {
                ev.Server.Map.ClearBroadcasts();
                string message = Translation("hale_spawn");
                message = message.Replace("%nick%", boss.player.Name);
                message = message.Replace("%class%", boss.role.ToString());
                message = message.Replace("%hp%", boss.player.HP.ToString());
                ev.Server.Map.Broadcast(2, message, false);
            }
            else
            {
                if (boss.player.TeamRole.Role != Smod2.API.RoleType.CHAOS_INSURGENCY)
                {
                    boss.EndTask();
                    boss = null;
                }
            }
        }

        public void OnSpawn(PlayerSpawnEvent ev)
        {
            if (ev.Player.TeamRole.Role == Smod2.API.RoleType.CLASSD)
                ev.Player.ChangeRole(Smod2.API.RoleType.SPECTATOR);
        }
    }
}
