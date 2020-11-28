using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerationManager : MonoBehaviour
{
    //Isto é para o teleporter conseguir encontrar o Manager facilmente
    public static GenerationManager instance;

    //Prefabs de salas de onde escolher
    public List<GameObject> rooms;
    //Prefabs de salas de onde escolher salas sem saida (finais)
    public List<GameObject> finalRooms;
    //Direçoes dos portais de cada sala, para não ter que fazer GetComponent cada vez que escolher um filho
    private List<List<RoomDir>> roomDirections = new List<List<RoomDir>>();
    //Direçoes dos portais de cada sala final, para não ter que fazer GetComponent cada vez que escolher um filho final
    private List<List<RoomDir>> finalRoomDirections = new List<List<RoomDir>>();
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

    //TODO e pelos vistos temos que ver se a sala é de gelo ou não
    //bem como qualquer outro modifier que façamos

    private void Awake()
    {
        instance = this;

        //Acho que posso (e devo) ter este código no awake, ser der mal, depois logo se vê

        //Guardar num load inicial as direções de cada sala
        foreach (GameObject room in rooms)
        {
            roomDirections.Add(room.GetComponent<RoomDirections>().PortalPositions);
        }

        //Guardar num load inicial as direções de cada sala final
        foreach (GameObject room in finalRooms)
        {
            finalRoomDirections.Add(room.GetComponent<RoomDirections>().PortalPositions);
        }
    }

    void Start()
    {
        //Escolher primeira sala aleatoriamente
        GameObject firstRoom = GetRandomRoot();

        //Instanciar essa sala
        GameObject aux = Instantiate(firstRoom, Vector3.zero, Quaternion.identity);

        //Criar raiz da árvore (depois de instanciar, porque instancia != prefab e porque só se cria o node se instanciar bem)
        treeRoot = new TreeNode<Room>(new Room(aux));

        //Nao sei se isto é preciso, mas só quero ter a certeza que o if do SpawnChildren da direção diferente não rebenta
        treeRoot.Data.EntranceDirection = RoomDir.Root;

        //Instanciar o player (vai ter que ser depois de instanciar a sala, não podemos pô-lo na cena no editor)
        _ = Instantiate(player, Vector3.zero, Quaternion.identity);

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

        //Não tem filhos, e não atingiu limite, tem que instanciar filhos
        if ((newRoom.IsLeaf && depthLimit < 0) || (newRoom.IsLeaf && depthLimit > 0 && newRoom.Level < depthLimit))
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
            if (Mathf.Abs(room.Level - depth) < 2 && (newRoom.Parent == room || newRoom.HasChild(room.Data)))
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
    /// <param name="node">Node pai</param>
    private void SpawnChildren(TreeNode<Room> node)
    {
        List<RoomDir> directions = node.Data.PortalPositions;
        foreach (RoomDir direction in directions)
        {
            //Nao queremos que cries uma saida onde entraste, nao quermos dar override no que j+a definimos para os portais
            if (node.Data.EntranceDirection != direction)
            {
                GameObject obj;
                //Ver se tem que instanciar filhos sem saida
                //TODO possivelmente introduzir uma chance (pequena), de mesmo sem chegar ao limite dar spawn de um beco sem saida
                //para o player ter que voltar atrás e experimentar paths diferentes
                //mas temos que ter cuidado para não chegar o caso em que mesmo sem atingir o depth, já é tudo becos sem saida
                if (depthLimit > -1 && node.Level == depthLimit - 1)
                {
                    obj = GetFinalRoom(direction);
                }
                else
                {
                    obj = GetRandomChild(direction);
                }
                Vector3 position = GetNewPosition();
                if (Instantiate(obj, position, Quaternion.identity) != null)
                {
                    //Passar parametros aos portais do pai para fazerem bem a ligação
                    //TODO ver se da para cortar o GetComponents, e ver se isto está a chamar cada ciclo
                    //Senão buscar primeiro para uma lista e depois fazer ciclo
                    foreach (Teleporter portal in node.Data.roomInstance.GetComponentsInChildren<Teleporter>())
                    {
                        //Se o pai/currente tiver 2 portais tenho de saber qual vai ligar
                        if (portal.direction == direction)
                        {
                            //Vai do pai para o filho
                            portal.SetRooms(node.Data.roomInstance, obj);
                        }
                    }

                    //Passar parametros aos portais do filho para fazerem bem a ligação
                    //TODO ver se da para cortar o GetComponents, e ver se isto está a chamar cada ciclo
                    //Senão buscar primeiro para uma lista e depois fazer ciclo
                    foreach (Teleporter portal in obj.GetComponentsInChildren<Teleporter>())
                    {
                        //Se o filho tiver 2 portais tenho de saber para onde vai (partilham direcao)
                        if (portal.direction == direction)
                        {
                            //Vai do filho para o pai
                            portal.SetRooms(obj, node.Data.roomInstance);
                        }
                    }
                    TreeNode<Room> child = new TreeNode<Room>(new Room(obj), node);
                    child.Data.EntranceDirection = direction;
                    treeNodes.Add(child);
                    //Estamos a guardar os indices logo tenho que reconverter de volta a Vector2
                    //Não faço no GetPosition porque só aqui é que dou spawn da sala
                    roomPositions.Add(new Vector2((int)position.x / gridSize, (int)position.z / gridSize));
                }
                else
                {
                    Debug.Log("Error: Could not instantiate room at " + position);
                }
            }
        }
    }

    /// <summary>
    /// Vai buscar uma sala qualquer para raiz
    /// </summary>
    /// <returns> Um prefab de uma sala para servir de raiz</returns>
    private GameObject GetRandomRoot()
    {
        return rooms[Random.Range(0, rooms.Count)];
    }

    /// <summary>
    /// Escolhe um filho aleatóriamente que consiga ligar com aquela saida
    /// </summary>
    /// <param name="direction">Direção compativel com a saida</param>
    /// <returns></returns>
    private GameObject GetRandomChild(RoomDir direction)
    {
        //TODO nao sei se o indexOf gasta demasiado, se calhar reescrever isto com um ciclo com i++;
        List<GameObject> list = (from room in rooms
                                 where roomDirections[rooms.IndexOf(room)].Contains(direction)
                                 select room).ToList();
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Retorna uma sala sem saidas, para fechar o mapa
    /// </summary>
    /// <param name="direction">Direção por onde o player vai entrar na sala</param>
    /// <returns>sala sem saidas, para fechar o mapa</returns>
    private GameObject GetFinalRoom(RoomDir direction)
    {
        //TODO nao sei se o indexOf gasta demasiado, se calhar reescrever isto com um ciclo com i++;
        foreach (GameObject room in finalRooms)
        {
            if (finalRoomDirections[finalRooms.IndexOf(room)].Contains(direction))
            {
                return room;
            }
        }
        return null;
    }

    /// <summary>
    /// Vai buscar a próxima posição para onde dar spawn da sala
    /// </summary>
    /// <returns>A posição onde dar o próximo spawn</returns>
    private Vector3 GetNewPosition()
    {
        Vector2 nextPos = Vector2.zero;
        //TODO ir buscar a posição.
        //melhor forma de manter isto organizado é imaginar uma grid e vamos preenchendo diagonalmente
        //imagina começar em 0,0
        //entao a proxima posiçao vai ser (0,0)>(1,0)>(0,1)>(2,0)>(1,1)>(0,2)>(3,0)>(2,1)>(1,2)>(0,3) e assim sucessivamente
        //embora iste fique mais organizado assim, tendo em conta que mesmo impondo um limite de depth, nao sabes quantas salas ha
        //em vez de tentar fazer um quadrado limpo se calhar mais facil fazer um retangulo de largura fixa, e ir preenchendo linha a linha
        //(0,0)>(0,1)>(0,2)>(1,0)>(1,1)>(1,2)
        return new Vector3(gridSize * nextPos.x, 0, gridSize * nextPos.y);
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
