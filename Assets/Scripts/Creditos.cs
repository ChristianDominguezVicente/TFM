using UnityEngine;
using UnityEngine.SceneManagement;

public class Creditos : MonoBehaviour
{
    // This function is used at the end of the credits' animation
    public void InicioMenu()
    {
        SceneManager.LoadScene("InicioMenu");
    }
}
