using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/item")]
public class Item : ScriptableObject
{
    public int itemID;
    public string itemEventType;
    protected ItemEvent itemEvent;

    public virtual void Use(APlayer player)
    {
        if (itemEvent == null)
        {
            Type eventType = Type.GetType(itemEventType);
            itemEvent = Activator.CreateInstance(eventType) as ItemEvent;
        }
        itemEvent.Trigger(player);
    }
}