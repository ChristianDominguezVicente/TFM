using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUpdateLoad : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textButton1;
    [SerializeField] private TextMeshProUGUI textButton2;
    [SerializeField] private TextMeshProUGUI textButton3;
    [SerializeField] private TextMeshProUGUI textButtonAutoSave;
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
        else if(indexButton == 2)
        {
            textButton3.text = text;
        }
        else
        {
            textButtonAutoSave.text = text+" - AutoGuardado";
        }
    }
    
}
