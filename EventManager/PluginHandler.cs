using Smod2;
using System;
using System.IO;
using System.Reflection;

namespace EventManager
{
    [Smod2.Attributes.PluginDetails(
    author = "Crawcik",
    configPrefix = "event",
    description = "Plugin with events",
    id = "crawcik.event_manager",
    langFile = "event",
    name = "Events",
    SmodMajor = 3,
    SmodMinor = 9,
    SmodRevision = 7,
    version = "3.3")]
    internal sealed class PluginHandler : Plugin
    {

        public string PluginDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                System.UriBuilder uri = new System.UriBuilder(codeBase);
                return System.IO.Path.GetDirectoryName(System.Uri.UnescapeDataString(uri.Path)) + System.IO.Path.DirectorySeparatorChar + this.Details.name;
            }
        }
        private EventHandler eventHandler;
        public override void OnDisable()
        {

        }

        public override void OnEnable()
        {

        }

        public override void Register()
        {
            eventHandler = new EventHandler(this);

            string directory = PluginDirectory;
            if (Directory.Exists(directory))
            {
                string[] dependencies = Directory.GetFiles(directory);
                foreach (string dependency in dependencies)
                {
                    if (!dependency.Contains(".dll"))
                        continue;
                    Logger.Info("PLUGIN_LOADER", "Loading plugin dependency: " + dependency);
                    try
                    {
                        Assembly a = Assembly.LoadFrom(dependency);
                        foreach (Type t in a.GetTypes())
                        {
                            if (t.IsSubclassOf(typeof(Event)) && t != typeof(Event))
                            {
                                Event plugin = (Event)Activator.CreateInstance(t);
                                eventHandler.RegisterCommand(plugin);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        this.Error($"Couldn't register {dependency}");
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
