using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//event that notifies when player enteres new room
public static class StaticEventHandler 
{
    //room changed event
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    public static void CallRoomChangedEvent(Room room)
    {
        OnRoomChanged?.Invoke(new RoomChangedEventArgs() { room = room });
    }

}
public class RoomChangedEventArgs : EventArgs
{
    public Room room;
}
