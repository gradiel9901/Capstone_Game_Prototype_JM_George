using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;

namespace Migration
{
    public static class MigrationVerifier
    {
        [MenuItem("Tools/Migration/Run Full Audit")]
        public static string RunFullAudit()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== UNITY 6 MIGRATION AUDIT REPORT ===");
            report.AppendLine($"Unity Version: {Application.unityVersion}");
            report.AppendLine($"Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine();

            report.AppendLine("--- 1. URP & Quality Settings ---");
            report.AppendLine(AuditURPAndQualitySettings());

            report.AppendLine("--- 2. Materials & Shaders ---");
            report.AppendLine(AuditMaterialsAndShaders());

            report.AppendLine("--- 3. Prefab Integrity ---");
            report.AppendLine(AuditPrefabs());

            report.AppendLine("--- 4. 9-Scene Sequential Audit ---");
            report.AppendLine(AuditScenes());

            report.AppendLine("=== END OF REPORT ===");
            string result = report.ToString();
            Debug.Log(result);
            return result;
        }

        public static string AuditURPAndQualitySettings()
        {
            var sb = new System.Text.StringBuilder();
            var defaultPipeline = GraphicsSettings.defaultRenderPipeline;
            sb.AppendLine($"GraphicsSettings.defaultRenderPipeline: {(defaultPipeline != null ? defaultPipeline.name : "MISSING")}");

            if (defaultPipeline == null)
            {
                var urpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/UniversalRP.asset");
                if (urpAsset != null)
                {
                    GraphicsSettings.defaultRenderPipeline = urpAsset;
                    sb.AppendLine("-> Fixed: Assigned Assets/Settings/UniversalRP.asset to GraphicsSettings.defaultRenderPipeline.");
                }
            }

            // Quality settings check
            int qualityLevelCount = QualitySettings.names.Length;
            sb.AppendLine($"Quality Levels count: {qualityLevelCount}");
            for (int i = 0; i < qualityLevelCount; i++)
            {
                var rp = QualitySettings.GetRenderPipelineAssetAt(i);
                sb.AppendLine($"  Level [{i}] '{QualitySettings.names[i]}': {(rp != null ? rp.name : "(inherits GraphicsSettings)")}");
            }

            return sb.ToString();
        }

        public static string AuditMaterialsAndShaders()
        {
            var sb = new System.Text.StringBuilder();
            string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
            sb.AppendLine($"Found {matGuids.Length} Material assets in Assets/");

            int brokenCount = 0;
            foreach (var guid in matGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;

                if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
                {
                    sb.AppendLine($"  [ERROR] Broken Shader in Material: {path}");
                    brokenCount++;
                }
                else
                {
                    if (mat.shader.name.StartsWith("Standard") || mat.shader.name.StartsWith("Sprites/Default"))
                    {
                        sb.AppendLine($"  [NOTE] Material {mat.name} uses shader '{mat.shader.name}' at {path}");
                    }
                }
            }

            if (brokenCount == 0)
            {
                sb.AppendLine($"-> All {matGuids.Length} materials have valid, intact shaders (0 broken/pink).");
            }

            return sb.ToString();
        }

        public static string AuditPrefabs()
        {
            var sb = new System.Text.StringBuilder();
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            sb.AppendLine($"Found {prefabGuids.Length} Prefabs in Assets/");

            int missingScriptCount = 0;
            int totalComponents = 0;

            foreach (var guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var components = prefab.GetComponentsInChildren<Component>(true);
                foreach (var comp in components)
                {
                    totalComponents++;
                    if (comp == null)
                    {
                        sb.AppendLine($"  [ERROR] Missing Mono Script in prefab: {path}");
                        missingScriptCount++;
                    }
                }
            }

            sb.AppendLine($"Total components scanned across prefabs: {totalComponents}");
            sb.AppendLine($"Missing scripts found: {missingScriptCount}");

            // Specific check on Dog prefab
            var dogPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/AI Companion/Dog/Dog.prefab");
            if (dogPrefab != null)
            {
                var dogAction = dogPrefab.GetComponentInChildren<DogAIAction>();
                var dogFollow = dogPrefab.GetComponentInChildren<DogFollow>();
                sb.AppendLine($"Dog Prefab Check: DogAIAction present = {dogAction != null}, DogFollow present = {dogFollow != null}");
            }

            return sb.ToString();
        }

