using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class Door : MonoBehaviour
{

    [SerializeField] private BoxCollider2D doorCollider;
    [HideInInspector] public bool isBossRoomDoor = false;
    private BoxCollider2D doorTrigger;
    private bool isOpen = false;
    private bool previouslyOpened = false;
    private Animator animator;

    private void Awake()
    {
        //disable door collider by default because of entrance
        doorCollider.enabled = false;

        animator = GetComponent<Animator>();
        doorTrigger = GetComponent<BoxCollider2D>();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if player triggers the door or the weapon triggers the door
        if(collision.tag ==Settings.playerTag || collision.tag == Settings.playerWeapon)
        {
            Debug.Log("opening Door");
            OpenDoor();
        }
    }

    private void OnEnable()
    {
        //when parent is disabled the animator gets reset so we need to reset it
        //so set it to wether its been opened or not
        animator.SetBool(Settings.open, isOpen);
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            previouslyOpened = true;
            doorCollider.enabled = false;
            doorTrigger.enabled = false;
            animator.SetBool(Settings.open, true);
            
        }
    }

    public void LockDoor()
    {
        isOpen = false;
        doorCollider.enabled = true;
        doorTrigger.enabled = false;
        //close door animation
        animator.SetBool(Settings.open, false);
    }

    public void UnlockDoor()
    {
        doorCollider.enabled = false;
        doorTrigger.enabled = true;
        if (previouslyOpened)
        {
            isOpen = false;
            OpenDoor();
        }
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValues(this, nameof(doorCollider), doorCollider); 
    }
#endif

}
