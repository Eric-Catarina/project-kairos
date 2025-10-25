// Local: Assets/Editor/CheatEnabler.cs

using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public static class CheatEnabler
{
    private const string CHEAT_SYMBOL = "ENABLE_CHEATS";
    private const string MENU_PATH = "Cheats/Enable Cheats for Build";

    [MenuItem(MENU_PATH)]
    private static void ToggleCheats()
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        List<string> allDefines = definesString.Split(';').ToList();

        if (allDefines.Contains(CHEAT_SYMBOL))
        {
            allDefines.Remove(CHEAT_SYMBOL);
            UnityEngine.Debug.Log("Cheats DESABILITADOS para a build.");
        }
        else
        {
            allDefines.Add(CHEAT_SYMBOL);
            UnityEngine.Debug.Log("Cheats HABILITADOS para a build.");
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            buildTargetGroup,
            string.Join(";", allDefines.ToArray())
        );
    }
    
    [MenuItem(MENU_PATH, true)]
    private static bool ValidateToggleCheats()
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        bool isEnabled = definesString.Split(';').Contains(CHEAT_SYMBOL);
        Menu.SetChecked(MENU_PATH, isEnabled);
        return true;
    }
}