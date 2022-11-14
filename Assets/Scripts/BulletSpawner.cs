using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletSpawner : NetworkBehaviour
{
    public Rigidbody bullet;
    private float bulletSpeed = 20f;
    private int _maxDamage = 20;

    public NetworkVariable<int> bulletDamage = new NetworkVariable<int>(1);

    public void IncreaseDamage()
    {
        if (bulletDamage.Value == 1)
        {
            bulletDamage.Value = 5;
        }
        else
        {
            bulletDamage.Value += 5;
        }

        if (bulletDamage.Value > _maxDamage)
        {
            bulletDamage.Value = _maxDamage;
        }
    }

    public bool IsAtMaxDamage()
    {
        return bulletDamage.Value == _maxDamage;
    }
    [ServerRpc]
    public void FireServerRpc(ServerRpcParams rpcParams = default) {
        Rigidbody newBullet = Instantiate(bullet, transform.position, transform.rotation);
        newBullet.velocity = transform.forward * bulletSpeed;
        newBullet.gameObject.GetComponent<NetworkObject>().SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        newBullet.GetComponent<Bullet>().damage.Value = bulletDamage.Value;
        Destroy(newBullet.gameObject, 3);
    }
}
