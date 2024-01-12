using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DisallowMultipleComponent]
public class PoolManager : SingletonMonobehaviour<PoolManager>
{

    [SerializeField] private Pool[] poolArray = null;

    private Transform objectPoolTransform;
    private Dictionary<int , Queue<Component>> poolDictionary = new Dictionary<int , Queue<Component>>();
    [System.Serializable]
    public struct Pool
    {
        public int poolSize;
        public GameObject prefab;
        public string componentType;
    }

    private void Start()
    {
        objectPoolTransform = this.gameObject.transform;

        for(int i=0; i< poolArray.Length; i++)
        {
            CreatePool(poolArray[i].prefab, poolArray[i].poolSize, poolArray[i].componentType);
        }
    }

    //create the object pool with specified prefabs and specified pool size
    private void CreatePool(GameObject prefab , int poolSize , string componentType)
    {
        int poolKey = prefab.GetInstanceID();//used to reference dictionary location
        string prefabName = prefab.name;//get prefab name

        GameObject parentGameObject = new GameObject(prefabName + "Anchor");//parent gameobject that thhings will be under
        parentGameObject.transform.SetParent(objectPoolTransform);

        if (!poolDictionary.ContainsKey(poolKey))//if its not in the dict already
        {
            poolDictionary.Add(poolKey, new Queue<Component>());//add it to the dict

            for(int i=0; i < poolSize; i++)//loop through each element in the pool
            {
                GameObject newObject = Instantiate(prefab, parentGameObject.transform) as GameObject;//create a gameobject for it

                newObject.SetActive(false);//disabled by default
                poolDictionary[poolKey].Enqueue(newObject.GetComponent(Type.GetType(componentType)));//add it to the queue
            }
        }



    }



    //used to reuse a gameobject componenet in the pool.

    //this takes a pool componenet and runs GetComponentFromPool which changes its queue position
    //then resets it and returns the componenetToReuse that has been reset and is at the end of the queue
    public Component ReuseComponenet(GameObject prefab , Vector3 position , Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();//used to reference dict

        if (poolDictionary.ContainsKey(poolKey))
        {
            Component componentToReuse = GetComponentFromPool(poolKey);
            ResetObject(position, rotation, componentToReuse, prefab);

            return componentToReuse;
        }
        else
        {
            Debug.Log("no object pool for " + prefab);//if theres no object pool specified
            return null;
        }
        
    }

    //get a gameobject componenet from the pool using the poolkey
    //it updates its queue position and disables it if enabled
    private Component GetComponentFromPool(int poolKey)
    {
        Component componentToReuse = poolDictionary[poolKey].Dequeue();//take it out

        poolDictionary[poolKey].Enqueue(componentToReuse);//put it at the end

        if(componentToReuse.gameObject.activeSelf == true)
        {
            componentToReuse.gameObject.SetActive(false);
        }

        return componentToReuse;
        
    }

    //resets the gameobject to its default
    private void ResetObject(Vector3 position, Quaternion rotation, Component componentToReuse, GameObject prefab)
    {
        //reset position , rotation and scale
        componentToReuse.transform.position = position;
        componentToReuse.transform.rotation = rotation;
        componentToReuse.transform.localScale = prefab.transform.localScale;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerateValues(this, nameof(poolArray), poolArray);
    }

#endif



}
