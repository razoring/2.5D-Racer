using System.Numerics;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject road; //prefab
    public float offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (road == null)
        {
            Debug.LogError("GameManager has no road prefab assigned.");
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("No main camera found.");
            return;
        }

        float bottomEdge = cam.ScreenToWorldPoint(new UnityEngine.Vector3(0f, 0f, 0f)).y - 1f;
        GameObject last = null;

        for (int i = 0; i < 1000; i++)
        {
            float spawnY = 0f;
            if (last != null)
            {
                SpriteRenderer lastRenderer = last.GetComponent<SpriteRenderer>();
                if (lastRenderer != null)
                {
                    spawnY = last.transform.position.y - lastRenderer.bounds.size.y;
                }
                else
                {
                    spawnY = last.transform.position.y - last.transform.localScale.y;
                }
            }

            GameObject strip = Instantiate(road, new UnityEngine.Vector3(0f, spawnY, 99f), UnityEngine.Quaternion.identity);
            if (last != null)
            {
                float previousXScale = last.transform.localScale.x;
                strip.transform.localScale = new UnityEngine.Vector3(previousXScale * offset, previousXScale * offset, strip.transform.localScale.z);
            }
            last = strip;

            if (strip.transform.position.y <= bottomEdge)
            {
                break;
            }

            if (strip.GetComponent<SpriteRenderer>() != null && strip.GetComponent<SpriteRenderer>().bounds.size.y <= 0.0001f)
            {
                Debug.LogWarning("Road tile height is zero; stopping spawn loop.");
                break;
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
}
