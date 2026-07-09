using System;
using System.Collections.Generic;
using System.Numerics;
using Mono.Cecil;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Sprites;

public class Movement : MonoBehaviour
{   
    [SerializeField] Transform obj;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] InputActionReference move;
    [SerializeField] float scaled;
    [SerializeField] float maxSpeed = 5f;
    [SerializeField] public BoxCollider2D carCollider;
    public UnityEngine.Vector2 moveDir { get; private set; }
    private float npcAbsoluteSpeed = 3f;

    [SerializeField] SpriteRenderer renderer;
    [SerializeField] int idleFrame;
    [SerializeField] string frameFolder;
    private Sprite[] frames;
    private float rot = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        frames = Resources.LoadAll<Sprite>(frameFolder);
        //Debug.Log(frames.Length);

        renderer.sprite = frames[idleFrame];
        renderer.sortingOrder = 20; // Ensure cars render over the road (10) and cacti (15)
        rot = FindAnyObjectByType<GameManager>().getRotation();
    }

    private float _aiChangeTimer = 0f;
    private UnityEngine.Vector2 _aiTargetDir = UnityEngine.Vector2.zero;
    private float _prevRoadX = 0f;
    private bool _hasPrevRoadX = false;
    private float _npcOffset = 0f;

    void LateUpdate()
    {   
        RoadManager[] allRoads = FindObjectsByType<RoadManager>();
        float minD = 9999f;
        float roadX = 0f;
        float roadWidth = 2f;
        int closestRoadMiddle = 0;
        foreach (RoadManager r in allRoads)
        {
            if (r.gameObject.CompareTag("Sand")) continue;

            float d = Mathf.Abs(r.transform.position.y - obj.position.y);
            if (d < minD)
            {
                minD = d;
                roadX = r.transform.position.x;
                closestRoadMiddle = r.getMiddle();
                
                BoxCollider2D col = r.GetComponent<BoxCollider2D>();
                if (col != null) 
                    roadWidth = col.bounds.extents.x * 0.8f; // Give a slight margin
                else 
                    roadWidth = r.transform.localScale.x * 0.4f; // Fallback to visual scale bounds
            }
        }

        if (gameObject.tag.Equals("Player"))
        {
            moveDir = move.action.ReadValue<UnityEngine.Vector2>();
            
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                if (Mathf.Abs(obj.position.x - roadX) > roadWidth)
                {
                    gm.speedMultiplier = Mathf.Lerp(gm.speedMultiplier, 0.2f, Time.deltaTime * 5f);
                }
                else
                {
                    gm.speedMultiplier = Mathf.Lerp(gm.speedMultiplier, 1f, Time.deltaTime * 5f);
                }

                float targetSpeed = 0f; 
                float accel = 0.3f; // Coasting friction
                
                if (moveDir.y > 0.1f) 
                {
                    targetSpeed = maxSpeed * 0.5f;
                    accel = 0.5f; // Acceleration
                }
                else if (moveDir.y < -0.1f) 
                {
                    targetSpeed = 0f;
                    accel = 1.5f; // Braking
                }
                
                gm.speed = Mathf.Lerp(gm.speed, targetSpeed * gm.speedMultiplier, Time.deltaTime * accel);
            }
            
            float depthFactor = Math.Max(0.1f, Math.Abs(obj.position.y) / 4f);
            // Player Y velocity is 0 visually (they control the road's speed)
            // Steering power is restricted by how fast the road is actually moving
            float forwardMomentum = Mathf.Clamp01(gm != null ? gm.speed / 2f : 1f);
            rb.linearVelocity = new UnityEngine.Vector2(moveDir.x * rot * depthFactor * forwardMomentum, 0f);
            
            if (obj.position.y > 0f)
            {
                obj.position = new UnityEngine.Vector3(obj.position.x, 0f, obj.position.z);
            }
        }
        else
        {
            _aiChangeTimer -= Time.deltaTime;
            if (_aiChangeTimer <= 0f)
            {
                _aiChangeTimer = UnityEngine.Random.Range(1f, 3f);
                _aiTargetDir = new UnityEngine.Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
                npcAbsoluteSpeed = UnityEngine.Random.Range(1f, 3f); // Random independent speed
            }
            
            Movement[] allCars = FindObjectsByType<Movement>();
            float avoidX = 0f;
            float avoidY = 0f;
            foreach (Movement car in allCars)
            {
                if (car != this && car.obj != null)
                {
                    float dx = obj.position.x - car.obj.position.x;
                    float dy = obj.position.y - car.obj.position.y;
                    if (Mathf.Abs(dx) < 2.5f && Mathf.Abs(dy) < 1.5f)
                    {
                        avoidX += (dx > 0 ? 1f : -1f) * (2.5f - Mathf.Abs(dx));
                        avoidY += (dy > 0 ? 1f : -1f) * (1.5f - Mathf.Abs(dy));
                    }
                }
            }
            
            float depthFactor = Math.Max(0.1f, Math.Abs(obj.position.y) / 4f);
            
            float maxRoadOffset = roadWidth * 0.8f;
            float desiredOffset = _aiTargetDir.x * 2f + avoidX * 3f;
            desiredOffset = Mathf.Clamp(desiredOffset, -maxRoadOffset, maxRoadOffset);
            
            // Steer towards the desired lateral offset relative to the road center
            float steerX = Mathf.Clamp((desiredOffset - _npcOffset) * 2f, -1f, 1f);
            float targetY = Mathf.Clamp(_aiTargetDir.y + avoidY, -1f, 1f);

            moveDir = UnityEngine.Vector2.Lerp(moveDir, new UnityEngine.Vector2(steerX, targetY), Time.deltaTime * 3f);
            
            // Track local offset relative to the road
            _npcOffset += moveDir.x * rot * depthFactor * Time.deltaTime;
            _npcOffset = Mathf.Clamp(_npcOffset, -maxRoadOffset, maxRoadOffset);

            float gmSpeed = 3f;
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null) gmSpeed = gm.speed;
            
            float bottomEdge = -5f;
            if (Camera.main != null) bottomEdge = Camera.main.ScreenToWorldPoint(UnityEngine.Vector3.zero).y;

            // Rubberband push if falling behind
            if (obj.position.y < bottomEdge + 0.5f && npcAbsoluteSpeed <= gmSpeed)
            {
                npcAbsoluteSpeed = gmSpeed + UnityEngine.Random.Range(0.75f, 1.5f);
            }

            float relativeYSpeed = (npcAbsoluteSpeed - gmSpeed) * 0.5f; 
            
            // Only use physics for forward/backward movement
            rb.linearVelocity = new UnityEngine.Vector2(0f, relativeYSpeed * depthFactor);
            
            // Strictly lock the X position to perfectly track the road's perspective sweep
            obj.position = new UnityEngine.Vector3(roadX + _npcOffset, obj.position.y, obj.position.z);

            if (obj.position.y > 0f)
            {
                obj.position = new UnityEngine.Vector3(obj.position.x, 0f, obj.position.z);
            }
            else if (obj.position.y < bottomEdge)
            {
                obj.position = new UnityEngine.Vector3(obj.position.x, bottomEdge, obj.position.z);
            }
        }
        
        
        int frameIndex = idleFrame;
        if (gameObject.tag.Equals("Player"))
        {
            frameIndex += ((int)obj.position.x - closestRoadMiddle);
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                frameIndex += ((int)obj.position.x - (int)playerObj.transform.position.x);
            }
            else
            {
                frameIndex += ((int)obj.position.x - closestRoadMiddle);
            }
        }
        
        frameIndex = Mathf.Clamp(frameIndex, 0, frames.Length - 1);
        renderer.sprite = frames[frameIndex];

        float alpha = 1f;
        if (obj.position.y > -0.05f)
        {
            alpha = Mathf.Clamp01(Mathf.Abs(obj.position.y));
        }
        UnityEngine.Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;

        obj.localScale = new UnityEngine.Vector3(Math.Abs(obj.position.y)/2*scaled+0.1f,Math.Abs(obj.position.y)/2*scaled+0.1f);
        obj.position = new UnityEngine.Vector3(obj.position.x, obj.position.y, -Math.Abs(obj.position.y));
    }   
}
