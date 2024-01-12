using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public static class HelperUtilities
{
    public static Camera mainCamera;

    //get the mouse world position
    public static Vector3 GetMouseWorldPosition()
    {
        if(mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        Vector3 mouseScreenPosition = Input.mousePosition;
        //clamp mouse position to screen size
        //clamps the position between 0 and the screen width/height
        mouseScreenPosition.x = Mathf.Clamp(mouseScreenPosition.x, 0f, Screen.width);
        mouseScreenPosition.y = Mathf.Clamp(mouseScreenPosition.y, 0f, Screen.height);

        //use ScreenToWorldPoint to convert screen position to world position
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        worldPosition.z = 0f;
        return worldPosition;
    }


    //get an angle in degrees from a direction vector
    //will be used to calculate the angle between player and mouse cursor
    public static float GetAngleFromVector(Vector3 vector)
    {
        float radians = Mathf.Atan2(vector.y, vector.x);
        float degrees = radians * Mathf.Rad2Deg;
        return degrees;
    }

    public static Vector3 GetDirectionVectorFromAngle(float angle)
    {
        //direction vector is d = (cos(angle) , sin(angle)) but we need the angle in radians so use Mathf.Deg2Rad 
        Vector3 directionVector = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0f);
        return directionVector;
    }

    //this method will take an angle in degrees and return the correct AimDirection enum value
    public static AimDirection GetAimDirection(float angleDegrees)
    {
        AimDirection aimDirection;
        if (angleDegrees >=22f && angleDegrees <= 67f)
        {
            aimDirection = AimDirection.UpRight;
        }
        else if (angleDegrees > 67f && angleDegrees <= 112f)
        {
            aimDirection = AimDirection.Up;
        }
        else if (angleDegrees > 112f && angleDegrees <= 158f)
        {
            aimDirection = AimDirection.UpLeft;
        }
        else if ( (angleDegrees <= 180f && angleDegrees > 158f) || (angleDegrees > -180f && angleDegrees <= -135f))
        {
            aimDirection = AimDirection.Left;
        }
        else if (angleDegrees > -135f && angleDegrees <= -45f)
        {
            aimDirection = AimDirection.Down;
        }
        else if ((angleDegrees > -45f && angleDegrees <=0f) || (angleDegrees > 0 && angleDegrees < 22f))
        {
            aimDirection = AimDirection.Right;
        }
        else
        {
            aimDirection = AimDirection.Right;
        }
        return aimDirection;

    }

    //empty stirng debug
    public static bool ValidateCheckEmptyString(Object thisObject , string fieldName , string stringToCheck)
    //looks to see if stringToCheck is empty
    {
        if (stringToCheck == "")
        {

            Debug.Log(fieldName + " is empty and must contain a value in object " + thisObject.name.ToString());
            return true;
        }
        return false;
    }


    //null value debug check
    public static bool ValidateCheckNullValues(Object thisObject , string fieldName , UnityEngine.Object objectToCheck)
    {
        if(objectToCheck == null)
        {
            Debug.Log(fieldName + "is null and must contain a value" + thisObject.name.ToString());
            return true;
        }
        return false;
    }


    //makes sure a list isnt empty and/or it doesnt have any null values
    public static bool ValidateCheckEnumerateValues(Object thisObject , string fieldName , IEnumerable enumerableObjectToCheck)
    {
        bool error = false;
        int count = 0;

        if (enumerableObjectToCheck == null)
        {
            Debug.Log(fieldName + " is null in object " + thisObject.name.ToString());
            return true;
        }
        foreach(var item in enumerableObjectToCheck)//iterate through an enumerableObject to see if there are any nulls.
        {
            if(item == null)
            {
                Debug.Log(fieldName = " has null values in object " + thisObject.name.ToString());
                error=true; 
            }
            else
            {
                count++;
            }
        }
        if(count == 0)
        {
            Debug.Log(fieldName + " has no values in object " + thisObject.name.ToString());
            error = true;
        }
        return error;

    }


    //positive v alue debug check - if zero is allowed isZeroAllowed can be passed as true 
    public static bool ValidateCheckPositiveValues(Object thisObject , string fieldName , int valueToCheck , bool isZeroAllowed)
    {
        bool error = false;
        if (isZeroAllowed)
        {
            //since 0 is allowed do we have negative numbers
            if(valueToCheck < 0)
            {
                Debug.Log(fieldName + "must contain a positive value or zero in object" + thisObject.name.ToString());
                error = true;
            }
        }
        else
        {
            //since zero is not allowed , do we have 0 or a negative
            if(valueToCheck <= 0)
            {
                Debug.Log(fieldName + "must contain a positive value or zero in object" + thisObject.name.ToString());
                error = true;
            }
        }
        return error;
    }
    //this one is the same as the one above except it accepts floats
    public static bool ValidateCheckPositiveValues(Object thisObject, string fieldName, float valueToCheck, bool isZeroAllowed)
    {
        bool error = false;
        if (isZeroAllowed)
        {
            //since 0 is allowed do we have negative numbers
            if (valueToCheck < 0)
            {
                Debug.Log(fieldName + "must contain a positive value or zero in object" + thisObject.name.ToString());
                error = true;
            }
        }
        else
        {
            //since zero is not allowed , do we have 0 or a negative
            if (valueToCheck <= 0)
            {
                Debug.Log(fieldName + "must contain a positive value or zero in object" + thisObject.name.ToString());
                error = true;
            }
        }
        return error;
    }

    public static bool ValidateCheckPositiveRange(Object thisObject, string fieldNameMinimum, float valueToCheckMinumum,  string fieldNameMaximum,
        float valueToCheckMaxium , bool isZeroAllowed)
    {
        bool error = false;
        //if the range isnt valid return false
        if(valueToCheckMinumum > valueToCheckMaxium)
        {
            Debug.Log(fieldNameMinimum + "must be less than or equal to " + fieldNameMaximum + " in object " + thisObject.name.ToString());
            error = true;
        }

        //make sure both numbers are positive
        if (ValidateCheckPositiveValues(thisObject, fieldNameMinimum, valueToCheckMinumum, isZeroAllowed))
        {
            error = true;
        }
        if (ValidateCheckPositiveValues(thisObject, fieldNameMaximum, valueToCheckMaxium, isZeroAllowed))
        {
            error = true;
        }
        return error;
    }




    public static Vector3 GetSpawnPositionNearestToPlayer(Vector3 playerPosition)
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();
        Grid grid = currentRoom.instantiatedRoom.grid;

        //set starting position to a very big value and whenever a spawn position is found
        //which is closer then update it
        Vector3 nearestSpawnPosition = new Vector3(10000f, 10000f, 0f);


        //loop thru room spawn positions
        foreach(Vector2Int spawnPositionGrid in currentRoom.spawnPositionArray)
        {
            //convert the spawn grid position to world positions
            Vector3 spawnPositionWorld = grid.CellToWorld((Vector3Int)spawnPositionGrid);

            //if distance from player to the world position from grid is less than our player to the arbitary nearestspawnposition
            //then update the nearestSpawnPosition
            if (Vector3.Distance(spawnPositionWorld , playerPosition) < Vector3.Distance(nearestSpawnPosition , playerPosition))
            {
                nearestSpawnPosition = spawnPositionWorld;
            }
        }

        return nearestSpawnPosition;
    }
}
