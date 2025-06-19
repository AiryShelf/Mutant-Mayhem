using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class PlanetManager : MonoBehaviour
{
    public static PlanetManager Instance { get; private set; }
    public List<PlanetSO> planetsSource;
    public PlanetSO currentPlanet { get; private set; }

    [Header("Current Multipliers")]
    public Dictionary<PlanetStatModifier, float> statMultipliers = new Dictionary<PlanetStatModifier, float>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SetCurrentPlanet(ProfileManager.Instance.currentProfile.lastPlanetVisited);
    }

    public void InitializeStatMultipliers()
    {
        statMultipliers = new Dictionary<PlanetStatModifier, float>();
        foreach (PlanetStatModifier stat in System.Enum.GetValues(typeof(PlanetStatModifier)))
        {
            statMultipliers[stat] = 1f;
        }
    }

    public void SetCurrentPlanet(int index)
    {
        currentPlanet = planetsSource[index];
        SetPlanetProperties();

        if (!currentPlanet.mission.isTutorial)
        {
            ProfileManager.Instance.currentProfile.lastPlanetVisited = index;
            ProfileManager.Instance.SaveCurrentProfile();
        }
    }

    public void SetCurrentPlanet(PlanetSO planet)
    {
        int index = planetsSource.IndexOf(planet);
        if (!planet.mission.isTutorial)
        {
            ProfileManager.Instance.currentProfile.lastPlanetVisited = index;
            ProfileManager.Instance.SaveCurrentProfile();
        }
            
        if (index != -1)
            {
                currentPlanet = planetsSource[index];
                SetPlanetProperties();
            }
            else
                Debug.LogError("Planet not found in PlanetManager's list");
    }

    public void SetTutorialPlanet()
    {
        SetCurrentPlanet(0);
    }

    void SetPlanetProperties()
    {
        if (currentPlanet == null)
        {
            Debug.LogError("No planet is set.");
            return;
        }

        InitializeStatMultipliers();

        // Apply each property from the current planet
        foreach (var property in currentPlanet.properties)
        {
            foreach (var entry in property.statModifierEntries)
            {
                if (statMultipliers.ContainsKey(entry.statModifier))
                {
                    statMultipliers[entry.statModifier] *= entry.multiplier;
                    statMultipliers[entry.statModifier] = Mathf.Clamp(statMultipliers[entry.statModifier], 0 , float.MaxValue);
                }
                else
                {
                    Debug.LogError("PlanetManager: StatModifier " +
                                   $"{entry.statModifier} not found in dictionary.");
                }
            }
        }
    }

    public void ApplyPlanetProperties()
    {
        SettingsManager.Instance.CreditsMult *= statMultipliers[PlanetStatModifier.CreditsMult];

        ApplyToPlayer();
        ApplyToBuildingSystem();
        ApplyToTurretManager(); 
        ApplyToWaveController();
        ApplyToBkgGenerator();
    }
    
    void ApplyToPlayer()
    {
        Player player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("Planet Manager: Could not find Player");
            return;
        }

        float maxHealth = player.stats.playerHealthScript.GetMaxHealth();
        maxHealth *= statMultipliers[PlanetStatModifier.PlayerHealth];
        player.stats.playerHealthScript.SetMaxHealth(maxHealth);
        player.stats.playerHealthScript.SetHealth(maxHealth);

        player.stats.moveSpeed *= statMultipliers[PlanetStatModifier.PlayerMoveSpeed];
        player.stats.strafeSpeed *= statMultipliers[PlanetStatModifier.PlayerMoveSpeed];
        player.GetComponent<Rigidbody2D>().drag *= statMultipliers[PlanetStatModifier.PlayerDrag];
        player.stats.meleeDamage *= statMultipliers[PlanetStatModifier.LaserDamage];
        player.stats.structureStats.structureMaxHealthMult *= statMultipliers[PlanetStatModifier.StructureIntegrity];

        foreach (GunSO gun in player.playerShooter.gunList)
        {
            switch (gun.gunType)
            {
                case GunType.Laser:
                    gun.damage *= statMultipliers[PlanetStatModifier.LaserDamage];
                    gun.bulletLifeTime *= statMultipliers[PlanetStatModifier.LaserRange];
                    break;
                case GunType.Bullet:
                    gun.damage *= statMultipliers[PlanetStatModifier.BulletDamage];
                    gun.bulletLifeTime *= statMultipliers[PlanetStatModifier.BulletRange];
                    break;
                case GunType.RepairGun:
                    gun.damage *= statMultipliers[PlanetStatModifier.RepairGunDamage];
                    break;
            }
        }
    }

    void ApplyToBuildingSystem()
    {
        BuildingSystem buildingSystem = FindObjectOfType<BuildingSystem>();
        if (buildingSystem == null)
        {
            Debug.LogError("Planet Manager: Could not find building system");
            return;
        }

        buildingSystem.structureCostMult *= statMultipliers[PlanetStatModifier.BuildCost];
    }

    void ApplyToTurretManager()
    {
        foreach (TurretGunSO gun in TurretManager.Instance.turretGunList)
        {
            switch (gun.gunType)
            {
                case GunType.Laser:
                    gun.damage *= statMultipliers[PlanetStatModifier.LaserDamage];
                    gun.bulletLifeTime *= statMultipliers[PlanetStatModifier.LaserRange];
                    gun.detectRange *= statMultipliers[PlanetStatModifier.TurretSensors];
                    gun.expansionDelay *= statMultipliers[PlanetStatModifier.TurretSensors];
                    break;
                case GunType.Bullet:
                    gun.damage *= statMultipliers[PlanetStatModifier.BulletDamage];
                    gun.bulletLifeTime *= statMultipliers[PlanetStatModifier.BulletRange];
                    gun.detectRange *= statMultipliers[PlanetStatModifier.TurretSensors];
                    gun.expansionDelay *= statMultipliers[PlanetStatModifier.TurretSensors];
                    break;
                case GunType.RepairGun:
                    gun.damage *= statMultipliers[PlanetStatModifier.RepairGunDamage];
                    gun.detectRange *= statMultipliers[PlanetStatModifier.TurretSensors];
                    gun.expansionDelay *= statMultipliers[PlanetStatModifier.TurretSensors];
                    break;
            }
        }
    }

    void ApplyToWaveController()
    {
        WaveControllerRandom waveController = FindObjectOfType<WaveControllerRandom>();
        if (waveController == null)
        {
            Debug.LogError("PlanetManager: Could not find WaveController in scene");
            return;
        }

        waveController.creditsPerWave = currentPlanet.creditsPerWave; // Additive bonus (waveIndex*creditsPerWave)
        waveController.timeBetweenWavesBase = currentPlanet.timeBetweenWavesBase; // Base amount of day-time
        waveController.wavesTillAddIndex = currentPlanet.wavesTillAddIndex; // Affects max index to choose subwaves from
        waveController.subwaveDelayMultStart = currentPlanet.subwaveDelayMultStart;
        waveController.spawnRadiusBuffer = currentPlanet.spawnRadiusBuffer;

        waveController.batchMultStart = currentPlanet.batchMultiplierStart; // Starting batch multiplier for each Subwave 
        waveController.damageMultStart = currentPlanet.damageMultiplier * statMultipliers[PlanetStatModifier.EnemyDamage]; 
        waveController.healthMultStart = currentPlanet.healthMultiplier * statMultipliers[PlanetStatModifier.EnemyHealth];
        waveController.speedMultStart = currentPlanet.speedMultiplier * statMultipliers[PlanetStatModifier.EnemyMoveSpeed];
        waveController.sizeMultStart = currentPlanet.sizeMultiplier * statMultipliers[PlanetStatModifier.EnemySize];
        waveController.batchMultGrowthTime = currentPlanet.batchMultGrowthTime;
        waveController.damageMultGrowthTime = currentPlanet.damageMultGrowthTime;
        waveController.attackDelayMultGrowthTime = currentPlanet.attackDelayMultGrowthTime;
        waveController.healthMultGrowthTime = currentPlanet.healthMultGrowthTime;
        waveController.speedMultGrowthTime = currentPlanet.speedMultGrowthTime;
        waveController.sizeMultGrowthTime = currentPlanet.sizeMultGrowthTime;
        waveController.subwaveDelayMultGrowthTime = currentPlanet.subwaveDelayMultGrowthTime;
    }

    void ApplyToBkgGenerator()
    {
        BackgroundGenerator bkgGenerator = FindObjectOfType<BackgroundGenerator>();
        if (bkgGenerator == null)
        {
            Debug.LogError("PlanetManager: Could not find BackgroundGenerator in scene");
            return;
        }

        bkgGenerator.terrainTile = currentPlanet.terrainTile;
    }
}
