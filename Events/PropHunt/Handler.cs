using EventManager;
using Smod2.API;
using Smod2.Events;
using System.Collections.Generic;

namespace PropHunt
{
    [Details("crawcik", 4, 2, 3, 9, "1.1")]
    class Handler : GameEvent
    {
        #region Settings
        public override void Register()
        {

        }

        public override string[] GetCommands() => new[] { "prop", "hunt", "ph", "prop_hunt" };

        public override string GetName() => "Prop Hunt";
        #endregion

        public override void EventStart(RoundStartEvent ev)
        {
            foreach(Player player in ev.Server.GetPlayers())
            {
                player.GetPlayerEffect(StatusEffect.SCP268).Enable(30f);
            }
        }

        public override void EventEnd(RoundEndEvent ev) { }
    }
}