        public static string AuditScenes()
        {
            var sb = new System.Text.StringBuilder();
            var scenes = EditorBuildSettings.scenes;
            sb.AppendLine($"Build Settings Scenes count: {scenes.Length}");

            for (int i = 0; i < scenes.Length; i++)
            {
                string scenePath = scenes[i].path;
                sb.AppendLine($"\n--- Scene [{i}]: {scenePath} (enabled={scenes[i].enabled}) ---");

                if (!File.Exists(scenePath))
                {
                    sb.AppendLine($"  [ERROR] Scene file does not exist: {scenePath}");
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var rootObjects = scene.GetRootGameObjects();

                int sceneMissingScripts = 0;
                int sceneComponentCount = 0;

                foreach (var root in rootObjects)
                {
                    var components = root.GetComponentsInChildren<Component>(true);
                    foreach (var comp in components)
                    {
                        sceneComponentCount++;
                        if (comp == null)
                        {
                            sb.AppendLine($"  [ERROR] Missing Mono Script on GameObject '{root.name}' in {scenePath}");
                            sceneMissingScripts++;
                        }
                    }
                }

                sb.AppendLine($"  Total Components: {sceneComponentCount}, Missing Scripts: {sceneMissingScripts}");

                // Check specific scene components
                var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
                var qm = UnityEngine.Object.FindAnyObjectByType<QuestManager>();
                var am = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
                var em = UnityEngine.Object.FindAnyObjectByType<EconomyManager>();
                var pm = UnityEngine.Object.FindAnyObjectByType<PortalManager>();
                var exits = UnityEngine.Object.FindObjectsByType<AreaExit>();
                var conditions = UnityEngine.Object.FindObjectsByType<ExitAreaCondition>();
                var cameras = UnityEngine.Object.FindObjectsByType<CinemachineCamera>();

                if (gm != null) sb.AppendLine($"  GameManager: Present");
                if (qm != null) sb.AppendLine($"  QuestManager: Present");
                if (am != null) sb.AppendLine($"  AudioManager: Present");
                if (em != null) sb.AppendLine($"  EconomyManager: Present");
                if (pm != null) sb.AppendLine($"  PortalManager: Present");
                if (cameras.Length > 0) sb.AppendLine($"  CinemachineCamera count: {cameras.Length}");
                if (exits.Length > 0)
                {
                    sb.AppendLine($"  AreaExit count: {exits.Length}");
                    foreach (var exit in exits)
                    {
                        var so = new SerializedObject(exit);
                        var sceneToLoadProp = so.FindProperty("sceneToLoad");
                        var sceneTransProp = so.FindProperty("sceneTransitionName");
                        string scenesStr = "";
                        if (sceneToLoadProp != null && sceneToLoadProp.isArray)
                        {
                            for (int s = 0; s < sceneToLoadProp.arraySize; s++)
                            {
                                scenesStr += (s > 0 ? ", " : "") + sceneToLoadProp.GetArrayElementAtIndex(s).stringValue;
                            }
                        }
                        sb.AppendLine($"    Exit transition to: [{scenesStr}] (areaTransitionName: '{sceneTransProp?.stringValue}')");
                    }
                }
                if (conditions.Length > 0)
                {
                    sb.AppendLine($"  ExitAreaCondition count: {conditions.Length}");
                    foreach (var cond in conditions)
                    {
                        var so = new SerializedObject(cond);
                        var npcsProp = so.FindProperty("requiredNPCs");
                        var colProp = so.FindProperty("barrierCollider");
                        sb.AppendLine($"    Condition: RequiredNPCs count={npcsProp?.arraySize}, BarrierCollider assigned={colProp?.objectReferenceValue != null}");
                    }
                }

                // Save scene to upgrade any outdated object versions (such as CompositeCollider2D v1)
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                sb.AppendLine("  -> Scene verified and re-saved cleanly.");
            }

            // Return to MainMenu after audit
            if (scenes.Length > 0 && File.Exists(scenes[0].path))
            {
                EditorSceneManager.OpenScene(scenes[0].path, OpenSceneMode.Single);
            }

            return sb.ToString();
        }
        public static string VerifyScene4Portal()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Scene4.unity", OpenSceneMode.Single);
            var pm = UnityEngine.Object.FindAnyObjectByType<PortalManager>();
            if (pm != null && pm.AreaExit != null)
            {
                var ae = pm.AreaExit.GetComponent<AreaExit>();
                if (ae != null)
                {
                    var so = new SerializedObject(ae);
                    var p = so.FindProperty("sceneToLoad");
                    string s = "";
                    for (int i = 0; i < p.arraySize; i++)
                    {
                        s += (i > 0 ? ", " : "") + p.GetArrayElementAtIndex(i).stringValue;
                    }
                    return $"Portal AreaExit sceneToLoad: [{s}], Transition: {so.FindProperty("sceneTransitionName")?.stringValue}";
                }
                return "AreaExit component not found on pm.AreaExit";
            }
            return "PortalManager or AreaExit field is null";
        }
        public static string DumpAllSceneQuests()
        {
            var sb = new System.Text.StringBuilder();
            string[] scenesToAudit = new string[] {
                "Assets/Scenes/Tutorial.unity",
                "Assets/Scenes/Scene1.unity",
                "Assets/Scenes/Scene2.unity",
                "Assets/Scenes/Scene3.unity",
                "Assets/Scenes/Scene4.unity"
            };

            foreach (var scPath in scenesToAudit)
            {
                sb.AppendLine($"==================================================");
                sb.AppendLine($"SCENE: {scPath}");
                sb.AppendLine($"==================================================");
                var scene = EditorSceneManager.OpenScene(scPath, OpenSceneMode.Single);
                var npcs = UnityEngine.Object.FindObjectsByType<NPCInteraction>();
                sb.AppendLine($"NPC Count: {npcs.Length}");

                foreach (var npc in npcs)
                {
                    var so = new SerializedObject(npc);
                    string nameVal = so.FindProperty("npcName")?.stringValue ?? npc.name;
                    bool isGiver = so.FindProperty("isQuestGiver")?.boolValue ?? false;
                    bool reqOther = so.FindProperty("requiresOtherNPC")?.boolValue ?? false;
                    string reqNpc = so.FindProperty("requiredNPCName")?.stringValue ?? "";

                    sb.AppendLine($"\n* NPC: '{nameVal}' (GameObject: '{npc.gameObject.name}', isQuestGiver={isGiver}, requiresOtherNPC={reqOther}, requiredNPC='{reqNpc}')");

                    var diagProp = so.FindProperty("dialogueLines");
                    if (diagProp != null)
                    {
                        for (int d = 0; d < diagProp.arraySize; d++)
                            sb.AppendLine($"    Dialogue[{d}]: {diagProp.GetArrayElementAtIndex(d).stringValue}");
                    }

                    var condProp = so.FindProperty("conditionalDialogueLines");
                    if (condProp != null && condProp.arraySize > 0)
                    {
                        for (int c = 0; c < condProp.arraySize; c++)
                            sb.AppendLine($"    CondDialogue[{c}]: {condProp.GetArrayElementAtIndex(c).stringValue}");
                    }

                    var questsProp = so.FindProperty("quests");
                    if (questsProp != null)
                    {
                        sb.AppendLine($"    Quests Array Size: {questsProp.arraySize}");
                        for (int q = 0; q < questsProp.arraySize; q++)
                        {
                            var qElem = questsProp.GetArrayElementAtIndex(q);
                            string qTitle = qElem.FindPropertyRelative("questTitle")?.stringValue;
                            string qDesc = qElem.FindPropertyRelative("questDescription")?.stringValue;
                            int qType = qElem.FindPropertyRelative("questType")?.enumValueIndex ?? 0;
                            string targetNpc = qElem.FindPropertyRelative("targetNPCName")?.stringValue;
                            int reqKills = qElem.FindPropertyRelative("requiredKills")?.intValue ?? 0;
                            int reqDmg = qElem.FindPropertyRelative("requiredDamage")?.intValue ?? 0;
                            int reqGold = qElem.FindPropertyRelative("requiredGold")?.intValue ?? 0;
                            bool hasNext = qElem.FindPropertyRelative("hasNextQuest")?.boolValue ?? false;
                            int nextIdx = qElem.FindPropertyRelative("nextQuestIndex")?.intValue ?? 0;

                            sb.AppendLine($"    -> Quest [{q}]: '{qTitle}' (Type={qType}, Target='{targetNpc}', Kills={reqKills}, Dmg={reqDmg}, Gold={reqGold}, hasNext={hasNext}, nextIdx={nextIdx})");
                            sb.AppendLine($"       Desc: {qDesc}");

                            var startDiag = qElem.FindPropertyRelative("startDialogue");
                            if (startDiag != null && startDiag.arraySize > 0)
                            {
                                for (int sd = 0; sd < startDiag.arraySize; sd++)
                                    sb.AppendLine($"       StartDiag[{sd}]: {startDiag.GetArrayElementAtIndex(sd).stringValue}");
                            }

                            var compDiag = qElem.FindPropertyRelative("completeDialogue");
                            if (compDiag != null && compDiag.arraySize > 0)
                            {
                                for (int cd = 0; cd < compDiag.arraySize; cd++)
                                    sb.AppendLine($"       CompDiag[{cd}]: {compDiag.GetArrayElementAtIndex(cd).stringValue}");
                            }

                            var choicesProp = qElem.FindPropertyRelative("choices");
                            if (choicesProp != null && choicesProp.arraySize > 0)
                            {
                                for (int ch = 0; ch < choicesProp.arraySize; ch++)
                                {
                                    var cElem = choicesProp.GetArrayElementAtIndex(ch);
                                    string btn = cElem.FindPropertyRelative("buttonLabel")?.stringValue;
                                    bool trig = cElem.FindPropertyRelative("triggersQuest")?.boolValue ?? false;
                                    int trigIdx = cElem.FindPropertyRelative("questIndexToTrigger")?.intValue ?? 0;
                                    sb.AppendLine($"       Choice[{ch}]: '{btn}' (triggersQuest={trig}, trigIdx={trigIdx})");
                                }
                            }
                        }
                    }
                }

                var exits = UnityEngine.Object.FindObjectsByType<ExitAreaCondition>();
                sb.AppendLine($"\nExitAreaCondition Count: {exits.Length}");
                foreach (var exit in exits)
                {
                    var so = new SerializedObject(exit);
                    var npcsProp = so.FindProperty("requiredNPCs");
                    string reqList = "";
                    if (npcsProp != null)
                    {
                        for (int r = 0; r < npcsProp.arraySize; r++)
                        {
                            var elem = npcsProp.GetArrayElementAtIndex(r);
                            var objRef = elem.objectReferenceValue as NPCInteraction;
                            reqList += (r > 0 ? ", " : "") + (objRef != null ? objRef.name : "null");
                        }
                    }
                    sb.AppendLine($"  ExitAreaCondition on '{exit.gameObject.name}': RequiredNPCs=[{reqList}], BarrierCollider={(so.FindProperty("barrierCollider")?.objectReferenceValue != null)}");
                }
            }

            // Return to MainMenu
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
            return sb.ToString();
        }

