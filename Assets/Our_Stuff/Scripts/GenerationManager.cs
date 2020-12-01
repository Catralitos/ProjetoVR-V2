using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerationManager : MonoBehaviour
{
    //Isto é para o teleporter conseguir encontrar o Manager facilmente
    public static GenerationManager instance;

    //Onde vamos por as salas todas no inspetor
    public List<RoomList> roomLists;

    //Prefabs de salas de onde escolher
    private Dictionary<RoomDir, List<GameObject>> rooms = new Dictionary<RoomDir, List<GameObject>>();
    //Prefabs de salas de onde escolher salas sem saida (finais)
    private Dictionary<RoomDir, List<GameObject>> finalRooms = new Dictionary<RoomDir, List<GameObject>>();
    //Prefabs de corredores de onde escolher
    private Dictionary<RoomDir, List<GameObject>> corridors = new Dictionary<RoomDir, List<GameObject>>();
    //Prefabs de corredores finais de onde escohler
    private Dictionary<RoomDir, List<GameObject>> finalCorridors = new Dictionary<RoomDir, List<GameObject>>();
    //Prefabs de salas de gelo onde escolher
    private Dictionary<RoomDir, List<GameObject>> iceRooms = new Dictionary<RoomDir, List<GameObject>>();
    //Prefabs de corredores de gelo onde escolher
    private Dictionary<RoomDir, List<GameObject>> iceCorridors = new Dictionary<RoomDir, List<GameObject>>();

    //Posições ocupadas por cada sala (grid, não espaço real, daí Vector2)
    private List<Vector2> roomPositions = new List<Vector2>();

    //Player controller/prefab a instanciar
    public GameObject player;

    //Raiz da árvore
    private TreeNode<Room> treeRoot;
    //Nodes todos da árvore
    private List<TreeNode<Room>> treeNodes = new List<TreeNode<Room>>();

    //-1 se nao houver limite, depois podemos meter outro valor, as este é o default
    public int depthLimit = -1;
    //Tamanho da maior sala, para quando metermos as salas como se numa grid cabem todas
    public int gridSize = 10;
    //Quando começa a criar as salas numa linha diferente
    public int maxSpawnWidth = 10;
    //Se começo com gelo
    public bool iceRoot = false;

    private void Awake()
    {
        instance = this;

        int lenght = roomLists.Count;

        //Meter keys para cada direção no inspetor, poupar algum trabalho
        for (int i = 0; i < lenght; i++)
        {
            if (roomLists[i].roomType == RoomType.Room)
            {
                if (roomLists[i].IceRooms)
                {
                    iceRooms.Add(roomLists[i].roomDirection, roomLists[i].rooms);
                }
                else
                {
                    rooms.Add(roomLists[i].roomDirection, roomLists[i].rooms);
                }
            }
            else if (roomLists[i].roomType == RoomType.Corridor)
            {
                if (roomLists[i].IceRooms)
                {
                    iceCorridors.Add(roomLists[i].roomDirection, roomLists[i].rooms);
                }
                else
                {
                    corridors.Add(roomLists[i].roomDirection, roomLists[i].rooms);
                }
            }
            else if (roomLists[i].roomType == RoomType.Final)
            {
                finalRooms.Add(roomLists[i].roomDirection, roomLists[i].rooms);
            }
            else if (roomLists[i].roomType == RoomType.FinalCorridor)
            {
                finalCorridors.Add(roomLists[i].roomDirection, roomLists[i].rooms);
            }
        }

    }

    void Start()
    {
        //Escolher primeira sala aleatoriamente
        GameObject firstRoom = GetRandomRoot(iceRoot);

        //Instanciar essa sala
        GameObject aux = Instantiate(firstRoom, Vector3.zero, Quaternion.identity, this.gameObject.transform);
        Debug.Log("Instanciou a sala " + aux);
        //Criar raiz da árvore (depois de instanciar, porque instancia != prefab e porque só se cria o node se instanciar bem)
        //Nao sei se isto do root é preciso, mas só quero ter a certeza que o if do SpawnChildren da direção diferente não rebenta
        treeRoot = new TreeNode<Room>(new Room(aux, RoomType.Room, RoomDir.Root, iceRoot));
        Debug.Log("Criou a raiz");
        treeNodes.Add(treeRoot);
        Debug.Log("Meteu a raiz na lista");

        //Instanciar o player (vai ter que ser depois de instanciar a sala, não podemos pô-lo na cena no editor)
        _ = Instantiate(player, new Vector3(0, 0.5f, 0), Quaternion.identity);
        Debug.Log("Instanciou o player");

        //Guardar a posição ocupada pela sala
        roomPositions.Add(Vector2.zero);

        //Criar filho para cada saida possivel
        if (depthLimit > 1 || depthLimit < 0)
        {
            SpawnChildren(treeRoot);
        }
    }

    /// <summary>
    /// Método com as operações a realizar quando passas para uma nova sala
    /// Ou seja, quando se entre num portal, nesse OnTriggerEnter, chama-se isto
    /// </summary>
    /// <param name="newRoom">A nova sala para onde o player vai (passada pelo portal)</param>
    public void OnPortalPass(GameObject obj)
    {
        TreeNode<Room> newRoom = GetTreeNode(obj);

        //Não tem filhos, tem que instanciar filhos (excepçoes tratadas no metodo em si)
        if (newRoom.IsLeaf)
        {
            SpawnChildren(newRoom);
        }

        //Mudar os objetos ativos na cena, para optimização
        GarbageCleanup(newRoom);
    }

    /// <summary>
    /// Certifica-se que só as salas para onde o player pode ir ficam ativas.
    /// </summary>
    /// <param name="newRoom">A nova sala para onde o player vai</param>
    private void GarbageCleanup(TreeNode<Room> newRoom)
    {
        int depth = newRoom.Level;
        foreach (TreeNode<Room> room in treeNodes)
        {
            //TODO ver se as comparações depois do && funcionam
            //Nao sei porque nenhuma destas funcionam
            //&& (newRoom.Parent == room || newRoom.HasChild(room.Data))
            //&& newRoom.Related(room)
            if (Mathf.Abs(room.Level - depth) < 2)
            {
                room.Data.roomInstance.SetActive(true);
            }
            else
            {
                room.Data.roomInstance.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Escolhe filhos de um node aleatoriamente e instancia-os
    /// </summary>
    /// <param name="parent">Node pai</param>
    private void SpawnChildren(TreeNode<Room> parent)
    {
        List<RoomDir> directions = parent.Data.PortalPositions;
        foreach (RoomDir direction in directions)
        {
            GameObject obj;
            RoomType type;
            bool ice = false;

            //Sala normal (não interessa ser de gelo o GetRandomCorridor vai à lista correcta)
            if (parent.Data.RoomType == RoomType.Room)
            {
                //Se a sala for a penultima em depth nao pode dar spawn de uma sala final a seguir
                //Logo dá spawn de um corredor final
                if (depthLimit > -1 && parent.Level == depthLimit - 1)
                {
                    obj = GetRandomFinalCorridor(direction);
                    type = RoomType.FinalCorridor;
                }
                //Senão escolhe um corredor ao calhas
                else
                {
                    obj = GetRandomCorridor(direction, parent);
                    type = RoomType.Corridor;
                }

            }
            //Se for corredor
            else if (parent.Data.RoomType == RoomType.Corridor)
            {
                //Se o corredor for de gelo, vai buscar um corredor normal
                //O método em si só retorna corredores de gelo se parent for sala de gelo
                if (parent.Data.IceRoom)
                {
                    obj = GetRandomCorridor(direction, parent);
                    type = RoomType.Corridor;
                    ice = true;
                }
                //Se não for de gelo não liga
                else
                {
                    obj = GetRandomRoom(direction);
                    type = RoomType.Room;
                }
            }
            //Final corridor
            else
            {
                //Buscar uma sala final
                obj = GetFinalRoom(direction);
                type = RoomType.Room;
            }
            Vector2 position = GetNewPosition();
            GameObject GenRoom = Instantiate(obj, new Vector3(position.y * gridSize, 0, position.x * gridSize), Quaternion.identity, this.gameObject.transform);
            if (GenRoom != null)
            {
                int c = 0;
                //Passar parametros aos portais do pai para fazerem bem a ligação
                List<Teleporter> parentPortals = parent.Data.roomInstance.GetComponent<RoomDirections>().Portals;
                foreach (Teleporter portal in parentPortals)
                {
                    //Se o pai/currente tiver 2 portais tenho de saber qual vai ligar
                    if (!portal.Generated && portal.direction == direction)
                    {
                        //Vai do pai para o filho
                        portal.SetRooms(parent.Data.roomInstance, GenRoom);
                        portal.Generated = true;
                        c++;
                    }
                }

                //Passar parametros aos portais do filho para fazerem bem a ligação
                List<Teleporter> childPortals = GenRoom.GetComponent<RoomDirections>().Portals;
                foreach (Teleporter portal in childPortals)
                {
                    //Se o filho tiver 2 portais tenho de saber para onde vai (partilham direcao)
                    if (!portal.Generated && portal.direction == direction)
                    {
                        //Vai do filho para o pai
                        portal.SetRooms(GenRoom, parent.Data.roomInstance);
                        portal.Generated = true;
                        c++;
                    }
                }
                if (c % 2 == 0)
                {
                    TreeNode<Room> child = parent.AddChild(new Room(GenRoom, type, direction, ice));
                    treeNodes.Add(child);
                    //Não faço no GetPosition porque só aqui é que dou spawn da sala
                    roomPositions.Add(position);
                }
                else
                {
                    Destroy(GenRoom);
                    Debug.Log("(C impar)");
                    Debug.Log("Error: Could not instantiate room at " + new Vector3(position.y * gridSize, 0, position.x * gridSize));
                }
            }
            else
            {
                Debug.Log("(Spawn nao deu)");
                Debug.Log("Error: Could not instantiate room at " + new Vector3(position.y * gridSize, 0, position.x * gridSize));
            }

        }
    }

    /// <summary>
    /// Vai buscar uma sala qualquer para raiz
    /// </summary>
    /// <returns> Um prefab de uma sala para servir de raiz</returns>
    private GameObject GetRandomRoot(bool iceRoot)
    {
        if (iceRoot)
        {
            return iceRooms[RoomDir.Root][Random.Range(0, iceRooms[RoomDir.Root].Count)];
        }
        return rooms[RoomDir.Root][Random.Range(0, rooms[RoomDir.Root].Count)];
    }

    /// <summary>
    /// Escolhe uma sala ao calhas
    /// </summary>
    /// <param name="direction">Direção de entrada na sala</param>
    /// <returns>Prefab para instanciar</returns>
    private GameObject GetRandomRoom(RoomDir direction)
    {
        List<GameObject> list = rooms[direction];
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Retorna uma sala sem saidas, para fechar o mapa
    /// </summary>
    /// <param name="direction">Direção por onde o player vai entrar na sala</param>
    /// <param name="iceRoom">Se o pai é de gelo</param>
    /// <returns>Sala sem saidas, para fechar o mapa</returns>
    private GameObject GetFinalRoom(RoomDir direction)
    {
        return finalRooms[direction][Random.Range(0, finalRooms[direction].Count)];
    }

    /// <summary>
    /// Retorna um corredor aleatório
    /// </summary>
    /// <param name="direction">Direção de entrada no corredor</param>
    /// <param name="iceRoom">Se o pai é de gelo</param>
    /// <returns>Prefab de um corredor para instanciar</returns>
    private GameObject GetRandomCorridor(RoomDir direction, TreeNode<Room> parent)
    {
        //Só retorna corredor de gelo se o pai for sala de gelo
        if (parent.Data.IceRoom && parent.Data.RoomType == RoomType.Room)
        {
            return iceCorridors[direction][Random.Range(0, iceCorridors.Count)];
        }
        else
        {
            return corridors[direction][Random.Range(0, corridors.Count)];
        }
    }

    //Ainda nao pus para dar gelo
    /// <summary>
    /// Retorna um corredor que tem que vir obrigatóriamente antes de uma sala final
    /// </summary>
    /// <param name="direction">Direção de entrada no corredor</param>
    /// <returns>Corredor final</returns>
    private GameObject GetRandomFinalCorridor(RoomDir direction)
    {
        return finalCorridors[direction][Random.Range(0, finalCorridors[direction].Count)];
    }

    /// <summary>
    /// Vai buscar a próxima posição para onde dar spawn da sala
    /// </summary>
    /// <returns>A posição onde dar o próximo spawn</returns>
    private Vector2 GetNewPosition()
    {
        //supostamente faz (0,0), (1,0), (2,0) e quando chega aos 10, sobe de linha
        Vector2 lastVector = roomPositions[roomPositions.Count - 1];
        if (lastVector.x < maxSpawnWidth - 1)
        {
            return lastVector + Vector2.right;
        }
        else
        {
            return new Vector2(0, lastVector.y + 1);
        }
    }

    /// <summary>
    /// Sabendo a instância da sala (pai do portal ou assim), ir buscar o node correcto na árvore
    /// </summary>
    /// <param name="obj">Sala instanciada cujo node queremos</param>
    /// <returns></returns>
    public TreeNode<Room> GetTreeNode(GameObject obj)
    {
        //TODO ver se isto do equals funciona
        //Mas em principio dá porque instanciamos a sala e depois fazemos a Room, logo a referencia será a mesma
        //senão temos que repensar isto de como a partir do obj vou buscar o node, 
        //porque vai ficar mais complicado se isto nao der
        foreach (TreeNode<Room> room in treeNodes)
        {
            if (room.Data.roomInstance.Equals(obj))
            {
                return room;
            }
        }
        return null;
    }




}
