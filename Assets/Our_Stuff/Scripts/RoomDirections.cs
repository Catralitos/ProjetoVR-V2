using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Varias direções para ligar as salas (supostamente faltam mais)
public enum RoomDir { Root, North, South, East, West };

//Este script é para dar attach aos prefabs das salas e preencher manualmente
public class RoomDirections : MonoBehaviour
{ 
    //Onde está o portal na sala
    public List<RoomDir> PortalPositions { get; }
    
    //Se a sala é de gelo
    public bool IceRoom { get; }


}
