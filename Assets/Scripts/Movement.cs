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
    private bool Iswall;//检测是否有墙
    private bool Iswallsliding;//由于还要考虑velocity.y所以再加一个bool作为是否触发动画的判断
    public Transform Checker;//跳跃检测
    public Transform Wallcheker;
    public float Checkerradius;
    public LayerMask Ground;
    public int Amountofjump;
    private int Amountofjumpleft;
    private bool Canwalljump;
    private bool Cannormaljump;
    public float Wallcheckerdistance;
    public float Wallslidingspeed;
    public float Moveinairforce;//与AddForce有关
    public float Jumpkeeping;//用于实现长按长跳，短按短跳
    public float Airfriction;//下落时较为柔和

    public float Walljumpforce;
    public Vector2 Walljumpdirection;
    private float Jumptimer;
    public float Jumptimerset;
    private bool Isattemptingtojump;
    private float Facingright = 1;
    public float Walljumptimerset;
    public float Walljumptimer;
    private bool Isattemptingtowalljump;
    public float Walljumpleavetimerset;
    private float Walljumpleavetime;
    private float Lastwalljumpdirection;
    private bool Haswalljumped;
    private float FallMultiplier = 2.5f;
    private float LowJumpMultiplier = 2;
    private float Vx;
    private float Vy;
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
        CheckJumpoftype();
        Judgeamountofjump();
        Walljumptimersystem();
        Vx = Myrb.velocity.x;
        Vy = Myrb.velocity.y;


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
       

        if (Amountofjumpleft <= 0)
        {
            Cannormaljump = false;
        }
        else
        {
            Cannormaljump = true;
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
        if (Isground && Amountofjumpleft == 2 && Input.GetButtonDown("Jump"))
        {

            Normaljump();
        }

       // else if (Iswall && Amountofjumpleft == 1 && Input.GetButtonDown("Jump")&&Myinput!=Facingright) 
       // {
       //     Walljump();
       // }
        else if (!Isground && Amountofjumpleft == 0 && Input.GetButtonDown("Jump"))
        {
            Jumptimer = Jumptimerset;
            Isattemptingtojump = true;
        }


        else if (!Isground && Amountofjumpleft == 1 && Input.GetButtonDown("Jump")&&!Iswall)
        {

            Normaljump();
        }

        if (Input.GetButtonUp("Jump"))//什么时候松开什么时候减速，即是按的越久就不会减速，大跳与小跳
        {
           Myrb.velocity = new Vector2(Myrb.velocity.x, Myrb.velocity.y * Jumpkeeping);
        }
    }
    private void Move()
    {
        if (!Isground && !Iswallsliding && Myinput == 0)

        {
            Myrb.velocity = new Vector2(Myrb.velocity.x * Airfriction, Myrb.velocity.y);

        }
        else
        {
            Myrb.velocity = new Vector2(Speed * Myinput, Myrb.velocity.y);

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
        Facingright = Facingright * -1;

        this.transform.Rotate(0.0f, 180.0f, 0.0f);

    }
    private void CheckJumpoftype()
    {
        if (Jumptimer > 0)
        {
            if (Isattemptingtojump)
            {
                Jumptimer = Jumptimer - Time.deltaTime;
            }
          //  if (!Isground && Iswall  && Myinput != Facingright) //!=? =-Facingright?
          //  {
            //    Walljump();
          //  }
            else if (Isground)
            {
                Normaljump();
            }
        }
    }

    private void Checkinsurrounding() 
    {
        Isground = Physics2D.OverlapCircle(Checker.position,Checkerradius,Ground);
        Iswall = Physics2D.Raycast(Wallcheker.position,transform.right,Wallcheckerdistance,Ground);
    }
    private void Checksliding() 
    {
        if (Iswall && Myinput==Facingright)
        {
            Iswallsliding = true;
        }
        else 
        {
            Iswallsliding = false;
        }
    }
    private void Normaljump()
    {
        if (Cannormaljump)
        {
            //if (Myrb.velocity.y < 0)
            //{
            //    Myrb.velocity += new Vector2(0, Physics2D.gravity.y * (FallMultiplier - 1) * Time.deltaTime);
            //}
            
            //else if (Myrb.velocity.y >0)
            //{
            //    Myrb.velocity += new Vector2(0, Physics2D.gravity.y * (LowJumpMultiplier - 1) * Time.deltaTime);
            //}
            Myrb.velocity = new Vector2(Myrb.velocity.x, Jumpforce);
            Amountofjumpleft--;
            Isattemptingtojump = false;
            Jumptimer = 0;
            
        }
    }
    private void Walljump()
    {             
            Myrb.velocity = new Vector2(Myrb.velocity.x, Jumpforce);
            Iswallsliding = false;
            //Vector2 Walljump = new Vector2(Walljumpforce * Walljumpdirection.x * Myinput, Walljumpforce * Walljumpdirection.y);
            //Myrb.AddForce(Walljump);
            Walljumptimer = 0;
            Isattemptingtowalljump = false;
            Walljumpleavetime = Walljumpleavetimerset;
        Lastwalljumpdirection = -Facingright;
        Haswalljumped = true;
    }
    private void Judgeamountofjump() 
    {
        if (Isground && Myrb.velocity.y <= 0.01 )
        {
            Amountofjumpleft = Amountofjump;
        }
        if (Isground) 
        {
            Amountofjump = 2;
        }
       
    }
    private void Walljumptimersystem() 
    {

        //if (Iswall && Myinput != Facingright && Input.GetButtonDown("Jump")&&!Isground) 
        //{
        //    Walljump();

        //}
        //if (!Isground && Iswall && Myinput != Facingright)
        //{
        //    Walljumptimer = Walljumptimerset;
        //    // Isattemptingtowalljump = true;
        //}
        //else if (Input.GetButtonDown("Jump") && Walljumptimer > 0)
        //{
        //    Walljump();
        //}


        //if (!Isground && Iswall && Input.GetButtonDown("Jump"))
        //{
        //    Walljumptimer = Walljumptimerset;
        //   // Isattemptingtowalljump = true;
        //    if (Myinput != Facingright && Walljumptimer > 0)
        //    {
        //        Walljump();
        //    }
        //}

        if (!Isground && Iswall && Myinput != Facingright)
        {
            Walljumptimer = Walljumptimerset;
            Isattemptingtowalljump = true;
        }
        if (Walljumptimer > 0)
        {

            if (Isattemptingtowalljump)
            {
                Walljumptimer = Walljumptimer - Time.deltaTime;
            }
            if (Input.GetButtonDown("Jump"))
            {
                Walljump();

            }
        }


        if (Walljumpleavetime > 0)
        {
            Walljumpleavetime -= Time.deltaTime;
            if (Myinput == -Lastwalljumpdirection && Haswalljumped)
            {
                Myrb.velocity = new Vector2(Myrb.velocity.x, 0);
                Haswalljumped = false;
            }
        }
        else if (Walljumpleavetime<=0) 
        {
            Haswalljumped = false;
        }
        
       
    }
}
