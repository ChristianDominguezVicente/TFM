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
            Debug.Log($"Slot{i}_PlayTime: {PlayerPrefs.GetFloat($"Slot{i}_PlayTime", -1f)}");
        }

        Debug.Log("<color=yellow>------------------------</color>");
    }
}
