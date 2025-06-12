using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] private DoorInteractuable principalDoorGO;
    [SerializeField] private MasterKeyInteractuable masterKeyGO;
    [SerializeField] private ValveInteractuable valveGO;
    [SerializeField] private SugarInteractuable sugarGO;
    [SerializeField] private FlourInteractuable flourGO;
    [SerializeField] private EggsInteractuable eggsGO;
    [SerializeField] private Recipe1Interactuable recipe1GO;
    [SerializeField] private Recipe2Interactuable recipe2GO;
    [SerializeField] private CodeInteractuable codeGO;
    [SerializeField] private TeddyInteractuable teddyGO;
    [SerializeField] private ToolBoxInteractuable toolBoxGO;
    [SerializeField] private GiftPaperInteractuable giftPaperGO;

    // looking object
    private bool looking = false;
    private Transform lookingObject;

    // puzle 1
    private bool principalDoor = false;
    private bool calendar = false;
    private bool masterKeyTaken = false;
    private bool valveActive = false;
    private bool sugar = false;
    private bool flour = false;
    private bool eggs = false;
    private bool recipe1 = false;
    private bool recipe2 = false;

    // puzle 2
    private bool teddy = false;
    private bool toolBox = false;
    private bool giftPaper = false;

    // puzle 3
    private bool icons = false;
    private bool poster = false;
    private bool tablet = false;
    private bool photo = false;
    private bool note = false;

    // puzle 4
    private bool correct = false;
    private bool incorrect = false;
    private GameObject currentObject;

    public bool Looking { get => looking; set => looking = value; }
    public Transform LookingObject { get => lookingObject; set => lookingObject = value; }

    public bool PrincipalDoor { get => principalDoor; set => principalDoor = value; }
    public bool Calendar { get => calendar; set => calendar = value; }
    public bool MasterKeyTaken { get => masterKeyTaken; set => masterKeyTaken = value; }
    public bool ValveActive { get => valveActive; set => valveActive = value; }
    public bool Sugar { get => sugar; set => sugar = value; }
    public bool Flour { get => flour; set => flour = value; }
    public bool Eggs { get => eggs; set => eggs = value; }
    public bool Recipe1 { get => recipe1; set => recipe1 = value; }
    public bool Recipe2 { get => recipe2; set => recipe2 = value; }
    public bool Icons { get => icons; set => icons = value; }
    public bool Poster { get => poster; set => poster = value; }
    public bool Tablet { get => tablet; set => tablet = value; }
    public bool Photo { get => photo; set => photo = value; }
    public bool Note { get => note; set => note = value; }
    public bool Correct { get => correct; set => correct = value; }
    public bool Incorrect { get => incorrect; set => incorrect = value; }
    public GameObject CurrentObject { get => currentObject; set => currentObject = value; }
    public bool Teddy { get => teddy; set => teddy = value; }
    public bool ToolBox { get => toolBox; set => toolBox = value; }
    public bool GiftPaper { get => giftPaper; set => giftPaper = value; }

    public void OnLoad()
    {
        if (principalDoorGO != null && principalDoor)
            principalDoorGO.Action();
        if (masterKeyGO != null && masterKeyTaken)
        {
            masterKeyGO.Action();
            if (SceneManager.GetActiveScene().name == "Greybox" || SceneManager.GetActiveScene().name == "Puzzle2")
                Destroy(codeGO);
        }
        if (valveGO != null && valveActive)
            valveGO.Action();
        if (sugarGO != null && sugar)
            sugarGO.gameObject.SetActive(false);   
        if (flourGO != null && flour)
            flourGO.gameObject.SetActive(false);   
        if (eggsGO != null && eggs)
            eggsGO.gameObject.SetActive(false);     
        if (recipe1GO != null && recipe1)
            recipe1GO.gameObject.SetActive(false);
        if (recipe2GO != null && recipe2)
            Destroy(recipe2GO);
        if (teddyGO != null && teddy)
            teddyGO.Action();
        if (toolBoxGO != null && toolBox)
            toolBoxGO.Action();
        if (giftPaperGO != null && giftPaper)
            giftPaperGO.gameObject.SetActive(false);
        if (currentObject != null)
        {
            if (correct)
            {
                CorrectInteractuable correctObject = currentObject.GetComponent<CorrectInteractuable>();
                if (correctObject != null)
                    correctObject.Action();
            }  
            else if (incorrect)
            {
                IncorrectInteractuable incorrectObject = currentObject.GetComponent<IncorrectInteractuable>();
                if (incorrectObject != null)
                    incorrectObject.Action();
            }
        }    
    }
}
