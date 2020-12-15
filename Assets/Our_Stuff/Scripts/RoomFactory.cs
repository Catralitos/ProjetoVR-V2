using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomFactory : MonoBehaviour
{
    private enum Directions
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
    }
    private enum Orientation
    {
        Right = 0,
        Left = 1,
        LeftRight = 2,
        RightLeft = 3,
    }
    class RoomDirection
    {
        public Directions dir { get; set; }
        public Orientation ori { get; set; }

        //TO-DO
        //compare method
    }
    private Dictionary<RoomDir, RoomDirection> DirToDirMap = new Dictionary<RoomDir, RoomDirection>() 
    {
        { RoomDir.East_L, new RoomDirection {dir = Directions.East, ori = Orientation.Left } },
        { RoomDir.East_R, new RoomDirection {dir = Directions.East, ori = Orientation.Right } },
        { RoomDir.East_LR, new RoomDirection {dir = Directions.East, ori = Orientation.LeftRight } },
        { RoomDir.East_RL, new RoomDirection {dir = Directions.East, ori = Orientation.RightLeft } },

        { RoomDir.North_L, new RoomDirection {dir = Directions.North, ori = Orientation.Left } },
        { RoomDir.North_R, new RoomDirection {dir = Directions.North, ori = Orientation.Right } },
        { RoomDir.North_LR, new RoomDirection {dir = Directions.North, ori = Orientation.LeftRight } },
        { RoomDir.North_RL, new RoomDirection {dir = Directions.North, ori = Orientation.RightLeft } },

        { RoomDir.South_L, new RoomDirection {dir = Directions.South, ori = Orientation.Left } },
        { RoomDir.South_R, new RoomDirection {dir = Directions.South, ori = Orientation.Right } },
        { RoomDir.South_LR, new RoomDirection {dir = Directions.South, ori = Orientation.LeftRight } },
        { RoomDir.South_RL, new RoomDirection {dir = Directions.South, ori = Orientation.RightLeft } },

        { RoomDir.West_L, new RoomDirection {dir = Directions.West, ori = Orientation.Left } },
        { RoomDir.West_R, new RoomDirection {dir = Directions.West, ori = Orientation.Right } },
        { RoomDir.West_LR, new RoomDirection {dir = Directions.West, ori = Orientation.LeftRight } },
        { RoomDir.West_RL, new RoomDirection {dir = Directions.West, ori = Orientation.RightLeft } },
    };
    public GameObject BaseStructure;
    public GameObject PortalRight;
    public GameObject PortalLeft;
    public GameObject MiniWall_R;
    public GameObject MiniWall_L;
    public GameObject InnerWall;
    public GameObject Door;

    private int CreateEntrance(RoomDirection entrance, GameObject room, int numberOfExits)
    {
        int newNumberOfExits = numberOfExits;
        GameObject entranceObj = new GameObject(entrance.dir.ToString()+entrance.ori.ToString());
        Instantiate(Door, entranceObj.transform);
        switch (entrance.ori)
        {
            case Orientation.Left:
                Instantiate(PortalLeft, entranceObj.transform);
                Instantiate(MiniWall_R,entranceObj.transform);
                break;            
            case Orientation.Right:
                Instantiate(PortalRight, entranceObj.transform);
                Instantiate(MiniWall_L, entranceObj.transform);
                break;
            case Orientation.LeftRight:
                Instantiate(PortalLeft, entranceObj.transform);
                Instantiate(PortalRight, entranceObj.transform);
                newNumberOfExits--;
                break;
            case Orientation.RightLeft:
                Instantiate(PortalLeft, entranceObj.transform);
                Instantiate(PortalRight, entranceObj.transform);
                newNumberOfExits--;
                break;
        }
        RotateToDir(entranceObj, entrance);
        entranceObj.transform.SetParent(room.transform);
        return newNumberOfExits;
    }

    private void RotateToDir(GameObject obj,RoomDirection dir)
    {
        Vector3 rotVal;
        switch (dir.dir)
        {
            case Directions.North:
                rotVal = new Vector3(0, 0, 0);
                break;
            case Directions.East:
                rotVal = new Vector3(0, 90, 0);
                break;
            case Directions.South:
                rotVal = new Vector3(0, 180, 0);
                break;
            case Directions.West:
                rotVal = new Vector3(0, -90, 0);
                break;
            default:
                rotVal = new Vector3(0, 0, 0);
                break;
        }
        obj.transform.Rotate(rotVal);
    }

    public GameObject CreateRoom(RoomDir entranceDir,int numberOfExits)//numberOfExits 0-7
    {
        GameObject room = new GameObject("room");
        Instantiate(BaseStructure, room.transform);
        RoomDirection entranceDirection = DirToDirMap[entranceDir];
        CreateEntrance(entranceDirection, room, numberOfExits);
        //TO_DO Create Exits
        return room;
    }

    private void Start()
    {
        CreateRoom(RoomDir.South_LR, 0);
    }
}
