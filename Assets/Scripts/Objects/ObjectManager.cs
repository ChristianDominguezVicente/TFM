using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    private bool looking = false;
    private Transform lookingObject;

    private bool masterKeyTaken = false;

    public bool Looking { get => looking; set => looking = value; }
    public Transform LookingObject { get => lookingObject; set => lookingObject = value; }
    
    public bool MasterKeyTaken { get => masterKeyTaken; set => masterKeyTaken = value; }
}
