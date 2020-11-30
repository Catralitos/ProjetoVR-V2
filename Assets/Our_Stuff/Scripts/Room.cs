using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Se o prefab é uma sala ou corredor
public enum RoomType { Room, Corridor, Final };

public class Room
{
    //Prefab que foi instanciado
    public GameObject roomInstance;

    //Se a sala é de gelo
    public bool IceRoom { get; }

    //Por onde o user entrou na sala
    public RoomDir EntranceDirection { get; set; }

    //Tipo de sala que é
    public RoomType RoomType { get; }

    //Posições dos portais
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
