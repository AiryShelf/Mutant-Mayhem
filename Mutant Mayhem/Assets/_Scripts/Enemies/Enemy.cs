using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeedBase;
    [SerializeField] float moveAccuracy;
    [SerializeField] float rotateSpeed;
    [SerializeField] float retargetFarDelay;
    [SerializeField] float minimumTargetDelay;

    [Header("Combat AI")]
    [SerializeField] float retargetNearDelay;
    [SerializeField] float proximityTimeFactor;
    [SerializeField] int proximityDist;
    [SerializeField] float proximitySpeedFactor;
    [SerializeField] float hitFeezeTime;
    [SerializeField] float moveSpeedBoostTime;
    [SerializeField] float moveSpeedBoostFactor;

    [Header("Randomization")]
    [SerializeField] float minSize;
    [SerializeField] float randomColorFactor;
    [SerializeField] float gaussMeanSize;
    [SerializeField] float gaussStdDev;

    [Header("References")]
    [SerializeField] Rigidbody2D myRb;
    [SerializeField] SpriteRenderer mySr;
    [SerializeField] Health myHealth;
    [SerializeField] AnimationControllerEnemy myAnimationController;
    [SerializeField] MeleeControllerEnemy meleeController;

    Transform playerTrans;
    CircleCollider2D playerCollider;
    float randomSizeFactor;
    bool proximityTrigger;
    [HideInInspector] public float moveAmount;
    float moveSpeed;
    bool isHit;
    bool boosting;
    Coroutine boostCoroutine;
    Coroutine retargetCoroutine;
    float meleeAttackDist;
    float playerDistance;
    Vector2 moveDir;
    float rotAngle;

    [SerializeField] bool displayDebugInfo;
    

    void Awake()
    {
        playerTrans = FindObjectOfType<Player>().transform;
        playerCollider = playerTrans.GetComponent<CircleCollider2D>();
        meleeAttackDist = playerCollider.radius;
    }

    void Start()
    {
        RandomizeStats();
        moveSpeed = moveSpeedBase;
    }

    void FixedUpdate()
    {
        CheckDistance();
        Rotate();
        Move(); 
    }

    void CheckDistance()
    {
        // Check distance to player
        playerDistance = (playerTrans.position - transform.position).magnitude;
        if (playerDistance < proximityDist && !proximityTrigger)
        {
            proximityTrigger = true;
            retargetCoroutine = StartCoroutine(Retarget());
        }

        // Check playerCollider distance from meleeControllerPos
        float meleeDistance = (playerTrans.position - meleeController.transform.position).magnitude;
        if (meleeDistance <= meleeAttackDist + meleeController.meleeAttackRange)
        {
            meleeController.isAttacking = true;
        }
        else
        {
            meleeController.isAttacking = false;
        }
    }

    void Rotate()
    {
        myRb.rotation = Mathf.LerpAngle(myRb.rotation, rotAngle, Time.deltaTime * rotateSpeed);
    }

    void Move()
    {
        // Set new move target
        if (retargetCoroutine == null)
        {
            retargetCoroutine = StartCoroutine(Retarget());
        }

        // Move, as long as not recently hit
        if (!isHit)
        {
            //transform.position += (Vector3)moveDir.normalized * moveAmount * Time.deltaTime;
            myRb.AddForce(moveDir * moveAmount);
        }
    }

    IEnumerator Retarget()
    {
        yield return new WaitForEndOfFrame();
        // Find initial moveDir
        moveDir = (Vector2)playerTrans.position - (Vector2)transform.position;
        moveDir.Normalize();

        // Get honing normal value
        float accuracyHoning;
        if (playerDistance < proximityDist)
        {
            // Create normalized value (0-1 range) increases as closer to player
            accuracyHoning = (proximityDist - playerDistance) / proximityDist;
        }
        else 
        {
            accuracyHoning = 0f;
        }

        // Convert moveDir to rotation, add moveAccuracy, and convert back
        rotAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        float accOffset = moveAccuracy * (1 - accuracyHoning);
        rotAngle += Random.Range(-accOffset, accOffset);
        float radians = rotAngle * Mathf.Deg2Rad;
        moveDir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

        // Randomize the wait time and move speed, speed up both when closer to the target
        float waitTime; 
        if (playerDistance < proximityDist)
        {
            waitTime = retargetNearDelay + Random.Range(0, retargetNearDelay);
            moveAmount = moveSpeed + (proximityDist - playerDistance) / proximityDist * proximitySpeedFactor;
            waitTime -= accuracyHoning * proximityTimeFactor / 10;
        }
        else
        {
            moveAmount = Random.Range(moveSpeed/3, moveSpeed);
            waitTime = retargetFarDelay + Random.Range(0, retargetFarDelay);
        }

        waitTime = Mathf.Clamp(waitTime, minimumTargetDelay, float.MaxValue);

        //Debug.Log("waitTime: " + waitTime);

        yield return new WaitForSeconds(waitTime);

        // Reset coroutine and proximity triggers
        proximityTrigger = false;
        retargetCoroutine = null;
    }

    public void IsHit()
    {
        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
            boosting = false;
        }
        boostCoroutine = StartCoroutine(MoveSpeedBoost());
        // Allow freezing when hit
        if (!isHit)
        {
            isHit = true;
            StartCoroutine(UnfreezeAfterTime());
            retargetCoroutine = StartCoroutine(Retarget());
        }
    }

    IEnumerator UnfreezeAfterTime()
    {
        yield return new WaitForSeconds(hitFeezeTime);
        isHit = false;
    }

    IEnumerator MoveSpeedBoost()
    {
        if (!boosting)
        {
            boosting = true;
            moveSpeed = moveSpeedBase * moveSpeedBoostFactor;
            yield return new WaitForSeconds(moveSpeedBoostTime);
            moveSpeed = moveSpeedBase;
            boosting = false;
            boostCoroutine = null;
        }
        else yield break;
    }

    void RandomizeStats()
    {
        // Randomize color
        float randomColorRed = Random.Range(-randomColorFactor, randomColorFactor);
        float randomColorGreen = Random.Range(-randomColorFactor, randomColorFactor);
        float randomColorBlue = Random.Range(-randomColorFactor, randomColorFactor);
        mySr.color = new Color(mySr.color.r + randomColorRed,
                               mySr.color.g + randomColorGreen,
                               mySr.color.b + randomColorBlue);
        
        // Randomize size
        GaussianRandom _gaussianRandomm = new GaussianRandom();
        randomSizeFactor = (float)_gaussianRandomm.NextDouble(gaussMeanSize, gaussStdDev);
        randomSizeFactor = Mathf.Clamp(randomSizeFactor, minSize, float.MaxValue);
        transform.localScale *= randomSizeFactor;

        // Randomize stats by size
        moveSpeedBase *= randomSizeFactor;
        myHealth.SetMaxHealth(myHealth.GetMaxHealth() * randomSizeFactor);
        myHealth.SetHealth(myHealth.GetMaxHealth());
        meleeController.meleeDamage *= randomSizeFactor;
        meleeController.knockback *= randomSizeFactor;
        //meleeController.selfKnockback *= randomSizeFactor; no good*
        meleeController.meleeAttackRange *= randomSizeFactor;
        myRb.mass *= randomSizeFactor;

        myAnimationController.animSpeedFactor /= randomSizeFactor;
    }

    void OnCollisionStay2D(Collision2D other)
    {
        // Structures layer# 12
        if (other.gameObject.layer == 12)
        {
            meleeController.HitStructure(other.GetContact(0).point);
        }
    }
}
