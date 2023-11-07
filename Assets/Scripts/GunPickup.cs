using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPickup : MonoBehaviour
{
    [SerializeField] GunStats gun;
    
    // Start is called before the first frame update
    void Start()
    {
        gun.currentAmmo = gun.maxAmmo;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.instance.playerScript.GunPickup(gun);
            Destroy(gameObject);
        }
    }

}
