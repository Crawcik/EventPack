using EventManager.Events;
using Newtonsoft.Json;
using Smod2;
using Smod2.Attributes;
using System.Collections.Generic;
using System.IO;

namespace EventManager
{

    [PluginDetails(
    author = "Crawcik",
    configPrefix = "event",
    description = "Plugin with events",
    id = "event.manager",
    langFile = "event",
    name = "Events Pack",
    SmodMajor = 3,
    SmodMinor = 8,
    SmodRevision = 0,
    version = "2.8")]
    public class PluginHandler : Plugin
    {
        public static PluginHandler Shared { private set; get; }

        private CommandHandler commands;
        public Dictionary<string, IDictionary<string, string>> AllTranslations;
        private Dictionary<string, IDictionary<string, string>> DefaultTranslations = new Dictionary<string, IDictionary<string, string>>{
                {
                    "Versus", new Dictionary<string, string> {
                        { "game_tutorial", "Event D-Class v.s. Scientists || Everyone has a gun || Checkpoints are locked" }
                    }
                }, {
                    "Dziady", new Dictionary<string, string> {
                        { "start", "Event 'Dziady' starts" },
                        { "scp049_start", "You're a priest" },
                        { "zombie_spawn", "The Dead rose from their graves!" }
                    }
                }, {
                    "Trouble in Terrorist Town", new Dictionary<string, string> {
                        { "not_enought_players", "'Not enought players to start gamemode" },
                        { "i_won", "Innocents won!" },
                        { "t_won", "Traitors won!" },
                        { "role_text", "Your role: " },
                        { "d_tutorial", "Your task is to find all traitors. You can give orders to the innocents and use disarmer, to check if someone is traitor (you have one use)" },
                        { "t_tutorial", "Your task is to kill everyone but other terrorists (cooperate with them). You can use the store, pick up a coin to open it" },
                        { "i_tutorial", "Your task is survival. Follow the detective's orders" },
                        { "ability_text1", "You can open ALL DOORS for 20 seconds!" },
                        { "opened_menu", "You have an open store" },
                        { "other_t", "Other rerrorists " },
                        { "checker_positive", " is traitor!" },
                        { "checker_negative", " is innocent" }
                    }
                }
            };
        public override void OnDisable()
        {
            this.EventManager.RemoveEventHandlers(this);
            commands.Commands.Clear();
        }

        public override void OnEnable()
        {
            if (this.config.Count == 0)
            {
                Info("Config has been generated");
                AllTranslations = GenerateConfig();
            }
            else
            {
                try
                {
                    AllTranslations = config.ToObject<Dictionary<string,IDictionary<string,string>>>();
                    Info("Config loaded");
                }
                catch
                {
                    Error("Config is incorrect!");
                    this.PluginManager.DisablePlugin(this);
                    return;
                }
            }
            if (!IsConfigCorrect())
                Warn("Config is incorrect! Please check if your config has any mistakes or isn't outdated! ");
            commands.RegisterCommand(new Versus());
            commands.RegisterCommand(new Dziady());
            commands.RegisterCommand(new TTT());
        }

        public override void Register()
        {
            Shared = this;
            commands = new CommandHandler(this);
            AddEventHandlers(commands);
        }

        private bool IsConfigCorrect()
        {
            if (AllTranslations.Keys == DefaultTranslations.Keys)
                return false;
            foreach (string key in DefaultTranslations.Keys)
            {
                foreach (KeyValuePair<string, string> pairs in AllTranslations[key])
                {
                    if (!DefaultTranslations[key].ContainsKey(pairs.Key))
                        return false;
                }
            }
            return true;
        }

        private Dictionary<string, IDictionary<string, string>> GenerateConfig()
        {
            //Saving config
            File.WriteAllText(this.PluginDirectory + $"\\servers\\{this.Server.Port}\\config.json", JsonConvert.SerializeObject(DefaultTranslations), System.Text.Encoding.Unicode);
            return DefaultTranslations;
        }
    }
}
