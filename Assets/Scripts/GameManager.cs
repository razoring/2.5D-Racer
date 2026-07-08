using System.Numerics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject road; //prefab
    public float rate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GameObject strip1 = Instantiate(road, new UnityEngine.Vector3(0,0f,0), UnityEngine.Quaternion.identity);
        /*
        GameObject strip2 = Instantiate(road, new UnityEngine.Vector3(0,0.05f,0), UnityEngine.Quaternion.identity);
        GameObject strip3 = Instantiate(road, new UnityEngine.Vector3(0,0.1f,0), UnityEngine.Quaternion.identity);
        GameObject strip4 = Instantiate(road, new UnityEngine.Vector3(0,0.15f,0), UnityEngine.Quaternion.identity);*/
    }

    public float getRate()
    {
        return rate;
    }
}
