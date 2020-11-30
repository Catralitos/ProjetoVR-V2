using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Varias direções para ligar as salas 
public enum RoomDir
{
    Root = 0,
    North_L = 1, North_R = 2, North_LR = 3,
    South_L = 4, South_R = 5, South_LR = 6,
    East_L = 7, East_R = 8, East_LR = 9,
    West_L = 10, West_R = 11, West_LR = 12
};

//Este script é para dar attach aos prefabs das salas e preencher manualmente
public class RoomDirections : MonoBehaviour
{
    //Onde está o portal na sala
    //NOTA podem meter um awake neste script e fazer getcomponent para cada portal e sacar as direçoes
    public List<RoomDir> PortalPositions { get; }

    //Portais da sala
    //NOTA podem meter um awake neste script e fazer getcomponent para cada portal e sacar os portais 
    public List<Teleporter> Portals { get; }

    //Se a sala é de gelo
    public bool IceRoom { get; }


}
