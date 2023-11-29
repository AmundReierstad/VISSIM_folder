using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Vertex 
    //simple class to store vertex data
{
    #region Members
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 Uv;
    #endregion

    #region Constructor
    public Vertex(Vector3 pos, Vector3 norm, Vector2 uv)
    {
        Position = pos;
        Normal = norm;
        Uv = uv;
    }
    #endregion

    #region Methods
    public static Vertex LoadFromFile(StreamReader reader)
    {
        string line = reader.ReadLine();
        // Split the source string at ') (' to get separate vectors
        string[] parts = line.Trim('(', ')').Split(new string[] { ") (" }, StringSplitOptions.None);

        if (parts.Length != 3)
        {
            throw new FormatException($"The line '{line}' is invalid. It should contain exactly 2 Vector3s and 1 Vector2.");
        }

        // Parse the vectors, now that they are properly separated
        var position = Utilites.ParseVector3(parts[0]);
        var normal = Utilites.ParseVector3(parts[1]);
        var uv = Utilites.ParseVector2(parts[2]);

        return new Vertex(position, normal, uv);
    }
    #endregion
}
