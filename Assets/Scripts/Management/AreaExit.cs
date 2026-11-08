using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaExit : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string[] sceneToLoad;
    [SerializeField] private string sceneTransitionName;
    [SerializeField] private float waitToLoadTime = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            // Strictly guard against leaving before completing area quests
            if (!CanLeaveCurrentArea())
            {
                Debug.LogWarning("[AreaExit] Player cannot exit area yet: Incomplete quests for this zone!");
                return;
            }

            // 1. Force Unpause to ensure physics/logic works
            Time.timeScale = 1f;

            // 2. Play Transition Effects
            if (SceneManagement.Instance != null)
                SceneManagement.Instance.SetTransitionName(sceneTransitionName);

            if (UIFade.Instance != null)
                UIFade.Instance.FadeToBlack();

            // 3. Start Loading Logic
            StartCoroutine(LoadSceneRoutine());
        }
    }

    private bool CanLeaveCurrentArea()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.Equals("Tutorial", System.StringComparison.OrdinalIgnoreCase))
        {
            if (QuestManager.Instance != null)
            {
                return QuestManager.Instance.IsQuestCompleted("TUT_01") && QuestManager.Instance.IsQuestCompleted("TUT_02");
            }
            if (GameManager.Instance != null)
            {
                return GameManager.Instance.IsQuestCompletedFrom("Warrior") && GameManager.Instance.HasTalkedTo("Elf");
            }
            return false;
        }
        else if (sceneName.Equals("Scene1", System.StringComparison.OrdinalIgnoreCase))
        {
            if (QuestManager.Instance != null)
                return QuestManager.Instance.IsQuestCompleted("SC1_01");
        }
        else if (sceneName.Equals("Scene2", System.StringComparison.OrdinalIgnoreCase))
        {
            if (QuestManager.Instance != null)
                return QuestManager.Instance.IsQuestCompleted("SC2_01") && QuestManager.Instance.IsQuestCompleted("SC2_02");
        }
        else if (sceneName.Equals("Scene3", System.StringComparison.OrdinalIgnoreCase))
        {
            if (QuestManager.Instance != null)
                return QuestManager.Instance.IsQuestCompleted("SC3_01") && QuestManager.Instance.IsQuestCompleted("SC3_02");
        }

        return true;
    }

    private IEnumerator LoadSceneRoutine()
    {
        // [FIX 1] Use Realtime. This timer works even if the game is paused/frozen.
        // It replaces the 'while' loop which was getting stuck.
        yield return new WaitForSecondsRealtime(waitToLoadTime);

        // 4. Safety Checks (Prevents the Black Screen freeze)
        if (EconomyManager.Instance == null)
        {
            Debug.LogError("CRITICAL ERROR: EconomyManager is missing! Cannot check gold.");
            yield break; // Stop here to prevent crash
        }

        if (sceneToLoad == null || sceneToLoad.Length == 0)
        {
            Debug.LogError("CRITICAL ERROR: You forgot to add scenes to the 'Scene To Load' array in the Inspector!");
            yield break;
        }

        // [FIX 2] Check Array bounds before loading
        if (EconomyManager.Instance.currentGold >= 10)
        {
            // Check if we actually HAVE a second scene (Element 1)
            if (sceneToLoad.Length > 1)
            {
                SceneManager.LoadScene(sceneToLoad[1]);
            }
            else
            {
                Debug.LogError("Error: Player has 10+ Gold, but you didn't add a 2nd Scene (Element 1) in the Inspector!");
                // Fallback to loading the first scene so the game doesn't get stuck
                SceneManager.LoadScene(sceneToLoad[0]);
            }
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad[0]);
        }
    }
}