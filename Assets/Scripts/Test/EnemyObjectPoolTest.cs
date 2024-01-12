using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObjectPoolTest : MonoBehaviour
{

    [SerializeField] private EnemyAnimationDetails[] enemyAnimationDetailsArray;
    [SerializeField] private GameObject enemyExamplePrefab;
    private float timer = 1f;

    [System.Serializable]
    public struct EnemyAnimationDetails
    {
        public RuntimeAnimatorController controller;
        public Color spriteColor;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0f)
        {
            GetEnemyExample();
            timer = 1f;
        }
    }

    private void GetEnemyExample()
    {
        //get info for ReuseComponent method
        Room currentRoom = GameManager.Instance.GetCurrentRoom();
        Vector3 spawnPosition = new Vector3(Random.Range(currentRoom.lowerBounds.x, currentRoom.upperBounds.x),
            Random.Range(currentRoom.lowerBounds.y, currentRoom.upperBounds.y));

        //this does the recycling of gameobjects
        EnemyAnimation enemyAnimation = (EnemyAnimation)PoolManager.Instance.ReuseComponenet(enemyExamplePrefab,
            HelperUtilities.GetSpawnPositionNearestToPlayer(spawnPosition), Quaternion.identity);
        ////////
        

        ////get random element from array
        int randomIndex = Random.Range(0, enemyAnimationDetailsArray.Length);
        enemyAnimation.gameObject.SetActive(true);//activate it

        //set it using random element
        enemyAnimation.SetAnimation(enemyAnimationDetailsArray[randomIndex].controller, enemyAnimationDetailsArray[randomIndex].spriteColor);
    }

}
