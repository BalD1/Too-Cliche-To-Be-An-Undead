using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEMP_HubDoor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Vector2 playerTPPos;
    [SerializeField] private BoxCollider2D boxCollider2D;

    [SerializeField] private FightArena fightArena;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            boxCollider2D.isTrigger = false;
            spriteRenderer.color = Color.red;
            if (collision.gameObject.transform.parent != null)
                collision.gameObject.transform.parent.position = playerTPPos;

            if (fightArena != null && !fightArena.started) fightArena.SpawnNext(0);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(playerTPPos, 0.5f);
    }
}