using Smod2.API;
using Smod2.Events;

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
            ev.Server.Map.OverchargeLights(20f, false);
        }

        private class PlayerExt
        {
            public Player player;
            public int batteryStatus;
        }

    }
}
