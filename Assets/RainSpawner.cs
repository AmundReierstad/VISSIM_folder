using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainSpawner : MonoBehaviour
{
    public GameObject rainPrefab;
    [SerializeField] private float timeToStartSpawning;
    [SerializeField] private float timeToStop;
    [SerializeField] private float frequency;
    [SerializeField] private float dropCount;
    [SerializeField] private float dropLifeTime;
    // Start is called before the first frame update
    void Start()
    {
        timeToStartSpawning = 0;
        timeToStop = 5;
        frequency = 3;
        dropLifeTime = 5;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > timeToStop)
        {
            for (int i = 0; i < frequency; i++)
            {
                spawnRainDrop();
                timeToStop -= Time.time;
            }
        }
    }

    private void spawnRainDrop()
    {
        if (rainPrefab)
        {
            GameObject rainDrop = Instantiate(rainPrefab);
            dropCount++;
            Destroy(rainDrop,dropLifeTime); //set drop to destroy after timetoStop seconds
            dropCount--;
            Debug.Log("Destroying object ");
        }
        else
        {
            Debug.Log("fok object "); 
        }
    }
}