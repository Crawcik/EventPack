using Smod2.Commands;
using Smod2.EventHandlers;
using Smod2.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManager.Events
{
    public class CommandHandler : IEventHandlerAdminQuery, IEventHandlerRoundEnd
    {
        private PluginHandler plugin;
        Dictionary<string, bool> user_quered = new Dictionary<string, bool>();
        private bool once_event = false;

        public CommandHandler(PluginHandler plugin)
        {
            this.plugin = plugin;
        }
        public List<Event> Commands { get; } = new List<Event>();
        public void RegisterCommand(Event command)
        {
            if (Commands.Find(x => x.GetName() == command.GetName() || Array.Exists(command.GetCommands(), y => x.GetCommands().Contains(y))) == null)
            { 
                Commands.Add(command);
                plugin.Info($"Added {command.GetName()} command with {command.GetCommandType()} type");
                plugin.AddEventHandlers(command as IEventHandler);
            }
            else
            {
                plugin.Error($"Couldn't add {command.GetName()}");   
            }
        }
        

        public void OnAdminQuery(AdminQueryEvent ev)
        {
            string command = null ;
            string arg = null;

            if (ev.Admin.Permissions <= 0)
                return;
            try
            {
                command = ev.Query.Split(' ')[0];
                arg = ev.Query.Split(' ')[1];
            }
            catch
            {}

            if (ev.Admin.Permissions < 3)
            {
                if (! this.user_quered.ContainsKey(ev.Admin.UserId) )
                {
                    if (this.user_quered[ev.Admin.UserId] == true)
                        arg = "once";
                    else
                        return;
                }
                else
                {
                    this.user_quered.Add(ev.Admin.UserId, true);
                    arg = "once";
                }
            }
            else
            { 
                if (arg == null)
                    arg = "once";
            }

            Event commandh = Commands.Find(x => x.GetCommands().Contains(command));
            if (commandh != null)
            {
                if (commandh.GetCommandType() == ConsoleType.RA)
                {
                    ev.Handled = true;
                    ev.Admin.SendConsoleMessage($"[{commandh.GetName()}] Tryb jest {arg}" + Environment.NewLine);
                    if (arg == "on")
                        commandh.isQueue = true;
                    else if (arg == "off")
                        commandh.isQueue = false;
                    else if (arg == "once")
                    {
                        commandh.isQueue = true;
                        this.once_event = true;
                    }
                    ev.Output = "Check Console";
                    ev.Successful = true;
                    if(ev.Admin.Permissions < 3)
                        if (this.user_quered[ev.Admin.UserId] == true)
                            GetTime(ev.Admin.UserId).GetAwaiter();
                }
            }
        }

        public async Task GetTime(string userId)
        {
            this.user_quered[userId] = false;
            await Task.Delay(TimeSpan.FromHours(2));
            this.user_quered[userId] = true;
        }

        public void OnRoundEnd(RoundEndEvent ev)
        {
            if (this.once_event)
                Commands.ForEach(x => x.isQueue = false);
            Commands.ForEach(x => x.Dispose());
        }
    }
    public abstract class Event
    {
        public bool isQueue = false;
        public abstract string[] GetCommands();
        public abstract ConsoleType GetCommandType();
        public abstract string GetName();
        public virtual void Dispose() { return; }
    }

    public enum ConsoleType
    {
        RA = 1,
        Client = 2,
        Server = 4
    }
}
