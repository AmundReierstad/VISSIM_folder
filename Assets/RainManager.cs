
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
public class RainManager : MonoBehaviour
{
    [SerializeField] private GameObject rainDropPrefab;
     private List<GameObject> _population;
    [SerializeField] float lifeTime=4;
    [SerializeField] float size=50;
    [SerializeField] float maxPopulation=10;
    private Vector2 _spawnSize;

    public RainManager()
    {
        _spawnSize = Vector2.one * size;
    }

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
        _population.RemoveAll(x => !x); //remove destroyed game-objects from list
        if (_population.Count > maxPopulation) return;
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
    void Start()
    {
        InvokeRepeating("SpawnRainDrop",0,0.4f);
    }

}