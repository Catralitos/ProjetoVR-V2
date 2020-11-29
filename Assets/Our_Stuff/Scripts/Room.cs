using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType { Room, Corridor};

public class Room
{
    //Prefab que foi instanciado
    public GameObject roomInstance;
    
    //Entradas do prefab
    //public List<RoomDir> PortalPositions { get; }

    //Se a sala é de gelo
    public bool IceRoom { get; }

    public RoomDir EntranceDirection { get; set; }
    
    public RoomType RoomType { get; }

    public List<RoomDir> PortalPositions { get; }

    public Room(GameObject _roomInstance, RoomType _RoomType, RoomDir _EntranceDirection)
    {
        roomInstance = _roomInstance;
        RoomType = _RoomType;
        EntranceDirection = _EntranceDirection;
        IceRoom = roomInstance.GetComponent<RoomDirections>().IceRoom;
        PortalPositions = roomInstance.GetComponent<RoomDirections>().PortalPositions;
    }

}
