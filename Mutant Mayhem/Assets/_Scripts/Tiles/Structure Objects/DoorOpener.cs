using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public static class DoorSortingAllocator
{
    private static int nextIndex = 1000;

    public static int GetNextIndex()
    {
        return nextIndex += 3;
    }
}

[System.Serializable]
public class DoorLeaf
{
    [Header("References")]
    public List<Sprite> damagedSprites;
    public Rigidbody2D body;
    public SpriteRenderer spriteRenderer;
    public ShadowCaster2D shadowCaster;
    public BoxCollider2D collider;

    [Header("Animation (Local Space)")]
    public Vector2 closedLocalPosition;
    public Vector2 openLocalPosition;

    [Header("Collider (Local Space)")]
    public Vector2 closedColliderOffset;
    public Vector2 closedColliderSize;
    public Vector2 openColliderOffset;
    public Vector2 openColliderSize;
}

public class DoorOpener : MonoBehaviour, ITileObject, ITileObjectExplodable
{
    [SerializeField] List<AnimatedTile> doorsOpen;
    [SerializeField] List<AnimatedTile> doorsClosed;
    [SerializeField] List<Light2D> doorPointLights;
    [SerializeField] SpriteMask doorMask;
    [SerializeField] bool usesPower;

    [Header("Sliding Door Leaves")]
    [SerializeField] DoorLeaf topLeaf;
    [SerializeField] DoorLeaf bottomLeaf;

    [Header("Animation Settings")]
    [SerializeField] float openDuration = 0.2f;
    [SerializeField] float closeDuration = 0.6f;

    public string explosionPoolName;

    Tilemap animatedTilemap;
    Vector3Int myGridPos;
    bool isOpen;
    bool destroyed;
    int damageIndex;

    Coroutine doorAnimationRoutine;
    float currentClosedAmount = 1f; // 1 = fully closed, 0 = fully open
    bool visualsOpen;

    int sortingOrder;

