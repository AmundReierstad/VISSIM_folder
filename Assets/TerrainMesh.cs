using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;

public class TerrainMesh : MonoBehaviour
{
    private MeshFilter _meshFilter;

    [SerializeField] TextAsset textFile;
    [SerializeField]private double _maxX;
    [SerializeField]private double _minX;
    [SerializeField]private double _maxY;
    [SerializeField]private double _minY;
    [SerializeField]private double _maxZ;
    [SerializeField]private double _minZ;
    [SerializeField] private int readInterval=10;
    [SerializeField] private double triangulationSquareSize = 5;
    // List<List<List<double>>> _grid=new List<List<List<double>>>();
    private List<double>[,] Grid;
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        using var stream = new StreamReader(new MemoryStream(textFile.bytes));
        using var reader = new StreamReader(stream.BaseStream);
        getCoordinateBounds(reader);
        reader.DiscardBufferedData();
        reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin); 
        setUpGrid(_maxX,_minX,_maxY,_minY,reader);
        SetUpMesh();
   
    }

    private void getCoordinateBounds(StreamReader reader) //works
    {
         _maxX = double.MinValue;
         _maxY = double.MinValue;
         _maxZ = double.MinValue;
         _minX = double.MaxValue;
         _minY = double.MaxValue;
         _minZ = double.MaxValue;
         
     
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
                _maxY = Math.Max(_maxY, y);
                _minY = Math.Min(_minY, y);
                _maxZ = Math.Max(_maxZ, z);
                _minZ = Math.Min(_minZ, z);
            }
        }
    }

    void setUpGrid(double maxX, double minX, double maxY, double minY, StreamReader reader)
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
                    Vector3 temp = new Vector3((float)x, (float)z, (float)y); //swap z and y, unity axises...
                    Debug.DrawLine(temp,(temp+Vector3.one*0.1f),Color.green,float.MaxValue,false); 
                    Grid[(int)(x / triangulationSquareSize),(int)(y / triangulationSquareSize)].Add(z);
                }
            }
        }
    }

    void SetUpMesh()
    {
        int xRange = Grid.GetUpperBound(0)+1; //returns index of last element, range therefore +1
        int yRange = Grid.GetUpperBound(1)+1;
        // double rangeZ = _maxZ - _minZ;
        Mesh newMesh = new Mesh();
        List<Vertex> vertices = new List<Vertex>();
        List<int> triangles = new List<int>();
        for (int i=0;i<xRange;i++) //x axis
        {
            for (int j = 0; j < yRange; j++) // y axis
            {
                double z=Grid[i,j].Average();
                var tempTransform=new Vector3((float)(i*triangulationSquareSize+triangulationSquareSize/2),(float)z,(float)(j*triangulationSquareSize+triangulationSquareSize/2)); //!unity uses y as up axis insert read z into y slot
               
                var tempVertex = new Vertex(tempTransform, Vector3.one, Vector2.zero);
                vertices.Add(tempVertex);
                // Debug.Log("vertex added");
            }
        }
        //setting triangles
        for (int x = 0; x < xRange-1; x++) //x
        {  
            for (int y = 0; y < yRange-2; y++)
            {   
                triangles.Add(x*yRange+y);
                triangles.Add((x+1)*yRange+y);
                triangles.Add(x*yRange+1+(y+1));
                //  3
                // I \
                // I  \
                // I   \
                // I    \
                //*1_____2
                //left lower
                //* current index (x*yRange+y)
        
                triangles.Add((x+1)*yRange+y);
                triangles.Add((x+1)*yRange+1+(y+1));
                triangles.Add(x*yRange+1+(y+1));
                //   3____2
                //   \    I
                //    \   I
                //     \  I
                //      \ I 
                //  *     1
                //right upper
        
            }
           
        }
        Debug.Log("yrange:"+yRange);
        Debug.Log("xrange:"+xRange);
        newMesh.vertices = vertices.Select(v => v.Position).ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.triangles=newMesh.triangles.Reverse().ToArray(); //flip normals
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();
        _meshFilter.mesh = newMesh;
        
        //debugging
       // for (int i=0;i<vertices.Count;i++)
       // {
       //     Vector3 temp = vertices[i].Position+Vector3.one*0.1f;
       //     if (i==0 || i==1 || i==yRange )
       //         Debug.DrawLine(vertices[i].Position,temp,Color.green,float.MaxValue,false); 
       //     else
       //         Debug.DrawLine(vertices[i].Position,temp,Color.red,float.MaxValue,false); 
       //  }
        Grid = null; //free memory
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
