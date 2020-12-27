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
    public GameObject miniWallLeft;
    public GameObject miniWallRight;
    public GameObject roomTpLeft;
    public GameObject roomTpRight;
    public GameObject corridorTpLeft;
    public GameObject corridorTpRight;
    public List<RoomProperties> innerMaterials;

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
        public string id;

        protected void CreateOuterShell()
        {
            GameObject.Instantiate(generator.roof, node.transform);
            GameObject.Instantiate(generator.outerFloor, node.transform);
            GameObject.Instantiate(generator.outerWalls, node.transform);
        }
    }

    public class Room : Node
    {
        public int properties;
        public Corridor[] corridors;
        //INCLUIR COUNTER DE LIMITE MAXIMO DE SALAS
        public Room()
        {
            id = "";
            for (int i = 0; i < 8; i++)
            {
                id += (int)Random.Range(0, 1);
            }
            properties = Random.Range(0, generator.innerMaterials.Count - 1);
            GenerateCorridors(null, -1);
        }

        public Room(string id, int properties, Corridor corridor, int corridorPos)
        {
            this.id = id;
            this.properties = properties;
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
                else if (id[i] == 1)
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
            origin = room;
            node = new GameObject("");
            int way = 1;
            if (entrance % 2 == 0)
                way = -1;
            id += entrance;
            entrance -= way;

            CreateOuterShell();
            CreateEntrance(true, entrance, way);

            int corridorsRemaining = (int)Random.Range(0, 3);
            GenerateDestination(corridorsRemaining, entrance, way);
        }

        private void CreateEntrance(bool originIsRoom, int entrance, int way)
        {
            if (originIsRoom)
            {
                string miniWalls = "";
                for (int i = 1; i < 4; i++)
                {
                    int position = entrance - i * way;
                    position = Wrap(position);
                    if (origin.id[position] == 0)
                    {
                        miniWalls += position;
                        if (position % 2 == 0)
                        {
                            Rotate(GameObject.Instantiate(generator.miniWallLeft, node.transform), position);
                        } 
                        else
                        {
                            Rotate(GameObject.Instantiate(generator.miniWallRight, node.transform), position);
                        }
                    } else
                    {
                        miniWalls += -1;
                    }
                }
                if (miniWalls[1] != -1 && miniWalls[2] != -1)
                {
                    Rotate(GameObject.Instantiate(generator.innerMaterials[((Room)origin).properties].innerWalls, node.transform), Wrap(entrance - way * 2));
                } 
                else
                {
                    Rotate(GameObject.Instantiate(generator.innerMaterials[((Room)origin).properties].innerDoorframe, node.transform), Wrap(entrance - way * 2));
                }
            }
        }

        private void Rotate(GameObject obj, int position)
        {
            switch (position)
            {
                case 0:
                case 1:
                    break;
                case 2:
                case 3:
                    obj.transform.Rotate(new Vector3(0, 90, 0));
                    break;
                case 4:
                case 5:
                    obj.transform.Rotate(new Vector3(0, 180, 0));
                    break;
                case 6:
                case 7:
                    obj.transform.Rotate(new Vector3(0, 270, 0));
                    break;
                default:
                    obj.transform.Rotate(new Vector3(0, 45, 0));
                    break;
            }
        }

        private void CreateExit()
        {

        }

        public Corridor(Corridor corridor, int entrance, int corridorsRemaining)
        {
            origin = corridor;

            int way = 1;
            if (entrance % 2 == 0)
                way = -1;
            entrance -= way;
            id += entrance;
            corridorsRemaining--;
            GenerateDestination(corridorsRemaining, entrance, way);
        }

        private void GenerateDestination(int corridorsRemaining, int entrance, int way)
        {
            int exit;
            if (corridorsRemaining == 0)
            {
                exit = entrance + 2 * way;
                id += exit;
                string roomId = "";
                for (int i = 0; i < 8; i++)
                {
                    if (i == exit)
                        roomId += 1;
                    else //if (depth != depth max)
                        roomId += (int)Random.Range(0, 1);
                    //else
                    //roomId += 0
                }
                int roomProperties = Random.Range(0, generator.innerMaterials.Count - 1);

                destination = new Room(roomId, roomProperties, this, exit);
            }
            else
            {
                exit = entrance + 3 * way;
                id += exit;
                destination = new Corridor(this, exit, corridorsRemaining - 1);
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

}
