using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class QuestMarkdownCompiler : AssetPostprocessor
{
    private const string MARKDOWN_PATH = "Assets/Resources/QuestChains.md";
    private const string DATABASE_PATH = "Assets/Resources/QuestDatabase.asset";

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string str in importedAssets)
        {
            if (str.Replace("\\", "/").Equals(MARKDOWN_PATH, StringComparison.OrdinalIgnoreCase))
            {
                CompileMarkdownToDatabase();
                return;
            }
        }
    }

    [MenuItem("Tools/Quests/Compile Quest Markdown")]
    public static void CompileMarkdownToDatabase()
    {
        if (!File.Exists(MARKDOWN_PATH))
        {
            Debug.LogError($"[QuestMarkdownCompiler] Markdown file not found at: {MARKDOWN_PATH}");
            return;
        }

        string markdownText = File.ReadAllText(MARKDOWN_PATH);
        List<QuestDefinition> parsedQuests = ParseMarkdown(markdownText);

        string dir = Path.GetDirectoryName(DATABASE_PATH);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        QuestDatabaseSO database = AssetDatabase.LoadAssetAtPath<QuestDatabaseSO>(DATABASE_PATH);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<QuestDatabaseSO>();
            AssetDatabase.CreateAsset(database, DATABASE_PATH);
        }

        database.quests = parsedQuests;
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[QuestMarkdownCompiler] Successfully compiled {parsedQuests.Count} quests into {DATABASE_PATH}");
    }

    public static List<QuestDefinition> ParseMarkdown(string text)
    {
        var quests = new List<QuestDefinition>();
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        QuestDefinition currentQuest = null;
        string currentDialogueSection = null;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("# ") || line.StartsWith("<!--"))
                continue;

            // Header: ## [QUEST_ID] Quest Title
            var headerMatch = Regex.Match(line, @"^##\s*\[(.*?)\]\s*(.*)$");
            if (headerMatch.Success)
            {
                currentQuest = new QuestDefinition();
                currentQuest.questID = headerMatch.Groups[1].Value.Trim();
                currentQuest.questTitle = headerMatch.Groups[2].Value.Trim();
                quests.Add(currentQuest);
                currentDialogueSection = null;
                continue;
            }

            if (currentQuest == null) continue;

            if (line.StartsWith("- Category:", StringComparison.OrdinalIgnoreCase))
            {
                string val = line.Substring("- Category:".Length).Trim();
                Enum.TryParse(val, true, out currentQuest.category);
                currentDialogueSection = null;
            }
            else if (line.StartsWith("- Scene:", StringComparison.OrdinalIgnoreCase))
            {
                currentQuest.sceneName = line.Substring("- Scene:".Length).Trim();
                currentDialogueSection = null;
            }
            else if (line.StartsWith("- Prerequisite:", StringComparison.OrdinalIgnoreCase))
            {
                currentQuest.prerequisiteQuestID = line.Substring("- Prerequisite:".Length).Trim();
                currentDialogueSection = null;
            }
            else if (line.StartsWith("- StartNPC:", StringComparison.OrdinalIgnoreCase))
            {
                // Format: - StartNPC: Warrior | TurnInNPC: Warrior | NextQuest: TUT_02
                string content = line.Substring("- StartNPC:".Length).Trim();
                string[] parts = content.Split('|');
                currentQuest.startNpcID = parts[0].Trim();

                foreach (var p in parts)
                {
                    string trimmed = p.Trim();
                    if (trimmed.StartsWith("TurnInNPC:", StringComparison.OrdinalIgnoreCase))
                    {
                        currentQuest.turnInNpcID = trimmed.Substring("TurnInNPC:".Length).Trim();
                    }
                    else if (trimmed.StartsWith("NextQuest:", StringComparison.OrdinalIgnoreCase))
                    {
                        currentQuest.nextQuestInChainID = trimmed.Substring("NextQuest:".Length).Trim();
                    }
                }
                if (string.IsNullOrEmpty(currentQuest.turnInNpcID))
                    currentQuest.turnInNpcID = currentQuest.startNpcID;

                currentDialogueSection = null;
            }
            else if (line.StartsWith("- Objective:", StringComparison.OrdinalIgnoreCase))
            {
                // Format: - Objective: RequirementQuest | Target: Dummy | Amount: 10 | GoldReward: 5
                string content = line.Substring("- Objective:".Length).Trim();
                string[] parts = content.Split('|');
                string typeStr = parts[0].Trim();
                Enum.TryParse(typeStr, true, out currentQuest.questType);

                foreach (var p in parts)
                {
                    string trimmed = p.Trim();
                    if (trimmed.StartsWith("Target:", StringComparison.OrdinalIgnoreCase))
                        currentQuest.targetID = trimmed.Substring("Target:".Length).Trim();
                    else if (trimmed.StartsWith("Amount:", StringComparison.OrdinalIgnoreCase))
                        int.TryParse(trimmed.Substring("Amount:".Length).Trim(), out currentQuest.requiredAmount);
                    else if (trimmed.StartsWith("GoldReward:", StringComparison.OrdinalIgnoreCase))
                        int.TryParse(trimmed.Substring("GoldReward:".Length).Trim(), out currentQuest.goldReward);
                }
                currentDialogueSection = null;
            }
            else if (line.StartsWith("- Description:", StringComparison.OrdinalIgnoreCase))
            {
                currentQuest.questDescription = line.Substring("- Description:".Length).Trim();
                currentDialogueSection = null;
            }
            else if (line.StartsWith("- OfferDialogue:", StringComparison.OrdinalIgnoreCase))
            {
                currentDialogueSection = "Offer";
            }
            else if (line.StartsWith("- InProgressDialogue:", StringComparison.OrdinalIgnoreCase))
            {
                currentDialogueSection = "InProgress";
            }
            else if (line.StartsWith("- TurnInDialogue:", StringComparison.OrdinalIgnoreCase))
            {
                currentDialogueSection = "TurnIn";
            }
            else if (line.StartsWith("- CompletedDialogue:", StringComparison.OrdinalIgnoreCase))
            {
                currentDialogueSection = "Completed";
            }
            else if (currentDialogueSection != null && line.Contains(":"))
            {
                int colonIdx = line.IndexOf(':');
                string speaker = line.Substring(0, colonIdx).Trim();
                string textVal = line.Substring(colonIdx + 1).Trim();
                var dLine = new QuestDialogueLine(speaker, textVal);

                switch (currentDialogueSection)
                {
                    case "Offer":
                        currentQuest.offerDialogue.Add(dLine);
                        break;
                    case "InProgress":
                        currentQuest.inProgressDialogue.Add(dLine);
                        break;
                    case "TurnIn":
                        currentQuest.turnInDialogue.Add(dLine);
                        break;
                    case "Completed":
                        currentQuest.completedDialogue.Add(dLine);
                        break;
                }
            }
        }

        return quests;
    }
}
