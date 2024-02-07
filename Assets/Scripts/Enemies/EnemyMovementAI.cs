using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Enemy))]
public class EnemyMovementAI : MonoBehaviour
{
    public MovementDetailsSO movementDetails;
    private Enemy enemy;
    private Stack<Vector3> movementSteps = new();
    private Vector3 playerReferencePosition;
    private Coroutine moveEnemyRoutine;
    private float currentEnemyPathRebuildCooldown;
    private WaitForFixedUpdate waitForFixedUpdate;
    [HideInInspector] public float moveSpeed;
    private bool chasePlayer = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        //create waitforfixedupdate
        waitForFixedUpdate = new WaitForFixedUpdate();
        //reset the players reference position
        playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();
    }

    private void Update()
    {
        MoveEnemy();
    }

    private void MoveEnemy()
    {
        currentEnemyPathRebuildCooldown -= Time.deltaTime;

        //if not chasing and the enemy distance and player distance is the chase range
        if(!chasePlayer && Vector3.Distance(transform.position , GameManager.Instance.GetPlayer().GetPlayerPosition()) < enemy.enemyDetails.chaseDistance){
            chasePlayer = true;

        }
        if (!chasePlayer)
        {
            return;
        }
        //if movement cooldown timer is reached or the player has moved enough to need a rebuild then rebuild the path
        if(currentEnemyPathRebuildCooldown <=0f || (Vector3.Distance(playerReferencePosition,GameManager.Instance.GetPlayer().GetPlayerPosition())
            > Settings.playerMoveDistanceToRebuildPath))
        {
            currentEnemyPathRebuildCooldown = Settings.enemyPathRebuildCooldown;
            playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

            //trigger the AStar , will fill movementSteps
            CreatePath();


            //if we have a path for enemy(returned from AStar)
            if (movementSteps != null)
            {
                //if theres a movement routine happening
                if (moveEnemyRoutine != null)
                {
                    enemy.idleEvent.CallIdleEvent();
                    StopCoroutine(moveEnemyRoutine);
                }
                //start a new routine
                moveEnemyRoutine = StartCoroutine(MoveEnemyRoutine(movementSteps));
            }
        }

    }

    private void CreatePath()
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();
        //get the grid of current room
        Grid grid = currentRoom.instantiatedRoom.grid;
        //get the enemy grid position by using worldtocell on its transform.position
        Vector3Int enemyGridPosition = grid.WorldToCell(transform.position);

        Vector3Int playerGridPosition = GetNearestNonObstaclePlayerPosition(currentRoom);

        movementSteps = AStar.BuildPath(currentRoom, enemyGridPosition, playerGridPosition);

        //pop off the first step from the stack because its the enemies current location
        if(movementSteps != null)
        {
            movementSteps.Pop();

        }
        //if we have no movement steps call the idle event
        else
        {
            enemy.idleEvent.CallIdleEvent();
        }
    }
    private Vector3Int GetNearestNonObstaclePlayerPosition(Room currentRoom)
    {
        //get players position
        Vector3 playerPosition = GameManager.Instance.GetPlayer().GetPlayerPosition();
        //convert it to a grid position
        Vector3Int playerCellPosition = currentRoom.instantiatedRoom.grid.WorldToCell(playerPosition);

        //adjust it with template lower boudns
        Vector2Int adjustedPlayerCellPosition = new Vector2Int(playerCellPosition.x - currentRoom.templateLowerBounds.x,
            playerCellPosition.y - currentRoom.templateLowerBounds.y);

        //
        int obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x, adjustedPlayerCellPosition.y];

        //remember, obstacle tiles are 0 on the room grid so if its !=0 its not an obstacle
        if(obstacle != 0)
        {
            return playerCellPosition;
        }
        //if we are on an obstacle
        else
        {
            //lopo through the tiles around it
            for(int i=-1; i <= 1; i++)
            {
                for(int j=-1; j<= 1; j++)
                {
                    if(i ==0 && j == 0)
                    {
                        continue;
                    }
                    try
                    {
                        //get the tile next to the obstacle we want to check
                        int tileNextToObstacleTile = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x+i , adjustedPlayerCellPosition.y+j];
                        //if its not an obstacle
                        if(tileNextToObstacleTile != 0)
                        {
                            //return the position of the tile that isnt an obstacle
                            return new Vector3Int(playerCellPosition.x +i, playerCellPosition.y +j ,0);

                        }
                    }
                    //if we get an error , just continue to the next iteration
                    catch
                    {
                        continue;
                    }
                }
            }
            //if we still havnt found a non obstacle position just return player position
            //rare but can happen
            return playerCellPosition;
        }
    }

    private IEnumerator MoveEnemyRoutine(Stack<Vector3> movementSteps)
    {
        while(movementSteps.Count > 0)
        {
            //get the next position
            Vector3 nextPosition = movementSteps.Pop();
            //while not close to the next position move and when < 0.2 move to the next one
            while(Vector3.Distance(nextPosition,transform.position) > 0.2f)
            {
                //trigger the movement event
                //since MovementToPosition is a subscriber and is on the enemy prefab
                //it will get this as args and move the enemy
                enemy.movementToPositionEvent.CallMovementToPosition(nextPosition, transform.position, moveSpeed, (nextPosition -
                    transform.position).normalized);

                //yield return will slowly move it but when it breaks out of the while loop
                yield return waitForFixedUpdate;
            }

            //this yield return will run the first while loop again
            yield return waitForFixedUpdate;
        }

        enemy.idleEvent.CallIdleEvent();
    }

    #region Validation
#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValues(this, nameof(movementDetails), movementDetails);
    }

#endif

    #endregion
}
