using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class TableUpdater
{
    private const string BytesDirectory = "Assets/Tables/";
    [MenuItem("Table/Update Table")]
    public static void UpdateTable()
    {
        string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] {BytesDirectory});
        HashSet<string> bytesHash = new HashSet<string>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid); 
            path = path.Replace(BytesDirectory, "");
            path = path.Replace(".bytes", "");
            bytesHash.Add(path);
        }
        
        string directory = Path.Combine(Application.dataPath, ".Tables");
        if (!Directory.Exists(directory))
        {
            Debug.Log("Table Directory Not Found");
            return;
        }
        string bytesDirectoryPath = Path.Combine(Application.dataPath, "Tables");
        string[] tsvPaths = Directory.GetFiles(directory, "*.tsv", SearchOption.TopDirectoryOnly);
        
        foreach (string tsvPath in tsvPaths)
        {
            string tableName = Path.GetFileNameWithoutExtension(tsvPath);
            if (bytesHash.Contains(tableName))
                bytesHash.Remove(tableName);
            
            DateTime tableLastWriteTime = File.GetLastWriteTime(tsvPath);
            string bytesPath = Path.Combine(bytesDirectoryPath, tableName + ".bytes");
            bool hasBytes = File.Exists(bytesPath);
            if (hasBytes)
            {
                DateTime bytesLastWriteTime = File.GetLastWriteTime(bytesPath);
                if (tableLastWriteTime > bytesLastWriteTime)
                    CreateBytes(tsvPath, bytesPath);
            }
            else
                CreateBytes(tsvPath, bytesPath);
        }

        foreach (var bytesPath in bytesHash)
        {
            AssetDatabase.DeleteAsset(BytesDirectory + bytesPath + ".bytes");
            Debug.Log($"<color=#ADD8E6>[Table Delete Success]</color> {bytesPath}.bytes");
        }
    }

    private static void CreateBytes(string tsvPath, string bytesPath)
    {
        try
        {
            string[] lines = File.ReadAllLines(tsvPath);
            long lastWriteTicks = File.GetLastWriteTime(tsvPath).Ticks;

            string directory = Path.GetDirectoryName(bytesPath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            using (FileStream fs = new FileStream(bytesPath, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    writer.Write(lastWriteTicks);
                    int dataCount = Mathf.Max(0, lines.Length - 1);
                    writer.Write(dataCount);
                    for (int i = 1; i < lines.Length; i++)
                    {
                        writer.Write(lines[i]);
                    }
                }
            }

            Debug.Log($"<color=#ADD8E6>[Table Create Success]</color> {Path.GetFileName(tsvPath)} -> {Path.GetFileName(bytesPath)}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Bake Error] {tsvPath} 변환 실패: {e.Message}");
        }
        AssetDatabase.Refresh();
    }
}
