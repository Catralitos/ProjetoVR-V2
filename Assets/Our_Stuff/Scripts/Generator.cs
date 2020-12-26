using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public static GameObject roof;
    public static GameObject outerFloor;
    public static GameObject outerWalls;
    public static GameObject miniWallLeft;
    public static GameObject miniWallRight;
    public static GameObject roomTpLeft;
    public static GameObject roomTpRight;
    public static GameObject corridorTpLeft;
    public static GameObject corridorTpRight;
    public static List<Material> innerMaterials;

    [System.Serializable]
    public struct RoomProperties
    {
        [SerializeField]
        GameObject innerFloor;
        [SerializeField]
        GameObject innerWalls;
        [SerializeField]
        GameObject innerDoorframe;
        [SerializeField]
        bool hasSpecialEffects;
        [SerializeField]
        GameObject specialEffects;
    }

    public class Node
    {
        public string id;
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
            properties = Random.Range(0, innerMaterials.Count - 1);
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
            int way = 1;
            if (entrance % 2 == 0)
                way = -1;
            id += entrance;
            entrance -= way;
            int corridorsRemaining = (int)Random.Range(0, 3);
            //TODO FUNÇAO PRIVADA DAQUI PARA BAIXO
            int exit;
            if (corridorsRemaining == 0)
            {
                exit = entrance + 2 *way;
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
                int roomProperties = Random.Range(0, innerMaterials.Count - 1);
                destination = new Room(roomId, roomProperties, this, exit);
            }
            else
            {
                exit = entrance + 3 * way;
                id += exit;
                destination = new Corridor(this, exit, corridorsRemaining - 1);
            }
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
            //TODO FUNÇAO PRIVADA DAQUI PARA BAIXO
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
                int roomProperties = Random.Range(0, innerMaterials.Count - 1);
                destination = new Room(roomId, roomProperties, this, exit);
            }
            else
            {
                exit = entrance + 3 * way;
                id += exit;
                destination = new Corridor(this, exit, corridorsRemaining - 1);
            }
        }

    }

}
