using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this sets up a table for spawning enemies.
//then we can pick a random value from the boundaries and get a random enemy based on its ratio
public class RandomSpawnableObject<T>
{
    private struct chanceBoundaries
    {
        public T spawnableObject;
        public int lowBoundaryValue;
        public int highBoundaryValue;
    }

    private int ratioValueTotal = 0;
    private List<chanceBoundaries> chanceBoundariesList = new();
    private List<SpawnableObjectsByLevel<T>> spawnableObjectsByLevelList;

    //constructor. when init get the right type + ratio for what we want to spawn
    public RandomSpawnableObject(List<SpawnableObjectsByLevel<T>> spawnableObjectsByLevelList)
    {
        this.spawnableObjectsByLevelList = spawnableObjectsByLevelList;
    }
    
    public T GetItem()
    {   
        //set base values
        int upperBoundary = -1;
        ratioValueTotal = 0;
        chanceBoundariesList.Clear();
        T spawnableObject = default(T);

        //loop through all SpawnableObjectsByLevel. 
        //these hold all the ratios for a specific dungeon level
        foreach (SpawnableObjectsByLevel<T> spawnableObjectsByLevel in spawnableObjectsByLevelList)
        {
            //check if current level matches
            if(spawnableObjectsByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                //if they match loop through the ratios that are held in the spawnableObjectsByLevel ratio list
                foreach (SpawnableObjectRatio<T> spawnableObjectRatio in spawnableObjectsByLevel.spawnableObjectRatios)
                {
                    //set upper and lower boundaries from the ratios
                    int lowerBoundary = upperBoundary + 1;
                    upperBoundary = lowerBoundary + spawnableObjectRatio.ratio - 1;
                    //increment the total ratio value
                    ratioValueTotal += spawnableObjectRatio.ratio;

                    //make a new table entry with the object and its upper and lower ratios
                    chanceBoundariesList.Add(new chanceBoundaries()
                    {
                        spawnableObject = spawnableObjectRatio.dungeonObject,
                        lowBoundaryValue = lowerBoundary,
                        highBoundaryValue = upperBoundary,
                    });
                }
            }

        }
        //if the list is empty return the default value
        if(chanceBoundariesList.Count == 0)
        {
            return default(T);
        }
        //get a random value between the ratios
        int lookUpValue = Random.Range(0, ratioValueTotal);

        //for each table entry in the chanceboundaries list
        foreach(chanceBoundaries spawnChance in chanceBoundariesList)
        {
            //if our lookup value falls inbetween a ratio for a specific thing
            if(lookUpValue >= spawnChance.lowBoundaryValue && lookUpValue <= spawnChance.highBoundaryValue)
            {
                //get the object that the value fell between
                spawnableObject = spawnChance.spawnableObject;
                break;
            }
        }
        //return it
        return spawnableObject;
    }
}
