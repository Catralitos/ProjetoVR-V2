using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Varias direções para ligar as salas (supostamente faltam mais)
public enum RoomDir { Root, 
    North_L, North_R, North_LR,
    South_L,South_R, South_LR,
    East_L, East_R, East_LR,
    West_L, West_R,West_LR};

//Este script é para dar attach aos prefabs das salas e preencher manualmente
public class RoomDirections : MonoBehaviour
{ 
    //Onde está o portal na sala
    public List<RoomDir> PortalPositions { get; }
    
    //Se a sala é de gelo
    public bool IceRoom { get; }


}
