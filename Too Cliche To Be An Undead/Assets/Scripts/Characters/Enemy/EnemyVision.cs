using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [SerializeField] private EnemyBase owner;

    [SerializeField] private float detectionRange;

    [SerializeField] private CircleCollider2D detectionTrigger;

    [SerializeField] private LayerMask detectionMask;

    [field: SerializeField] public bool isActive { get; private set; }

#if UNITY_EDITOR
    [SerializeField] private bool debugMode;
#endif

    private Vector3 dir;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isActive) return;

        if (collision.CompareTag("VisionTarget"))
        {
            PlayerCharacter player = collision.GetComponentInParent<PlayerCharacter>();
            if (player == null) return;

            if (owner.DetectedPlayers.Contains(player) == false)
            {
                dir = (player.transform.position - owner.transform.position);
                RaycastHit2D hit = Physics2D.Raycast(owner.transform.position, dir, detectionRange, detectionMask);
#if UNITY_EDITOR
                if (debugMode)
                    Debug.DrawRay(owner.transform.position, dir, Color.red);
#endif
                if (hit)
                {
                    if (hit.collider.CompareTag("VisionTarget")) owner.AddDetectedPlayer(player);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isActive) return;

        if (collision.CompareTag("VisionTarget"))
        {
            PlayerCharacter player = collision.GetComponentInParent<PlayerCharacter>();
            if (owner.DetectedPlayers.Contains(player))
            {
                owner.RemoveDetectedPlayer(player);
            }
        }
    }

    public void SetVisionState(bool active)
    {
        isActive = active;
        if (!isActive)
        {
            foreach (var item in owner.DetectedPlayers)
            {
                owner.RemoveDetectedPlayer(item);
            }
        }
    }

    private void OnValidate()
    {
        if (detectionTrigger != null)
            detectionTrigger.radius = detectionRange;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!debugMode) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(this.transform.position, detectionRange);
#endif
    }
}
