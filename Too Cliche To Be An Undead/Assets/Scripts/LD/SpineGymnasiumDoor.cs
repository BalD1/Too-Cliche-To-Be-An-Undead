using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineGymnasiumDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private SkeletonAnimation skeleton;

    [SerializeField] private BoxCollider2D blocker;

    [Header("Animations")]

    [SpineAnimation] [SerializeField] private string firstKeyAnim;
    [SpineAnimation] [SerializeField] private string secondKeyAnim;
    [SpineAnimation] [SerializeField] private string thirdKeyAnim;
    [SpineAnimation] [SerializeField] private string doorOpenAnim;

    private Queue<string> animationsToPlay;

    [SerializeField] [ReadOnly] private List<GameObject> currentInteractors = new List<GameObject>();

    private bool canBeInteracted = true;
    private bool isOpen = false;

    private int keycardsOfferedToDoor = 0;


    private void Awake()
    {
        animationsToPlay = new Queue<string>();
        animationsToPlay.Enqueue(firstKeyAnim);
        animationsToPlay.Enqueue(secondKeyAnim);
        animationsToPlay.Enqueue(thirdKeyAnim);
    }

    public bool CanBeInteractedWith()
    {
        return canBeInteracted;
    }

    public void EnteredInRange(GameObject interactor)
    {
        currentInteractors.Add(interactor);
    }

    public void ExitedRange(GameObject interactor)
    {
        currentInteractors.Remove(interactor);
    }

    public void Interact(GameObject interactor)
    {
        if (isOpen) return;

        if (keycardsOfferedToDoor >= GameManager.AcquiredCards)
        {
            skeleton.AnimationState.SetAnimation(0, doorOpenAnim, false);
            canBeInteracted = false;
            isOpen = true;
            blocker.enabled = false;
            return;
        }

        int keysToOffer = GameManager.AcquiredCards - keycardsOfferedToDoor;

        if (keysToOffer <= 0)
        {
            canBeInteracted = true;
            return;
        }

        if (animationsToPlay.Count > 0)
            skeleton.AnimationState.SetAnimation(0, animationsToPlay.Dequeue(), false);

        canBeInteracted = false;

        keycardsOfferedToDoor++;

        StartCoroutine(WaitForNextAnimation());
    }

    private IEnumerator WaitForNextAnimation()
    {
        yield return new WaitForSeconds(.5f);
        Interact(null);
    }

    public float GetDistanceFrom(Transform target) => Vector2.Distance(this.transform.position, target.position);

    public void TryOpen()
    {
        if (keycardsOfferedToDoor >= GameManager.NeededCards)
        {
            canBeInteracted = false;
        }
    }
}
