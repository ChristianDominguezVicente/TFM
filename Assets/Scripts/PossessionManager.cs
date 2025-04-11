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
    private NPCInteractuable currentNPC;

    public bool CanPossess => !isPossessing && currentTime >= maxTime;

    public bool IsPossessing { get => isPossessing; set => isPossessing = value; }

    private void Update()
    {
        if (isPossessing)
        {
            currentTime -= Time.deltaTime * drainSpeed;
            UpdateBar();

            if (currentTime <= 0)
            {
                StopPossession();
            }
        }
        else if (currentTime < maxTime)
        {
            currentTime += Time.deltaTime * rechargeSpeed;
            currentTime = Mathf.Min(currentTime, maxTime);
            UpdateBar();
        }
    }

    public void StartPossession(NPCInteractuable npc, float duration)
    {
        if (CanPossess)
        {
            isPossessing = true;
            currentNPC = npc;
            maxTime = duration;
            currentTime = duration;

            npc.EnablePossession();

            player.SetActive(false);
        }
    }

    private void StopPossession()
    {
        isPossessing = false;

        if (currentNPC != null)
        {
            Vector3 spawnPos = SpawnPosition(currentNPC.transform.position);
            player.transform.position = spawnPos;
            player.SetActive(true);

            currentNPC.DisablePossession();
            currentNPC = null;
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
        Vector3[] directions = new Vector3[]
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };
        float checkRadius = 0.6f;

        foreach (var dir in directions)
        {
            Vector3 checkPos = center + dir.normalized * spawnOffset;
            Collider[] overlaps = Physics.OverlapSphere(checkPos, checkRadius, layerCollision);
            if (overlaps.Length == 0)
            {

                return checkPos;
            }
        }

        return center;
    }
}
