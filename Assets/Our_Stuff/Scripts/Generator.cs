using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public static Generator generator;

    private void Awake()
    {
        generator = this;
    }

    public GameObject roof;
    public GameObject outerFloor;
    public GameObject outerWalls;
    public GameObject miniWallEven;
    public GameObject miniWallOdd;
    public GameObject roomTpEven;
    public GameObject roomTpOdd;
    public GameObject corridorTpEven;
    public GameObject corridorTpOdd;
    public GameObject corridorInnerWalls;
    public List<RoomProperties> innerMaterials;
    public int maxDepth;

    [System.Serializable]
    public struct RoomProperties
    {
        [SerializeField]
        public GameObject innerFloor;
        [SerializeField]
        public GameObject innerWalls;
        [SerializeField]
        public GameObject innerDoorframe;
        [SerializeField]
        public bool hasSpecialEffects;
        [SerializeField]
        public GameObject specialEffects;
    }

    public class Node
    {
        public GameObject node;
        public int[] id;
        public int depth;

        protected void CreateOuterShell()
        {
            GameObject.Instantiate(generator.roof, node.transform);
            GameObject.Instantiate(generator.outerFloor, node.transform);
            GameObject.Instantiate(generator.outerWalls, node.transform);
        }

        protected string idToString()
        {
            string idString = "";
            foreach(int i in id){
                idString += i;
            }
            return idString;
        }
    }

    public class Room : Node
    {
       
        public int properties;
        public Corridor[] corridors;
        public Room()
        {
            depth = 0;
            id = new int[8];
            for (int i = 0; i < 8; i++)
            {
                float rngVal = Random.value;
                if (rngVal > 0.5f)
                {
                    id[i]=1;
                }
                else
                {
                    id[i] = 0;
                }
            }
            node = new GameObject("Room: " + depth + " | " + idToString());
            properties = (int)(Random.value * (generator.innerMaterials.Count - 1));
            GenerateCorridors(null, -1);
        }

        public Room(int[] id, int properties, Corridor corridor, int corridorPos, int depth)
        {
            this.id = id;
            this.properties = properties;
            node = new GameObject("Room: " + depth + " | " + idToString()); 
            GenerateCorridors(corridor, corridorPos);
        }

        private void GenerateCorridors(Corridor corridor, int corridorPos)
        {
            this.corridors = new Corridor[8];
            for (int i = 0; i < 8; i++)
            {
                if (i == corridorPos)
                {
                    corridors[i] = corridor;
                }
                else if (int.Parse(id[i].ToString()) == 1)
                {
                    corridors[i] = new Corridor(this, i);
                }
                else
                {
                    corridors[i] = null;
                }
            }
        }
    }

    public class Corridor : Node
    {
        public Node origin;
        public Node destination;

        public Corridor(Room room, int entrance)
        {
            id = new int[2];
            origin = room;
            node = new GameObject("");
            int way = 1;
            if (entrance % 2 == 0)
                way = -1;
            entrance = Wrap(entrance);
            id[0] =  entrance;
            entrance -= way;
            entrance = Wrap(entrance);

            CreateOuterShell();
            CreateEntrance(true, entrance, way);
            int corridorsRemaining = (int)(Random.value * 3);
            GenerateDestination(corridorsRemaining, entrance, way,true);
            node.name = "corridor: " + idToString();
        }

        public Corridor(Corridor corridor, int entrance, int corridorsRemaining)
        {
            id = new int[2];
            origin = corridor;
            node = new GameObject("");
            int way = 1;
            if (entrance % 2 == 0)
                way = -1;
            entrance -= way;
            entrance = Wrap(entrance);
            id[0] = entrance;
            corridorsRemaining--;
            CreateOuterShell();
            CreateEntrance(false, entrance, way);

            GenerateDestination(corridorsRemaining, entrance, way);

            node.name = "corridor: " + idToString();
        }

        private void CopyRoomFeatures(int[] idToCopy, int propertiesToCopy,int entrance, int way)
        {
            int[] miniWalls = new int[3];
            for (int i = 1; i < 4; i++)
            {
                int position = entrance + i * way;
                position = Wrap(position);
                if (idToCopy[position] == 0)
                {
                    miniWalls[i - 1] = position;
                    if (position % 2 == 0)
                    {
                        generator.Rotate(GameObject.Instantiate(generator.miniWallEven, node.transform), position);
                    }
                    else
                    {
                        generator.Rotate(GameObject.Instantiate(generator.miniWallOdd, node.transform), position);
                    }
                }
                else
                {
                    miniWalls[i - 1] = -1;
                }
            }
            generator.Rotate(GameObject.Instantiate(generator.innerMaterials[propertiesToCopy].innerDoorframe, node.transform), Wrap(entrance));
            Debug.Log(miniWalls);
            if (miniWalls[1] > -1 && miniWalls[2] > -1)
            {
                generator.Rotate(GameObject.Instantiate(generator.innerMaterials[propertiesToCopy].innerWalls, node.transform), Wrap(entrance + way * 2));
            }
            else
            {
                generator.Rotate(GameObject.Instantiate(generator.innerMaterials[propertiesToCopy].innerDoorframe, node.transform), Wrap(entrance + way * 2));
            }
        }

        private void SetPortal(int position, GameObject destination)
        {
            //TO-DO this code
        }

        private void CreateEntrance(bool originIsRoom, int entrance, int way)
        {
            depth = origin.depth;
            if (originIsRoom)
            {
                CopyRoomFeatures(origin.id,((Room)origin).properties, entrance, -way);
               
            }
            else
            {
                PopulateWalls(entrance, -way);
            }
            SetPortal(id[0], origin.node);
        }

        private void CreateExit(bool destinationISRoom, int exit, int way, bool isSingle = false)
        {
            if (destinationISRoom)
            {
                CopyRoomFeatures(destination.id, ((Room)destination).properties, exit, -way);
                if (isSingle)
                {
                    //TO-DO Triger that changes inner floor
                }
            }
            else
            {
                PopulateWalls(exit, way);
            }
            SetPortal(int.Parse(id[1].ToString()), destination.node);
        }

        private void PopulateWalls(int startPosition, int way)
        {
            generator.Rotate(GameObject.Instantiate(generator.corridorInnerWalls, node.transform), Wrap(startPosition + way * 0));
            generator.Rotate(GameObject.Instantiate(generator.corridorInnerWalls, node.transform), Wrap(startPosition + way * 2));
        }

       

        

       

        private void GenerateDestination(int corridorsRemaining, int entrance, int way, bool isSingle = false)
        {
            int exit;
            if (corridorsRemaining == 0)
            {
                int destinationDepth = depth + 1;
                exit = Wrap(entrance + 2 * way);
                id[1] = exit;
                int[] roomId = new int[8];
                for (int i = 0; i < 8; i++)
                {
                    if (i == exit)
                        roomId[i] = 1;
                    else if (destinationDepth < generator.maxDepth)
                        roomId[i] = (int)Random.value;
                    else
                        roomId[i] = 0;
                }
                int roomProperties = (int)(Random.value * (generator.innerMaterials.Count - 1));
                destination = new Room(roomId, roomProperties, this, exit, destinationDepth);
                CreateExit(true, exit, way, isSingle);
            }
            else
            {
                exit = Wrap(entrance + 3 * way);
                id[1] = exit;
                destination = new Corridor(this, exit, corridorsRemaining);
                CreateExit(false, exit, way, false);
            }
        }

        private int Wrap(int position)
        {
            if (position< 0)
            {
                position += 8;
            }
            if (position > 7)
            {
                position -= 8;
            }
            return position;
        }

    }

    public void Rotate(GameObject obj, int position)
    {
        switch (position)
        {
            case 0:
            case 1:
                Debug.Log("_____________");
                Debug.Log(position);
                Debug.Log("(0, 0, 0)");
                break;
            case 2:
            case 3:
                obj.transform.Rotate(new Vector3(0, 90, 0));
                Debug.Log("_____________");
                Debug.Log(position);
                Debug.Log("(0, 90, 0)");
                break;
            case 4:
            case 5:
                obj.transform.Rotate(new Vector3(0, 180, 0));
                Debug.Log("_____________");
                Debug.Log(position);
                Debug.Log("(0, 180, 0)");
                break;
            case 6:
            case 7:
                obj.transform.Rotate(new Vector3(0, -90, 0));
                Debug.Log("_____________");
                Debug.Log(position);
                Debug.Log("(0, -90, 0)");
                break;
            default:
                obj.transform.Rotate(new Vector3(0, 45, 0));
                Debug.Log("_____________");
                Debug.Log(position);
                Debug.Log("(0, 45, 0)");
                break;
        }
    }

    private void Start()
    {
        Node first = new Room();
    }
}
