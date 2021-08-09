using UnityEngine;
using System.Collections;

public class Owned : MonoBehaviour
{
    public User Owner;
}

public static class OwnershipExtensions
{
    public static User GetOwner(this Craft craft)
    {
        return GetOwner(craft.gameObject);
    }

    public static User GetOwner(this Inventory inventory)
    {
        return GetOwner(inventory.gameObject);
    }

    public static User GetOwner(this Item item)
    {
        User owner = GetOwner(item.gameObject);
        if (owner != null)
            return owner;

        Inventory inventory = item.GetComponentInParent<Inventory>();
        if (inventory == null)
            return null;

        return inventory.GetOwner();
    }

    static User GetOwner(GameObject game_object)
    {
        if (!game_object.HasComponent<Owned>())
            return null;

        return game_object.GetComponent<Owned>().Owner;
    }
}
