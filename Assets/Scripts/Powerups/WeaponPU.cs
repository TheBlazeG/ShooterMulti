using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class WeaponPU : APickUp
{
    public WeaponObject weaponType;

    [Server]
    public override void TakeEffect(Jugador player)
    {
        base.TakeEffect(player);
        player.currentWeapon = weaponType.ToData();

    }
}
