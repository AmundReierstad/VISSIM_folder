using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Utilites 
{
    public static string ReadUntil(StreamReader reader, char terminator)
    {
        var text = "";
        while (!reader.EndOfStream)
        {
            var nextChar = (char)reader.Peek(); // Peek next char, don't advance
            if (nextChar == terminator)
            {
                reader.Read(); // Consume the terminator before breaking
                break;
            }
            text += (char)reader.Read(); // Now read, advancing position
        }

        return text.Trim();
    }

    public static Vector3 ParseVector3(string text)
    {
        string[] parts = text.Trim('(', ')').Split(',');
        return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
    }

    public static Vector2 ParseVector2(string text)
    {
        string[] parts = text.Trim('(', ')').Split(',');
        return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
    }
}
