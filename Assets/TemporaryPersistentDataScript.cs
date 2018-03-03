using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TemporaryPersistentDataScript 
{
    private static Vector2 destinationPosition;

    static TemporaryPersistentDataScript()
    {
        destinationPosition = new Vector2(-23, 4);
    }
	

    public static void setDestinationPosition(Vector2 pos)
    {
        destinationPosition = pos;
    }
    public static Vector2 getDestinationPosition()
    {
        return destinationPosition;
    }
}
