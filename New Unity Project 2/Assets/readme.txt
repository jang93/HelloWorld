Top-Down Shooter Kit for Unity

v1.0 Documentation File

VERY IMPORTANT!!!

Once you have downloaded the Unity package file and imported it into a new project, the first
thing you should do is set up tags and layers (which are not supported by Unity packages) 
so the game will work properly.

Go to Edit->Project Settings->Tags.

Click the little arrow next to Tags to drop the array down, and a "PathNode" tag.

Then, add the following layers, starting at "User Layer 8":

Ground
Civilian
Enemy
World
Item
Dead
Military
Map

Choose File->Save Project, and you're good to go!

This is necessary because Project settings like Tags and Layers are not included in Unity 
packages.  If you would like to see this support added, give a vote to this request on the 
Unitty feedback system:

http://feedback.unity3d.com/forums/15792-unity/suggestions/2232767-include-tags-layers-in-unitypackage-exports

With that taken care of, let's dive into the Top Down Shooter Kit for Unity!


Introduction

Open the MainMenu scene and click run!

The best way to get started is to play the different game modes (Survival, Bughunt, and 
Zombie Infection Simulation) to get a feeling for how the game works.

This document describes the Top Down Shooter Kit in general terms, providing a high level 
overview of the system.  For specifics, it is recommended you have a look at, and modify, 
individual script files, which are extensively commented and written as clearly as possible.

Project Organization

The project is organized into folder by asset type, i.e. C# scripts are located in the Scripts 
folder, prefabs in the Prefabs folder, GUI assets in the GUI folder, etc.

The Main Menu

The Main Menu is controlled primarily by the MainMenu.cs C# script file, which is 
responsible for rendering the GUI.  The Main Menu's primary function is to load a game 
scene file depending on which button the user clicks.

The Game Scene Files

The three game mode scene files (Survival, Bughunt, and Survival) are standalone, meaning 
they can be opened and run in the Unity Editor, and even built and run separate from the 
rest of the game, with no cross-scene independences or requirements.

The GameManager class

The game is controlled at a high level by the GameManager script, which keeps track of 
various things including Player score, system preferences like audio settings, pause menu, 
etc.

The exposed GUISkin reference allows for each game mode to customize the appearance of 
common menus like the pause menu.

The GameManager is implemented using a Singleton design pattern, which ensures there is 
only ever one GameManager instance created at run time, and also allows global access to 
the current GameManager instance.  This is done using a static GameManager instance 
within the class which is assigned in the Start() function.

Each game mode implements a GameManager subclass, which extends the GameManager 
by adding game mode specific code.  The game mode specific GameManager classes are 
called SurvivalManager, BughuntManager and SimulationManager.  These game mode 
specific managers are responsible for displaying elements unique to the three game modes, 
i.e. wave info in Survival, mission briefing in Bughunt, and the PDA in the Zombie Infection 
Simulation.

Units

All units (i.e. characters, monsters, etc) are derived from the Unit base class, which is 
mainly responsible for unit movement and item/weapon management.  The Unit class, it is 
worth noting, is based on the Damageable class, which allows units to take damage and die.

Instead of using the Unity CharacterController Component, TDSK units handle collision and 
movement using a capsule collider and rigidbody combination.  For this reason, movement 
commands are executed in the Unit.FixedUpdate() function.

Unit subclasses include UnitAI (for AI controlled units) and UnitPlayer (for player 
controlled units, as well as a special UnitPlayerSim subclass used on the Simulation player 
unit.

The UnitAI class includes several capabilities, including enemy evaluation and targeting, 
fighting/fleeing, path following, basic obstacle avoidance, wandering and tethering.

Weapons

Weapons attached to a unit are fired when the unit's target enemy is within the weapon's 
minimum range.  The unit attempts to fire constantly, and the weapon controls rate of fire.  

The base Weapon.Fire() function handles common aspects such as ammo and reloading, 
rate of fire, muzzle flash and other effects.  The base Fire( ) function is called by Weapon 
subclasses, and returns true if the Weapon is capable of firing in the current frame.

Weapon subclasses implement different weapon behaviours: WeaponRay implement 
"instant line" based weapons such as guns and melee attacks; WeaponProjectile 
implements projectile based weapons such as bows or the "pulse rifle" in the Bughunt 
game; WeaponParticle implements particle system weapons such as the Flame Thrower.

Mini Map

The Mini Map in the Bughunt game mode is based on a top down camera which follows the 
player, renders only Map objects, and uses a RenderTexture technique to clip to a circle.

Other Features

There are numerous other features at work in the various game modes, including spawns, 
triggers and triggered events, mission objectives, visual and audio effects, etc.

There are also several "best" techniques demonstrated, including using parent 
GameObjects to keep the run-time hierarchy neat (see the ObjectRoot class), a static Util 
class that provides global access to various useful functions, and other useful tips and 
tricks.

Once again, it is recommended you play the various game modes to get a sense of how they 
work, and then have a look at specific areas of the code that are of interest to you.

Feedback, comments, questions?

Visit the Top Down Shooter Kit forum at 
http://www.powerupstudios.com/phpBB3/viewforum.php?f=17.

