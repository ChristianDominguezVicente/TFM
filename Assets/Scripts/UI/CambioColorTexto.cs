using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CambioColorTexto : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private TextMeshProUGUI textoBoton;
    private Color normal = new Color(1f, 0.9529f, 0.6902f);// FFF3B0
    private Color resaltado = Color.white;


    public void OnPointerEnter(PointerEventData eventData)
    {
        textoBoton.color = resaltado;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        textoBoton.color = normal;
    }
}