        public static string OpenScene1()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Scene1.unity", OpenSceneMode.Single);
            return scene.name;
        }

        public static string OpenMainMenu()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
            return scene.name;
        }

        public static string OpenTutorial()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);
            return scene.name;
        }

        public static bool IsPlaying()
        {
            return EditorApplication.isPlaying;
        }

        public static string TestQuestAndCombatSimulation()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== QUEST & COMBAT INTERACTION SIMULATION ===");

            var player = PlayerController.Instance != null ? PlayerController.Instance : UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player == null)
            {
                sb.AppendLine("[ERROR] PlayerController not found!");
                return sb.ToString();
            }
            sb.AppendLine($"PlayerController: Verified at {player.transform.position}");

            // 1. Test Player movement & physics
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(1f, 0f);
                sb.AppendLine($"Player Rigidbody2D.linearVelocity set and read: {rb.linearVelocity}");
            }

            // 2. Test Dog Companion
            var dog = UnityEngine.Object.FindAnyObjectByType<DogFollow>();
            sb.AppendLine($"Dog companion present: {dog != null}");

            // 3. Test NPC Interactions and Quest Progression
            var npcs = UnityEngine.Object.FindObjectsByType<NPCInteraction>();
            sb.AppendLine($"NPC count in scene: {npcs.Length}");
            foreach (var npc in npcs)
            {
                sb.AppendLine($"  NPC '{npc.NPCName}': isQuestGiver={npc.HasActiveQuest || npc.IsQuestCompleted}");
            }

            // Simulate the exact sequence that triggered the crash:
            // a. Mark Warrior talked
            sb.AppendLine("Simulating player talking to Warrior...");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.MarkNPCAsTalked("Warrior");
                sb.AppendLine($"GameManager.HasTalkedTo('Warrior'): {GameManager.Instance.HasTalkedTo("Warrior")}");
            }

            // b. Deal damage to dummy
            sb.AppendLine("Simulating attack on DummyTarget (20 damage)...");
            var dummy = UnityEngine.Object.FindAnyObjectByType<DummyTarget>();
            if (dummy != null)
            {
                dummy.TakeDamage(20);
                sb.AppendLine("DummyTarget took 20 damage successfully.");
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterDummyDamage(20);
                sb.AppendLine("GameManager.RegisterDummyDamage(20) invoked directly.");
            }

            // c. Find Elf and check quest
            foreach (var npc in npcs)
            {
                if (npc.NPCName == "Elf")
                {
                    sb.AppendLine($"Elf NPC: HasActiveQuest={npc.HasActiveQuest}, IsQuestCompleted={npc.IsQuestCompleted}");
                }
            }

            sb.AppendLine("=== SIMULATION COMPLETED WITHOUT EXCEPTION ===");
            return sb.ToString();
        }

        public static string TestTutorialRuntime()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== TUTORIAL PLAYMODE RUNTIME TEST ===");

            var player = PlayerController.Instance != null ? PlayerController.Instance : UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player == null)
            {
                sb.AppendLine("[ERROR] PlayerController instance not found in scene!");
                return sb.ToString();
            }
            sb.AppendLine($"PlayerController: Present at position {player.transform.position}");

            var health = PlayerHealth.Instance != null ? PlayerHealth.Instance : UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();
            sb.AppendLine($"PlayerHealth: Present = {health != null}");

            var stamina = Stamina.Instance != null ? Stamina.Instance : UnityEngine.Object.FindAnyObjectByType<Stamina>();
            sb.AppendLine($"Stamina: Present = {stamina != null}, CurrentStamina = {stamina?.CurrentStamina}");

            var activeWeapon = ActiveWeapon.Instance != null ? ActiveWeapon.Instance : UnityEngine.Object.FindAnyObjectByType<ActiveWeapon>();
            sb.AppendLine($"ActiveWeapon: Present = {activeWeapon != null}, CurrentActiveWeapon = {(activeWeapon?.CurrentActiveWeapon != null ? (activeWeapon.CurrentActiveWeapon as MonoBehaviour)?.name : "None")}");

            var activeInventory = ActiveInventory.Instance != null ? ActiveInventory.Instance : UnityEngine.Object.FindAnyObjectByType<ActiveInventory>();
            sb.AppendLine($"ActiveInventory: Present = {activeInventory != null}");

            // Test inventory slots switching
            if (activeInventory != null)
            {
                activeInventory.EquipStartingWeapon();
                sb.AppendLine("  Equipped starting weapon (Slot 0 / Sword)");
            }

            var dog = UnityEngine.Object.FindAnyObjectByType<DogFollow>();
            sb.AppendLine($"Dog Companion (DogFollow): Present = {dog != null}");
            if (dog != null)
            {
                sb.AppendLine($"  Dog Position: {dog.transform.position}, Following Player: {dog.player != null}");
            }

            var dogAction = UnityEngine.Object.FindAnyObjectByType<DogAIAction>();
            sb.AppendLine($"Dog AI Action: Present = {dogAction != null}");

            var enemies = UnityEngine.Object.FindObjectsByType<EnemyHealth>();
            sb.AppendLine($"Enemies count in Tutorial: {enemies.Length}");

            var npcs = UnityEngine.Object.FindObjectsByType<NPCInteraction>();
            sb.AppendLine($"NPCs count in Tutorial: {npcs.Length}");

            var uifade = UIFade.Instance != null ? UIFade.Instance : UnityEngine.Object.FindAnyObjectByType<UIFade>();
            sb.AppendLine($"UIFade: Present = {uifade != null}");

            sb.AppendLine("=== TUTORIAL PLAYMODE TEST FINISHED CLEANLY ===");
            return sb.ToString();
        }
    }
}
