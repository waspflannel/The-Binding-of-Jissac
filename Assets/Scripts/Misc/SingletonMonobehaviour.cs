
using UnityEngine;

//the point of this singleton class is to make sure only 1 instance is allowed at any given time
public abstract class SingletonMonobehaviour<T>: MonoBehaviour where T: MonoBehaviour
{
    private static T instance;

    public static T Instance//getter for instance
    {
        get
        {
            return instance;
        }
    }
    //virtual allows method to be overidden by inherited classes
    //this gets called on awake()
    protected virtual void Awake()
    {
        if(instance == null)//if there is no instance
        {
            instance = this as T;//set instance to T
        }
        else//already a game manager in scene
        {
            Destroy(gameObject);//then destroy it
        }
    }
}
