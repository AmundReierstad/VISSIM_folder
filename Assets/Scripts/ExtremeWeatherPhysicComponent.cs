using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class ExtremeWeatherPhysicComponent : MonoBehaviour
    //class managing the extreme weather physics of the game object, specifically dragforces when object is within a liquid stream
{
    #region Members
    [SerializeField] private float colliderRadius=3;
    [SerializeField] private bool _on;
    [SerializeField] private bool _onUpdate;
    [SerializeField] private float thresholdSplineProximity=3;
    [SerializeField] private float updateFrequencyForProximalRainDrops=1f;
    private List<GameObject> proximalRainDrops;
    private List<List<Vector3>> splinesFromRainDrops;
    private List<Vector3> velocitiesFromRainDrops;
    private SphereCollider _collider;
    private float _mass;
    private float _radiusBall;
    private float _areaBall;
    private const float DragCoefficient = 0.46f; //as for sphere
    private Vector3 _acceleration;
    private Vector3 _direction;
    private Vector3 _flowVelocity;
    private Vector3 _velocity;
    private const float MassDensityOfFluid = 1000; //kg/m^3, as for water
    private bool _radiusUpdated;
    #endregion

    #region Constructors
    private void Awake()
    {
        proximalRainDrops = new List<GameObject>();
        splinesFromRainDrops = new System.Collections.Generic.List<System.Collections.Generic.List<Vector3>>();
        velocitiesFromRainDrops = new List<Vector3>();
        _radiusUpdated = false;
        _on = false;
        _onUpdate = false;
    }
    void Start()
    {
    
        _collider = gameObject.GetComponent<SphereCollider>();
        _collider.isTrigger = true;
        _radiusBall = _collider.radius;
        _areaBall = (float)((float) Math.PI * Math.Pow(_radiusBall,2));
        _mass = gameObject.GetComponent<PhysicsLogic>().GetMass();
    }
    private void LateUpdate()
    {
        if (_radiusUpdated) return;
        _collider.radius = colliderRadius;
        _radiusUpdated = true;
    }
    #endregion

    #region Methods
    void FixedUpdate()
    {
        if (proximalRainDrops.Count<2) return;
        if(!_on )
        {
            _on = true;
            InvokeRepeating(nameof(UpdateRainDrops),2,updateFrequencyForProximalRainDrops);
            InvokeRepeating(nameof(FindAccelerationVector),2.1f,updateFrequencyForProximalRainDrops);
            return;//must return to get data from first invoke before continuting update loop
        };
        // if (!_on) return;
        /*drag equation
         Fd=1/2 * p * u^2* Cd * A
         Fd= dragForce
         p= _mass density fluid ; water=997 kg/m³
         Cd= drag coefficient; related to the objects shape, in this instance a sphere=0.47; 
         A=reference area, in this case the maximal cross sectional area of the sphere; =PI*r^2
        */
        if (!_onUpdate) return;
        var u =CalculateFluidVelocity();
        var p = MassDensityOfFluid;
        var A = _areaBall;
        var Cd = DragCoefficient;
        // f=ma ;a=f/m
        _acceleration = (0.5f*p *(float)Math.Pow(u,2)*Cd * A)/(_mass) * _direction;
        _velocity += _acceleration * Time.fixedDeltaTime;
        if (_velocity.magnitude > u) _velocity = _velocity.normalized * u; //clamp velocity to max velocity of stream
         transform.Translate(_velocity*Time.fixedDeltaTime);
    }
    private void FindAccelerationVector()
    {
        _onUpdate = true;
        //finds the mean of the points upstream from proximal splines/streams
        List<Vector3> proximalPointsUpStream = new List<Vector3>();

        #region FindProximalTangents

        int i = 0;
        //approximate proximal point on spline eulers method
        foreach (var spline in splinesFromRainDrops)
        {
            #region SplineParams
            var splineLength = spline.Count;
            i += 1;
            // Debug.Log("spline"+i+" capacity:"+spline.Count);
            #endregion
            #region Init
            int prevIndex=0;
            int index = splineLength- (splineLength / 2);
            // Debug.Log("spline"+i+" startIndex:"+index);
            float prevDistance=(transform.position - spline[0]).magnitude;
            int rounds = 0;
            var indexDif = splineLength / 2;
            #endregion
            while (rounds<10) //try to find index corresponding to proximal point on spline, over max 10 loops
            {
                //check if point is within proximity threshold
                var distance = (transform.position - spline[index]).magnitude;
                if (thresholdSplineProximity>prevDistance) 
                {
                 
                    break;
                }
                
                //calculate in which direction to traverse spline
                if (prevDistance > distance) 
                {
                    index += (indexDif/2);
                    
                }
                else index -= (indexDif/2);
                //update index, difindex and distance for next loop
                indexDif = indexDif / 2;
                prevIndex = index;
                prevDistance = distance;
                //update round parameter
                rounds += 1;
            }
            
            //calculate point upstream from the proximal index
            var upStreamPoint = (spline[index + 1]);
            proximalPointsUpStream.Add(upStreamPoint);
        }
        #endregion
        
        #region CalculateAccelerationVector
        //calculate acceleration vector by mean of the tangents
        Vector3 a=Vector3.zero;
        foreach (var point in proximalPointsUpStream)
        {
            a += point;
        }
        a /= proximalPointsUpStream.Count;
        _direction =(a - transform.position).normalized;
        #endregion
    }
    private float CalculateFluidVelocity()
    {
        Vector3 v=Vector3.zero;
        foreach (var velocity in velocitiesFromRainDrops)
        {
            v += velocity;
        }
        v /= velocitiesFromRainDrops.Count;
        return v.magnitude;
    }
    
    #region CollisionMethods
    private void OnTriggerEnter(Collider other)
    {
        var o = other.gameObject;
        if(proximalRainDrops.Count<3)
            proximalRainDrops.Add(o);
    }
    #endregion
    
    #region Utility
    void UpdateRainDrops()
    //updates proximal raindrop parameters
    {
        splinesFromRainDrops.Clear();
        velocitiesFromRainDrops.Clear();
        foreach (var raindrop in proximalRainDrops)
        {
            splinesFromRainDrops.Add(raindrop.GetComponent<BSpline>().GetSpline());
            velocitiesFromRainDrops.Add(raindrop.GetComponent<Rigidbody>().velocity);
        }
        
    }
    void OnDrawGizmosSelected()
        //renders collider raidus in the editor
    {
        Gizmos.color=Color.red;
        Gizmos.DrawWireSphere(transform.position, colliderRadius);
    }
    #endregion
    #endregion

}
