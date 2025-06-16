using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class PowerupSpawner : NetworkBehaviour
{
	public PowerType powerType;
	public enum PowerType
	{
		Health,
		Weapon
	}

	public GameObject[] powerUps = new GameObject[2];

	private GameObject spawned;

	public float spawnTime=5;

    public override void OnStartServer()
    {
        base.OnStartServer();
		CmdSpawn();
    }

	[Server]
	private void CmdSpawn()
	{
        spawned = Instantiate(powerUps[(int)powerType], transform.position, Quaternion.identity);
        spawned.GetComponent<APickUp>().Initialize(this);
        PositionPowerup(spawned, transform.position);
        NetworkServer.Spawn(spawned);
    }


	[ClientRpc]
	private void PositionPowerup(GameObject powerUp,Vector3 position)
	{
		powerUp.transform.position = position;
	}
	 
	public IEnumerator CollectPowerUp()
	{

		var foo = spawned.GetComponent<Health>();
		foo.isActive = false;
		yield return new WaitForSeconds(spawnTime);
		foo.isActive = true;
	}

}
