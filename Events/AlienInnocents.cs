using Smod2;
using Smod2.API;
using Smod2.Commands;
using Smod2.Events;
using Smod2.EventHandlers;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EventManager.Events
{
    class AlienInnocents : Event, IEventHandlerRoundStart, IEventHandlerWarheadChangeLever, IEventHandlerCheckRoundEnd
    {
        private PluginHandler plugin;
        private bool lightsEnable;
        private EventRoundStatus RoundStatus;
        private Random random = new Random();
        private Dictionary<int, InnocentData> innocent_data = new Dictionary<int, InnocentData>();

        #region Settings
        public AlienInnocents(PluginHandler plugin)
        {
            this.plugin = plugin;
        }

        public override string[] GetCommands()
        {
            return new string[] { "event_alien" };
        }

        public override ConsoleType GetCommandType()
        {
            return ConsoleType.RA;
        }

        public override string GetName()
        {
            return "Alien Innocence";
        }
        #endregion
        public void OnRoundStart(RoundStartEvent ev)
        {
            if (!isQueue)
                return;
            plugin.Server.Map.WarheadLeverEnabled = true;
            RoundStatus = EventRoundStatus.ON_GOING;
            ev.Server.GetPlayers().ForEach(x =>
            {
                x.ChangeRole(Smod2.API.Role.CLASSD);
                x.GiveItem(Smod2.API.ItemType.FLASHLIGHT);
            });
            CheckPlayers().GetAwaiter();
        }
        public async Task CheckPlayers()
        {
            while (RoundStatus == EventRoundStatus.ON_GOING)
            {
                plugin.Server.GetPlayers().FindAll(x => x.TeamRole.Role != Smod2.API.Role.SPECTATOR).ForEach(x =>
                  {
                      //Sprawdzanie i wykonywanie anomali zdrowego
                      if (!innocent_data.ContainsKey(x.PlayerId))
                          innocent_data.Add(x.PlayerId, new InnocentData());
                      byte status_num = innocent_data[x.PlayerId].GetStatus();
                      if (status_num == 1)
                      {
                          innocent_data[x.PlayerId].necessityStartTimeout = 160;
                          innocent_data[x.PlayerId].necessity = (NECESSITY)random.Next(0, Enum.GetNames(typeof(NECESSITY)).Length - 1);
                          innocent_data[x.PlayerId].necessityStartTimeout = random.Next(40, 80);
                      }
                      if (status_num == 2)
                          x.Damage(2, DamageType.DECONT);
                      if (status_num == 3)
                          x.SetCurrentItem(Smod2.API.ItemType.NONE);
                      if (x.GetCurrentItem().ItemType == Smod2.API.ItemType.FLASHLIGHT)
                      {
                          innocent_data[x.PlayerId].battery -= 4;
                      }
                      if (innocent_data[x.PlayerId].battery <= 0)
                      {
                          x.SetCurrentItem(Smod2.API.ItemType.NONE);
                          innocent_data[x.PlayerId].battery = 0;    
                      }

                      x.PersonalBroadcast(1, $"Bateria {innocent_data[x.PlayerId].battery}% | Potrzeba {innocent_data[x.PlayerId].necessity.ToString("g")} | {innocent_data[x.PlayerId].necessityTimeLeft} | {innocent_data[x.PlayerId].necessityStartTimeout}", true);
                  });
                await Task.Delay(1000);
            }
        }
        public async Task LightsOff()
        {
            lightsEnable = false;
            Room[] rooms = plugin.Server.Map.Get079InteractionRooms(Scp079InteractionType.CAMERA).Where(x => x.ZoneType != ZoneType.ENTRANCE && x.ZoneType != ZoneType.UNDEFINED).ToArray();
            try
            {
                while (!lightsEnable && isQueue)
                {
                    foreach (Room x in rooms)
                        x.FlickerLights();
                    await Task.Delay(8000);
                }
            }
            catch(Exception e)
            {
                plugin.Error(e.Message);
            }
        }

        public void OnCheckRoundEnd(CheckRoundEndEvent ev)
        {
            if (!isQueue)
                RoundStatus = EventRoundStatus.INACTIVE;
            if (RoundStatus != EventRoundStatus.INACTIVE)
            {
                ev.Status = (ROUND_END_STATUS)RoundStatus;
                innocent_data.Clear();
            }
        }
        public void OnChangeLever(WarheadChangeLeverEvent ev)
        {
            if (ev.Allow == false && lightsEnable)
                LightsOff().GetAwaiter();
            else if (ev.Allow == true)
                lightsEnable = false;
        }

        private enum EventRoundStatus
        {
            ON_GOING = 0,
            INACTIVE = 1,
            INNOCENT_WIN = 7,
            ALIEN_WIN = 2
        }
        private enum CLASS
        {
            DEAD,
            DEAD_BY_ALIEN,
            INNOCENT,
            ALIEN_MOTHER,
            ALIEN
        }
        private enum NECESSITY
        {
            NONE,
            SPANIE,
            SRANIE,
            PRYSZNIC,
            JEDZENIE,
        }

        private class InnocentData
        {
            public int battery = 100;
            public int necessityTimeLeft = 160;
            public int necessityStartTimeout = 30;
            public NECESSITY necessity = NECESSITY.NONE;

            public byte GetStatus()
            {
                if(battery != 100)
                    battery++;
                if (necessityStartTimeout == 0 && necessity == NECESSITY.NONE)
                    return 1;
                if (necessityTimeLeft == 0 && necessity != NECESSITY.NONE)
                    return 2;
                if (battery >= 0)
                    return 3;
                if (necessityTimeLeft > 0 && necessity != NECESSITY.NONE)
                    necessityTimeLeft--;
                if (necessityStartTimeout > 0 && necessity == NECESSITY.NONE)
                    necessityStartTimeout--;
                return 0;
            }
        }
    }
}
