using Smod2;
using Smod2.Events;

using System.Collections.Generic;

namespace EventManager
{
    public abstract class GameEvent
    {
        public abstract void Register();
        public abstract string[] GetCommands();
        public abstract string GetName();
        public abstract void EventStart(RoundStartEvent ev);
        public abstract void EventEnd(RoundEndEvent ev);

        public IDictionary<string, string> DefaultTranslation;
        protected string Translation(string name) 
        {
            if(EventHandler.AllTranslations == null)
                return DefaultTranslation[name];
            if (!EventHandler.AllTranslations.ContainsKey(GetName()))
                return DefaultTranslation[name];
            var translation = EventHandler.AllTranslations[GetName()];
            if (!translation.ContainsKey(name))
                return DefaultTranslation[name];
            return translation[name];
        }
    }
}
