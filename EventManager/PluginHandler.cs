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
    version = "3.5")]
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
            this.CommandManager.UnregisterCommands(this);
            this.EventManager.RemoveEventHandlers(this);
        }

        public override void OnEnable()
        {
            this.AddCommand("event", eventHandler);
            this.AddEventHandlers(eventHandler);
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
                using (FileStream fs = File.Create(file))
                {
                    string text = Newtonsoft.Json.JsonConvert.SerializeObject(eventHandler.GetAllDefaultTranslations(), Newtonsoft.Json.Formatting.Indented);
                    if (!string.IsNullOrEmpty(text))
                    {
                        byte[] info = new System.Text.UTF8Encoding(true).GetBytes(text);
                        fs.Write(info, 0, info.Length);
                    }
                }
            }
            else
            {
                Dictionary<string, IDictionary<string, string>> translations;
                bool file_override = false;
                string text = File.ReadAllText(file);
                translations = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, IDictionary<string, string>>>(text);
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
                    text = Newtonsoft.Json.JsonConvert.SerializeObject(translations, Newtonsoft.Json.Formatting.Indented);
                    if (!string.IsNullOrEmpty(text))
                        File.WriteAllText(file, text);
                }
            }
        }
    }
}
