using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class Health : NetworkBehaviour
{
	public int healing = 5;

	[SyncVar]
	public bool isActive= true;

	private PowerupSpawner powerupSpawner;

	[Server]
	public void HealPlayer(Jugador player)
	{
		player.IncreaseHealth(healing);
	}

	private void ActiveChanged(bool old, bool newActive)
	{
		gameObject.SetActive(newActive);
	}

	public void Initialize(PowerupSpawner spawner)
	{
		powerupSpawner = spawner;
	}

    private void OnTriggerEnter(Collider other)
    {
		if (other.gameObject.TryGetComponent(out Jugador player))
		{
			HealPlayer(player);
			powerupSpawner.StartCoroutine(nameof(PowerupSpawner.CollectPowerUp));
		}
    }
}
