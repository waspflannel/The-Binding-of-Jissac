using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{

    #region Tooltip
    [Tooltip("MovementDetailsSO scriptable object containing movement details such as speed")]
    #endregion Tooltip

    [SerializeField] private MovementDetailsSO movementDetails;



    private Player player;

    private bool leftMouseDownPreviousFrame = false;
    private float moveSpeed;
    private int currentWeaponIndex = 1;
    private Coroutine playerRollCoroutine;
    private WaitForFixedUpdate waitForFixedUpdate;
    private bool isPlayerRolling = false;
    private float playerRollCooldownTimer = 0f;

    private void Start()
    {
        
        waitForFixedUpdate = new WaitForFixedUpdate();
        SetStartingWeapon();//Set the starting weapon for player
    }

    //loop through all weapons in the players weaponList
    //if it matches the respective starting weapon set in playerDetails
    //then set that weapon as starting
    private void SetStartingWeapon()
    {

        int index = 1;
        //iterates through player.weaponList, if we dont run CreatePlayerStartingWeapons
        //the list will be empty..
        foreach (Weapon weapon in player.weaponList)
        {
            if(weapon.weaponDetails == player.playerDetails.startingWeapon)
            {
                SetWeaponByIndex(index);
                break;
            }
      
            index++;
        }
    }
    //we are setting the starting weapon that we found matches the playerDetailsSO
    private void SetWeaponByIndex(int index)
    {
        if(index - 1 < player.weaponList.Count)
        {
            
            currentWeaponIndex = index;
            //publish since we are setting active weapon(starting)
            player.setActiveWeaponEvent.CallSetActiveWeapon(player.weaponList[index - 1]);
            Debug.Log("setting starting weapon");
        }
    }

    private void Awake()
    {
        player = GetComponent<Player>();
        moveSpeed = movementDetails.GetMoveSpeed();

    }

    public void Update()
    {
        //if player is rolling return straight away
        if (isPlayerRolling) return;


        //process player movement input
        MovementInput();
        //process player weapon input
        WeaponInput();

        PlayerRollCooldownTimer();
    }

    //player movement input
    private void MovementInput()
    {
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");

        bool rightMouseButtonDown = Input.GetMouseButtonDown(1);

        Vector2 direction = new Vector2(horizontalMovement, verticalMovement);
        //adjust speed for diagonal movements
        if(horizontalMovement != 0f && verticalMovement != 0f)
        {
            direction *= 0.7f;
        }

        //if there is movement either move or roll
        if (direction != Vector2.zero)
        {
            if (!rightMouseButtonDown)
            {
                //trigger movement event
                //send the args to the publisher
                player.movementByVelocityEvent.CallMovementByVelocityEvent(direction, moveSpeed);
            }
            //else player roll if not cooling down
            else if(playerRollCooldownTimer <=0f)
            {
               PlayerRoll((Vector3)direction);
            }
        }
        else
        {
            //send the args to the publisher
            player.idleEvent.CallIdleEvent();
        }

    }
    private void PlayerRoll(Vector3 direction)
    {
        playerRollCoroutine = StartCoroutine(PlayerRollRoutine(direction));
    }

    private IEnumerator PlayerRollRoutine(Vector3 direction)
    {
        //min distance used to decide when to exit coroutine loop
        float minDistance = 0.2f;
        isPlayerRolling = true;

        //get the roll distance
        //calculated by the current position + the direction being faced and the distance that was set in SO
        Vector3 targetPosition = player.transform.position + (Vector3)direction * movementDetails.rollDistance;

        //whilethe distance is > than min distance
        while(Vector3.Distance(player.transform.position , targetPosition) > minDistance)
        {
            //send the args to the publisher
            player.movementToPositionEvent.CallMovementToPosition(targetPosition, player.transform.position, movementDetails.rollSpeed, 
                direction, isPlayerRolling);

            //this waits and pauses the coroutine until the next physics update
            yield return waitForFixedUpdate;
        }
        isPlayerRolling = false;
        playerRollCooldownTimer = movementDetails.rollCooldownTime;
        player.transform.position = targetPosition;
    }

    private void PlayerRollCooldownTimer()
    {
        if(playerRollCooldownTimer >= 0f)
        {
            playerRollCooldownTimer -= Time.deltaTime;
        }
    }
    //weapon input
    private void WeaponInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        //Aim weapon input
        //we will calculate the values for all the parameters and use out keyword to update them 
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);
        FireWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);
        SwitchWeaponInput();
        ReloadWeaponInput();

        

    }

    private void SwitchWeaponInput()
    {
        //mouse wheel
        if(Input.mouseScrollDelta.y < 0f)
        {
            PreviousWeapon();
        }
        if(Input.mouseScrollDelta.y > 0f)
        {
            NextWeapon();
        }
        #region Keyboard Number Inputs
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetWeaponByIndex(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetWeaponByIndex(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetWeaponByIndex(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetWeaponByIndex(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetWeaponByIndex(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SetWeaponByIndex(6);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SetWeaponByIndex(7);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SetWeaponByIndex(8);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SetWeaponByIndex(9);
        }
        #endregion Keyboard Number Inputs
    }

    private void NextWeapon()
    {
        currentWeaponIndex++;
        if(currentWeaponIndex > player.weaponList.Count)
        {
            currentWeaponIndex = 1;
        }
        SetWeaponByIndex(currentWeaponIndex);
    }
    private void PreviousWeapon()
    {
        currentWeaponIndex--;
        if(currentWeaponIndex < 1)
        {
            currentWeaponIndex = player.weaponList.Count;
        }
        SetWeaponByIndex(currentWeaponIndex);
    }


    private void AimWeaponInput(out Vector3 weaponDirection , out float weaponAngleDegrees, out float playerAngleDegrees, out AimDirection playerAimDirection)
    {
        Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();

        //calculate direction vector of mouse cursor from weaponshootposition
        //weaponshootposition is the transform defined in inspector
        weaponDirection = (mouseWorldPosition - player.activeWeapon.GetShootPosition());


        //calculate direction vector from mouse cursor from player transform position
        Vector3 playerDirection = (mouseWorldPosition - transform.position);

        //get weapon to cursor angle
        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        //get player to cursor angle
        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        //set player aim direction
        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

        //trigger weapon aim event
        //now that this is called with the proper info all other methods that are subscribed will also get the information
        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);


    }

    private void FireWeaponInput(Vector3 weaponDirection, float weaponAngleDegrees, float playerAngleDegrees, AimDirection playerAimDirection)
    {
        if (Input.GetMouseButton(0))
        {
            //publish the OnWeaponFireEvent
            player.fireWeaponEvent.CallOnWeaponFire(true, leftMouseDownPreviousFrame, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
            leftMouseDownPreviousFrame = true;
        }
        else
        {
            leftMouseDownPreviousFrame = false;
        }
    }
    private void ReloadWeaponInput()
    {
        Weapon currentWeapon = player.activeWeapon.GetCurrentWeapon();

        //if already reloading
        if (currentWeapon.isWeaponReloading)
        {
            return;
        }
        //if theres more ammo in the clip than total ammo we have and we dont have infinite ammo in the weapon details
        if(!currentWeapon.weaponDetails.hasInfiniteAmmo && currentWeapon.weaponClipRemainingAmmo > currentWeapon.weaponRemainingAmmo)
        {
            return;
        }
        //if the ammo is full(ammo in clip is same as the one set in the weapon details)
        if(currentWeapon.weaponClipRemainingAmmo == currentWeapon.weaponDetails.weaponClipAmmoCapacity)
        {
            return;
        }
        //if r key is clicked , publish to reloadWeaponEvent, send in currentWeapon.
        if (Input.GetKeyDown(KeyCode.R))
        {
            player.reloadWeaponEvent.CallOnReloadWeaponEvent(player.activeWeapon.GetCurrentWeapon(), 0);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StopPlayerRollRoutine();
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        StopPlayerRollRoutine();
    }
    private void StopPlayerRollRoutine()
    {
        if(playerRollCoroutine != null)
        {
            StopCoroutine(playerRollCoroutine);
            isPlayerRolling = false;
        }
    }

    #region Validation
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValues(this, nameof(movementDetails), movementDetails);
    }
    #endregion Validation
}
