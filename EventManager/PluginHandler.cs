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
    description = "Plugin to manage events/gamemodes",
    id = "crawcik.event_manager",
    langFile = "event",
    name = "Events",
    SmodMajor = 3,
    SmodMinor = 9,
    SmodRevision = 7,
    version = "4.1")]
    internal sealed class PluginHandler : Plugin
    {
        public int PLUGIN_MAJOR { private set; get; }
        public int PLUGIN_MINOR { private set; get; }

        const string translationFile = "translation.json";
        const string configFile = "config.json";

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
            ReloadConfigs();
        }

        public override void Register()
        {
            string[] version = this.Details.version.Split('.');
            PLUGIN_MAJOR = int.Parse(version[0]);
            PLUGIN_MINOR = int.Parse(version[0]);
            eventHandler = new EventHandler(this);
            string directory = PluginDirectory;
            if (Directory.Exists(directory))
            {
                string[] dependencies = Directory.GetFiles(directory);
                foreach (string dependency in dependencies)
                {
                    if (!dependency.Contains(".dll"))
                        continue;
                    Logger.Debug("EVENT_LOADER", "Loading Gamemodes/Events: " + dependency);
                    try
                    {
                        Assembly a = Assembly.LoadFrom(dependency);
                        foreach (Type t in a.GetTypes())
                            if (t.IsSubclassOf(typeof(GameEvent)) && t != typeof(GameEvent))
                                eventHandler.RegisterCommand((GameEvent)Activator.CreateInstance(t), t);
                    }
                    catch (Exception)
                    {
                        this.Error($"Couldn't register {dependency}. Isn't it outdated?");
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(directory);
                this.Info($"Directory {directory} created!");
            }
        }

        private void LoadData(string file, Dictionary<string, IDictionary<string, string>> AllData)
        {
            file = PluginDirectory + Path.DirectorySeparatorChar + file;
            if (!File.Exists(file))
            {
                using (FileStream fs = File.Create(file))
                {
                    string text = Newtonsoft.Json.JsonConvert.SerializeObject(file == configFile ? eventHandler.GetAllDefaultConfig() : eventHandler.GetAllDefaultTranslations(), Newtonsoft.Json.Formatting.Indented);
                    if (!string.IsNullOrEmpty(text))
                    {
                        byte[] info = new System.Text.UTF8Encoding(true).GetBytes(text);
                        fs.Write(info, 0, info.Length);
                    }
                }
            }
            else
            {
                Dictionary<string, IDictionary<string, string>> data;
                bool file_override = false;
                string text = File.ReadAllText(file);
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, IDictionary<string, string>>>(text);
                AllData = data;
                this.Info($"{file} loaded!");
                var default_translations = file == configFile ? eventHandler.GetAllDefaultConfig() : eventHandler.GetAllDefaultTranslations();
                foreach (string def_translation in default_translations.Keys)
                {
                    if (data.ContainsKey(def_translation))
                        continue;
                    data.Add(def_translation, default_translations[def_translation]);
                    file_override = true;
                }
                if (file_override)
                {
                    text = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                    if (!string.IsNullOrEmpty(text))
                        File.WriteAllText(file, text);
                }
            }
        }

        public void ReloadConfigs()
        {
            LoadData(translationFile, EventHandler.AllTranslations);
            LoadData(configFile, EventHandler.AllConfigs);
        }
    }
}
