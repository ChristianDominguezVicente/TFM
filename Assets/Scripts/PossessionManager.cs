using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class PossessionManager : MonoBehaviour
{
    [Header("Possession Duration")]
    [SerializeField] private Image possessBarFill;
    [SerializeField] private float drainSpeed = 1f;
    [SerializeField] private float rechargeSpeed = 1f;

    [Header("Spawn")]
    [SerializeField] private GameObject player;
    [SerializeField] private LayerMask layerCollision;
    [SerializeField] private float spawnOffset = 1.5f;

    private float currentTime;
    private float maxTime;
    private bool isPossessing = false;
    private NPCPossessable currentNPC;
    private ThirdPersonController currentController;

    public bool CanPossess => !isPossessing && currentTime >= maxTime;

    public bool IsPossessing { get => isPossessing; set => isPossessing = value; }
    public ThirdPersonController CurrentController { get => currentController; set => currentController = value; }

    private void Start()
    {
        // set the player controller as the active one
        currentController = player.GetComponent<ThirdPersonController>();
    }

    private void Update()
    {
        if (isPossessing)
        {
            // if possessed, reduces time and refreshes bar
            currentTime -= Time.deltaTime * drainSpeed;
            UpdateBar();

            // if time runs out, possession is automatically cancelled
            if (currentTime <= 0)
            {
                StopPossession();
            }
        }
        else if (currentTime < maxTime)
        {
            // if not possessed, refills the bar to the maximum
            currentTime += Time.deltaTime * rechargeSpeed;
            currentTime = Mathf.Min(currentTime, maxTime);
            UpdateBar();
        }
    }

    public void StartPossession(NPCPossessable npc, float duration)
    {
        if (CanPossess)
        {
            isPossessing = true;
            currentNPC = npc;
            maxTime = duration;
            currentTime = duration;

            // activate the NPC to receive control
            npc.EnablePossession();

            // hide the original player
            player.SetActive(false);

            // change the current controller to the NPC's
            currentController = npc.GetComponent<ThirdPersonController>();
        }
    }

    private void StopPossession()
    {
        isPossessing = false;

        if (currentNPC != null)
        {
            // calculate a safe position to respawn
            Vector3 spawnPos = SpawnPosition(currentNPC.transform.position);
            // move and activate the player
            player.transform.position = spawnPos;
            player.SetActive(true);

            // deactivate the NPC and release the reference
            currentNPC.DisablePossession();
            currentNPC = null;

            // reuse the player controller
            currentController = player.GetComponent<ThirdPersonController>();
        }
    }

    private void UpdateBar()
    {
        if (possessBarFill != null)
        {
            possessBarFill.fillAmount = currentTime / maxTime;
        }
    }

    public void CancelPossession()
    {
        if (!isPossessing) return;

        StopPossession();
    }

    private Vector3 SpawnPosition(Vector3 center)
    {
        // possible addresses to try to spawn in
        Vector3[] directions = new Vector3[]
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };
        float checkRadius = 0.6f;

        // check each address looking for a free space
        foreach (var dir in directions)
        {
            Vector3 checkPos = center + dir.normalized * spawnOffset;
            Collider[] overlaps = Physics.OverlapSphere(checkPos, checkRadius, layerCollision);
            if (overlaps.Length == 0)
            {
                // if ther are no collisions, return that position
                return checkPos;
            }
        }
        // if all positions are occupied, respawn at the NPC's original position
        return center;
    }
}
