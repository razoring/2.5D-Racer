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
                float previousHeight = 1f;
                SpriteRenderer lastRenderer = last.GetComponentInChildren<SpriteRenderer>();
                if (lastRenderer != null && lastRenderer.sprite != null)
                {
                    previousHeight = lastRenderer.bounds.size.y;
                }
                else
                {
                    previousHeight = last.transform.localScale.y;
                }

                spawnY = last.transform.position.y - previousHeight;
            }

            GameObject strip = Instantiate(road, new UnityEngine.Vector3(0f, spawnY, 99f), UnityEngine.Quaternion.identity);
            if (last != null)
            {
                float previousXScale = last.transform.localScale.x;
                strip.transform.localScale = new UnityEngine.Vector3(previousXScale * offset, previousXScale * offset, strip.transform.localScale.z);
            }
            last = strip;
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
