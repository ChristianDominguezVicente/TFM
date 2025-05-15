using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    private bool looking = false;
    private Transform lookingObject;

    public bool Looking { get => looking; set => looking = value; }
    public Transform LookingObject { get => lookingObject; set => lookingObject = value; }
}
