using UnityEngine;

public static class Helpers
{
    public static float Map(float min1, float max1, float min2, float max2, float value)
    {
        float normalizedValue = Mathf.InverseLerp(min1, max1, value);
        float newValue = Mathf.Lerp(min2, max2, normalizedValue);
        return newValue;
    }

    public static Vector3Int ToVector3Int(this Vector3 vector)
    {
        return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
    }

/*    public static Direction Opposite(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Left: return Direction.Right;
            case Direction.Right: return Direction.Left;
            case Direction.Up: return Direction.Down;
            case Direction.Down: return Direction.Up;
            default: return Direction.None;
        }
    }

    public static Quaternion GetRotation(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Down: return Quaternion.Euler(0,180,0);
            case Direction.Left: return Quaternion.Euler(0,-90,0);
            case Direction.Right: return Quaternion.Euler(0,90,0);
            case Direction.Up: 
            default: return Quaternion.Euler(0, 0, 0);
        }
    }*/
}
