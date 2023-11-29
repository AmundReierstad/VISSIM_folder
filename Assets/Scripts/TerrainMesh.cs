using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Serialization;

public class TerrainMesh : MonoBehaviour
    //class for building and loading the terrain mesh
{
    #region Members
    private MeshFilter _meshFilter;
    [SerializeField] TextAsset textFile;
    [SerializeField] public  double _maxX;
    [SerializeField]private double _minX;
    [SerializeField] public double _maxZ;
    [SerializeField]private double _minZ;
    [SerializeField]private double _maxY; 
    [SerializeField]private double _minY;
    [SerializeField] private int readInterval=10;
    [SerializeField] public double triangulationSquareSize = 5;
    [SerializeField] public bool drawPointCloud = false;
    public int zRange;
    private List<double>[,] Grid;
    #endregion

    #region Constructors
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        using var stream = new StreamReader(new MemoryStream(textFile.bytes));
        using var reader = new StreamReader(stream.BaseStream);
        GetCoordinateBounds(reader);
        reader.DiscardBufferedData();
        reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin); 
        SetUpGrid(_maxX,_minX,_maxZ,_minZ,reader);
        SetUpMesh();
    }
    #endregion

    #region Methods
    private void GetCoordinateBounds(StreamReader reader) 
    {
         _maxX = double.MinValue;
         _maxZ = double.MinValue;
         _maxY = double.MinValue;
         _minX = double.MaxValue;
         _minZ = double.MaxValue;
         _minY = double.MaxValue;
         
     
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] parts = line.Trim().Split(' ');
            if (parts.Length >= 3)
            {
                double x = double.Parse(parts[0]);
                double y = double.Parse(parts[1]);
                double z = double.Parse(parts[2]);
                _maxX = Math.Max(_maxX, x);
                _minX = Math.Min(_minX, x);
                _maxZ = Math.Max(_maxZ, y);
                _minZ = Math.Min(_minZ, y);
                _maxY = Math.Max(_maxY, z);
                _minY = Math.Min(_minY, z);
            }
        }
    }

    void SetUpGrid(double maxX, double minX, double maxY, double minY, StreamReader reader)
    {
        int xRange = (int)Math.Round(maxX / triangulationSquareSize);
        int yRange = (int)Math.Round(maxY / triangulationSquareSize);
        Grid = new List<double>[xRange,yRange];
        for (var i = 0; i < xRange; i++)
        {
            for (var j = 0; j < yRange; j++)
            {
                Grid[i, j] = new List<double>();
            }
        }
        
        int counter = 0;
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (counter < readInterval)
            {
                counter++;
            }
            else
            {
                counter = 0;
                string[] parts = line.Trim().Split(' ');
                if (parts.Length >= 3)
                {
                    double x = double.Parse(parts[0]);
                    double y = double.Parse(parts[1]);
                    double z = double.Parse(parts[2]);
                    //draw point-cloud:
                    if (drawPointCloud)
                    {
                        var temp = new Vector3((float)x, (float)z, (float)y); //swap z and y, unity axises...
                        Debug.DrawLine(temp,(temp+Vector3.one*0.1f),Color.green,float.MaxValue,false); 
                    }
                    Grid[(int)(x / triangulationSquareSize),(int)(y / triangulationSquareSize)].Add(z);
                }
            }
        }
    }

    void SetUpMesh()
    {
        int xRange = Grid.GetUpperBound(0) + 1; //returns index of last element, range therefore +1
        zRange = Grid.GetUpperBound(1) + 1;
        Mesh newMesh = new Mesh();
        List<Vertex> vertices = new List<Vertex>();
        List<int> triangles = new List<int>();
        for (int i = 0; i < xRange; i++) //x axis
        {
            for (int j = 0; j < zRange; j++) // y axis
            {
                double y = Grid[i, j].Average();
                var tempTransform = new Vector3((float)(i * triangulationSquareSize + triangulationSquareSize / 2),
                    (float)y,
                    (float)(j * triangulationSquareSize +
                            triangulationSquareSize / 2)); //!unity uses y as up axis insert read z into y slot

                var tempVertex = new Vertex(tempTransform, Vector3.one, Vector2.zero);
                vertices.Add(tempVertex);
            }
        }
        //unity is using clockwise winding order
        //setting triangles
        for (int x = 0; x < xRange - 1; x++) // unity X
        {
            for (int y = 0; y < zRange - 1; y++) //unity Z
            {
                triangles.Add(x * zRange + y); //1
                triangles.Add(x * zRange + (y + 1)); //2
                triangles.Add((x + 1) * zRange + y); //3
                //  2
                // I \
                // I  \
                // I   \
                // I    \
                //*1_____3
                //left lower
                //* current index (x*yRange+y)

                triangles.Add((x + 1) * zRange + y);//1
                triangles.Add(x * zRange + (y + 1));//2
                triangles.Add((x + 1) * zRange + (y + 1));//3
                //   2____3
                //   \    I
                //    \   I
                //     \  I
                //      \ I 
                //  *     1
                //right upper
            }
        }
  
        newMesh.vertices = vertices.Select(v => v.Position).ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();
        _meshFilter.mesh = newMesh;
        Grid = null; //free memory
    
    }
    #endregion
}
