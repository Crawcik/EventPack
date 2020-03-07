using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace EventManager.Events
{
    public class TTT : Event, IEventHandlerRoundStart,IEventHandlerCheckRoundEnd, IEventHandlerPlayerDie, IEventHandlerTeamRespawn, IEventHandlerPlayerDropItem, IEventHandlerLCZDecontaminate
    {
        private PluginHandler plugin;
        private Random random = new Random();
        private List<Alives> alives = new List<Alives>();
        private Plugin logbot;
        #region Settings
        public TTT(PluginHandler _plugin)
        {
            this.plugin = _plugin;
            
        }
        public override void Dispose()
        {
            alives.ForEach(x => x.EndTasks());
            alives.Clear();
            try
            {
                plugin.PluginManager.EnablePlugin(logbot);
                logbot = null;
            }
            catch
            {
                this.plugin.Error("Cannot enable LogBot");
            }
        }
        public override string[] GetCommands()
        {
            return new string[] { "event_ttt" };
        }

        public override ConsoleType GetCommandType()
        {
            return ConsoleType.RA;
        }

        public override string GetName()
        {
            return "Trouble in Terrorist Town";
        }
        #endregion

        public void OnRoundStart(RoundStartEvent ev)
        {
            if (!isQueue)
                return;
            //Disabled LogBot
            try
            {
                this.logbot = plugin.PluginManager.GetPlugin("pag.logbot.plugin");
                plugin.PluginManager.DisablePlugin(logbot);
            }
            catch
            {
                this.plugin.Error("Cannot disable LogBot");
            }
            //Initializing
            alives.Clear();
            Player[] players = ev.Server.GetPlayers().ToArray();
            if (players.Length < 3)
            {
                ev.Server.Map.Broadcast(10, "Zamało graczy na tryb TTT", false);
                return;
            }
            ev.Server.GetPlayers().Clear();
            List<int> terrorists = new List<int>();
            int detectiveID;
            players.ToList().ForEach(x => x.SetRank(text:""));
            //Setting players
            for (int i = -1; i < players.Length / 10; i++)
            {
                while (true)
                {
                    int temp = random.Next(0, players.Length - 1);
                    if (!terrorists.Contains(temp))
                    {
                        terrorists.Add(temp);
                        break;
                    }
                }

            }
            while (true)
            {
                int temp = random.Next(0, players.Length - 1);
                if (!terrorists.Contains(temp))
                {
                    detectiveID = temp;
                    break;
                }
            }
            for (int i = 0; i < players.Length; i++)
            {
                if (terrorists.Contains(i))
                    alives.Add(new Alives(players[i], Klasy.ZDRAJCA));
                else if (detectiveID == i)
                    alives.Add(new Alives(players[i], Klasy.DETEKTYW));
                else
                    alives.Add(new Alives(players[i], Klasy.NIEWINNY));
            }
            List<Smod2.API.Door> doors = ev.Server.Map.GetDoors();
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_A").Locked = true;
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_B").Locked = true;
            //Spawning Weapons
            foreach (Smod2.API.Door room in ev.Server.Map.GetDoors())
            {
                Vector vec = room.Position;
                int rng = random.Next(0, 4);
                for (int i = 0; i < rng; i++)
                {
                    int x = random.Next(Convert.ToInt32(vec.x - 15f), Convert.ToInt32(vec.x + 15f));
                    int z = random.Next(Convert.ToInt32(vec.z - 15f), Convert.ToInt32(vec.z + 15f));
                    int y = Convert.ToInt32(vec.y + 1);
                    Vector spanw_pos = new Vector(x, y, z);

                    Array values = Enum.GetValues(typeof(WeaponTypeGame));
                    Smod2.API.ItemType item = (Smod2.API.ItemType)values.GetValue(random.Next(values.Length));
                    ev.Server.Map.SpawnItem(item, spanw_pos, Vector.Zero);
                }
            }
            List<Alives> terro = alives.FindAll(x => x.Rola == Klasy.ZDRAJCA);
            terro.ForEach(x => x.SetFriends(terro.Where(y=>y != x)));
            //Disposing
            doors = null;
            terro = null;
            terrorists = null;
        }

        public void OnCheckRoundEnd(CheckRoundEndEvent ev)
        {
            if (!isQueue)
                return;
            if (!alives.Exists(x => x.Rola == Klasy.ZDRAJCA)) {
                alives.ForEach(x => x.EndTasks());
                ev.Server.Map.Broadcast(5, "Niewinni wygrali!", false);
                ev.Status = ROUND_END_STATUS.OTHER_VICTORY; } 
            else if (!alives.Exists(x => x.Rola == Klasy.NIEWINNY)){
                alives.ForEach(x => x.EndTasks());
                ev.Server.Map.Broadcast(5, "Zdrajcy wygrali!", false);
                ev.Status = ROUND_END_STATUS.OTHER_VICTORY; }
            else
                ev.Status = ROUND_END_STATUS.ON_GOING;
        }

        public void OnPlayerDie(PlayerDeathEvent ev)
        {
            if (!isQueue)
                return;
            if (!alives.Exists(x => x.Player.UserId == ev.Player.UserId))
                return;
            if (alives.Exists(x => x.Player.UserId == ev.Killer.UserId && x.Rola == Klasy.ZDRAJCA))
                alives.Find(x => x.Player.UserId == ev.Killer.UserId).AddMoney(15);
            alives.Find(x => x.Player.UserId == ev.Player.UserId).EndTasks();
        }

        public void OnTeamRespawn(TeamRespawnEvent ev) {
            if (!isQueue)
                return;
            ev.PlayerList.ForEach(x => x.ChangeRole(Smod2.API.Role.SPECTATOR));
        }

        public void OnPlayerDropItem(PlayerDropItemEvent ev)
        {
            if (!isQueue)
                return;
            try
            {
                Alives vc = alives.Find(x => x.Player.UserId == ev.Player.UserId);
                if (vc.isMenuOpen)
                    ev.Item.Remove();
            }
            catch
            {
                plugin.Error("[TTT] Menu item remove failed!");
            }
        }

        public void OnDecontaminate()
        {
            alives.FindAll(x => x.Rola == Klasy.ZDRAJCA).ForEach(x => x.EndTasks());
        }

        private enum Klasy
        {
            NONE,
            NIEWINNY,
            DETEKTYW,
            ZDRAJCA
        }

        public enum WeaponTypeGame
        {
            COM15 = 13,
            E11_STANDARD_RIFLE = 20,
            P90 = 21,
            MP7 = 23,
            AMMO556 = 22,
            AMMO762 = 28,
            AMMO9MM = 29,
            USP = 30
        }

        private class Alives
        {
            public Player Player;
            public Klasy Rola;
            public bool isMenuOpen { private set; get; } = false;
            private List<Smod2.API.Item> normal_inventory;
            public string[] other_terrorists = null;
            public bool isDisposing = false;
            public int money = 0;
            public Alives(Player _Player, Klasy _klasy)
            {
                this.Player = _Player;
                this.Rola = _klasy;
                this.Player.PersonalBroadcast(5, $"Rola: {Enum.GetName(typeof(Klasy), this.Rola)}", false);
                if (_klasy == Klasy.DETEKTYW)
                {
                    this.Player.ChangeRole(Smod2.API.Role.SCIENTIST);
                    this.Player.SetRank(color: "cyan", text: "Detektyw");
                    this.Player.PersonalBroadcast(20, "Twoim zadaniem jest znalezienie zdrajcy. Możesz dawać rozkazy niewinnym i kożystać z sklepu, podnieś monetę by go otworzyć", false);

                    //CheckMenu().GetAwaiter();
                }
                else if (_klasy == Klasy.ZDRAJCA)
                {
                    this.Player.ChangeRole(Smod2.API.Role.CLASSD);
                    this.Player.GiveItem(Smod2.API.ItemType.COIN);
                    this.Player.SetRank(group: "tet");
                    this.Player.PersonalBroadcast(20, "Twoim zadaniem jest zabicie wszystkich, oprócz innych terrorystów (współpracuj z nimi). Możesz kożystać z sklepu, podnieś monetę by go otworzyć", false);
                    CheckMenu().GetAwaiter();
                }
                else 
                {
                    this.Player.HideTag(true);
                    this.Player.ChangeRole(Smod2.API.Role.CLASSD);
                    this.Player.PersonalBroadcast(20, "Twoim zadaniem jest przetrwanie. Musisz wykonywać rozkazy detektywa", false);
                }
            }

            public void EndTasks()
            {
                isMenuOpen = false;
                isDisposing = true;
                this.Rola = Klasy.NONE;
            }

            private async Task CheckMenu()
            {
                while(!isDisposing && Player.TeamRole.Role != Smod2.API.Role.SPECTATOR && Player.TeamRole.Role != Smod2.API.Role.UNASSIGNED)
                {
                    await Task.Delay(500);
                    if (isMenuOpen)
                    {
                        this.Player.PersonalBroadcast(1, "Masz otwarty sklep Terrorysty || Money: "+ money, false);
                        switch (this.Player.GetCurrentItem().ItemType)
                        {
                            case Smod2.API.ItemType.COIN:
                                CloseSpecialMenu();
                                break;
                            case Smod2.API.ItemType.KEYCARDJANITOR:
                                CloseSpecialMenu();
                                this.Player.GiveItem(Smod2.API.ItemType.RADIO);
                                this.money -= 10;
                                break;
                            case Smod2.API.ItemType.KEYCARDSCIENTIST:
                                CloseSpecialMenu();
                                this.Player.GiveItem(Smod2.API.ItemType.ADRENALINE);
                                this.money -= 20;
                                break;
                            case Smod2.API.ItemType.KEYCARDNTFCOMMANDER:
                                CloseSpecialMenu();
                                                                this.Player.PersonalClearBroadcasts();
                                this.Player.PersonalBroadcast(10, "Możesz otworzyć WSZYSTKIE DRZWI przez 20 sekund!", false);
                                this.Player.BypassMode = true;
                                this.money -= 40;
                                await Task.Delay(20000);
                                this.Player.PersonalClearBroadcasts();
                                this.Player.BypassMode = false;
                                break;
                            case Smod2.API.ItemType.KEYCARDCHAOSINSURGENCY:
                                CloseSpecialMenu();
                                this.Player.GiveItem(Smod2.API.ItemType.GUNLOGICER);
                                this.money -= 60;
                                break;
                        }
                    }
                    else if (this.Player.GetCurrentItem().ItemType == Smod2.API.ItemType.COIN)
                        OpenSpecialMenu();
                    else
                    {
                        if (other_terrorists.Length > 0)
                        {
                            string osoby = string.Join(", ", other_terrorists);
                            this.Player.PersonalBroadcast(1, "Inni terroryści: " + osoby, false);
                        }
                    }
                }
            }

            public void OpenSpecialMenu()
            {
                this.Player.SetCurrentItem(Smod2.API.ItemType.NONE);
                this.normal_inventory = this.Player.GetInventory();  
                GiveMenuItems();
            }

            public void CloseSpecialMenu()
            {
                isMenuOpen = false;
                this.Player.GetInventory().ForEach(x => x.Remove());
                foreach (Smod2.API.Item item in this.normal_inventory)
                    this.Player.GiveItem(item.ItemType);
            }

            private void GiveMenuItems()
            {
                isMenuOpen = true;
                this.Player.GetInventory().ForEach(x => x.Remove());
                if (this.Rola == Klasy.ZDRAJCA)
                {
                    this.Player.GiveItem(Smod2.API.ItemType.COIN);
                    if (money < 10)
                        this.Player.GiveItem(Smod2.API.ItemType.KEYCARDJANITOR);
                    if (money < 20)
                        this.Player.GiveItem(Smod2.API.ItemType.KEYCARDSCIENTISTMAJOR);
                    if (money < 40)
                        this.Player.GiveItem(Smod2.API.ItemType.KEYCARDNTFCOMMANDER);
                    if (money < 60)
                        this.Player.GiveItem(Smod2.API.ItemType.KEYCARDCHAOSINSURGENCY);
                }
            }
            public void SetFriends(IEnumerable<Alives> other)
            {
                List<string> temp = new List<string>();
                foreach (Alives terro in other)
                {
                    temp.Add(terro.Player.Name);
                }
                other_terrorists = temp.ToArray();
            }
            public void AddMoney(int money)
            {
                this.money = this.money + money;
            }
        }
    }
}
