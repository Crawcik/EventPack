using EventManager;

using Smod2.API;
using Smod2.Events;

using System.Collections.Generic;

namespace Versus
{
    public class Handler : GameEvent
    {
        #region Settings
        public Handler()
        {
            DefaultTranslation = new Dictionary<string, string>()
            {
                { "game_tutorial", "Event D-Class v.s. Scientists || Everyone has a gun || Checkpoints are locked" }
            };
        }

        public override string[] GetCommands() => new[] { "versus" };

        public override string GetName() => "Versus";
        #endregion

        public override void EventStart(RoundStartEvent ev)
        {
            ev.Server.Map.Broadcast(20, Translation("game_tutorial"), false);
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

        public override void EventEnd(RoundEndEvent ev) { }
    }
}