    public void Explode()
    {
        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
            explosion.transform.position = TileManager.Instance.TileCellsCenterToWorld(rootPos);
        }
    }

    void Start()
    {
        animatedTilemap = TileManager.AnimatedTilemap;
        myGridPos = animatedTilemap.WorldToCell(transform.position);

        // Allocate a unique sorting range for this door and configure mask/leaves
        SetupSortingRange();

        // Initialize visuals & leaves to match initial logical state
        visualsOpen = isOpen;
        UpdateOpenClose();

        float initialClosedAmount = isOpen ? 0f : 1f;
        currentClosedAmount = initialClosedAmount;
        ApplyLeafAnimation(topLeaf, initialClosedAmount);
        ApplyLeafAnimation(bottomLeaf, initialClosedAmount);
    }

    void OnDisable()
    {
        destroyed = true;
    }

    void SetupSortingRange()
    {
        // Reserve a block of sorting orders for this door instance
        sortingOrder = DoorSortingAllocator.GetNextIndex();
        
        // Set sorting orders for the leaves
        topLeaf.spriteRenderer.sortingOrder = sortingOrder;
        bottomLeaf.spriteRenderer.sortingOrder = sortingOrder;

        // Configure the sprite mask to only affect this door's sorting range
        if (doorMask != null)
        {
            doorMask.isCustomRangeActive = true;

            // The mask will affect only renderers whose sortingOrder is within this range
            doorMask.frontSortingOrder = sortingOrder + 1;
            doorMask.backSortingOrder = sortingOrder - 1;
        }
    }

    public void UpdateHealthRatio(float healthRatio)
    {
        damageIndex = GetDamageIndex(healthRatio);

        UpdateDamageTile();
    }

    void UpdateOpenClose()
    {
        //Debug.Log("Damage Index: " + damageIndex);
        if (animatedTilemap != null)
        {
            if (visualsOpen)
            {
                animatedTilemap.SetTile(myGridPos, doorsOpen[damageIndex]);
            }
            else
            {
                animatedTilemap.SetTile(myGridPos, doorsClosed[damageIndex]);
            }
        }

        UpdateLights();
    }

    void UpdateDamageTile()
    {
        if (animatedTilemap == null)
            return;

        if (visualsOpen)
        {
            animatedTilemap.SetTile(myGridPos, doorsOpen[damageIndex]);
        }
        else
        {
            animatedTilemap.SetTile(myGridPos, doorsClosed[damageIndex]);
        }

        if (damageIndex >= 0 && damageIndex < topLeaf.damagedSprites.Count)
        {
            topLeaf.spriteRenderer.sprite = topLeaf.damagedSprites[damageIndex];
        }
        if (damageIndex >= 0 && damageIndex < bottomLeaf.damagedSprites.Count)
        {
            bottomLeaf.spriteRenderer.sprite = bottomLeaf.damagedSprites[damageIndex];
        }
    }

    void ApplyLeafAnimation(DoorLeaf leaf, float closedAmount)
    {
        if (leaf == null || leaf.body == null)
            return;

        // Ensure the body is kinematic so it can push dynamic rigidbodies without being pushed itself
        if (leaf.body.bodyType != RigidbodyType2D.Kinematic)
            leaf.body.bodyType = RigidbodyType2D.Kinematic;

        // closedAmount: 0 = fully open, 1 = fully closed

        // 1) Move the leaf sprite/body
        Vector2 localPos = Vector2.Lerp(leaf.openLocalPosition, leaf.closedLocalPosition, closedAmount);
        Transform t = leaf.body.transform;
        t.localPosition = new Vector3(localPos.x, localPos.y, t.localPosition.z);

        // 2) Adjust the collider shape if present
        if (leaf.collider != null)
        {
            // Lerp size and base offset as before
            Vector2 size      = Vector2.Lerp(leaf.openColliderSize,   leaf.closedColliderSize,   closedAmount);
            Vector2 baseOffset = Vector2.Lerp(leaf.openColliderOffset, leaf.closedColliderOffset, closedAmount);

            // Movement of the body relative to the CLOSED pose (in local space)
            Vector2 movementFromClosed = localPos - leaf.closedLocalPosition;

            // Negate that movement so the collider stays anchored in the door frame
            Vector2 adjustedOffset = baseOffset - movementFromClosed;

            leaf.collider.size   = size;
            leaf.collider.offset = adjustedOffset;

            // 3) Match ShadowCaster2D to the collider rectangle, if present
            if (leaf.shadowCaster != null)
            {
                // Make sure it stays enabled so shadows update continuously
                if (!leaf.shadowCaster.enabled)
                    leaf.shadowCaster.enabled = true;

                float absWidth  = Mathf.Abs(size.x);
                float absHeight = Mathf.Abs(size.y);

                // Clamp to a tiny minimum so we never produce a degenerate 0-size shape
                const float minShadowSize = 0.01f;
                absWidth  = Mathf.Max(absWidth,  minShadowSize);
                absHeight = Mathf.Max(absHeight, minShadowSize);

                float halfX = absWidth * 0.5f;
                float halfY = absHeight * 0.5f;
                Vector2 center = adjustedOffset;

                // Build a simple 4-point rectangle in local space (counter-clockwise)
                Vector3[] path = new Vector3[4];
                path[0] = new Vector3(center.x - halfX, center.y - halfY, 0f); // bottom-left
                path[1] = new Vector3(center.x - halfX, center.y + halfY, 0f); // top-left
                path[2] = new Vector3(center.x + halfX, center.y + halfY, 0f); // top-right
                path[3] = new Vector3(center.x + halfX, center.y - halfY, 0f); // bottom-right

                ShadowCaster2DHelper.SetShapePath(leaf.shadowCaster, path);
            }
        }
    }

    IEnumerator AnimateDoor(bool openTarget)
    {
        // closedAmount: 0 = fully open, 1 = fully closed
        float startClosedAmount = currentClosedAmount;
        float endClosedAmount = openTarget ? 0f : 1f;

        // Use a a longer duration when closing
        float duration = openTarget ? openDuration : closeDuration;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / duration);
            float closedAmount = Mathf.Lerp(startClosedAmount, endClosedAmount, lerp);

            currentClosedAmount = closedAmount;

            ApplyLeafAnimation(topLeaf, closedAmount);
            ApplyLeafAnimation(bottomLeaf, closedAmount);

            yield return null;
        }

        // Snap to final state
        currentClosedAmount = endClosedAmount;
        ApplyLeafAnimation(topLeaf, endClosedAmount);
        ApplyLeafAnimation(bottomLeaf, endClosedAmount);

        // Update visuals to match final logical state
        visualsOpen = openTarget;
        UpdateOpenClose();

        doorAnimationRoutine = null;
    }

    int GetDamageIndex(float healthRatio)
    {
        int damageIndex;
        int spriteCount = doorsOpen.Count;

        // Reserve index 0 for full health
        if (healthRatio >= 1f)
        {
            damageIndex = 0;
        }
        else
        {
            // Divide remaining indices (1 to spriteCount-1) across the 0–99% damage range
            float normalized = 1f - healthRatio;
            damageIndex = 1 + Mathf.FloorToInt(normalized * (spriteCount - 1));
            damageIndex = Mathf.Clamp(damageIndex, 1, spriteCount - 1);
        }

        return damageIndex;
    }

    void UpdateLights()
    {
        Color lightColor = Color.green;
        if (visualsOpen)
            lightColor = Color.red;

        foreach (var light in doorPointLights)
        {
            light.color = lightColor;
        }
    }

    void OpenDoor()
    {
        if (isOpen || !gameObject.activeInHierarchy)
            return;

        isOpen = true;
        visualsOpen = true; // switch to open visuals immediately
        UpdateOpenClose();

        if (doorAnimationRoutine != null)
            StopCoroutine(doorAnimationRoutine);

        doorAnimationRoutine = StartCoroutine(AnimateDoor(true));

        // Clear bullet holes in the area covered by the closed door leaves
        if (ParticleManager.Instance != null)
        {
            bool hasBounds = false;
            Bounds worldBounds = new Bounds();

            if (topLeaf != null && topLeaf.collider != null)
            {
                worldBounds = topLeaf.collider.bounds;
                hasBounds = true;
            }

            if (bottomLeaf != null && bottomLeaf.collider != null)
            {
                if (hasBounds)
                {
                    worldBounds.Encapsulate(bottomLeaf.collider.bounds);
                }
                else
                {
                    worldBounds = bottomLeaf.collider.bounds;
                    hasBounds = true;
                }
            }

            if (hasBounds)
            {
                ParticleManager.Instance.ClearBulletHolesInBounds(worldBounds);
            }
        }
    }

    void CloseDoor()
    {
        if (!isOpen || !gameObject.activeInHierarchy)
            return;

        isOpen = false;

        if (doorAnimationRoutine != null)
            StopCoroutine(doorAnimationRoutine);

        doorAnimationRoutine = StartCoroutine(AnimateDoor(false));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OpenDoor();
            //Debug.Log("Door detected Player");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!destroyed)
        {
            if (other.CompareTag("Player"))
            {
                CloseDoor();
                //Debug.Log("Player away from door");
            }
        }
    }
}