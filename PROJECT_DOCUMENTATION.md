# Capstone Game Prototype: 2D Top-Down Action RPG
*Comprehensive Technical and Gameplay Architecture Documentation*

> For full repository documentation, also see [README.md](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/README.md).

---

## 📌 Executive Summary

* **Project Name**: `Capstone_Game_Prototype_JM_George`
* **Product Name**: `2DTopDown`
* **Version**: `1.1` (Bundle version code `1`)
* **Engine / Editor Version**: **Unity `6000.6.7f1` (Unity 6 Tech / Preview)** (Revision: `b22767f04f04`)
* **Render Pipeline**: **Universal Render Pipeline (URP 2D)** v`17.6.0`
* **Input System**: **Unity New Input System** v`1.20.1`

---

## 🎮 How the Game Works

### 1. High-Level Game Concept
**2DTopDown** is a top-down action adventure RPG. The player navigates through corrupted areas filled with monsters, takes on quests from various NPCs, wields interchangeable weapons (Sword, Bow, Staff), commands a loyal AI dog companion, and battles the **Demon King**.

### 2. Multi-Ending Branching Structure
The game contains **9 interconnected scenes** in `EditorBuildSettings.asset`:
* **`MainMenu` (Scene 0)**: Initial start menu with Tutorial, Play Game, and Quit.
* **`Tutorial` (Scene 1)**: Introduces the player to the Warrior Chief, Training Dummy, and Elf guard. Teaches movement, dashing, weapon combat, and companion commands.
* **`Scene1` (Scene 2)**: Forest Outpost where the Red Knight tasks the player with purging Blue Slimes.
* **`Scene2` (Scene 3)**: Deep Forest where a Merchant asks the player to rescue his lost Daughter and cleanse the monsters.
* **`Scene3` (Scene 4)**: Haunted Ruins where an Elf Scout prompts the player to defeat wraiths/ghosts.
* **`Scene4` (Scene 5)**: The Demon King's Lair. Slaying the 400 HP Bullet Hell Demon King unlocks the exit portal.
* **Multi-Ending Condition (`AreaExit.cs`)**:
  - If the player escapes with **< 10 Gold Coins** $\rightarrow$ **`GameWin.unity` (Scene 6)**.
  - If the player collects **≥ 10 Gold Coins** $\rightarrow$ **`GameWinHidden.unity` (Scene 7 - Secret True Ending)**.
* **Death State**:
  - If player health drops to 0 $\rightarrow$ **`GameOver.unity` (Scene 8)**.

---

## ⌨️ Controls & Input Mapping

| Action | Keyboard / Mouse | Gamepad | Description |
| :--- | :--- | :--- | :--- |
| **Movement** | `W`, `A`, `S`, `D` | **Left Stick** | 8-directional top-down movement |
| **Attack** | **Left Mouse Button** | **Button East (B / ⭕)** | Attacks with currently equipped weapon |
| **Dash** | **Spacebar** | **Button South (A / ❌)** | 4x speed burst, costs 1 stamina |
| **Interact** | **E Key** | **Button West (X / ⏹️)** | Advances dialogue, accepts quests |
| **Weapon 1 (Sword)** | **1 Key** | **Left Shoulder (LB)** | Melee blade with slash wave animation |
| **Weapon 2 (Bow)** | **2 Key** | **Right Shoulder (RB)** | Long-range arrow shooter (50 dmg) |
| **Weapon 3 (Staff)** | **3 Key** | **Left Trigger (LT)** | Auto-targeting laser beam (5 dmg) |
| **Dog Command** | **G Key** | Mouse Target + Click | Commands dog to attack targeted enemy or move to point |

---

## 📦 Installed Packages & Modules

Configured in `Packages/manifest.json`:
* `com.unity.render-pipelines.universal` (`17.6.0`): Universal Render Pipeline with 2D lighting and post-processing.
* `com.unity.inputsystem` (`1.20.1`): Event-based New Input System.
* `com.unity.cinemachine` (`6.6.0`): Procedural camera tracking and impulse screen shake.
* `com.unity.ugui` (`2.6.0`): Canvas UI & TextMesh Pro.
* `com.unity.feature.2d` (`2.0.2`): 2D animation, physics, and tilemap features.
* `com.unity.visualscripting` (`1.9.12`): Visual scripting graphs.
* `com.unity.timeline` (`6.6.0`): Timeline cutscenes.
* `com.unity.services.authentication` (`3.8.0`) & `com.unity.multiplayer.center` (`2.1.0`): Gaming services hooks.
* Modular components: `physics2d`, `physicscore2d`, `tilemap`, `audio`, `particlesystem`, `animation`.

---

## 🛠️ Complete C# Script Catalog

