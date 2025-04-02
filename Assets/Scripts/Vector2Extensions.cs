using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extensions
{
    public static Vector2 Vec2Parse(this string v) {
        string raw = v.Trim('(').Trim(')');
        string [] args = raw.Split(',');
        return new Vector2(float.Parse(args[0].Trim()), float.Parse(args[1].Trim()));
    }
}
