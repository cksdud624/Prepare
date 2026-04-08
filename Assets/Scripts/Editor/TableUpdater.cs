using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class TableUpdater
{
    private const string BytesDirectory = "Generated/Table";
    private const string ScriptsDirectory = "Scripts/Generated/Table";
    private const string TableDirectory = ".Table/";
    private const string TableManifestFile = "TableManifest.json";
    [MenuItem("Table/Update Table")]
    public static void UpdateTable()
    {
        string tableDirectoryPath = Path.Combine(Application.dataPath, TableDirectory);
        string tableManifestFilePath = Path.Combine(tableDirectoryPath, TableManifestFile);
        
        TableManifest tableManifest;
        if (File.Exists(tableManifestFilePath))
            tableManifest = JsonUtility.FromJson<TableManifest>(File.ReadAllText(tableManifestFilePath));
        else
            tableManifest = new TableManifest();
        
        if (!Directory.Exists(tableDirectoryPath))
            Directory.CreateDirectory(tableDirectoryPath);
        
        string[] tablePaths = Directory.GetFiles(tableDirectoryPath, "*.tsv", SearchOption.TopDirectoryOnly);
        
        TableManifest newTableManifest = new TableManifest();
        Dictionary<TableHashData, List<string>> scriptReference = new();
        foreach (var tablePath in tablePaths)
        {
            string[] lines = File.ReadAllLines(tablePath);
            string tableName = Path.GetFileNameWithoutExtension(tablePath);
            
            if (lines.Length < 2)
            {
                Debug.Log($"{tablePath} has 2 less lines");
                return;
            }

            byte[] tableDataBytes;
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                foreach (var line in lines)
                    bw.Write(line);
                tableDataBytes = ms.ToArray();
            }

            string md5Hash = string.Empty;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(tableDataBytes);
                StringBuilder sb = new StringBuilder();
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                md5Hash = sb.ToString();
            }

            TableHashData hashData = tableManifest.tables.FirstOrDefault(data => data.fileName == tableName);
            if (hashData != null)
            {
                if (hashData.hash != md5Hash)
                {
                    CreateTable(tableName ,tableDataBytes);
                    TableHashData newHashData = new TableHashData(tableName, md5Hash);
                    newTableManifest.tables.Add(newHashData);
                    scriptReference.Add(newHashData, new List<string>{lines[0], lines[1]});
                }
                else
                {
                    newTableManifest.tables.Add(hashData);
                    scriptReference.Add(hashData, new List<string>{lines[0], lines[1]});
                }
                tableManifest.tables.Remove(hashData);
            }
            else
            {
                CreateTable(tableName, tableDataBytes);
                TableHashData newHashData = new TableHashData(tableName, md5Hash);
                newTableManifest.tables.Add(newHashData);
                scriptReference.Add(newHashData, new List<string>{lines[0], lines[1]});
            }
        }

        if (tableManifest.tables.Count > 0)
        {
            foreach (var tableName in tableManifest.tables)
            {
                string tablePath = Path.Combine(Application.dataPath, BytesDirectory, tableName.fileName) + ".bytes";
                if (File.Exists(tablePath))
                    File.Delete(tablePath);
            }
        }
        
        //scriptReference
        
        /*
         * 1. TableManifest.json을 로드
         * 
         * 2. .Table의 모든 테이블들을 모두 가져온다
         * 3. .Table을 bytes파일로 변환한다.
         * 4. TableManifest.json에 있는 json 정보들과 .Table의 모든 tsv와 비교한다
         * 5. TableManifest.json에 없는 테이블들은 추가하고 변경되었으면 변경하고 TableManifest.json에 주회후 데이터가 남아있으면 bytes파일을 삭제한다
         * 6. 수정, 추가, 삭제된 bytes파일에 맞게 스크립트 디렉토리도 수정, 추가, 삭제한다
         */
        AssetDatabase.Refresh();
    }

    private static void CreateTable(string tableName, byte[] byteDatas)
    {
        string bytesDirectoryPath = Path.Combine(Application.dataPath, BytesDirectory);
        if(!Directory.Exists(bytesDirectoryPath))
            Directory.CreateDirectory(bytesDirectoryPath);
        string bytesPath = Path.Combine(bytesDirectoryPath, tableName + ".bytes");
        
        File.WriteAllBytes(bytesPath, byteDatas);
    }
    

    [Serializable]
    public class TableManifest
    {
        public List<TableHashData> tables = new List<TableHashData>();
    }

    [Serializable]
    public class TableHashData
    {
        public string fileName;
        public string hash;

        public TableHashData(string filename, string hash)
        {
            this.fileName = filename;
            this.hash = hash;
        }
    }
}
