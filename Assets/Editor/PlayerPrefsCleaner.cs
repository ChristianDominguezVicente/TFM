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
        Debug.Log($"PlayerPosX: {PlayerPrefs.GetFloat("PlayerPosX", -1)}");
        Debug.Log($"PlayerPosY: {PlayerPrefs.GetFloat("PlayerPosY", -1)}");
        Debug.Log($"PlayerPosZ: {PlayerPrefs.GetFloat("PlayerPosZ", -1)}");

        Debug.Log("<color=yellow>------------------------</color>");
    }
}
