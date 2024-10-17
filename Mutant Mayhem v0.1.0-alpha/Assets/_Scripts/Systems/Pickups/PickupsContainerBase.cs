using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickupsContainerBase : MonoBehaviour
{
    public List<Pickup> container = new List<Pickup>();

    [SerializeField] ParticleSystem pickupEffect;
    [SerializeField] int pickupFxBurstCount;

    public virtual void AddToContainer(Pickup pickup)
    {
        // Add to list and make child of this
        container.Add(pickup);
        pickup.transform.SetParent(transform);
        pickup.gameObject.SetActive(false);

        pickupEffect.transform.position = pickup.transform.position;
        pickupEffect.Emit(pickupFxBurstCount);
    }
}
