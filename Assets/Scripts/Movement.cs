using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    // Start is called before the first frame update

    public float Speed = 10.0f;
    public float Jumpforce = 2.0f;
    private Rigidbody2D Myrb;
    private float Myinput;
    private bool Isfacingright = true;
    private Animator Myanimator;
    private bool Iswalking;
    private bool Isground;
    private bool Iswall;//����Ƿ���ǹ
    private bool Iswallsliding;//���ڻ�Ҫ����velocity.y�����ټ�һ��bool��Ϊ�Ƿ񴥷��������ж�
    public Transform Checker;//��Ծ���
    public Transform Wallcheker;
    public float Checkerradius;
    public LayerMask Ground;
    public int Amountofjump;
    private int Amountofjumpleft;
    private bool Canjump;
    public float Wallcheckerdistance;
    public float Wallslidingspeed;
    public float Moveinairforce;//��AddForce�й�
    public float Jumpkeeping;//����ʵ�ֳ����������̰�����
    public float Airfriction;//����ʱ��Ϊ���

    public float Walljumpforce;
    public Vector2 Walljumpdirection;
    void Start()
    {
        Myrb = this.GetComponent<Rigidbody2D>();
        Myanimator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Checkinput();
        Checkflip();
        Checkjumpinput();
        Checkifjump();
        Checkwalking();
        Checksliding();
        Animationset();


    }
    private void FixedUpdate()
    {
        Move();
        Checkinsurrounding();
        Slide();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(Checker.position, Checkerradius);
        Gizmos.DrawLine(Wallcheker.position, new Vector3(Wallcheker.position.x + Wallcheckerdistance, Wallcheker.position.y, Wallcheker.position.z));
    }



    private void Animationset()
    {
        Myanimator.SetBool("Iswalking", Iswalking);
        Myanimator.SetBool("Isground", Isground);
        Myanimator.SetFloat("Yvelocity", Myrb.velocity.y);
        Myanimator.SetBool("Iswallsliding", Iswallsliding);

    }
    private void Checkifjump()
    {
        if ((Isground && Myrb.velocity.y <= 0.01f) || Iswallsliding)
        {
            Canjump = true;
            Amountofjumpleft = Amountofjump;
        }
        else if (Amountofjumpleft <= 0)
        {
            Canjump = false;
        }
    }
    private void Checkwalking()
    {
        if (Myrb.velocity.x != 0)
        {
            Iswalking = true;
        }
        else
        {
            Iswalking = false;

        }


    }
    private void Checkinput()
    {
        Myinput = Input.GetAxisRaw("Horizontal");

    }
    private void Checkjumpinput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        if (Input.GetButtonUp("Jump"))
        {
            Myrb.velocity = new Vector2(Myrb.velocity.x, Myrb.velocity.y * Jumpkeeping);
        }
    }
    private void Move()
    {
        if (Isground)
        {
            Myrb.velocity = new Vector2(Speed * Myinput, Myrb.velocity.y);

        }
        else if (!Isground && !Iswallsliding && Myinput != 0)
        {
            Vector2 Forcetoadd = new Vector2(Moveinairforce * Myinput, 0);
            Myrb.AddForce(Forcetoadd);
            if (Mathf.Abs(Myrb.velocity.x) > Speed)
            {
                Myrb.velocity = new Vector2(Speed * Myinput, Myrb.velocity.y);
            }
        }
        else if (!Isground && !Iswallsliding && Myinput == 0)

        {
            Myrb.velocity = new Vector2(Myrb.velocity.x * Airfriction, Myrb.velocity.y);

        }
    }
    private void Slide()
    {
        if (Iswallsliding)
        {
            if (Myrb.velocity.y < -Wallslidingspeed)
            {

                Myrb.velocity = new Vector2(Myrb.velocity.x, -Wallslidingspeed);
            }

        }
    }
    private void Checkflip()
    {
        if (Isfacingright && Myinput < 0)
        {
            Flip();
        }
        else if (!Isfacingright && Myinput > 0)
        {
            Flip();
        }
    }
    private void Flip()
    {
        Isfacingright = !Isfacingright;

        this.transform.Rotate(0.0f, 180.0f, 0.0f);

    }
    private void Jump()
    {
        if (Canjump && !Iswallsliding)
        {
            Myrb.velocity = new Vector2(Myrb.velocity.x, Jumpforce);
            Amountofjumpleft--;
        }

        else if (Canjump && Iswallsliding)
        {
            Iswallsliding = false;
            Amountofjumpleft--;
            Vector2 Walljump = new Vector2(Walljumpforce * Walljumpdirection.x * Myinput, Walljumpforce * Walljumpdirection.y);
            Myrb.AddForce(Walljump);
        }


    }
    private void Checkinsurrounding()
    {
        Isground = Physics2D.OverlapCircle(Checker.position, Checkerradius, Ground);
        Iswall = Physics2D.Raycast(Wallcheker.position, transform.right, Wallcheckerdistance, Ground);
    }
    private void Checksliding()
    {
        if (Iswall && Myrb.velocity.y < 0 && !Isground)
        {
            Iswallsliding = true;
        }
        else
        {
            Iswallsliding = false;
        }
    }


}
