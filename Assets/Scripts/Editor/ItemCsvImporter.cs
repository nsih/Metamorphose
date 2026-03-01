using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using UnityEditor;
using UnityEngine;

namespace Metamorphose.Editor
{
    public static class ItemCsvImporter
    {
        private const string ItemOutputPath = "Assets/GameData/SO/Item/Items";
        private const string DatabasePath = "Assets/GameData/SO/Item/ItemDatabase.asset";

        [MenuItem("Item/CSV Import")]
        public static void ImportFromCsv()
        {
            string csvPath = EditorUtility.OpenFilePanel("select item csv", "", "csv");
            if (string.IsNullOrEmpty(csvPath))
            {
                Debug.Log("import cancelled");
                return;
            }

            string[] lines = File.ReadAllLines(csvPath);
            if (lines.Length < 2)
            {
                Debug.LogError("csv empty or header only");
                return;
            }

            EnsureDirectory(ItemOutputPath);

            string[] headers = ParseCsvLine(lines[0]);
            var headerIndex = BuildHeaderIndex(headers);

            var importedItems = new List<ItemSO>();

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                string[] fields = ParseCsvLine(line);
                ItemSO item = ParseRow(fields, headerIndex);

                if (item == null)
                {
                    Debug.LogWarning($"row {i} parse fail, skip");
                    continue;
                }

                importedItems.Add(item);
                Debug.Log($"imported: {item.itemId}");
            }

            UpdateDatabase(importedItems);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"done: {importedItems.Count} items");
        }

        private static Dictionary<string, int> BuildHeaderIndex(string[] headers)
        {
            var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                string key = headers[i].Trim();
                if (!string.IsNullOrEmpty(key))
                    index[key] = i;
            }
            return index;
        }

        private static ItemSO ParseRow(string[] fields, Dictionary<string, int> headerIndex)
        {
            string itemId = GetField(fields, headerIndex, "ItemId");
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("missing ItemId");
                return null;
            }

            string assetPath = $"{ItemOutputPath}/{itemId}.asset";
            ItemSO item = AssetDatabase.LoadAssetAtPath<ItemSO>(assetPath);

            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemSO>();
                AssetDatabase.CreateAsset(item, assetPath);
            }

            item.itemId      = itemId;
            item.itemName    = GetField(fields, headerIndex, "ItemName");
            item.nameKr      = GetField(fields, headerIndex, "NameKr");
            item.description = GetField(fields, headerIndex, "Description");

            string tierStr = GetField(fields, headerIndex, "Tier");
            if (Enum.TryParse(tierStr, out ItemTier tier))
                item.tier = tier;
            else
                Debug.LogWarning($"{itemId}: unknown Tier '{tierStr}'");

            string typeStr = GetField(fields, headerIndex, "ItemType");
            if (Enum.TryParse(typeStr, out ItemType itemType))
                item.itemType = itemType;
            else
                Debug.LogWarning($"{itemId}: unknown ItemType '{typeStr}'");

            string isUniqueStr = GetField(fields, headerIndex, "IsUnique");
            item.isUnique = isUniqueStr.Equals("TRUE", StringComparison.OrdinalIgnoreCase);

            string spritePath = GetField(fields, headerIndex, "SpritePath");
            item.spritePath = spritePath;
            item.sprite     = LoadSprite(spritePath);

            string statModifiersRaw = GetField(fields, headerIndex, "StatModifiers");
            item.statModifiers = ParseStatModifiers(statModifiersRaw, itemId);

            string onPickupHealStr = GetField(fields, headerIndex, "OnPickupHeal");
            if (float.TryParse(onPickupHealStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float onPickupHeal))
                item.onPickupHeal = onPickupHeal;
            else
                item.onPickupHeal = 0f;

            string activeCooldownStr = GetField(fields, headerIndex, "ActiveCooldown");
            if (float.TryParse(activeCooldownStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float activeCooldown))
                item.activeCooldown = activeCooldown;
            else
                item.activeCooldown = 0f;

            // Passive인데 ActiveCooldown이 0이 아니면 경고
            if (item.itemType == ItemType.Passive && item.activeCooldown != 0f)
                Debug.LogWarning($"{itemId}: Passive item has non-zero ActiveCooldown");

            string tagsRaw = GetField(fields, headerIndex, "Tags");
            item.tags = ParseTags(tagsRaw);

            EditorUtility.SetDirty(item);
            return item;
        }

        private static StatModifierEntry[] ParseStatModifiers(string raw, string itemId)
        {
            if (string.IsNullOrEmpty(raw))
                return Array.Empty<StatModifierEntry>();

            string[] pairs = raw.Split(';');
            var result = new List<StatModifierEntry>();

            foreach (string pair in pairs)
            {
                string trimmed = pair.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                string[] parts = trimmed.Split(':');

                // statName:value:modType 세 파트 필수
                if (parts.Length != 3)
                {
                    Debug.LogWarning($"{itemId}: invalid StatModifier format '{trimmed}' (expected key:value:modType)");
                    continue;
                }

                string statName = parts[0].Trim();
                string valueStr = parts[1].Trim();
                string modTypeStr = parts[2].Trim();

                if (!float.TryParse(valueStr, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float value))
                {
                    Debug.LogWarning($"{itemId}: invalid value '{valueStr}' in '{trimmed}'");
                    continue;
                }

                if (!Enum.TryParse(modTypeStr, out StatModType modType))
                {
                    Debug.LogWarning($"{itemId}: unknown ModType '{modTypeStr}' in '{trimmed}'");
                    continue;
                }

                result.Add(new StatModifierEntry
                {
                    statName = statName,
                    value    = value,
                    modType  = modType
                });
            }

            return result.ToArray();
        }

        private static string[] ParseTags(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return Array.Empty<string>();

            return raw.Split(';')
                      .Select(t => t.Trim())
                      .Where(t => !string.IsNullOrEmpty(t))
                      .ToArray();
        }

        private static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
                Debug.LogWarning($"sprite not found: {path}");

            return sprite;
        }

        private static void UpdateDatabase(List<ItemSO> items)
        {
            ItemDatabase database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(DatabasePath);

            if (database == null)
            {
                database = ScriptableObject.CreateInstance<ItemDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            database.SetItems(items.ToArray());
            EditorUtility.SetDirty(database);
        }

        private static string GetField(string[] fields, Dictionary<string, int> headerIndex, string key)
        {
            if (!headerIndex.TryGetValue(key, out int idx))
                return string.Empty;

            if (idx >= fields.Length)
                return string.Empty;

            return fields[idx].Trim();
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        // RFC 4180 기본 지원
        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}