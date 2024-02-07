
using System.Collections.Generic;

[System.Serializable]
//holds a list of the spawnable object ratios for a dungeon level
public class SpawnableObjectsByLevel<T>
{
    public DungeonLevelSO dungeonLevel;

    public List<SpawnableObjectRatio<T>> spawnableObjectRatios;
}

