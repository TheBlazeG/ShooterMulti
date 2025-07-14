using System;
using Mirror;
using UnityEngine;

public class BoomBullet : ABullet
{
    public float radius;
    public float forcePush = 15;
    
    private void OnCollisionEnter(Collision collision)
    {
        HasHit(collision);
    }

    public override void HasHit(Collision col)
    {
        Push(col.contacts[0].point);
        base.HasHit(col);
    }

    // Update is called once per frame
    [Server]
    private void Push(Vector3 point)
    {
        var hits=Physics.OverlapSphere(point,radius);
        foreach (var deadGuy in hits)
        {
            if (deadGuy.TryGetComponent(out Jugador player))
            {
                player.GitPushed(point,forcePush ,radius);
            }
        }
    }
    
    
    
}
