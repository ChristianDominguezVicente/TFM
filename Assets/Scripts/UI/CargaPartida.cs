using UnityEngine;
using UnityEngine.SceneManagement;

public class CargaPartida : MonoBehaviour
{


    public void Jugar()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Greybox");
    }
}
