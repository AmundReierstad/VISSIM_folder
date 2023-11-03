using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PhysicsLogic : MonoBehaviour
{
        [SerializeField] private float mass = 1;
        [SerializeField] private float accelerationFactor = 1;
        [SerializeField] private float bouncinessFactor=1;
        [SerializeField]  private GameObject _planeRef;
        private GameObject _ballRef;
        private float _ballRadius;
        private MeshFilter _planeMeshRef;
        private Vector3 _origoGrid;
        private double _gridSquareSize;
        private Vector3 _planeNormalVector;
        private Vector3 _planeNormalVectorPrev;
        private Vector3 _gravityForce;
        private Vector3 _normalForce;
        private Vector3 _velocity;
        private Vector3 _startVelocity;
        private Vector3 _acceleration;
        private int _prevIndex;
        private int _planeRefZRange;
        private bool _onGrid;
    public struct CollisionData
    {
        public CollisionStates State;
        public float MagnitudeY;
    }
    public enum CollisionStates
    {
        InContact,
        ClippedThrough,
        ClippingMassCenterAboveSurface,
        ClippingMassCenterBelowSurface,
        AboveSurface
    }

        
    // Start is called before the first frame update
    void Awake()
    {
        _planeRef = GameObject.Find("TerrainMesh");
        _ballRef = GameObject.Find("Ball");
        
        _ballRadius = _ballRef.GetComponent<SphereCollider>().radius;
        _planeMeshRef = _planeRef.GetComponent<MeshFilter>();
        _gridSquareSize = _planeRef.GetComponent<TerrainMesh>().triangulationSquareSize;
        _gravityForce = Physics.gravity * mass*accelerationFactor;    //Physics.gravity , can be used as gravity force vector, vec 3, with -9.81 y component
   
        _startVelocity=Vector3.zero;
        _acceleration=Vector3.zero;
        _velocity = Vector3.zero;
        _origoGrid=_planeMeshRef.mesh.vertices[0];
    }

    void Start()
    {
        _planeRefZRange = _planeRef.GetComponent<TerrainMesh>().zRange; //needed for calculation further on
        // int startIndex = GetWhichTriangleBallIsIn();
        int startIndex = FindInitialTriangleOptimized();
        if (startIndex != -1) _onGrid = true;
        // Debug.Log("Index calculated:"+FindInitialTriangleOptimized());
        _prevIndex = startIndex;
        _planeNormalVector = CalculateNormalizedNormalForTriangleSurface(startIndex);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //DONT UPDATE IF NOT ON GRID
        if (!_onGrid) return;
        
        //determine position in relation to mesh
        int newIndex = GetWhichTriangleBallIsInOptimized(_prevIndex);
        if (newIndex == -1) {_onGrid = false; Debug.Log(name+" fell off"); return;}
        _prevIndex = newIndex;
        _planeNormalVectorPrev = _planeNormalVector;
        _planeNormalVector=CalculateNormalizedNormalForTriangleSurface(newIndex);
        
        //Calculate forces:
        //NormalF
        _normalForce =-Vector3.Dot(_planeNormalVector,_gravityForce)*_planeNormalVector;
       // CollisionResponse(index);
       if (_planeNormalVectorPrev != _planeNormalVector) //reached edge, reflect velocity around plane between angles
       {
           /*Reflection vector: r=d−2(d⋅n)n,
            r= reflection vector, 
            d=incoming vector(for our purposes velocity), 
            n=normalized normal-vector of reflection-plane*/
           Vector3 reflectionPlaneNormal = (_planeNormalVectorPrev + _planeNormalVector).normalized;
           _velocity = _velocity - 2*(Vector3.Dot(_velocity, reflectionPlaneNormal)) * reflectionPlaneNormal;
       }
        
        //calculate acceleration from sum of forces over mass
        _acceleration = (_normalForce+_gravityForce)/mass;
        //calculate velocity
        _velocity+=_startVelocity+_acceleration*Time.fixedDeltaTime;
        //apply velocity and translate ball
        transform.Translate(_velocity*Time.fixedDeltaTime);
       
        // Debug.Log("Velocity:"+_velocity.magnitude+"  Acceleration:"+_acceleration.magnitude+"  CurrentPlaneNormal:"+_planeNormalVector);
    }

    Vector3 CalculateNormalizedNormalForTriangleSurface(int index)
    {
        var mesh = _planeMeshRef.mesh;
        
        Vector3 ab = mesh.vertices[mesh.triangles[index]]-mesh.vertices[mesh.triangles[index+1]];
        Vector3 ac= mesh.vertices[mesh.triangles[index]]-mesh.vertices[mesh.triangles[index+2]];
        Vector3 normal = Vector3.Cross(ab,ac).normalized;
        return normal;
    }

    private Vector3 ReturnBarycentricCordsXZplane(Vector3 ballPosition, Vector3 a, Vector3 b, Vector3 c)
    {
        //check in relation to XZ plane, therefore set Y to 0 for all points
        ballPosition.y = 0;
        a.y = 0;
        b.y = 0;
        c.y = 0;
        Vector3 baryCentricCoordinates = Vector3.zero;
        float temp;
        //declare and set vectors we need
        Vector3 ballA = a - ballPosition;
        Vector3 ballB = b - ballPosition;
        Vector3 ballC = c - ballPosition;

        Vector3 aB = b - a;
        Vector3 aC = c - a;
            //calculate coordinates
            float abcArea= Vector3.Cross(aB,aC).y; //could divide by 2 for actual area, but redundant for its use later on because of same factor under divider+ better performance
        
            //U coordinate / A influence
            temp = Vector3.Cross(ballB, ballC).y / abcArea;
            baryCentricCoordinates.x = temp;
            
            //v coordinate / B influence
            temp = Vector3.Cross(ballC, ballA).y / abcArea;
            baryCentricCoordinates.y = temp;
            
            //w coordinate/ C influence
            temp = Vector3.Cross(ballA, ballB).y / abcArea;
            baryCentricCoordinates.z = temp;
        
        return baryCentricCoordinates;
    }

    private int GetWhichTriangleBallIsIn() 
    //returns index of starting vertex of triangle ball is in. O(n triangles)
    {
        var mesh = _planeMeshRef.mesh;
        
        //iterate over triangles
        for (int i = 0; i <= mesh.triangles.Length-3; i+=3)
        {
            Vector3 baryCentricCords = ReturnBarycentricCordsXZplane(
                this.transform.position,
                mesh.vertices[mesh.triangles[i]],
                mesh.vertices[mesh.triangles[i + 1]],
                mesh.vertices[mesh.triangles[i + 2]]);
            // Debug.Log("CurrentIndex: "+i);
            // Debug.Log("Barys: "+baryCentricCords);
            if (baryCentricCords.x is >= 0 and <= 1)
            {
                if (baryCentricCords.y is >= 0 and <= 1)
                {
                    if (baryCentricCords.z is >= 0 and <= 1)
                    {
                        Debug.Log("Index found>"+i);
                        return i;
                    }
                }
            }
          

        }
        Debug.Log("Ball not found on triangle surface initially");
        return 0; //not found
    }

    private int FindInitialTriangleOptimized() 
    //returns index of starting triangle by calculating grid position by world position. O(1) much faster
    {
        var objectPosition = transform.position;
        
        var dif = objectPosition - _origoGrid;
        int offsetX = (int)(dif.x / _gridSquareSize);
        int offsetZ = (int)(dif.z / _gridSquareSize);
        int triangle = 3;
        
        var index = ((offsetX) * (_planeRefZRange-1)*triangle*2) //x component
                    +(offsetZ)*(triangle*2); //y component

        // Debug.DrawLine(mesh.vertices[mesh.triangles[index]],mesh.vertices[mesh.triangles[index]]-=Vector3.one*0.1f,Color.cyan,float.MaxValue,false);
        //found triangle index corresponding to lower triangle of a quad, need to determine if its in the upper or lower triangle
        var mesh = _planeMeshRef.mesh;
        float tmpX = objectPosition.x - mesh.vertices[mesh.triangles[index]].x;
        float tmpZ = objectPosition.z - mesh.vertices[mesh.triangles[index]].z;
        if (tmpX>tmpZ)//test diagonal relation to square its in
            index += 3;
        // Debug.Log("TmpX:"+tmpX);
        // Debug.Log("TmpZ:"+tmpZ);
        
        
        
        // Debug.Log("difX:"+dif.x);
        // Debug.Log("offsetX:"+offsetX);
        // Debug.Log("offsetZ:"+offsetZ);
        return index;
    }
    private int GetWhichTriangleBallIsInOptimized(int prevIndex) 
    //returns index of starting vertex of triangle ball is in by searching around previous index. 
    {
        var zRange = _planeRefZRange;
        var mesh = _planeMeshRef.mesh;
        int triangle = 3;
        /* search triangles around previous index *
         *    I  I  I
         * ^  I  *  I
         * Z  I  I  I
         *    X ->
         */
        for (int j = -1; j <=2; j++)
        {
            int searchIndex = prevIndex+((zRange - 1) * 3 * 2*j); // (zRange - 1) * 3 * 2*j) factor to shift to next strip
            if (searchIndex < 2*triangle || searchIndex>mesh.triangles.Length-2*triangle) break; //bounds check
            //iterate over current z triangle strip
            for (int i = searchIndex-2*triangle; i <= searchIndex+2*triangle; i+=triangle)
            {
                Vector3 baryCentricCords = ReturnBarycentricCordsXZplane(
                    this.transform.position,
                    mesh.vertices[mesh.triangles[i]],
                    mesh.vertices[mesh.triangles[i + 1]],
                    mesh.vertices[mesh.triangles[i + 2]]);
                // Debug.Log("CurrentIndex: "+i);
                // Debug.Log("Barys: "+baryCentricCords);
                if (baryCentricCords.x is >= 0 and <= 1)
                {
                    if (baryCentricCords.y is >= 0 and <= 1)
                    {
                        if (baryCentricCords.z is >= 0 and <= 1)
                        {
                            // Debug.Log("Index found>"+i);
                            return i;
                        }
                    }
                }
            }
        }
        Debug.Log("Ball not found on triangle surface");
        return -1; //not found
    }
    public CollisionData DetectCollisionWithTriangleSurface(Vector3 pointInSurface, Vector3 surfaceNormalVector, Vector3 ballPosition, float radiusBall)
   {
       CollisionData returnData;
        var y = ballPosition-pointInSurface;
        var n = surfaceNormalVector;
        /* ~y~n = ~n · (~y · ~n),
            ~ signifies vector, 
            y~n~ signfies y vector projected onto n vector
            y is the vector from pointInSurface-ballPositon-> y=ballPositon-pointInSurface
            n vector is the normal vector of the plane */
        var yProjectedOnN = n * Vector3.Dot(y, n);
        /*
         The relation between the radius of the ball and y(projected on n) 
         and the relation of the mass center to the plane, given by m=y*n/|n| determines the collison states:
         |y|=r: ball is in contact with surface
         |y|<r and m positive: ball is clipping into surface, with masscenter above the surface
         |y|<r and m negative: ball is clipping into surface, with masscenter below the surface
         |y|>r, and m negative: ball has clipped through surface
         |y|>r, and m positive: ball is above surface
         */
        float massCenterRelationToPlane = Vector3.Dot(y, n) / y.magnitude;
        returnData.MagnitudeY = yProjectedOnN.magnitude;
        if (Math.Abs(yProjectedOnN.magnitude - radiusBall) < 0.1f)
        {
            returnData.State = CollisionStates.InContact;
            return returnData;
        }
        if (yProjectedOnN.magnitude < radiusBall)
        {
            if (massCenterRelationToPlane > 0)
            {
                returnData.State = CollisionStates.ClippingMassCenterAboveSurface; 
            }

            returnData.State = CollisionStates.ClippingMassCenterBelowSurface;
            return returnData;
        }
        if (massCenterRelationToPlane < 0)
            returnData.State = CollisionStates.ClippedThrough;
        
        returnData.State = CollisionStates.AboveSurface;
        return returnData;

    }
    public void CollisionResponse(int indexOfCurrentTriangle)
    {
        var mesh = _planeMeshRef.mesh;
        CollisionData currentData=
            DetectCollisionWithTriangleSurface(mesh.vertices[mesh.triangles[indexOfCurrentTriangle]], _planeNormalVector,
                this.transform.position, _ballRadius);

        CollisionStates currentCollisionState = currentData.State;
        switch (currentCollisionState)
        {
            case CollisionStates.AboveSurface:
                _normalForce = Vector3.zero;
                break;
            case CollisionStates.InContact:
                _normalForce =-Vector3.Dot(_planeNormalVector,_gravityForce)*_planeNormalVector;
                _velocity = (_velocity - 2*(Vector3.Dot(_velocity, _planeNormalVector)) * _planeNormalVector)*bouncinessFactor;
                break;
            case CollisionStates.ClippedThrough:
            {
                _normalForce =-Vector3.Dot(_planeNormalVector,_gravityForce)*_planeNormalVector;
                _ballRef.transform.Translate(_planeNormalVector*(currentData.MagnitudeY+_ballRadius)); //translate back onto surface
                break;
            }
            case CollisionStates.ClippingMassCenterAboveSurface:
            {
                _normalForce =-Vector3.Dot(_planeNormalVector,_gravityForce)*_planeNormalVector;
                _ballRef.transform.Translate(_planeNormalVector*(_ballRadius-currentData.MagnitudeY)); //translate back onto surface
                break;
            }
            case CollisionStates.ClippingMassCenterBelowSurface:
            {
                _normalForce =-Vector3.Dot(_planeNormalVector,_gravityForce)*_planeNormalVector;
                _ballRef.transform.Translate(_planeNormalVector*(_ballRadius+currentData.MagnitudeY));
                break;
            }
        }
    }
}
