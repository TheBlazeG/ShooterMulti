using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public abstract class APickUp : NetworkBehaviour
{

    public PowerupSpawner maiSpawner;
  [SyncVar(hook = (nameof(ActiveChanged)))] public bool isActive = true;

  private void OnTriggerEnter(Collider other)
  {
      if (!isServer) return;
      if (other.gameObject.TryGetComponent(out Jugador jugador))
      {
          TakeEffect(jugador);
      }
  }

  [Server]
  public virtual void TakeEffect(Jugador player)
  {
      maiSpawner.StartCoroutine(nameof(PowerupSpawner.CollectPowerUp));
  }

  private void ActiveChanged(bool oldActive, bool newActive)
  {
		gameObject.SetActive(newActive);
  }
  public void Initialize(PowerupSpawner spawner)
  {
      maiSpawner = spawner;
  }
}
