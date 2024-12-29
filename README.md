# <span style="color: cyan;"> **<ins>~ Mutant Mayhem ~<ins>** </span>

<br>

**A fast paced 2D top-down shooter with base building and roguelite mechanics.**

<br>

**<span style="color: red;"> You'll die often...</span>**

<br>

Approach each play as a customized clone created by an AI powered Mothership on a mission to learn about the Mutant anomalies.  Gain a foothold on all the planets in the Mutant solar system to win the game.  To progress through the planets you must survive waves of increasingly difficult and numerous enemies.  Strategically upgrade your gear, build structures, and master your chosen arsenal to survive.  Accumulate Research Points after each run and use them customize your next clone.

<br>

<span style="color: red;"> ---= [\~~ **|| <ins>DOWNLOAD HERE</ins> ||** \~~ ](https://kamjamgames.itch.io/mutant-mayhem) <span style="color: red;"> =---

<br>

## <span style="color: white;"> Video Trailer </span>

[![Watch the demo](https://img.youtube.com/vi/cPmlhL2QJg8/hqdefault.jpg)](https://www.youtube.com/watch?v=cPmlhL2QJg8)  
<span style="color: cyan;">_Click the image to watch a gameplay demo!_</span>

## <span style="color: white;"> Current Features </span>

- Challenging fast paced top-down shooting action
- Base building, defense and repair
- EVERYTHING is upgradable (Structures, player stats, gun stats)
- Roguelite mechanics.  Collect and spend Research Points to customize your character before each run
- Choose between 3 character types
- 3 enemy types with unique behaviors
- 3 attack types: Gun, melee, and throw
- 2 damage types: Lasers and bullets
- Built in music player to choose your vibe
- 3 difficulty levels
- A solar system of unique planets to conquer <span style="color: white;"> **(Coming Soon)** </span>

## <span style="color: white;"> Installation </span>

Bypass your operating system's security pop-ups

- **<span style="color: cyan;">Windows:</span>** Click 'More Info' in the security pop-up. Then click 'Run Anyway'.
- **<span style="color: cyan;">Mac:</span>** *After attempting to open the game*, go to 'System Settings', navigate down the left-side menu to 'Privacy and Security' and scroll to the bottom 'Security' section to allow the game to run.

## <span style="color: white;"> Development Tools </span>

- <span style="color: cyan;">**Game Engine**: </span> Unity v2022.3.27f1 <br>
- <span style="color: cyan;">**Programming:** </span> VS Code, C#, ChatGPT <br>
- <span style="color: cyan;">**Version Control:** </span> Git and GitHub <br>
- <span style="color: cyan;">**Graphics:** </span> GIMP, Aseprite, Blender, Materialize <br>
- <span style="color: cyan;">**Audio:** </span> Audacity, Ableton

## <span style="color: white;"> Credits </span>

- <span style="color: cyan;">**Game Developer:** </span> Matt Van Alstyne <br>
- <span style="color: cyan;">**Graphics:** </span> OpenGameArt.org, Matt Van Alstyne, ChatGPT (gun icons only) <br>
- <span style="color: cyan;">**Audio:** </span> OpenGameArt.org and a long list of content creators which you will soon be able to find in-game.  Matt Van Alstyne for audio editing and the 'Laser Sword' Ableton instrument creation

## <span style="color: white;"> Developer's Journey </span>

This is the first game I've made in Unity.  It's a remake of a project I did in GameMaker around 18 years ago, where I spent a good chunk of time in my teenage years learning and making a handful of rough games.  After a good decade plus of minimal game-making and maximal life-living, the game-maker in me, and Mutant Mayhem, has been reborn.  I'm amazed at all the tools and systems available in Unity, it sure beats the features I had to work with in GameMaker back then!

I'm really enjoying the fact that I'm now able to learn about and implement any system I manage to dream up.  When I was younger I quickly ran into walls and did not know how to learn properly.  The biggest challenge lately has been to get to sleep on on time, and remember to take breaks from learning and practicing full time.  I absolutely love it.

1400+ hours into game development later, I'm submitting Mutant Mayhem as my final project to the CS50 course which taught me all the skill I was missing in my younger years.  I plan to continue to develop the game, and see where it ends up.  Multiplayer and mobile are currently on the table with fates yet to be determined, much like my own.

## <span style="color: white;"> Known Issues <span>

- <span style="color: orange;"> Frame rate drops in busy scenes </span>
  - Object pooling needs to be used in more places
  - Caching and reference injection can be used in more places
- <span style="color: orange;"> Occasional loss of control of build menu </span>
  - The scroll wheel input should be removed from the UI InputAsset and the build menu should be controlled by script only, not the UI.
    - I've 'fixed' this bug a dozen times or more, but this is hopefully the solution
- <span style="color: orange;"> Insufficient beginner guidance, UI experience, and game content </span>
  - Some parts of the game are still in rough draft or incomplete

## <span style="color: white;"> Future Plans (in no order) <span>

- Complete the 'Planets' branch
- Add more guns and enemies
- Add bosses
- Attempt to add controller support
- Play around with mobile
- Learn more about Netcode and Multiplayer in Unity
  - Attempt to add multiplayer
- Add obstacles
- Add traps
- Add resource patches and mines





## <span style="color: white;"> Contact <span>

**Thanks for your interest in Mutant Mayhem!**   
Please feel free to reach out!  
All feedback is greatly appreciated

- KamJamGames@gmail.com

<br><br><br>

# <span style="color: white;"> Explanation of Systems <span>
##### <span style="color: white;"> This section is for nerds, and my CS50 final project submission <span>

Below is a representation of the main systems and core mechanics of the Mutant Mayhem

### <span style="color: cyan;"> **The Player:** </span>

- Contains:
  - Walking sounds, grenade prefabs, head and hand Transforms
  - PlayerStats class:
    - Contains stat values and multipliers
    - Affected by the Augmentations, Upgrades, and Planet Property modifiers
    - Contains references to the Player’s StructureStats, Health, and Shooter scripts
- Communicates with:
  - AnimationControllerPlayer 
  - MeleeControllerPlayer
- Used for:
  - Looking, moving, sprinting, and turning the head
  - Listening for weapon change or grenade throw
  - Initializing the game systems on level start
- Reflection:
  - Some parts of the player could be separated into their own scripts, to follow SOLID principles of OOP
  - Will need some changes if multiplayer becomes a thing
  - Is quite simple and well encapsulated
  
### <span style="color: cyan;"> **AnimationControllerPlayer:** </span>
  
- Contains:
  - Animation speeds and timings,
- Communicates With:
  - Player
  - PlayerShooter
  - BuildingSystem
- Used for:
  - Listens for various player inputs
  - Control the animations WITHOUT a state machine
- Reflection:
  - This should really be a state machine and is a prime example of when to use one
    - The script was made before I knew what a state machine was or how to make one
  - Some inputs are not related to animation, and therefore should be refactored
  - It's a large and complicated script compared to most others in the game

### <span style="color: cyan;"> **MeleeControllerPlayer:** </span>

- Contains: 
  - Reference to it's player's PlayerStats (includes melee damage)
  - Stats for stamina use, accuracy loss, damage variance, sound
- Communicates with:
  - Sword Controller
  - PlayerShooter
  - AnimationControllerPlayer
- Used for:
  - Hitting enemies' health
  - Setting animation trigger
  - Playing sound
  - Toggling the laser sword on/off
  - Affecting stamina and accuracy
  - Triggering effects
- Reflection: 
  - The sword sounds and other stats could be moved to the SwordController

### <span style="color: cyan;"> **The Enemy:** </span>

- Started super simple, then was expanded some months ago into a state machine
- Contains:
  - Object Pool Name, EnemyHealth script, Randomization variables, movement variables
  - State machine with it's states and trigger booleans
- Communicates with:
  - AnimationControllerEnemy
  - MeleeControllerEnemy
  - WaveController
  - Object Pool
- Used for:
  - Running a Scriptable Object (SO) state machine
    - The SO's hold behaviour logic and can be plugged in to the state machine
      - This allows for incredibly simple creation of new enemy AI
      - Ex. IdleState, ChaseState, ShootState, MeleeState all run in a regular state machine.  Instead of running code in each state, they reference an SO and run it's code.  So, when creating a new enemy's behaviours, I can just plug in a mix of desired logic, or change the logic during runtime by replacing the SO reference
  - Randomizing it's own stats when spawned (recycled in object pool)
    - Difficulty increases each wave via WaveController's settings
- Refection:
  - The state machine was an amazing upgrade from it's simple roots
  - I played with A* Pathfinding for a bit but decided to scrap pathfinding
  - The game started getting laggy quickly before the state machines which improved performance
  - It was during adding state machines that I discovered the Profiler for viewing specific system utilization info

### <span style="color: cyan;"> **Health** </span>

- Contains:
  - MaxHealth, multiplier for credit value on kill, painSounds
  - Colors for textFly damage effect
- Communicates with:
  - PoolManager (object pooling) for textFly
- Used for:
  - Keeping track of health, adjusting health
  - Knockback
  - Pain sounds
  - Is a parent to PlayerHealth, EnemyHealth, and QCubeHealth
- Reflection:
  - This script evolved a few times over the last 6 months, and has become fairly simple
  - The textFly effects could exist as their own class

### <span style="color: cyan;"> **Gun SO** </span>

- Is a scriptable object (like a data container)
- Contains:
  - GunType, Gun stats, bullet stats, upgrade amounts for each stat
  - Sound, muzzle flash, GunSights
- Communicates with:
  - Nothing, others use it as a data container
- Used for:
  - Storing weapon and bullet stats, upgrade values, and effects
  - Is a parent to TurretGunSO
- Reflection:
  - This has proven very handy for managing multiple weapons and weapon types through all the games systems

### <span style="color: cyan;"> **Bullet** </span>

- Contains:
  - Only a few things in it's prefab that make up the visuals and AI triggers
  - Empty stats set by the shooter's GunSO
- Communicates with:
  - Health
  - TileManager
  - Object Pool
- Used for:
  - Flying across the screen
  - Dealing damage to health scripts or hitting structures
  - Is a parent to RepairBullet
- Reflection:
  - It fun learning to use ray casting here, to prevent fast bullets from going through walls
  - The precise collision point allows for cool directional and positional effects

### <span style="color: cyan;"> **Sword Controller** </span>

- Contains:
  - Sword handle and tip positions
  - A Polygon Collider 2D
  - A Trail Renderer
  - Trail size variables
- Communicates with:
  - MeleeControllerPlayer
- Used for:
  - Dynamically draws a triangle each frame from the swords last to current positions
  - Hits enemies who collide with it
  - Shortens it's draw distance when colliding with a wall
    - Which in turn adjusts the line renderer's width, so the sword does not go through walls
    - When elevated (On a raised platform) the sword will go through walls
- Reflection:
  - When even a small piece of a wall gets between the players hand (sword handle) and tip, it will block the sword, which can be hard to see visually, and this somethings causes an issue
  - I'm very proud of my ideas for preventing the sword going through walls

### <span style="color: cyan;"> **Throw** </span>

- Contains:
  - Explosion prefab
  - Throw flight variables
  - Target position
- Communicates with:
  - Nothing, it is handed it's information from the thrower
- Used for:
  - Creates an effect on the grenade sprite to make it look like it flies through the air
    - The y-position is adjusted to make it follow a parabolic path, despite the top down view
    - The sprite is scaled up then down by linearly interpolating over a parabolic curve, to make it appear to come closer to the camera    
    - It slows down as it reaches the target
    - Rotates as it flies
- Reflection:
  - Looks fairly natural, all things considered
  - I would like to improve the throwing mechanic to have a range limit, accuracy, and interactive throw aimer

### <span style="color: cyan;"> **Explosion** </span>

- Contains:
  - Sound
  - Explosion settings
    - Force, radius, damage
  - Can link to Unity's Wind
- Communicates with:
  - Health
  - TileManager (structures)
- Used for:
  - Creates a realistic physics based explosion
  - Raycasts to all tiles within the radius
  - Deals damage to any structures that block the ray paths
  - Ends up with a list of unblocked tiles
  - Deals damage to all Health scripts in the unblocked tiles
  - Does damage relative to explosion proximity
- Reflection:
  - Explosions used to go through walls, and even did no damage to structures or the Quantum Cube for a while
  - The final product of the physics based raycast explosion is fantastic
  - Another of my proud accomplishments, also very fun

### <span style="color: cyan;"> **Building System** </span>

- Contains:
  - A list of all structures
  - A list of structures unlocked at start
  - A dictionary of unlocked structures
  - LineRendererCircle script I made for build range
  - Player Credits
  - LayerMasks for build is clear check
  - Build cost multiplier
  - Current rotation
- Communicates with:
  - BuildMenuController
  - QCubeController
  - MouseLooker
  - CameraController
  - TileManager
  - TurretManager
  - CursorManager
  - StructureRotator
- Used for:
  - Keeps track of current structure in hand
  - Building, rotating, and removing structures
  - Showing the outline and building preview highlights
  - Toggles the Build Menu and locks some controls
- Reflection:
  - This is an older script in the game, and I've learned a lot since creating it
  - It has many dependencies and general bad design. It would be really nice to clean up one day
  - It was really cool learning how to rotate matrices of vertices with the help of ChatGPT
  - The build highlights and rotation and all got very complex, this was a deep project
    - Improved design from the start would likely have made things much easier

### <span style="color: cyan;"> **TileManager** </span>

- Contains:
  - A dictionary of all structures positions and their TileStats
    - TileStats contains the the RuleTileStructure, health, and it's grid position
      - RuleTileStructure contains the StructureSO and a list of Tiles (for damage)
        - StructureSO contains the remaining info for the structure (UI name/image, descriptions, cell positions, cost)
- Communicates with:
  - Player
  - BuildingSystem
  - TurretManager
- Used for:
  - Checking if grid is clear
  - Adding and removing tiles
  - Keeping track of each tile's health, stats, and grid positions
  - Applying changes to tile health
  - Applying upgrades to all structures
- Reflection:
  - This was some of the first work I've done with dictionaries and was thus very satisfying

### <span style="color: cyan;"> **Wave Controller** </span>

- Contains:
  - Wave properties
    - Current wave index, time between waves, and other difficulty settings
  - Enemy multipliers
    - Batch size, damage, health, speed, size
- Communicates with:
  - WaveSpawner
  - Player
  - Other scripts check the wave index and adjust multipliers externally
- Used for:
  - Controlling and incrementing the difficulty of the current wave and enemies spawned in it
- Reflection:
  - Multiplier adjustments / Wave settings could be controlled internally more, to avoid making a more of a mess

### <span style="color: cyan;"> **Wave Spawner** </span>

- Contains:
  - Starting wave settings
    - Max index to select at start from enemy subwaves list
    - Number of subwaves in first wave
    - Time to next subwave base value
    - Min time to next subwave
- Communicates with:
  - WaveController
  - TileManager
  - 
- Used for:
  - Spawns enemies dynamically around a circle
    - Subwave Style
      - Determines where around the circle the current batch with be spread, based on a center point on the circumference
        - Ex. In a small section of the circle, or around half the circle, etc
      - Determines where next batch will be centered from on the circle
        - Ex. 30 degrees clockwise, or +- 60 degrees, or random between range
  - The circumference of the spawn circle grows as the player's base grows in size, to help avoid enemies spawned on screen
  - In the starting waves, only a few enemies on the Master Wave list are available for the wave generator
    - A wave setting controls how quickly new enemies in the list become available to spawn.  This allows for more difficult enemies to come out later, and also for a bit more flow control
- Reflection:
  - This was really fun to make and had a few iterations to get here
  - Originally it was much more linear where I would have to design every batch of enemies of every wave of every level
    - This was obviously much too tedious, but also much too linear.  Due to the game's nature of having lots of options, highly randomized and unique waves seemed suiting
  

### <span style="color: cyan;"> **Upgrades** </span>

- Contains:
  - All the child classes of upgrades
  - Each upgrade has it's own logic for:
    - Upgrade cost increment
    - Upgrade amount increment
    - Applying the upgrade
- Communicates with:
  - UpgradeManager
  - UpgStatGetter
- Used for:
  - Applies upgrades in exchange for credits
  - Includes consumables like healing, grenades, and ammo
- Reflection:
  - This design ended up feeling overly complex for it's function.  I often think of ways to simplify it
  - I used AI to brainstorm ideas for a while, and definitely learned some cool tricks while expanding my C# knowledge

### <span style="color: cyan;"> **Augmentations** </span>

- Contains:
  - Singleton Pattern
  - A child class for each augmentation
  - UI info for the augmentation
  - Max level, cost, cost increment, and cost increment multiplier
  - Min level, refund, refund increment, and refund increment multiplier
- Communicates with:
  - AugManager
  - Scripts that the aug affects
- Used for:
  - Adjusting stat and stat cost multipliers in exchange for research points
- Reflection:
  - These turned out simple, much nicer to work with than Upgrades
  - There are now multiple layers of multipliers multiplying each other between difficulty, upgrades, player class, augmentations, and planet modifiers.  Things get a little confusing sometimes, but are working well.

### <span style="color: cyan;"> **Planets** </span>

- Contains:
  - Singleton Pattern
  - PlanetManager has a list of PlanetSOs and a dictionary of stat multipliers for the current planet
    - Planet SO contains the bulk of the data
    - Each planet has a mission, a list of wave difficulty settings, and a list of properties
      - Each property contains a list of PlanetStatModifiers
        - PlanetStatModifier does the actual adjustment to the stat multipliers
- Communicates with:
  - Player
  - Building System
  - TurretManager
  - WaveController
- Used for:
  - Applying planet mission, difficulty, and stat multipliers
- Reflection:
  - Very easy to adjust and create new planets, and properties
  - This feature is still in development and has not yet been merged with the main branch.  See the add-planets branch in the [GitHub repository](https://github.com/AiryShelf/Mutant-Mayhem.git) for the latest details

### <span style="color: cyan;"> **Profile Manager** </span>

- Contains:
  - Singleton pattern
  - A list of .json profiles saved on the device
  - The currently selected profile
    - Profiles contain preference settings, difficulty, progress, Research Points, and more
- Communicates with:
  - Nothing, other scripts check, modify, and save the current profile via the persistent static ProfileManager
- Used for:
  - Creating, saving, loading, removing profiles
  - Tracking planet progress
- Reflection:
  - Planet progress could be separated from the profile manager, though it is so closely entwined, I don't see enough reason at the point.  Maybe if more features are added

### <span style="color: cyan;"> **FadeCanvasGroupsWave** </span>

- A messy but highly versatile script I made for controlling fades of UI elements
- Contains:
  - Lots of options
  - Ability to fade individuals or batches or all
  - Settings for fade time and delay between elements
- Communicates with:
  - Optional: Starting Canvas
  - Optional: A list of CanvasGroups
  - Optional: Next FadeCanvasGroupsWave to trigger
- Used for:
  - Is used extensively across the game, in almost every UI section to fade UI sections in a wave or all at once
  - Highly customizable fade behaviours
  - Optional: Deactivates individuals with fade
- Reflection:
  - This script is very messy and long
  - I made it very early on while learning to program, I'm sure it could be simplified
  - I think one of it's features is not working correctly, but it's been so long since I needed the feature I've forgotten if I fixed it

### <span style="color: cyan;"> **Pool Manager** </span>

- Contains:
  - Singleton pattern
  - 3 linked lists for pool prefabs, names, and amounts to create
  - A dictionary keeping track of how many of each object have been created
- Communicates with:
  - Nothing, other scripts make requests for objects or pools
- Used for:
  - Supplying and holding pools of objects to optimize performance
  - Creating the starting object pools
  - Creating new object pools
  - Increasing pool size when needed
- Reflection:
  - This had such a profound impact on performance, I was surprised at the increased frame rate!
  - It was easier to implement than I thought it might be, even though there was a lot of changes and re-initializing that had to happen

### <span style="color: cyan;"> **SFX Manager** </span>

- Contains:
  - Singleton pattern
  - SoundSO contains info for each sound
    - Sound name and type
    - Mixer group
    - Audio clips
    - Loop option
    - Pitch setting and random range
    - spatial blend with min and max distances
  - Music, SFX, and UI mixer groups
  - Mixer snapshots, to transition settings for certain game moments
  - Lists of available and used audio source pools for music, sfx, and ui sounds
- Communicates with:
  - Nothing, objects request audioSources to play their sfx on
- Used for:
  - Optimizing high volumes of sfx by re-using audio sources that have already been initialized for the same sound
- Reflection:
  - This seems to work well, but when a machine gun fires really fast, it seems to take over all the sound
    - This may have to do with the compressor on the mixer

### <span style="color: cyan;"> **Particle Manager** </span>

- Contains:
  - Singleton pattern
  - A serialized field for each of the main particle systems used in the game
  - A public method for each effect, often combining multiple particle systems
- Communicates with:
  - Nothing, other scripts call the public methods to have their effects played
- Used for:
  - Performance optimization by massively reducing the number of particle systems
    - A 20 minute game could easily have 1000 particle systems on the screen at times before the optimization was made
    - After optimization, ~1000+ systems was reduced to a consistent 23
  - Keeping all the particle system effects in one place for ease of maintenance 
- Reflection:
  - Consolidating the particle systems was on my mind for a while, and the performance gain was tangible
  - I feel I could design a more modular system, or something more flexible.  Currently, I have to add a new method to add a new effect

<br><br>

### <span style="color: white;"> **Closing Statements** </span>

I tried a lot of different designs throughout this project, as I've been keen to learn and experiment.  Therefore, it is a bit of a mess in places and is somewhat inconsistent in design/coding style.

_It's been one heck of a ride so far with  a road full of challenges, learning, and conquering both behind me and ahead.  I can't wait to see what I accomplish next!_

<br><br>

<span style="color: cyan;"> **Thanks for your interest in Mutant Mayhem!** </span>

Let me know your thoughts!  
- KamJamGames@gmail.com
