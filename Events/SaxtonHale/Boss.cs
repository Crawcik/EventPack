using Smod2.API;

using System.Collections.Generic;

namespace SaxtonHale
{
    public partial class Boss
    {
        public Player player;
        public Class role;
        private List<Abbility> ActiveAbbilities;
        private bool onGoing = true;
        public Boss(Class _class, Player player, string hint)
        {
            this.role = _class;
            this.player = player;
            this.ActiveAbbilities = new List<Abbility>() { Abbility.RAGE, Abbility.TAUNT, Abbility.SPECIAL };
            this.player.ChangeRole(Smod2.API.RoleType.CHAOS_INSURGENCY);
            Handle().GetAwaiter();
            SetNormalInventory();
            player.ShowHint(hint, 20f);
        }

        public void EndTask()
        {
            onGoing = false;
            ActiveAbbilities.Clear();
        }

        private void SetNormalInventory()
        {
            this.player.GetInventory().ForEach(x => x.Remove());
            ActiveAbbilities.ForEach(x => player.GiveItem((Smod2.API.ItemType)x));
            player.GiveItem(Smod2.API.ItemType.USP);
            this.player.SetAmmo(AmmoType.DROPPED_9, 1000);
        }
    }
}
