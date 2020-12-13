using Smod2.API;
using Smod2.Commands;
using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventManager
{
    internal sealed partial class EventHandler : ICommandHandler, IEventHandlerRoundStart, IEventHandlerRoundEnd
    {
        private PluginHandler Plugin { get; }
        private GameEvent NextEvent { set; get; }

        public static Dictionary<string, IDictionary<string, string>> AllTranslations;
        public static Dictionary<string, IDictionary<string, string>> AllConfigs;
        private List<GameEvent> Gamemodes;
        private Dictionary<string,int> Cooldowns;
        private GameEvent Permissions;
        private bool eventOnGoing;
        private bool autoStopEvent;

        private EventHandler() { }
        internal EventHandler(PluginHandler plugin)
        {
            Permissions.Register();
            autoStopEvent = true;
            Plugin = plugin;
            Gamemodes = new List<GameEvent>();
            Cooldowns = new Dictionary<string, int>();
        }



        public void RegisterCommand(GameEvent command, Type type)
        {
            if (Gamemodes.Find(x => x.GetName() == command.GetName() || command.GetCommands().Any(y => x.GetCommands().Contains(y))) == null)
            {
                DetailsAttribute details = null;
                Gamemodes.Add(command);
                if(type.CustomAttributes.Count() > 0)
                    details = (DetailsAttribute)Attribute.GetCustomAttribute(type, typeof(DetailsAttribute));
                if (details != null)
                {
                    if(details.EVENT_MINOR!= Plugin.PLUGIN_MINOR)
                    {
                        string add = string.Empty;
                        if (details.EVENT_MAJOR != Plugin.PLUGIN_MAJOR)
                            add = "REALLY ";
                        Plugin.Logger.Warn("EVENT_LOADER", $"{command.GetName()} is written for {add}outdated version of EventManager!");
                    }
                    if (details.SMOD_MINOR > Smod2.PluginManager.SMOD_MINOR-2)
                    {
                        string add = string.Empty;
                        if (details.SMOD_MAJOR != Smod2.PluginManager.SMOD_MAJOR)
                            add = "REALLY ";
                        Plugin.Logger.Warn("EVENT_LOADER", $"{command.GetName()} is written for {add}outdated version of Smod2!");
                    }
                    Plugin.Logger.Info("EVENT_LOADER", $"Added {command.GetName()} by {details.author}");
                }
                command.Register();
                Plugin.Logger.Info("EVENT_LOADER", $"Added {command.GetName()}");
            }
            else
            {
                Plugin.Logger.Error("EVENT_LOADER", $"Couldn't add {command.GetName()}");
            }
        }

        public void OnRoundEnd(RoundEndEvent ev)
        {
            if (ev.Status != Smod2.API.ROUND_END_STATUS.ON_GOING)
            {
                eventOnGoing = false;
                NextEvent.EventEnd(ev);
                Plugin.EventManager.RemoveEventHandlers(Plugin);
                if(autoStopEvent)
                    NextEvent = null;
                Plugin.AddEventHandlers(this);
            }
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            if (NextEvent != null)
            {
                eventOnGoing = true;
                NextEvent.EventStart(ev);
                if (NextEvent is IEventHandler)
                    Plugin.AddEventHandlers(NextEvent as IEventHandler);
            }
        }

        public string[] OnCall(ICommandSender sender, string[] args)
        {

            string command = args[0];
            string arg = "once";
            string[] access_full = Permissions.Config<string>("access_full").Split(',');
            string[] access_queue = Permissions.Config<string>("access_queue").Split(',');

            Player player = sender as Player;
            bool isQueue = access_queue.Contains(player.GetRankName());
            bool hasFullAccess = access_full.Contains(player.GetRankName());

            if (!hasFullAccess && !isQueue)
                return new string[] { Permissions.Translation("access_denied") };

            if (args.Length == 0)
                return GameList();

            //Checking list or reload
            if (args.Length == 1)
            {
                if (command == "list")
                    return GameList();
                if (command == "refresh")
                {
                    Plugin.ReloadConfigs();
                    return new string[] { Permissions.Translation("configs_are_reloaded") };
                }
            }

            //Checking if changes blocked
            if (args.Length == 2)
            {
                arg = args[1].ToLower();
                if (arg == "off")
                {
                    if (!hasFullAccess)
                        return new string[] { Permissions.Translation("access_denied") };
                    autoStopEvent = true;
                    if (!eventOnGoing)
                        NextEvent = null;
                    return new string[] { string.Format(Permissions.Translation("event_success"), NextEvent.GetName(), arg) };
                }
            }
            if (args.Length > 2)
                return new string[] { Permissions.Translation("invalid_command"), " - event <gamemode>", "- event <gamemode> <on/off/once>" }; return new string[] { Permissions.Translation("invalid_command"), " - event <gamemode>", "- event <gamemode> <on/off/once>" };
#pragma warning disable CS0162
            if (!autoStopEvent)
#pragma warning restore CS0162
                return new string[] { string.Format(Permissions.Translation("event_is_looped"), NextEvent.GetName()) };
            if (eventOnGoing)
                return new string[] { Permissions.Translation("event_is_ongoing") };
            if (NextEvent != null)
                return new string[] { string.Format(Permissions.Translation("event_is_inqueue"), NextEvent.GetName()) };

            //Setting eventnt
            GameEvent commandh = Gamemodes.Find(x => x.GetCommands().Contains(command));
            if (commandh != null)
            {
                if (arg.Contains("on") && !hasFullAccess)
                    return new string[] { Permissions.Translation("access_denied") };
                autoStopEvent = !arg.Contains("on");
                NextEvent = commandh;
            }
            else return new string[] { Permissions.Translation("event_dont_exist") };
            return new string[] { string.Format(Permissions.Translation("event_success"), NextEvent.GetName(), arg) };
        }


        public string GetUsage() => "event <gamemode> <on/off/once>";

        public string GetCommandDescription() => "Runs events/gamemodes";

        public IDictionary<string, IDictionary<string, string>> GetAllDefaultTranslations()
        {
            var data = new Dictionary<string, IDictionary<string, string>>();
            data.Add(Permissions.GetName(), Permissions.DefaultTranslation);
            foreach (GameEvent gamemode in Gamemodes)
            {
                data.Add(gamemode.GetName(), gamemode.DefaultTranslation);
            }
            return data;
        }

        public IDictionary<string, IDictionary<string, string>> GetAllDefaultConfig()
        {
            var data = new Dictionary<string, IDictionary<string, string>>();
            data.Add(Permissions.GetName(), Permissions.DefaultConfig);
            foreach (GameEvent gamemode in Gamemodes)
            {
                data.Add(gamemode.GetName(), gamemode.DefaultConfig);
            }
            return data;
        }

        private string[] GameList()
        {
            List<string> list = new List<string>();
            list.Add(Permissions.Translation("event_list"));
            foreach(GameEvent gamemode in Gamemodes)
            {
                list.Add($"- {gamemode.GetName()} || {string.Join(",", gamemode.GetCommands())}");
            }
            return list.ToArray();
        }
    }
}
