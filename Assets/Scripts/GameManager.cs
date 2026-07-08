using System.Numerics;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject road; //prefab
    public float rate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bool finished = false;
        float edge = Camera.main.ScreenToWorldPoint(new UnityEngine.Vector3(0f, 0f, 0f)).y - 1f;
        GameObject last = null;

        while (finished == false) {
            Debug.Log("Hello World");
            float size = 0;
            float pos = 0;
            if (last!=null)
            {
                size = last.transform.localScale.y/2;
                pos = last.transform.position.y;
            }
            GameObject strip = Instantiate(road, new UnityEngine.Vector3(0,pos+size,0), UnityEngine.Quaternion.identity);
            last = strip;
            if (transform.position.y < edge)
            {
                finished = true;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        /*
        GameObject strip2 = Instantiate(road, new UnityEngine.Vector3(0,0.05f,0), UnityEngine.Quaternion.identity);
        */
    }

    public float getRate()
    {
        return rate;
    }
}
