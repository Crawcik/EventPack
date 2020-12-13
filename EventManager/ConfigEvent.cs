using Smod2.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager
{
    internal class ConfigEvent : GameEvent
    {
        #region Useless
        public override void EventEnd(RoundEndEvent ev)
        {
            throw new NotImplementedException();
        }

        public override void EventStart(RoundStartEvent ev)
        {
            throw new NotImplementedException();
        }

        public override string[] GetCommands()
        {
            throw new NotImplementedException();
        }
        #endregion

        public override string GetName() => "Permissions";

        public override void Register()
        {
            DefaultConfig = new Dictionary<string, string>()
            {
                { "access_full", "owner, admin" },
                { "access_queue", "owner, admin, moderator" },
                { "queue_cooldown", "0" }
            };
            DefaultTranslation = new Dictionary<string, string>()
            {
                { "event_success", "[{0}] Event is {1}" },
                { "event_is_looped", "Event {0} is set to always on!" },
                { "event_is_ongoing", "Event is currently on going, try after this round" },
                { "event_is_inqueue", "{0} is currently in queue, try another time" },
                { "event_dont_exist", "This event doesn't exist!" },
                { "event_list", "Avalible gamemodes:" },
                { "access_denied", "You don't have permission to do this!" },
                { "cooldown_alert", "You'll be able to use this after {0} round" },
                { "invalid_command", "Command is incorrect! Try:" },
                { "configs_are_reloaded", "Events configs are reloaded" },
                { "cooldown", "0" }
            };
        }
    }
}
