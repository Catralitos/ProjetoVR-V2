using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Se o prefab é uma sala ou corredor
public enum RoomType { Room, Corridor, Final , FinalCorridor};

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

    public Room(GameObject _roomInstance, RoomType _RoomType, RoomDir _EntranceDirection, bool _IceRoom)
    {
        roomInstance = _roomInstance;
        Debug.Log("Guardou a instancia da sala");
        RoomType = _RoomType;
        Debug.Log("Guardou o tipo da sala");
        EntranceDirection = _EntranceDirection;
        Debug.Log("Guardou a entrada da sala");
        //IceRoom = roomInstance.GetComponent<RoomDirections>().IceRoom;
        IceRoom = _IceRoom;
        Debug.Log("Guardou se a sala é de gelo");
        PortalPositions = roomInstance.GetComponent<RoomDirections>().PortalPositions;
        Debug.Log("Guardou as posiçoes dos portais");
    }

}
