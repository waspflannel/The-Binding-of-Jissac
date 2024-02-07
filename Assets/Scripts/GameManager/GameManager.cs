using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]//allows only 1 component
public class GameManager : SingletonMonobehaviour<GameManager>//this ensures only 1 can be there at one time
{
    #region Header DUNGEON LEVELS
    [Space(10)]
    [Header("DUNGEON LEVELS")]
    #endregion Header DUNGEON LEVELS

    #region Tooltip
    [Tooltip("Populate with the dungeon level scriptable objects")]
    #endregion Tooltip
    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

    #region Tooltip
    [Tooltip("Populate with the starting dungeon level for testing , first level =0")]
    #endregion Tooltip
    [SerializeField] private int currentDungeonListIndex = 0;

    private Room currentRoom;
    private Room previousRoom;
    private PlayerDetailsSO playerDetails;
    private Player player;

    [HideInInspector] public GameState gameState; //refers to enum GameState
   
    protected override void Awake()
    {
        base.Awake();
        //set player details - saved in current player SO 
        playerDetails = GameResources.Instance.currentPlayer.playerDetails;

        //instantiate player
        InstantiatePlayer();
    }
    //create player in scene at a position
    private void InstantiatePlayer()
    {
        //get the prefab from playerDetails and instantiate
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);
        //after getting the prefab which is stored in playerGameObject get the player by using getComponent
        player = playerGameObject.GetComponent<Player>();
        //call the init function in player class with playerDetails
        player.Initialize(playerDetails);
    }

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetCurrentRoom(roomChangedEventArgs.room);
    }




    // Start is called before the first frame update
    private void Start()
    {
        gameState = GameState.gameStarted;
    }

    // Update is called once per frame
    private void Update()
    {
        //Debug.Log(player.activeWeapon.GetCurrentWeapon().weaponDetails.weaponName);
        HandleGameState();
    }

    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:
                //play first level
                PlayDungeonLevel(currentDungeonListIndex);
                gameState = GameState.playingLevel;
                break;
        }
    }

    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;
    }

    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        //build dungeon for level
        bool dungeonBuiltSucessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);
        if (!dungeonBuiltSucessfully)
        {
            Debug.LogError("Couldnt build dungeon from specified rooms and node graphs");
        }
        StaticEventHandler.CallRoomChangedEvent(currentRoom);
        //get the player roughly mid room by calcualting the avg of the current room
        player.gameObject.transform.position = new Vector3((currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f, (currentRoom.lowerBounds.y +
            currentRoom.upperBounds.y) / 2f, 0f);

        //theres a chance it may put us in an invalid spot so get nearest spawn point in room to player
        player.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(player.gameObject.transform.position);


    }
    public Player GetPlayer()
    {
        return player;
    }

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    #region Validation
    private void OnValidate()
    {
        //make sure the dungoen level list is populated
        HelperUtilities.ValidateCheckEnumerateValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }

    public DungeonLevelSO GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonListIndex];
    }

    #endregion Validation
}
