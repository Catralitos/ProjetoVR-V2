using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerationManager : MonoBehaviour
{
    //Isto é para o teleporter conseguir encontrar o Manager facilmente
    public static GenerationManager instance;

    //Prefabs de salas de onde escolher
    public Dictionary<RoomDir, List<GameObject>> rooms;
    //Prefabs de salas de onde escolher salas sem saida (finais)
    public Dictionary<RoomDir, List<GameObject>> finalRooms;
    //Prefabs de corredores de onde escolher
    public Dictionary<RoomDir, List<GameObject>> corridors;

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

    private void Awake()
    {
        instance = this;

        int lenght = System.Enum.GetValues(typeof(RoomDir)).Length;

        //Meter keys para cada direção no inspetor, poupar algum trabalho
        for (int i = 1; i < lenght; i++)
        {
            rooms.Add((RoomDir)i, new List<GameObject>());
            finalRooms.Add((RoomDir)i, new List<GameObject>());
            corridors.Add((RoomDir)i, new List<GameObject>());
        }

    }

    void Start()
    {
        //Escolher primeira sala aleatoriamente
        GameObject firstRoom = GetRandomRoot();

        //Instanciar essa sala
        GameObject aux = Instantiate(firstRoom, Vector3.zero, Quaternion.identity, this.gameObject.transform);

        //Criar raiz da árvore (depois de instanciar, porque instancia != prefab e porque só se cria o node se instanciar bem)
        //Nao sei se isto do root é preciso, mas só quero ter a certeza que o if do SpawnChildren da direção diferente não rebenta
        treeRoot = new TreeNode<Room>(new Room(aux, RoomType.Room, RoomDir.Root));

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
        //Ja nao precisamos desse codigo porque metemos a bool Generated aliás ia dar erro se tivessemos isto
        //Nao queremos que cries uma saida onde entraste, nao queremos dar override no que já definimos para os portais
        //directions.Remove(node.Data.EntranceDirection);
        foreach (RoomDir direction in directions)
        {
            GameObject obj;
            RoomType type;
            //TODO possivelmente introduzir uma chance (pequena), de mesmo sem chegar ao limite dar spawn de um beco sem saida
            //para o player ter que voltar atrás e experimentar paths diferentes
            //mas temos que ter cuidado para não chegar o caso em que mesmo sem atingir o depth, já é tudo becos sem saida

            //Se for sala tem que ser corredor
            //E vou por abaixo que se um corredor for spawned na depth final, ele consegue ainda dar spawn de uma sala final
            if (node.Data.RoomType == RoomType.Room)
            {
                obj = GetRandomCorridor(direction, node.Data.IceRoom);
                type = RoomType.Corridor;
            }
            else
            {
                //Se a próxima sala a dar spawn for no limite dá spawn de sala final
                //Se o corredor for o limite, ultrapassa o limite para dar spawn de uma sala final
                if (depthLimit > -1 && (node.Level == depthLimit - 1 || node.Level == depthLimit))
                {
                    obj = GetFinalRoom(direction, node.Data.IceRoom);
                    type = RoomType.Room;
                }
                else
                {
                    //Só ver se metade das vezes é corredor ou sala
                    float random = Random.Range(0, 1);
                    if (random <= 0.5)
                    {
                        obj = GetRandomRoom(direction, node.Data.IceRoom);
                        type = RoomType.Room;
                    }
                    else
                    {
                        obj = GetRandomCorridor(direction, node.Data.IceRoom);
                        type = RoomType.Corridor;
                    }
                }
            }

            Vector2 position = GetNewPosition();
            if (Instantiate(obj, new Vector3(position.x * gridSize, 0, position.y * gridSize), Quaternion.identity, this.gameObject.transform) != null)
            {
                //Passar parametros aos portais do pai para fazerem bem a ligação
                //TODO ver se da para cortar o GetComponents
                foreach (Teleporter portal in node.Data.roomInstance.GetComponentsInChildren<Teleporter>())
                {
                    //Se o pai/currente tiver 2 portais tenho de saber qual vai ligar
                    if (!portal.Generated && portal.direction == direction)
                    {
                        //Vai do pai para o filho
                        portal.SetRooms(node.Data.roomInstance, obj);
                        portal.Generated = true;
                    }
                }

                //Passar parametros aos portais do filho para fazerem bem a ligação
                //TODO ver se da para cortar o GetComponents
                foreach (Teleporter portal in obj.GetComponentsInChildren<Teleporter>())
                {
                    //Se o filho tiver 2 portais tenho de saber para onde vai (partilham direcao)
                    if (!portal.Generated && portal.direction == direction)
                    {
                        //Vai do filho para o pai
                        portal.SetRooms(obj, node.Data.roomInstance);
                        portal.Generated = true;
                    }
                }
                TreeNode<Room> child = new TreeNode<Room>(new Room(obj, type, direction), node);
                treeNodes.Add(child);
                //Não faço no GetPosition porque só aqui é que dou spawn da sala
                roomPositions.Add(position);
            }
            else
            {
                Debug.Log("Error: Could not instantiate room at " + position);
            }

        }
    }

    /// <summary>
    /// Vai buscar uma sala qualquer para raiz
    /// </summary>
    /// <returns> Um prefab de uma sala para servir de raiz</returns>
    private GameObject GetRandomRoot()
    {
        List<GameObject> aux = rooms[(RoomDir)Random.Range(0, rooms.Count)];
        return aux[Random.Range(0, aux.Count)];
    }

    /// <summary>
    /// Escolhe uma sala ao calhas
    /// </summary>
    /// <param name="direction">Direção de entrada na sala</param>
    /// <param name="iceRoom">Se o pai é de gelo</param>
    /// <returns>Prefab para instanciar</returns>
    private GameObject GetRandomRoom(RoomDir direction, bool iceRoom)
    {
        //TODO meter isto com gelo, nao sei se é outra lista ou o que é
        List<GameObject> list = rooms[direction];
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Retorna uma sala sem saidas, para fechar o mapa
    /// </summary>
    /// <param name="direction">Direção por onde o player vai entrar na sala</param>
    /// <param name="iceRoom">Se o pai é de gelo</param>
    /// <returns>Sala sem saidas, para fechar o mapa</returns>
    private GameObject GetFinalRoom(RoomDir direction, bool iceRoom)
    {
        //TODO meter isto com gelo, nao sei se é outra lista ou o que é
        List<GameObject> list = finalRooms[direction];
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Retorna um corredor aleatório
    /// </summary>
    /// <param name="direction">Direção de entrada no corredor</param>
    /// <param name="iceRoom">Se o pai é de gelo</param>
    /// <returns>Prefab de um corredor para instanciar</returns>
    private GameObject GetRandomCorridor(RoomDir direction, bool iceRoom)
    {
        //TODO meter isto com gelo, nao sei se é outra lista ou o que é
        List<GameObject> list = corridors[direction];
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Vai buscar a próxima posição para onde dar spawn da sala
    /// </summary>
    /// <returns>A posição onde dar o próximo spawn</returns>
    private Vector2 GetNewPosition()
    {
        //TODO ir buscar a posição.
        //melhor forma de manter isto organizado é imaginar uma grid e vamos preenchendo diagonalmente
        //imagina começar em 0,0
        //entao a proxima posiçao vai ser (0,0)>(1,0)>(0,1)>(2,0)>(1,1)>(0,2)>(3,0)>(2,1)>(1,2)>(0,3) e assim sucessivamente
        //mas nao consegui fazer, portanto fica assim
        ; Vector2 lastVector = roomPositions[roomPositions.Count - 1];
        int x = 0, y = 0;
        if (lastVector.x < maxSpawnWidth)
        {
            x = (int)lastVector.x + 1;
        }
        else
        {
            y = (int)lastVector.y + 1;
        }
        return new Vector2(x, y);
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
