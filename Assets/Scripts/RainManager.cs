
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
public class RainManager : MonoBehaviour
{
    #region Members
    [SerializeField] private GameObject rainDropPrefab;
    [SerializeField] List<GameObject> _population;
    [SerializeField] float lifeTime=4;
    [SerializeField] float size=50;
    [SerializeField] float maxPopulation=10;
    private Vector2 _spawnSize;
    #endregion

    #region Constructor
    public RainManager()
    {
        _spawnSize = Vector2.one * size;
    }
    void Start()
    {
        InvokeRepeating("SpawnRainDrop",0,0.4f);
    }
    #endregion

    #region Methods
    Vector3 GetRandomRainDropPosition()
    {
        float x = Random.Range(-_spawnSize.x, _spawnSize.x);
        float z = Random.Range(-_spawnSize.y, _spawnSize.y);

        Vector3 position = transform.position; //get parent position

        position.x += x;
        position.z += z;
        return position; //add random offset and return;
    }
    void SpawnRainDrop()
    {
        if (_population!=null)
        {
             if (_population.Count > maxPopulation)
             {
                 _population.RemoveAll(x => !x);
                 return;
             }
        }
        GameObject rainDrop = Instantiate(rainDropPrefab,GetRandomRainDropPosition(),Quaternion.identity); //quaterionon.id->no change in rotation
        _population.Add(rainDrop);
        Destroy(rainDrop,lifeTime);
    }

    void OnDrawGizmosSelected()
    //renders spawn area in the editor
    {
        Gizmos.color=Color.red;
        Vector3 spawnBound = new Vector3(_spawnSize.x, 0, _spawnSize.y)*2;
        Gizmos.DrawWireCube(transform.position, spawnBound);
    }
    #endregion

}