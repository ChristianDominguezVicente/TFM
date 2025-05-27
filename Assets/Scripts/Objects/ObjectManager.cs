using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    // looking object
    private bool looking = false;
    private Transform lookingObject;

    // flags
    private bool masterKeyTaken = false;
    private bool valveActive = false;
    private bool sugar = false;
    private bool flour = false;
    private bool eggs = false;
    private bool recipe1 = false;
    private bool recipe2 = false;

    //puzle 2
    private bool teddy = false;
    private bool toolBox = false;
    private bool giftPaper = false;

    // puzle 4
    private bool correct = false;
    private bool incorrect = false;
    private GameObject currentObject;

    public bool Looking { get => looking; set => looking = value; }
    public Transform LookingObject { get => lookingObject; set => lookingObject = value; }
    
    public bool MasterKeyTaken { get => masterKeyTaken; set => masterKeyTaken = value; }
    public bool ValveActive { get => valveActive; set => valveActive = value; }
    public bool Sugar { get => sugar; set => sugar = value; }
    public bool Flour { get => flour; set => flour = value; }
    public bool Eggs { get => eggs; set => eggs = value; }
    public bool Recipe1 { get => recipe1; set => recipe1 = value; }
    public bool Recipe2 { get => recipe2; set => recipe2 = value; }
    public bool Correct { get => correct; set => correct = value; }
    public bool Incorrect { get => incorrect; set => incorrect = value; }
    public GameObject CurrentObject { get => currentObject; set => currentObject = value; }
    public bool Teddy { get => teddy; set => teddy = value; }
    public bool ToolBox { get => toolBox; set => toolBox = value; }
    public bool GiftPaper { get => giftPaper; set => giftPaper = value; }

}
