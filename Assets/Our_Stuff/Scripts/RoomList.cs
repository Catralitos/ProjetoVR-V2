using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomList
{
    public RoomType roomType;
    public RoomDir roomDirection;
    public List<GameObject> rooms;
}
