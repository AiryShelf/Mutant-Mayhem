using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    [SerializeField] Animator animator;
    public Shooter shooter;
    public string objectPoolName = "";
    public DroneType droneType = DroneType.Builder;
    public SpriteRenderer noPowerSr;
    public GameObject radialEnergy;
    public float moveSpeed = 3;
    float moveSpeedStart;
    int _energy;
    public int energy
    {
        get => _energy;
        set
        {
            _energy = Mathf.Clamp(value, 0, energyMax);
            onEnergyChanged?.Invoke(_energy);
        }
    }
    public int energyMax = 100;
    int _energyStart;
    public Action<int> onEnergyChanged;
    public string currentAction = "";
    public DroneJob currentJob;
    public Rigidbody2D rb;
    public SpriteRenderer sr;
    public GameObject lights;
    [SerializeField] protected float minJobDist = 1f;
    [SerializeField] float hoverEffectTime = 1;
    [SerializeField] float hoverEffectVariationFactor = 0.2f;
    [SerializeField] float hoverEffectForceFactor = 0.2f;
    [SerializeField] float hoverScaleFactor = 0.05f;
    public float rotationSpeed = 0.0025f;
    public float rotationSpeedStart { get; private set; }
    [SerializeField] float launchOrLandMinScale = 0.3f;
    [SerializeField] float launchOrLandScaleSpeed = 0.05f;
    [SerializeField] float flyingAlpha = 0.5f;
    [SerializeField] float jobHeightMinScale = 0.6f;

    public DroneContainer myHangar;
    public bool isDocked = false;
    public bool hasPower = true;
    public bool isFlying = false;
    public float heightScaleStart = 1;
    float heightScale = 1;
    protected bool jobDone = false;
    Coroutine actionCoroutine; // Used for states
    Coroutine hoverCoroutine;
    protected Coroutine alignCoroutine;
    protected Coroutine jobHeightCoroutine;
    protected Coroutine jobCheckCoroutine;
    public DroneHealth droneHealth;

    bool initialized = false;

    void Awake()
    {
        _energyStart = energyMax;
        moveSpeedStart = moveSpeed;
        rotationSpeedStart = rotationSpeed;
    }

    public virtual void RefreshStats()
    {
        moveSpeed = moveSpeedStart * DroneManager.Instance.droneSpeedMult;
        rotationSpeed = rotationSpeedStart * DroneManager.Instance.droneRotationSpeedMult;
        energyMax = Mathf.RoundToInt(_energyStart * DroneManager.Instance.droneEnergyMult);
    }

    public virtual void Initialize(TurretGunSO droneGun)
    {
        noPowerSr.enabled = false;
        energy = energyMax;
        radialEnergy.SetActive(false);
        droneHealth = GetComponent<DroneHealth>();
        if (droneHealth == null)
        {
            Debug.LogError("Drone: Could not find DroneHealth on self!");
            return;
        }
        droneHealth.SetHealth(droneHealth.GetMaxHealth());

        shooter.InitializeDroneShooter(droneGun);

        if (this is AttackDrone attackDrone)
            attackDrone.shooter.StartChargingGuns();

        initialized = true;
        RefreshStats();
    }

    public void PowerOn()
    {
        hasPower = true;
        noPowerSr.enabled = false;
        animator.SetBool("hasPower", true);
        sr.sortingLayerName = "FireParticles";
        sr.sortingOrder = 0;

        if (initialized && !isDocked)
        {
            lights.SetActive(true);

            hoverCoroutine = StartCoroutine(HoverEffect());
            jobHeightCoroutine = StartCoroutine(RaiseFromJob());

            SetNewAction(FlyToHangar);
        }
    }

    public void PowerOff()
    {
        CheckIfHangarDestroyed();

        hasPower = false;
        noPowerSr.enabled = true;
        animator.SetBool("hasPower", false);

        lights.SetActive(false);
        //myHangar.RemoveDroneFromJob(this);
        SetJobDone();

        // Simulate losing power, going down
        if (jobHeightCoroutine != null)
            StopCoroutine(jobHeightCoroutine);
        if (hoverCoroutine != null)
            StopCoroutine(hoverCoroutine);

        SetNewAction(LowerToGround);
    }
    
    void CheckIfHangarDestroyed()
    {
        if (myHangar == null)
            Die();
    }   

    #region Launch / Land

    public void Launch()
    {
        isDocked = false;
        rb.simulated = true;
        sr.enabled = true;
        radialEnergy.SetActive(true);
        lights.SetActive(true);

        heightScale = heightScaleStart * launchOrLandMinScale;
        transform.localScale = new Vector3(heightScale, heightScale, 1);
        jobHeightCoroutine = StartCoroutine(RaiseFromJob());
        hoverCoroutine = StartCoroutine(HoverEffect());
    }

    protected IEnumerator LowerToJob()
    {
        //Debug.Log("LOWER FROM JOB STARTED");
        yield return null;

        isFlying = false;

        while (heightScale > jobHeightMinScale)
        {
            yield return new WaitForSeconds(0.05f);
            heightScale -= launchOrLandScaleSpeed;
            UpdateAlphaBasedOnHeight();
        }
    }

    protected IEnumerator LowerToGround()
    {
        yield return null;

        isFlying = false;

        while (heightScale > launchOrLandMinScale)
        {
            yield return new WaitForSeconds(0.05f);
            heightScale -= launchOrLandScaleSpeed;
            transform.localScale = new Vector3(heightScale, heightScale, 1f);
            UpdateAlphaBasedOnHeight();
        }

        sr.sortingLayerName = "Enemy";
        sr.sortingOrder = -1000;
    }

    protected IEnumerator RaiseFromJob()
    {
        //Debug.Log("RAISE FROM JOB STARTED");
        yield return null;

        while (heightScale < heightScaleStart)
        {
            yield return new WaitForSeconds(0.05f);
            heightScale += launchOrLandScaleSpeed;
            UpdateAlphaBasedOnHeight();
        }

        isFlying = true;
    }

    IEnumerator LandInHangar()
    {
        CheckIfHangarDestroyed();

        //Debug.Log("LAND IN HANGAR STARTED");
        yield return null;
        if (jobHeightCoroutine != null)
            StopCoroutine(jobHeightCoroutine);
        isFlying = false;

        while (heightScale > launchOrLandMinScale)
        {
            yield return new WaitForSeconds(0.05f);
            heightScale -= launchOrLandScaleSpeed;
            UpdateAlphaBasedOnHeight();
        }

        yield return new WaitForFixedUpdate();
        
        myHangar.LandDrone(this);
        StopAllCoroutines();
    }

    void UpdateAlphaBasedOnHeight()
    {
        // Ensure we don't divide by zero
        float heightRange = heightScaleStart - jobHeightMinScale;
        if (Mathf.Approximately(heightRange, 0)) return;

        // Normalize the height scale
        float t = Mathf.Clamp01((heightScale - jobHeightMinScale) / heightRange);

        // Interpolate alpha
        float alpha = Mathf.Lerp(1, flyingAlpha, t);

        // Update the SpriteRenderer's alpha
        Color color = sr.color;
        color.a = alpha;
        sr.color = color;
    }

    public void Die()
    {
        StopAllCoroutines();
        if (myHangar != null)
            myHangar.RemoveDrone(this);

        DroneManager.Instance.RemoveDrone(this);
        initialized = false;
    }

    #endregion

    #region Main Actions

    protected virtual void SetNewAction(System.Func<IEnumerator> coroutineMethod)
    {
        //StopAllCoroutines();
        if (alignCoroutine != null)
            StopCoroutine(alignCoroutine);
        if (actionCoroutine != null)
            StopCoroutine(actionCoroutine);
        if (jobCheckCoroutine != null)
            StopCoroutine(jobCheckCoroutine);
        //if (jobHeightCoroutine != null)
        //StopCoroutine(jobHeightCoroutine);

        actionCoroutine = StartCoroutine(coroutineMethod());
        
        currentAction = coroutineMethod.Method.Name;
        //Debug.Log("Drone: Set new action to " + coroutineMethod.Method.Name);
    }

    IEnumerator MoveToJob()
    {
        yield return null;
        jobCheckCoroutine = StartCoroutine(CheckIfJobDone());
        if (jobHeightCoroutine != null)
            StopCoroutine(jobHeightCoroutine);
        jobHeightCoroutine = StartCoroutine(RaiseFromJob());
        isFlying = true;
        Vector2 target = Vector2.zero;
        if (currentJob != null)
        {
            if (currentJob.jobType == DroneJobType.Build ||
                currentJob.jobType == DroneJobType.Repair)
            {
                target = currentJob.jobPosition;
            }   
        }

        bool arrived = false;
        while (!arrived)
        {
            if (currentJob is DroneAttackJob attackJob)
                target = attackJob.targetTrans.position;
            MoveTowards(target, 1);
            RotateTowards(target);
            yield return new WaitForFixedUpdate();
            if (Vector2.Distance(transform.position, target) < minJobDist)
            {
                arrived = true;
                DoJob();
            }
        }
    }

    protected void MoveTowards(Vector2 target, float forceFactor)
    {
        Vector2 dir = target - (Vector2)rb.transform.position;
        float force = moveSpeed * forceFactor * rb.mass;
        rb.AddForce(dir.normalized * force);
    }

    protected void RotateTowards(Vector2 target)
    {
        Vector2 dir = target - (Vector2)transform.position;
        float desiredAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
        float currentAngle = transform.eulerAngles.z + 90;

        float angleDiff = Mathf.DeltaAngle(currentAngle, desiredAngle);
        angleDiff = Mathf.Clamp(angleDiff, -15, 15);
        float torque = angleDiff * rotationSpeed * rb.mass;

        rb.AddTorque(torque, ForceMode2D.Force);
    }

    IEnumerator FlyToHangar()
    {
        yield return null;

        CheckIfHangarDestroyed();

        if (jobHeightCoroutine != null)
            StopCoroutine(jobHeightCoroutine);
        jobHeightCoroutine = StartCoroutine(RaiseFromJob());

        if (hasPower && energy > 0)
        {
            DroneJob newJob = myHangar.GetDroneJob(droneType);
            if (newJob.jobType != DroneJobType.None)
            {
                SetJob(newJob);
                yield break;
            }
        }

        Vector2 target = myHangar.transform.position;

        float checkTimer = 0;
        bool arrived = false;
        if (Vector2.Distance(transform.position, target) < 1)
                arrived = true;

        while (!arrived)
        {
            MoveTowards(target, 1);
            RotateTowards(target);
            yield return new WaitForFixedUpdate();

            // Check for jobs
            checkTimer += Time.fixedDeltaTime;
            if (checkTimer >= 1)
            {
                checkTimer = 0;
                if (hasPower && energy > 0)
                {
                    DroneJob newJob = myHangar.GetDroneJob(droneType);
                    if (newJob.jobType != DroneJobType.None)
                    {
                        SetJob(newJob);
                        yield break;
                    }
                }
            }
            if (Vector2.Distance(transform.position, target) < 1)
                arrived = true;
        }

        SetNewAction(LandInHangar);
    }

    #endregion

    #region 2nd Actions

    protected IEnumerator AlignToPos(Vector2 pos)
    {
        yield return null;
        while (true)
        {
            yield return new WaitForFixedUpdate();

            bool aligned = Vector2.Distance(transform.position, pos) < minJobDist;
            if (!aligned)
            {
                MoveTowards(pos, 1f);
                RotateTowards(pos);
                //if (aligned)
                    //Debug.Log("Drone aligned");
            }
        }
    }

    IEnumerator HoverEffect()
    {
        yield return null;
        while (true)
        {
            // pick how long we apply force in cycle
            float directionDuration = UnityEngine.Random.Range(
                hoverEffectTime * (1 - hoverEffectVariationFactor),
                hoverEffectTime * (1 + hoverEffectVariationFactor)
            );

            // pick a random direction (unit circle normalized)
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;

            float timer = 0f;
            //float forceFactor = Random.Range(hoverEffectForceFactor * (1 - hoverEffectVariationFactor),
            //                                 hoverEffectForceFactor * (1 + hoverEffectVariationFactor));
            while (timer < directionDuration)
            {
                // Normalized progress from 0..1
                float t = timer / directionDuration;

                // Force ramps up from 0 to hoverEffectForceFactor
                float sineValue = Mathf.Sin(Mathf.PI * t);
                float currentForce = hoverEffectForceFactor * sineValue;

                // Apply force each frame
                rb.AddForce(randomDir * currentForce, ForceMode2D.Force);

                float scaleFactor = heightScale + hoverScaleFactor * sineValue * (1 + hoverEffectVariationFactor);
                transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

                // Wait until next frame
                yield return new WaitForFixedUpdate();
                timer += Time.fixedDeltaTime;
            }
        }
    }

    #endregion

    #region Jobs

    public void SetJob(DroneJob job)
    {
        if (job.jobType == DroneJobType.None)
        {
            jobDone = true;
            currentJob = job;
            SetNewAction(FlyToHangar);
        }
        else if (job.jobType == DroneJobType.Recharge)
        {
            jobDone = false;
            currentJob = job;
            // Do nothing, handled in DroneContainer
        }
        else
        {
            jobDone = false;
            currentJob = job;
            SetNewAction(MoveToJob);
        }
        
        //Debug.Log($"Drone: Job type set to: {job.jobType}");
    }

    void DoJob()
    {
        if (this is ConstructionDrone constructionDrone)
        {
            if (currentJob.jobType == DroneJobType.Build)
                SetNewAction(constructionDrone.Build);
            else if (currentJob.jobType == DroneJobType.Repair)
                SetNewAction(constructionDrone.Repair);
            else
                SetJobDone();
        }
        else if (this is AttackDrone attackDrone && currentJob is DroneAttackJob attackJob)
        {
            if (attackJob.targetTrans != null)
            {
                attackDrone.targetTrans = attackJob.targetTrans;
                //StartCoroutine(DelayNewAction(attackDrone, attackJob.targetTrans, attackDrone.Attack));
                SetNewAction(attackDrone.Attack);
            }
        }
        else
            SetJobDone();

        if (currentJob == null)
            return;
    }

    public void SetJobDone()
    {
        myHangar.RemoveDroneFromJob(this);
        jobDone = true;
        
        if (currentJob.jobType == DroneJobType.Recharge)
        {
            currentJob = new DroneJob(DroneJobType.None, Vector3.zero);
            return;
        }

        currentJob = new DroneJob(DroneJobType.None, Vector3.zero);
        SetNewAction(FlyToHangar);
    }

    protected virtual IEnumerator CheckIfJobDone() { yield return null; }

    #endregion
}

public enum DroneType
{
    Builder,
    Attacker
}