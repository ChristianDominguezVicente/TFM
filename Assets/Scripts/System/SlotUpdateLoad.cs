using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUpdateLoad : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textButton1;
    [SerializeField] private TextMeshProUGUI textButton2;
    [SerializeField] private TextMeshProUGUI textButton3;
    public void UpdateText(int indexButton,string text)
    {
        if (indexButton == 0)
        {
            textButton1.text = text;
        }
        else if (indexButton == 1)
        {
            textButton2.text = text;
        }
        else
        {
            textButton3.text = text;
        }
    }
    
}
