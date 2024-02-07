
[System.Serializable]
//if we were going to create a ratio for example enemySO, we would pass it in and dungeonObject would be that type.
public class SpawnableObjectRatio<T>
{
    public T dungeonObject;
    public int ratio;
}
