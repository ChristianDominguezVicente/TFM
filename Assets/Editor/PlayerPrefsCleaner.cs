using UnityEditor;
using UnityEngine;

public class PlayerPrefsCleaner : EditorWindow
{
    [MenuItem("Tools/PlayerPrefs/Borrar Todos")]
    private static void BorrarTodosLosPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=green>¡PlayerPrefs borrados exitosamente!</color>");
    }

    [MenuItem("Tools/PlayerPrefs/Ver Valores")]
    private static void VerPlayerPrefs()
    {
        Debug.Log("<color=yellow>--- Valores Guardados ---</color>");
        Debug.Log($"Resolución: {PlayerPrefs.GetInt("ResolucionIndex", -1)}");
        Debug.Log($"Modo Pantalla: {PlayerPrefs.GetInt("ModoPantallaIndex", -1)}");

        //player pos
        for (int i = 0; i < 3; i++) // Suponiendo 3 slots
        {
            Debug.Log($"<color=cyan>--- Slot {i + 1} ---</color>");
            Debug.Log($"Slot{i}_PosX: {PlayerPrefs.GetFloat($"Slot{i}_PosX", -999f)}");
            Debug.Log($"Slot{i}_PosY: {PlayerPrefs.GetFloat($"Slot{i}_PosY", -999f)}");
            Debug.Log($"Slot{i}_PosZ: {PlayerPrefs.GetFloat($"Slot{i}_PosZ", -999f)}");
            Debug.Log($"Slot{i}_Scene: {PlayerPrefs.GetString($"Slot{i}_Scene", "N/A")}");
            Debug.Log($"Slot{i}_Karma: {PlayerPrefs.GetFloat($"Slot{i}_Karma", -999f)}");
            Debug.Log($"Slot{i}_PlayTime: {PlayerPrefs.GetFloat($"Slot{i}_PlayTime", -1f)}");
            Debug.Log($"Slot{i}_History: {PlayerPrefs.GetString($"Slot{i}_History", "N/A")}");
            Debug.Log($"Slot{i}_PrincipalDoor: {PlayerPrefs.GetInt($"Slot{i}_PrincipalDoor", -999)}");
            Debug.Log($"Slot{i}_Calendar: {PlayerPrefs.GetInt($"Slot{i}_Calendar", -999)}");
            Debug.Log($"Slot{i}_MasterKey: {PlayerPrefs.GetInt($"Slot{i}_MasterKey", -999)}");
            Debug.Log($"Slot{i}_Valve: {PlayerPrefs.GetInt($"Slot{i}_Valve", -999)}");
            Debug.Log($"Slot{i}_Sugar: {PlayerPrefs.GetInt($"Slot{i}_Sugar", -999)}");
            Debug.Log($"Slot{i}_Flour: {PlayerPrefs.GetInt($"Slot{i}_Flour", -999)}");
            Debug.Log($"Slot{i}_Eggs: {PlayerPrefs.GetInt($"Slot{i}_Eggs", -999)}");
            Debug.Log($"Slot{i}_Recipe1: {PlayerPrefs.GetInt($"Slot{i}_Recipe1", -999)}");
            Debug.Log($"Slot{i}_Recipe2: {PlayerPrefs.GetInt($"Slot{i}_Recipe2", -999)}");
            Debug.Log($"Slot{i}_Teddy: {PlayerPrefs.GetInt($"Slot{i}_Teddy", -999)}");
            Debug.Log($"Slot{i}_ToolBox: {PlayerPrefs.GetInt($"Slot{i}_ToolBox", -999)}");
            Debug.Log($"Slot{i}_GiftPaper: {PlayerPrefs.GetInt($"Slot{i}_GiftPaper", -999)}");
            Debug.Log($"Slot{i}_Icons: {PlayerPrefs.GetInt($"Slot{i}_Icons", -999)}");
            Debug.Log($"Slot{i}_Poster: {PlayerPrefs.GetInt($"Slot{i}_Poster", -999)}");
            Debug.Log($"Slot{i}_Tablet: {PlayerPrefs.GetInt($"Slot{i}_Tablet", -999)}");
            Debug.Log($"Slot{i}_Photo: {PlayerPrefs.GetInt($"Slot{i}_Photo", -999)}");
            Debug.Log($"Slot{i}_Note: {PlayerPrefs.GetInt($"Slot{i}_Note", -999)}");
            Debug.Log($"Slot{i}_Correct: {PlayerPrefs.GetInt($"Slot{i}_Correct", -999)}");
            Debug.Log($"Slot{i}_Incorrect: {PlayerPrefs.GetInt($"Slot{i}_Incorrect", -999)}");
            Debug.Log($"Slot{i}_CurrentObject: {PlayerPrefs.GetString($"Slot{i}_CurrentObject", "N/A")}");
        }

        Debug.Log("<color=yellow>------------------------</color>");
    }
}
