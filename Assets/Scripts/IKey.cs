using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public abstract class IKey : NetworkBehaviour
{
    public enum KeyType
    {
        Universal,
        RoomNumber
    }

    public abstract KeyType type { get; }
    
}
