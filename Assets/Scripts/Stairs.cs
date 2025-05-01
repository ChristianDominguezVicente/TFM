using Cinemachine;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    [SerializeField] private Vector3 destination;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            cc.enabled = false;
            other.transform.position = destination;
            Vector3 currentEuler = other.transform.eulerAngles;
            currentEuler.y += 180f;
            other.transform.eulerAngles = currentEuler;
            cc.enabled = true;
        }
    }
}
