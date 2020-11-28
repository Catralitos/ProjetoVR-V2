using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Room
{
    //Prefab que foi instanciado
    public GameObject roomInstance;
    
    //Entradas do prefab
    public List<RoomDir> PortalPositions { get; }

    public RoomDir EntranceDirection;
    public Room(GameObject _roomInstance)
    {
        roomInstance = _roomInstance;
        PortalPositions = roomInstance.GetComponent<RoomDirections>().PortalPositions;
    }

}
