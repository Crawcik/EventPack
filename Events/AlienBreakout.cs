using GameCore;
using Smod2.API;
using Smod2.Events;
using System.Linq;

namespace EventManager.Events
{
    public class AlienBreakout : Event
    {
        #region Settings
        public override string[] GetCommands()
        {
            return new string[] { "alien", "breakout" };
        }

        public override string GetName()
        {
            return "Alien Breakout";
        }
        #endregion
        public override void EventStart(RoundStartEvent ev)
        {
            
        }

        private class PlayerExt
        {
            public Player player;
            public int batteryStatus;
        }

    }
}
