using System.Numerics;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject road; //prefab
    [SerializeField] GameObject sand; //prefab
    [SerializeField] GameObject cactus; //prefab
    [SerializeField] GameObject skyline; //prefab
    private GameObject _activeSkyline;
    [SerializeField] float rate;
    public float rotation = 2;
    public float speed = 1f;
    public float speedMultiplier = 1f;
    [SerializeField] float curve;
    [SerializeField] RectTransform speedometerHand;
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

        if (skyline != null)
        {
            _activeSkyline = Instantiate(skyline, new UnityEngine.Vector3(0f, 0f, 900f), UnityEngine.Quaternion.identity);
            SpriteRenderer sr = _activeSkyline.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 100; // Force skyline to render over the top half of the first road/sand segments

                // Ensure the bottom edge rests exactly at Y=0 regardless of pivot
                float bottomY = sr.bounds.min.y;
                float shiftY = 0f - bottomY;
                _activeSkyline.transform.position += new UnityEngine.Vector3(0f, shiftY, 0f);
            }
        }

        float bottomEdge = cam.ScreenToWorldPoint(new UnityEngine.Vector3(0f, 0f, 0f)).y - 1f;

        GameObject last = null;
        int i = 0;

        for (int i = 0; i < 1000; i++)
        {
            float spawnY = 0f;
            if (last != null)
            {
                float previousHeight = 1f;
                SpriteRenderer lastRenderer = last.GetComponentInChildren<SpriteRenderer>();
                if (lastRenderer != null && lastRenderer.sprite != null)
                {
                   spawnY = last.transform.position.y - (lastRenderer.bounds.size.y);
                }
                else
                {
                    previousHeight = last.transform.localScale.y;
                }

                spawnY = last.transform.position.y - previousHeight;
            }

            GameObject strip = Instantiate(road, new UnityEngine.Vector3(0f, spawnY, 99f), UnityEngine.Quaternion.identity);
            float t = spawnY / bottomEdge;
            float scaleMultiplier = Mathf.Lerp(0.1f, 0.7f, t);
            strip.transform.localScale = new UnityEngine.Vector3(road.transform.localScale.x * scaleMultiplier * 7f, road.transform.localScale.y * scaleMultiplier, strip.transform.localScale.z);
            last = strip;
            
            RoadManager rm = strip.GetComponent<RoadManager>();
            if (rm != null) rm.segmentIndex = i;
            SpriteRenderer rmSr = strip.GetComponent<SpriteRenderer>();
            if (rmSr != null) rmSr.sortingOrder = 10; // Road on top of sand

            if (sand != null)
            {
                GameObject sandStrip = Instantiate(sand, new UnityEngine.Vector3(0f, spawnY, 899f), UnityEngine.Quaternion.identity);
                float sandScaleX = Mathf.Max(5f, sand.transform.localScale.x * scaleMultiplier * 15f);
                sandStrip.transform.localScale = new UnityEngine.Vector3(sandScaleX, sand.transform.localScale.y * scaleMultiplier, sandStrip.transform.localScale.z);
                
                RoadManager sandRm = sandStrip.GetComponent<RoadManager>();
                if (sandRm == null) sandRm = sandStrip.AddComponent<RoadManager>();
                sandRm.segmentIndex = i;
                if (rm != null && sandRm.move == null) sandRm.move = rm.move;
                
                SpriteRenderer sandSr = sandStrip.GetComponent<SpriteRenderer>();
                if (sandSr != null) sandSr.sortingOrder = 5; // Sand in the background
            }

            if (strip.transform.position.y <= bottomEdge)
            {
                break;
            }
            i++;
        }
    }

    private float _targetCurve = 0f;
    private float _curveDistanceTimer = 10f;
    public float scrollDistance = 0f;
    private float _cactusSpawnTimer = 0f;

    void Update()
    {
        float currentDisplaySpeed = speed * 80f;

        // Auto-find hand if not assigned in inspector
        if (speedometerHand == null)
        {
            GameObject handObj = GameObject.Find("Hand");
            if (handObj != null) speedometerHand = handObj.GetComponent<RectTransform>();
        }

        if (speedometerHand != null)
        {
            // Map speed 0-200 to Z rotation 179 to -92.3
            float zRot = Mathf.Lerp(179f, -92.3f, Mathf.Clamp01(currentDisplaySpeed / 200f));
            speedometerHand.rotation = UnityEngine.Quaternion.Euler(0f, 0f, zRot);
        }

        if (_activeSkyline != null)
        {
            // Find player steering to rotate skyline opposite to turn
            float steerX = 0f;
            Movement[] allCars = FindObjectsByType<Movement>(FindObjectsSortMode.None);
            foreach (Movement m in allCars)
            {
                if (m.gameObject.CompareTag("Player"))
                {
                    steerX = m.moveDir.x;
                    break;
                }
            }

            // The panorama "rotates" (translates) opposite to the user's turn
            // We combine the player's steering and the actual road curve
            float turnAmount = (steerX + (curve * 0.5f)) * speed;
            _activeSkyline.transform.position += new UnityEngine.Vector3(-turnAmount * 0.1f * Time.deltaTime, 0f, 0f);
        }
    }

    void FixedUpdate()
    {
        scrollDistance += speed * Time.fixedDeltaTime;
        
        _cactusSpawnTimer -= speed * Time.fixedDeltaTime;
        if (_cactusSpawnTimer <= 0f && cactus != null)
        {
            _cactusSpawnTimer = Random.Range(0.1f, 0.3f) / Mathf.Max(0.1f, speed);
            int spawnCount = Random.Range(2, 6);
            for (int k = 0; k < spawnCount; k++)
            {
                GameObject c = Instantiate(cactus, new UnityEngine.Vector3(0f, 0f, 98f + (k * 0.1f)), UnityEngine.Quaternion.identity);
                SpriteRenderer csr = c.GetComponent<SpriteRenderer>();
                if (csr != null) csr.sortingOrder = 15; // Cacti on top of road and sand, but behind skyline
                
                Prop p = c.AddComponent<Prop>();
                p.sideOffset = (Random.value > 0.5f ? 1f : -1f) * Random.Range(4f, 35f); 
            }
        }

        _curveDistanceTimer -= speed * Time.fixedDeltaTime;
        if (_curveDistanceTimer <= 0f)
        {
            if (_targetCurve != 0f)
            {
                _targetCurve = 0f;
                _curveDistanceTimer = Random.Range(6f, 12f);
            }
            else
            {
                _targetCurve = Random.Range(-5f, 5f);
                _curveDistanceTimer = Random.Range(6f, 12f);
            }
        }
        
        curve = Mathf.Lerp(curve, _targetCurve, Time.fixedDeltaTime * speed * 0.2f);
    }

    public float getRotation()
    {
        return rotation;
    }

    public float getCurve(){
        return curve;
    }
}