### Core & Game Systems
* [**`GameManager.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/GameManager.cs): Global persistent game singleton managing NPC conversations, active quest givers, kill counts, and damage tallies.
* [**`AudioManager.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/AudioManager.cs): Plays background music loops and SFX (dash, death, portal).
* [**`MainMenu.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/MainMenu.cs): Main menu scene navigation logic.
* [**`DemonKing.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/DemonKing.cs): Boss entity identifier and health bridge.
* [**`PortalManager.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/PortalManager.cs): Monitors Demon King health and activates the exit portal on death.
* [**`QuestManager.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/QuestManager.cs): Central reference hub for dialogue and quest UI elements.

### Player & Weapons
* [**`PlayerController.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Player/PlayerController.cs): Player movement, dashing, facing direction, trail rendering, and input routing.
* [**`PlayerHealth.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Player/PlayerHealth.cs): Player HP, damage recovery invulnerability, hurt flash, screen shake, and death sequence.
* [**`Stamina.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Player/Stamina.cs): Stamina orb management and automated cooldown regeneration.
* [**`ActiveWeapon.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Player/ActiveWeapon.cs): Weapon swapping, attack throttling, and cooldown coroutines.
* [**`Sword.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Player/Sword.cs): Melee sword swing logic and slash animation generation.
* [**`Bow.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/UI/Bow.cs): Ranged projectile shooting.
* [**`Staff.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/UI/Staff.cs): Auto-targeting laser weapon targeting closest enemy or dummy within 8 units.
* [**`Magic Laser.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/UI/Magic%20Laser.cs): Expanding beam collider and sprite scaling logic.
* [**`Projectile.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Player/Projectile.cs): Linear projectile movement, distance culling, and impact handling.
* [**`DamageSource.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Player/DamageSource.cs): Applies active weapon damage to enemies and training dummies.
* [**`WeaponInfo.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/UI/WeaponInfo.cs): ScriptableObject containing weapon cooldown, damage, range, and prefab.
* [**`ActiveInventory.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/UI/ActiveInventory.cs): Hotbar slot switcher responding to 1-5 number keys.

### Companion AI (Dog)
* [**`DogFollow.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/AI%20Companion/Dog/DogFollow.cs): Smooth player following, distance teleport failsafe, and `G` key command interceptor.
* [**`DogAIAcontion.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/AI%20Companion/Dog/DogAIAcontion.cs): Moves to ordered point or attacks target (10 damage, 1s cooldown).
* [**`DogController.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Player/DogController.cs): Autonomous companion behavior finding and engaging nearest enemies.

### Enemies & Boss
* [**`EnemyAI.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Enemies/EnemyAI.cs): Roaming and attacking state machine for standard enemies.
* [**`EnemyHealth.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Enemies/EnemyHealth.cs): Health, knockback, flash, item drops, and kill reporting to quests.
* [**`EnemyPathfinding.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Enemies/EnemyPathfinding.cs): Smooth velocity movement and sprite flipping.
* [**`Shooter.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Enemies/Shooter.cs): Bullet hell pattern generator (bursts, spreads, staggered volleys, oscillations).
* [**`Grape.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Enemies/Grape.cs): Single projectile emitter with attack animations.

### NPC, Quests & Progression
* [**`NPCInteraction.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/NPC/NPCInteraction.cs): Handles NPC dialogue, portrait display, choices, quest requirements, and quest completion.
* [**`QuestDefinitions.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/NPC/QuestDefinitions.cs): Data structures defining quests, kill targets, damage requirements, and gold goals.
* [**`DummyTarget.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Dummy/DummyTarget.cs): Practice dummy in Tutorial that logs damage toward the Warrior Chief's quest.
* [**`ExitAreaCondition.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/UI/ExitAreaCondition.cs): Barrier collider that only deactivates when required NPCs and quests are finished.
* [**`AreaExit.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Management/AreaExit.cs): Loads adjacent scenes; branches between Normal Win and Hidden Win based on gold count ($\ge 10$).
* [**`AreaEntrance.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Management/AreaEntrance.cs): Aligns player position to the corresponding entrance node.

### Economy, Pickups & World Polish
* [**`EconomyManager.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Misc/EconomyManager.cs): Tracks collected gold coins and updates 3-digit UI.
* [**`Pickup.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Pickup.cs): Bouncing pickup items (coins, health, stamina) with player magnetic homing.
* [**`PickUpSpawner.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/PickUpSpawner.cs): Drops random loot from broken objects and dead enemies.
* [**`Destructible.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Misc/Destructible.cs): Destructible barrels, crates, and bushes.
* [**`ScreenShakeManager.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Misc/ScreenShakeManager.cs): Cinemachine camera impulse triggers.
* [**`TransparentDetection.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Misc/TransparentDetection.cs): Fades trees and roofs when player is occluded behind them.
* [**`Flash.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Misc/Flash.cs): Material flash effect on hit.
* [**`Knockback.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Misc/Knockback.cs): Physics knockback impulse when taking damage.
* [**`UIFade.cs`**](file:///c:/Users/JM/Documents/Unity%20Projects/Capstone_Game_Prototype_JM_George/Assets/Scripts/Management/UIFade.cs): Screen fade-to-black and fade-in transitions.

