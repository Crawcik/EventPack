using Smod2;
using Smod2.API;
using System.Collections;
using System.Threading.Tasks;

using UnityEngine;

namespace SaxtonHale
{
    public partial class Boss
    {
        private async Task Handle()
        {
            while (onGoing)
            {
                await Task.Delay(500);
                int type = (int)player.GetCurrentItem().ItemType;
                switch (type)
                {
                    case (int)Abbility.RAGE:
                        if (!ActiveAbbilities.Contains(Abbility.RAGE))
                            break;
                        ActiveAbbilities.Remove(Abbility.RAGE);
                        float hp = player.HP;
                        Vector vector = this.player.GetPosition();
                        await Task.Delay(50);
                        this.player.ChangeRole(Smod2.API.RoleType.SCP_096, spawnTeleport: false);
                        await Task.Delay(250);
                        this.player.Teleport(vector);
                        this.player.HP = hp;
                        await Task.Delay(18000);
                        vector = this.player.GetPosition();
                        hp = player.HP;
                        await Task.Delay(100);
                        this.player.ChangeRole(Smod2.API.RoleType.CHAOS_INSURGENCY);
                        await Task.Delay(250);
                        this.player.Teleport(vector);
                        this.player.HP = hp;
                        SetNormalInventory();
                        break;
                    case (int)Abbility.TAUNT:
                        if (!ActiveAbbilities.Contains(Abbility.TAUNT))
                            break;
                        ActiveAbbilities.Remove(Abbility.TAUNT);
                        PluginManager.Manager.Server.Map.Shake();
                        await Task.Delay(100);
                        foreach (Player x in PluginManager.Manager.Server.GetPlayers(Smod2.API.RoleType.NTF_LIEUTENANT))
                        {
                            await Task.Delay(100);
                            x.GetInventory().ForEach(y => y.Drop());
                        }
                        SetNormalInventory();
                        break;
                    case (int)Abbility.SPECIAL:
                        if (!ActiveAbbilities.Contains(Abbility.SPECIAL))
                            break;
                        ActiveAbbilities.Remove(Abbility.SPECIAL);
                        SpecialAbbility().Start();
                        SpecialAbbility().Wait();
                        SetNormalInventory();
                        break;
                    case (int)Abbility.DROP:
                        player.GetCurrentItem().Drop();
                        break;

                }
            }
        }

        private async Task SpecialAbbility()
        {
            switch (role)
            {
                case Class.SAXTON:
                    this.player.GetInventory().ForEach(x => x.Remove());
                    player.GodMode = true;
                    await Task.Delay(100);
                    player.GiveItem(Smod2.API.ItemType.LOGICER);
                    await Task.Delay(100);
                    player.SetCurrentItem(Smod2.API.ItemType.LOGICER);
                    await Task.Delay(10000);
                    player.GodMode = false;
                    break;
                case Class.RIPPER:
                    Vector vector = this.player.GetPosition();
                    foreach (Player x in PluginManager.Manager.Server.GetPlayers(Smod2.API.RoleType.SPECTATOR))
                    {
                        x.ChangeRole(Smod2.API.RoleType.ZOMBIE);
                        await Task.Delay(100);
                        x.Teleport(vector);
                    };
                    break;
                case Class.DEMOMAN:
                    this.player.GodMode = true;
                    for (int i = 0; i < 40; i++)
                    {
                        Vector v = new Vector(60f, Random.Range(0f, 359f), 0f);
                        this.player.ThrowGrenade(GrenadeType.FRAG_GRENADE, v.Normalize, 3f, false);
                        await Task.Delay(150);
                    }
                    this.player.GodMode = false;
                    break;
                case Class.FLASH:
                    this.player.GodMode = true;
                    for (int i = 0; i < 40; i++)
                    {
                        Vector v = new Vector(60f, Random.Range(0f, 359f), 0f);
                        this.player.ThrowGrenade(GrenadeType.FRAG_GRENADE, v.Normalize, 3f, false);
                        await Task.Delay(30);
                    }
                    this.player.GodMode = false;
                    break;
                case Class.MINIMIKE:
                    this.player.GetInventory().ForEach(x => x.Remove());
                    this.player.GodMode = true;
                    player.GiveItem(Smod2.API.ItemType.GUN_PROJECT90);
                    await Task.Delay(100);
                    player.SetCurrentItem(Smod2.API.ItemType.GUN_PROJECT90);
                    Functions.SetHitboxScale((UnityEngine.GameObject)player.GetGameObject(), 0.77f, 0.77f, 0.77f);
                    await Task.Delay(12000);
                    Functions.SetHitboxScale((UnityEngine.GameObject)player.GetGameObject(), 1f, 1f, 1f);
                    this.player.GodMode = false;
                    break;
            }
        }
    }
}
