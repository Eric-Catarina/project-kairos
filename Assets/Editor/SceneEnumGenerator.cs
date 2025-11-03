#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public static class SceneEnumGenerator
{
    private const string enumPath = "Assets/Scripts/GameSceneEnum.cs";
    private const string enumName = "GameScene";

    [MenuItem("Tools/Atualizar Enum de Cenas")]
    public static void GenerateEnum()
    {
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        var sceneNames = guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(Path.GetFileNameWithoutExtension)
            .Distinct()
            .OrderBy(n => n)
            .ToArray();

        using (StreamWriter writer = new StreamWriter(enumPath))
        {
            writer.WriteLine("using System;");
            writer.WriteLine();
            writer.WriteLine("[Serializable]");
            writer.WriteLine($"public enum {enumName}");
            writer.WriteLine("{");

            foreach (string name in sceneNames)
                writer.WriteLine($"    {SanitizeEnumName(name)},");

            writer.WriteLine("}");
        }

        AssetDatabase.Refresh();
        Debug.Log($"Enum {enumName} atualizado com {sceneNames.Length} cenas.");
    }

    private static string SanitizeEnumName(string name)
    {
        name = new string(name.Where(char.IsLetterOrDigit).ToArray());
        if (char.IsDigit(name[0]))
            name = "_" + name;
        return name;
    }
}
#endif
