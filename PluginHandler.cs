using EventManager.Events;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Smod2;
using Smod2.Attributes;

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EventManager
{

    [PluginDetails(
    author = "Crawcik",
    configPrefix = "event",
    description = "Plugin with events",
    id = "crawcik.event_manager",
    langFile = "event",
    name = "Events Pack",
    SmodMajor = 3,
    SmodMinor = 9,
    SmodRevision = 0,
    version = "3.1")]
    public class PluginHandler : Plugin
    {
        public static PluginHandler Shared { private set; get; }
        private JObject config;
        public string PluginDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                System.UriBuilder uri = new System.UriBuilder(codeBase);
                return Path.GetDirectoryName(System.Uri.UnescapeDataString(uri.Path)) + Path.DirectorySeparatorChar + this.Details.name;
            }
        }

        private CommandHandler commands;
        public IDictionary<string, IDictionary<string, string>> AllTranslations;
        private readonly static IDictionary<string, IDictionary<string, string>> DefaultTranslations = new Dictionary<string, IDictionary<string, string>>{
            {
                "Versus", new Dictionary<string, string> {
                    { "game_tutorial", "Event D-Class v.s. Scientists || Everyone has a gun || Checkpoints are locked" }
                }
            },
            {
                "Dziady", new Dictionary<string, string> {
                    { "start", "Event 'Dziady' starts" },
                    { "scp049_start", "You're a priest" },
                    { "zombie_spawn", "The Dead rose from their graves!" }
                }
            },
            {
                "Trouble in Terrorist Town", new Dictionary<string, string> {
                    { "not_enought_players", "Not enought players to start gamemode" },
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
            },
            {
                "Saxton Hale", new Dictionary<string, string> {
                    { "tutorial", "<size=20>Janitor card - RAGE, MTF card - Taunt, O5 card - SPECJAL</size>" },
                    { "hale_spawn", "%nick% become %class%" }
                }
            },
            {
                "Prop Hunt", new Dictionary<string, string> {
                    { "hunters_wait", "You will soon become hunter, find them all before decontamination!" },
                    { "props_start", "You're now prop, try to hide somewhere before hunters find you!" }
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
            this.Info(PluginDirectory + Path.DirectorySeparatorChar + "translation.json");
            if (!Directory.Exists(PluginDirectory))
            {
                Directory.CreateDirectory(PluginDirectory);
            }
            if (!File.Exists(PluginDirectory + Path.DirectorySeparatorChar + "translation.json"))
            {
                File.Create(PluginDirectory + Path.DirectorySeparatorChar + "translation.json");
                this.AllTranslations = GenerateConfig();
                this.Info("Created config in: " + PluginDirectory + Path.DirectorySeparatorChar + "translation.json");
            }
            else
            {
                string json = File.ReadAllText(PluginDirectory + Path.DirectorySeparatorChar + "translation.json", Encoding.UTF8);
                if (string.IsNullOrEmpty(json))
                {
                    this.Error("Config file is empty!");
                    this.AllTranslations = GenerateConfig();
                }
                else
                {
                    try
                    {
                        this.AllTranslations = JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, string>>>(json);
                    }
                    catch
                    {
                        this.Error("Config is incorrect! Loading default config");
                        this.AllTranslations = DefaultTranslations;
                    }
                } 
            }
            if (!IsConfigCorrect())
                Warn("Config is incorrect! Please check if your config has any mistakes or isn't outdated! ");
            commands.RegisterCommand(new Versus());
            commands.RegisterCommand(new Dziady());
            commands.RegisterCommand(new TTT());
            commands.RegisterCommand(new Saxton_Hale());
            commands.RegisterCommand(new AlienBreakout());
            commands.RegisterCommand(new PropHunt());
        }

        public override void Register()
        {
            Shared = this;
            commands = new CommandHandler();
            AddEventHandlers(commands);
            AddCommand("event", commands);
        }

        private bool IsConfigCorrect()
        {
            if (AllTranslations.Keys != DefaultTranslations.Keys)
                return false;
            foreach (string key in AllTranslations.Keys)
            {
                if(!DefaultTranslations.ContainsKey(key))
                    return false;
                foreach (KeyValuePair<string, string> pairs in DefaultTranslations[key])
                {
                    if (!AllTranslations[key].ContainsKey(pairs.Key))
                        return false;
                }
            }
            return true;
        }

        private IDictionary<string, IDictionary<string, string>> GenerateConfig()
        {
            //Saving config
            File.WriteAllText(this.PluginDirectory + Path.DirectorySeparatorChar + "translation.json", JsonConvert.SerializeObject(DefaultTranslations), Encoding.UTF8);
            return DefaultTranslations;
        }

        public void ExecuteCommand(string command)
        {
            commands.OnCall(this.Server, command.Split(' '));
        }
    }
}
