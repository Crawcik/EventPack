using Smod2.Commands;
using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventManager
{
    public sealed class EventHandler : ICommandHandler, IEventHandlerRoundStart, IEventHandlerRoundEnd
    {
        public static IDictionary<string, IDictionary<string, string>> AllTranslations { private set; get; }
        private PluginHandler Plugin { get; }
        private Event NextEvent { set; get; }

        private List<Event> Commands;
        private bool eventOnGoing;
        private bool autoStopEvent;

        private EventHandler() { }
        internal EventHandler(PluginHandler plugin)
        {
            Plugin = plugin;
            Commands = new List<Event>();
        }

        public void RegisterCommand(Event command)
        {
            if (Commands.Find(x => x.GetName() == command.GetName() || command.GetCommands().Any(y => x.GetCommands().Contains(y))) == null)
            {
                Plugin.Info($"Added {command.GetName()} event");
                Commands.Add(command);
                //if (AllTranslations.ContainsKey(command.GetName()))

                if (command is IEventHandler)
                    Plugin.AddEventHandlers(command as IEventHandler);
            }
            else
            {
                Plugin.Error($"Couldn't add {command.GetName()}");
            }
        }

        public void OnRoundEnd(RoundEndEvent ev)
        {
            if (ev.Status == Smod2.API.ROUND_END_STATUS.ON_GOING)
                eventOnGoing = true;
            else
            {
                eventOnGoing = false;
                NextEvent.EventEnd(ev);
                if(autoStopEvent)
                    NextEvent = null;
            }
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            if (NextEvent != null)
                NextEvent.EventStart(ev);
            else return;
            eventOnGoing = true;
        }

        public string[] OnCall(ICommandSender sender, string[] args)
        {
            if (eventOnGoing)
            {
                return new string[] { "Event is currently on going, try after this round" };
            }
            if (NextEvent != null)
            {
                return new string[] { "Event is currently in queue, try another time" };
            }

            string command = "";
            string arg = "";

            try
            {
                command = args[0];
                if (arg.Length == 2)
                    arg = args[1];
            }
            catch (Exception exp)
            {
                return new string[] { "Command is incorrect! ", exp.ToString(), "","Try:", "- event <gamemode>", "- event <gamemode> <on/off/once>" };
            }
            if (arg == "")
                arg = "once";

            Event commandh = Commands.Find(x => x.GetCommands().Contains(command));
            if (commandh != null)
            {
                autoStopEvent = arg != "on";
                if(NextEvent == null)
                    NextEvent = commandh;
            }
            else return new string[] { $"This event doesn't exist!" };
            return new string[] { $"[{commandh.GetName()}] Event is {arg}" };
        }


        public string GetUsage() => "event <gamemode> <on/off/once>";

        public string GetCommandDescription() => "Runs events/gamemodes";
    }
}
