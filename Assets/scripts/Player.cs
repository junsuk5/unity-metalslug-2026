using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody2D _rigidbody;

    private bool _isJump = false;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft();
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveRight();
        }
        else
        {
            Idle();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }

        if (!_isJump && Input.GetKeyDown(KeyCode.UpArrow))
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.AddForce(Vector2.up * 500f);
            _isJump = true;
        }
    }

    private void Attack()
    {
        _animator.SetTrigger("attack");
    }

    private void Idle()
    {
        _animator.SetBool("Walk", false);
    }

    private void MoveRight()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x);
        transform.localScale = scale;

        transform.Translate(6.0f * Time.deltaTime * Vector3.right);

        _animator.SetBool("Walk", true);
    }

    private void MoveLeft()
    {
        Vector3 scale = transform.localScale;
        scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;

        transform.Translate(6.0f * Time.deltaTime * Vector3.left);

        _animator.SetBool("Walk", true);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            _animator.SetBool("Grounded", true);
            _isJump = false;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            _animator.SetBool("Grounded", false);
        }
    }
}