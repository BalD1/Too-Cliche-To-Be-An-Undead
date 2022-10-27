using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public GameObject player;
    private float dist;
    private float stepp;
    public float drawdistance = 3;
    public AnimationCurve animationCurve;
    public ParticleSystem particle;
    public int coinValue = 1;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //avant d'attirer la pi�ce, faire qu'elle soit "expuls�e" d'un ennemi, et apr�s activer l'attirance
        //actuellement la pi�ce ne se dirige que vers un seul joueur
        //potentiellement faire une draw distance sur le joueur et que ce soit lui qui attire les pi�ces

        dist = Vector2.Distance(this.transform.position, player.transform.position);

        //Draw Coin toward Player
        if (dist <= drawdistance)
        {
            //l'attirance est d�finie par une courbe

            stepp = animationCurve.Evaluate(1 - dist / drawdistance) / 90;

            this.transform.position = Vector2.MoveTowards(transform.position, player.transform.position, stepp);

        }

    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            //spawn particle
            if (particle != null)
            Instantiate(particle, this.transform.position, Quaternion.identity);

            //play sound

            //add coin
            TouchedPlayer(collision.transform.root.GetComponent<PlayerCharacter>());

            Destroy(this.gameObject);
        }
    }

    protected virtual void TouchedPlayer(PlayerCharacter player)
    {

    }
    
}
