using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

    enum MoveList
    {
        STOP = 0,
        LEFT,
        RIGHT,
    }
    public AudioClip Dead, ShotGun;
    public GameObject Bullet;

    bool active = false, attack = false;
    public bool dead = false;
    int move = (int)MoveList.STOP;
    GameObject Hero;

	// Use this for initialization
	void Start () {

        Hero = GameObject.Find("Hero");
	}

    public void EnemyDead()
    {
        if (!dead)
        {
            GetComponent<AudioSource>().clip = Dead;
            GetComponent<AudioSource>().Play();
//            gameObject.GetComponent<tk2dSpriteAnimator>().Play("Dead");
            dead = true;
        }
    }

    void MoveLeft()
    {
        transform.Translate(Vector3.left * 3.0f * Time.deltaTime);
//        gameObject.GetComponent<tk2dSprite>().FlipX = false;
//        gameObject.GetComponent<tk2dSpriteAnimator>().Play("Walk");
    }

    void MoveRight()
    {
        transform.Translate(Vector3.right * 3.0f * Time.deltaTime);
//        gameObject.GetComponent<tk2dSprite>().FlipX = true;
//        gameObject.GetComponent<tk2dSpriteAnimator>().Play("Walk");
    }

    // Update is called once per frame
	void FixedUpdate () {

        if (!dead)
        {
            if (!active)
            {
                if (Vector3.Distance(Hero.transform.position, transform.position) < 10.0f) active = true;
            }
            else
            {
                if (!attack)
                {
                    if (Mathf.Abs(transform.position.x - Hero.transform.position.x) > 1.0f)
                    {
                        if (transform.position.x > Hero.transform.position.x) MoveLeft(); else MoveRight();
                    }

                    if (Random.Range(0, 100) == 0)
                    {
                        GetComponent<AudioSource>().clip = ShotGun;
                        GetComponent<AudioSource>().Play();

//                        GameObject temp = Instantiate(Bullet, new Vector3(transform.position.x + (gameObject.GetComponent<tk2dSprite>().FlipX ? 0.3f : -0.3f), transform.position.y + 1.4f, transform.position.z + 1), Quaternion.identity) as GameObject;
//                        temp.GetComponent<tk2dSprite>().FlipX = !gameObject.GetComponent<tk2dSprite>().FlipX;
//                        temp.GetComponent<Bullet>().TargetName = "Hero";

//                        gameObject.GetComponent<tk2dSpriteAnimator>().Play("Attack");
                        attack = true;
                    }
                }
                else
                {
//                    if (!gameObject.GetComponent<tk2dSpriteAnimator>().IsPlaying("Attack")) attack = false;
                }
            }
        }
        else
        {
//            if (!gameObject.GetComponent<tk2dSpriteAnimator>().IsPlaying("Dead")) Destroy(gameObject, 2.0f);
        }
	}
}
