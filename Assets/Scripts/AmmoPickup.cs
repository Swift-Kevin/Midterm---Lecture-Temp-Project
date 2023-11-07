using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount;

    private void OnTriggerEnter(Collider other)
    {
        IAmmo ammo = other.GetComponent<IAmmo>();
        if (ammo != null)
        {
            ammo.PickupAmmo(ammoAmount, gameObject);
        }
    }
}
