using Smod2.API;
using Smod2.Commands;
using Smod2.EventHandlers;
using Smod2.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManager.Events
{
    public class CommandHandler : IEventHandlerRoundEnd, IEventHandlerRoundStart, ICommandHandler
    {
        private bool once_event = false, round_ongoing = false;
        private string queue_event = null;
        public List<Event> Commands { get; } = new List<Event>();
        public void RegisterCommand(Event command)
        {
            if (Commands.Find(x => x.GetName() == command.GetName() || Array.Exists(command.GetCommands(), y => x.GetCommands().Contains(y))) == null)
            { 
                Commands.Add(command);
                PluginHandler.Shared.Info($"Added {command.GetName()} event");
                if(command is IEventHandler)
                    PluginHandler.Shared.AddEventHandlers(command as IEventHandler);
            }
            else
            {
                PluginHandler.Shared.Error($"Couldn't add {command.GetName()}");   
            }
        }

        public void OnRoundEnd(RoundEndEvent ev)
        {
            if (ev.Status == Smod2.API.ROUND_END_STATUS.ON_GOING )
                this.round_ongoing = true;
            else
                this.round_ongoing = false;
            if (this.once_event)
                Commands.ForEach(x => x.isQueue = false);
            Commands.ForEach(x => x.Dispose());
            if (!string.IsNullOrEmpty(queue_event))
            {
                Commands.Find(x => x.GetName() == queue_event).isQueue = true;
            }
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            if (Commands.Exists(e => e.isQueue))
                Commands.Find(e => e.isQueue).EventStart(new RoundStartEvent(ev.Server));
            else return;
            this.round_ongoing = true;
            queue_event = null;
        }

        public string[] OnCall(ICommandSender sender, string[] args)
        {
            if (!string.IsNullOrEmpty(queue_event))
            {
                return new string[] { "Obecnie event jest w poczekalni, spróbuj w następnej rundzie" };
            }

            string command = "";
            string arg = "";

            try
            {
                command = args[0];
                if(arg.Length == 2)
                    arg = args[1];
            }
            catch (Exception exp)
            {
                return new string[] { "Command is incorrect: ", exp.ToString() };
            }
            if (arg == "")
                arg = "once";

            Event commandh = Commands.Find(x => x.GetCommands().Contains(command));
            if (commandh != null)
            {
                if (arg == "on")
                    commandh.isQueue = true;
                else if (arg == "off")
                    commandh.isQueue = false;
                else if (arg == "once")
                {
                    if (round_ongoing)
                    {
                        queue_event = commandh.GetName();
                        return new string[] { "Event will be runned in next round" };
                    }
                    else
                        commandh.isQueue = true;
                    this.once_event = true;
                }
            }
            else return new string[] { $"This event doesn't exist!" };
            return new string[] { $"[{commandh.GetName()}] Event is {arg}" };
        }


        public string GetUsage() => "event <gamemode> <on/off/once>";


        public string GetCommandDescription() => "Runs events/gamemodes";
    }
    public abstract class Event
    {
        public bool isQueue = false;
        public abstract string[] GetCommands();
        public abstract string GetName();
        public abstract void EventStart(RoundStartEvent ev);
        public virtual void Dispose() { return; }
        public virtual IDictionary<string, string> Translation { get; set; }
    }
}
