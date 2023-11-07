
using System.Collections.Generic;
using UnityEngine;
public class BSpline : MonoBehaviour
{
    private LineRenderer _renderer;
    [SerializeField] private List<int> _knotVector; 
    [SerializeField]  private List<Vector3> controlPoints;
    [SerializeField]  private List<GameObject> controlPointsSceneRef;
    [SerializeField] private List<Vector3> spline;
    [SerializeField] private int grade=2;
    [SerializeField] private float deltaT=0.1f;
    [SerializeField] private float splineUpdateInterval = 2;
    [SerializeField] private float addControlPointInterval = 4;
    // Start is called before the first frame update
    private void Awake()
    {
        _renderer = GetComponent<LineRenderer>();
        _renderer.startWidth = 0.1f;
        _renderer.endWidth = 0.1f;
        _renderer.loop = false;
    }
    
    void Start()
    {
        InvokeRepeating(nameof(AddControlPoint),1,addControlPointInterval);
        InvokeRepeating(nameof(DrawSpline),1,splineUpdateInterval);
        // for (float i = 0f; i < (_controlPoints.Count - grade); i += deltaT) //changing here only extends line through last node
        // {
        //     spline.Add(EvaluateBSpline(i));
        // }
        //
        // _renderer.positionCount = spline.Count;
        // _renderer.SetPositions(spline.ToArray());
    }


    private void DrawSpline()
    //sets new knotvector, evalutes points for spline and sends spline to line renderer
    {
        _renderer.positionCount = 0;
        SetupKnotVector();
        spline.Clear();
        for (float i = 0f; i < (controlPoints.Count - grade); i += deltaT) //changing here only extends line through last node
        {
            spline.Add(EvaluateBSpline(i));
        }

        _renderer.positionCount = spline.Count;
        _renderer.SetPositions(spline.ToArray());
   
        // Destroy(_renderer,splineUpdateInterval);
    }
    private void AddControlPoint()
    //use to add during runtime
    {
        controlPoints.Add(transform.position);
    }
    
    void SetupKnotVector()
    {
        _knotVector.Clear();
        var d = grade;
        var n = controlPoints.Count;
        int innerKnot = 0;
        //make knot vector with clamping such that the spline interpolates the endpoints (of the controlpoints)
        for (int i = 0; i <= (d+n + 1); i++)
        {
            if (i < d + 1)
            {
                _knotVector.Add(0); //start d+1 clamp
              
            }
            if (i > d + 1 && i <= n) //innerknots
            {
                innerKnot += 1;
                _knotVector.Add(innerKnot);
                
            }
            if (i>n)
                _knotVector.Add(innerKnot+1); //end d+1 clamp;
        }
    }
    void SetupControlPoints()
    //use if controlpoints are set pre simulation
    {
       foreach (var t in controlPointsSceneRef)
       {
           controlPoints.Add(t.transform.position);
       }
    }

    int FindKnotInterval(float x) //find U, right interval
    {
        var n = controlPoints.Count;
        // Tu<=x<Tu+1
        int my = n - 1; //index to last control point equal upper bound of indifference
        while (x < _knotVector[my])
            my--;
        return (my);
        
    }

    Vector3 EvaluateBSpline(float x)
    { //de boor algorithm
        var d = grade;
        int my = FindKnotInterval(x);
        List<Vector3> point=new List<Vector3>();
        for( var i=0;i<=d;i++)
            point.Add(Vector3.zero);// init size
        
        for (int j = 0; j <= d; j++)
        {
            point[d - j] = controlPoints[my - j]; //bugggg? trying -1 gets right start but does not iterate through all nodes
            Debug.Log("Index: "+ (my-j));
        }

        for (int k = d; k > 0; k--)
        {
            int a = my - k;
            for (int l = 0; l < k; l++)
            {
                a++;
                float w = (x - _knotVector[a]) / (_knotVector[a + k] - _knotVector[a]);
                point[l] = (point[l] * (1 - w) + point[l + 1] * w);
            }
        }
        return point[0];
    }
}