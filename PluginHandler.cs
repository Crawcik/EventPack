using EventManager.Events;
using Smod2;
using Smod2.Attributes;

namespace EventManager
{

    [PluginDetails(
    author = "Crawcik",
    configPrefix = "event",
    description = "EventManager with events",
    id = "event.manager",
    langFile = "event",
    name = "Event Manager",
    SmodMajor = 3,
    SmodMinor = 8,
    SmodRevision = 0,
    version = "2.6")]
    public class PluginHandler : Plugin
    {
        public override void OnDisable()
        {
            Info("EventPlugin has been disabled");
        }

        public override void OnEnable()
        {
            Info("EventPlugin is enable");
        }

        public override void Register()
        {
            var commands = new CommandHandler(this);
            AddEventHandlers(commands);
            commands.RegisterCommand(new Versus(this));
            commands.RegisterCommand(new SaxtonHale(this));
            commands.RegisterCommand(new Dziady(this));
            commands.RegisterCommand(new AlienInnocents(this));
            commands.RegisterCommand(new TTT(this));
        }
    }
}
