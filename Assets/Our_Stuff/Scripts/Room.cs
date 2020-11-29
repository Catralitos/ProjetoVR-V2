using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Room
{
    //Prefab que foi instanciado
    public GameObject roomInstance;
    
    //Entradas do prefab
    public List<RoomDir> PortalPositions { get; }

    //Se a sala é de gelo
    public bool IceRoom { get; }

    public RoomDir EntranceDirection;
    public Room(GameObject _roomInstance, List<RoomDir> _PortalPositions, bool _IceRoom)
    {
        roomInstance = _roomInstance;
        PortalPositions = _PortalPositions;
        IceRoom = _IceRoom;
    }

}
