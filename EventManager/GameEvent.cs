using System.Collections.Generic;

namespace EventManager
{
    public abstract class GameEvent
    {
        public abstract void Register();
        public abstract string[] GetCommands();
        public abstract string GetName();
        public abstract void EventStart(Smod2.Events.RoundStartEvent ev);
        public abstract void EventEnd(Smod2.Events.RoundEndEvent ev);

        public IDictionary<string, string> DefaultTranslation;
        public IDictionary<string, string> DefaultConfig;

        protected string Translation(string key) => GetKey(key, EventHandler.AllTranslations);
        protected T Config<T>(string key) => (T)System.Convert.ChangeType(GetKey(key, EventHandler.AllConfigs), typeof(T));

        private string GetKey(string key, Dictionary<string, IDictionary<string,string>> data)
        {
            if (data == null)
                return DefaultTranslation[key];
            if (!data.ContainsKey(GetName()))
                return DefaultTranslation[key];
            var table = data[GetName()];
            if (!table.ContainsKey(key))
                return DefaultTranslation[key];
            return table[key];
        }
    }
}
