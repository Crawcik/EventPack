using Smod2;
using System;
using System.Collections.Generic;
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
        const string translationFile = "translation.json";

        public string PluginDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                return Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path)) + Path.DirectorySeparatorChar + this.Details.name;
            }
        }
        private EventHandler eventHandler;
        public override void OnDisable()
        {

        }

        public override void OnEnable()
        {
            LoadTranslation();
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
                            if (t.IsSubclassOf(typeof(GameEvent)) && t != typeof(GameEvent))
                            {
                                GameEvent plugin = (GameEvent)Activator.CreateInstance(t);
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

        private void LoadTranslation()
        {
            string file = PluginDirectory + Path.DirectorySeparatorChar + translationFile;
            if (!File.Exists(file))
            {
                File.Create(file);
                string text = Newtonsoft.Json.JsonConvert.SerializeObject(eventHandler.GetAllDefaultTranslations());
                File.WriteAllText(file, text);
                return;
            }
            else
            {
                IDictionary<string, IDictionary<string, string>> translations;
                bool file_override = false;
                string text = File.ReadAllText(file);
                try
                {
                    translations = Newtonsoft.Json.JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, string>>>(text);
                    EventHandler.AllTranslations = translations;
                    this.Info($"Translation loaded");
                    var default_translations = eventHandler.GetAllDefaultTranslations();
                    foreach (string def_translation in default_translations.Keys)
                    {
                        if (translations.ContainsKey(def_translation))
                            continue;
                        translations.Add(def_translation, default_translations[def_translation]);
                        file_override = true;
                    }
                    if (file_override)
                    {
                        text = Newtonsoft.Json.JsonConvert.SerializeObject(translations);
                        File.WriteAllText(file, text);
                    }
                }
                catch
                {
                    this.Error($"Couldn't load translations!");
                }
            }
        }
    }
}
