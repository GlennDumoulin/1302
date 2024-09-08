using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class CoordinatesHelper
{
    //Convert the double coordinates system to world coordinates
    public static Vector3 DoubleCoordinatesToWorld(Vector2 doubleCoordinates, Vector2 dimensions)
    {
        float x = doubleCoordinates.x * dimensions.x;
        float y = doubleCoordinates.y * (0.75f * dimensions.y);

        return new Vector3(x , 1, y * 2);
    }

    //Convert the double coordinates system to qrs
    public static Vector3 DoubleCoordinatesToQrs(Vector2 doubleCoordinates)
    {
        int r = (int)(doubleCoordinates.y);
        int q = (int)((doubleCoordinates.x - doubleCoordinates.y) / 2);
        int s = -q - r;

        return new Vector3(q, r, s);
    }
}
