using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    public string TargetName = "";

	// Use this for initialization
	void Start () {
        Destroy(gameObject, 0.5f);
	}
	
	// Update is called once per frame
	void Update () {

//        if (!gameObject.GetComponent<tk2dSprite>().FlipX)
//        {
//            transform.Translate(Vector3.right * 20.0f * Time.deltaTime);
//        }
//        else
//        {
//            transform.Translate(Vector3.left * 20.0f * Time.deltaTime);
//        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.name == TargetName)
        {
            if (TargetName == "Enemy")
            {
                if (!coll.gameObject.GetComponent<Enemy>().dead)
                {
                    Destroy(gameObject);
                    coll.gameObject.GetComponent<Enemy>().EnemyDead();
                }
            }
        }
    }
}
