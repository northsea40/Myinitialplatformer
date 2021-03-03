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
    private bool Iswall;//chec the wall;
    private bool Iswallsliding;//wallsliding animation bool
    public Transform Checker;//跳跃检测
    public Transform Wallcheker;
    public float Checkerradius;
    public LayerMask Ground;
    public int Amountofjump;
    private int Amountofjumpleft;
    public float Wallcheckerdistance;
    public float Wallslidingspeed;
    public float Moveinairforce;//与AddForce有关
    public float Jumpkeeping;//用于实现长按长跳，短按短跳
    public float Airfriction;//下落时较为柔和

    public float Walljumpforce;
    public Vector2 Walljumpdirection;
    public float Jumptimerset;
    private float Jumptimer;
    private bool Isattemptingtojump=false;
    private float Facingright=1;
    private bool Cannormaljump;
    private bool Canwalljump;
    private bool Checkjumpmultiplier;
    private bool Canmove;
    private bool Canflip;
    public float Turntimerset;
    private float Turntimer;
    void Start()
    {
        Myrb = this.GetComponent<Rigidbody2D>();
        Myanimator = this.GetComponent<Animator>();
        Walljumpdirection.Normalize();
        Amountofjumpleft = Amountofjump;
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
        Jumptimersystem();


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
        if (Isground && Myrb.velocity.y <= 0.01f)
        { 
            Amountofjumpleft = Amountofjump;
        }
        if (Iswall) 
        {
            Canwalljump = true;
            Checkjumpmultiplier = false;
            //贴着墙时不允许二连跳，防止跳到墙上再直接跳时误算一次二连跳
        }
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
        if (Input.GetButtonDown("Jump"))
        {
            if (Isground ||( Amountofjumpleft > 0&&!Iswall))//墙跳必进jumptimer
            { 
                Normaljump();
            }
            else
            {
                Jumptimer = Jumptimerset;
                Isattemptingtojump = true;
            }
        }
        //控制长按长跳短按短跳，如果没有按住则乘一个分数减少速度，为什么要加checkmultiplier？
        //checkmultiplier在normaljump或walljump触发的瞬间后会变为true；在iswall的情况下变成false
        //有没有时候时按下空格后不触发checkjumpmultiplier为true的呢？因为按下空格
        if (Checkjumpmultiplier&&!Input.GetButton("Jump"))
        {
            Checkjumpmultiplier = false;
            Myrb.velocity = new Vector2(Myrb.velocity.x, Myrb.velocity.y * Jumpkeeping);
        }
        if (Input.GetButtonDown("Horizontal") && Iswall)   //如果在墙壁上按下方向键并且向墙的相反方向移动，禁止移动和人物的翻转       
        {
            if (!Isground&&Myinput!=Facingright) 
            {
                Canmove = false;
                Canflip = false;
                Turntimer = Turntimerset;
                //turntimer 的系统让角色在贴到墙上后想向相反方向脱离墙时有一个延迟，避免了先按下方向键
                //再按下空格键导致脱离了墙壁而无法进行walljump
            }
        }
        if (!Canmove) //只有在反向脱离墙壁时使canmove=false应该用另一个bool替换
        {
            Turntimer = Turntimer - Time.deltaTime;
            if (Turntimer<=0) 
            {
                Canmove = true;
                Canflip = true;
            }
        }
    }
    private void Jumptimersystem() //checjump function in the tutorial
    {
        if (Jumptimer > 0)
        {
            if (!Isground && Iswall && Myinput != Facingright)
                //先按了空格再按了方向键，所以配合了turntimer，解决了先按方向键再按空格和先按空格再按方向键的两种
                //误触
            {
                Walljump();

            }
            else if (Isground)
            {
                Normaljump();
            }

        }
        if(Isattemptingtojump)//换一个bool
        {
            Jumptimer = Jumptimer - Time.deltaTime;
        }
    }
    private void Move()
    {
        if (Canmove)
        {
            Myrb.velocity = new Vector2(Speed * Myinput, Myrb.velocity.y);

        }
        //else if (!isground && !iswallsliding && myinput != 0)//control the movement in the air
        //{
        //    vector2 forcetoadd = new vector2(moveinairforce * myinput, 0);
        //    myrb.addforce(forcetoadd);
        //    if (mathf.abs(myrb.velocity.x) > speed)
        //    {
        //        myrb.velocity = new vector2(speed * myinput, myrb.velocity.y);
        //    }
        //}
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
        if (Canflip&&!Iswallsliding)
        {
            Isfacingright = !Isfacingright;
            Facingright = Facingright * -1;
            this.transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }
    private void Normaljump()
    {
        if (Cannormaljump)
        {
            Myrb.velocity = new Vector2(Myrb.velocity.x, Jumpforce);
            Amountofjumpleft--;
            Isattemptingtojump = false;
            Jumptimer = 0;
            Checkjumpmultiplier=true;
        }

    }
    private void Walljump() 
    {
        if (Canwalljump)
        {
            Myrb.velocity = new Vector2(Myrb.velocity.x,0);
            Iswallsliding = false;
            Amountofjumpleft = Amountofjump;
            Amountofjumpleft--;
            Vector2 Walljump = new Vector2(Walljumpforce * Walljumpdirection.x * Myinput, Walljumpforce * Walljumpdirection.y);
            Myrb.AddForce(Walljump,ForceMode2D.Impulse);//forcemode?
            Isattemptingtojump = false;//关闭jumptimer
            Jumptimer = 0;
            Checkjumpmultiplier = true;
            Turntimer = 0;
            Canflip = true;
            Canmove = true;
        }



    }
    private void Checkinsurrounding()
    {
        Isground = Physics2D.OverlapCircle(Checker.position, Checkerradius, Ground);
        Iswall = Physics2D.Raycast(Wallcheker.position, transform.right, Wallcheckerdistance, Ground);
    }
    private void Checksliding()
    {
        if (Iswall && Myrb.velocity.y < 0 && Myinput==Facingright)
        {
            Iswallsliding = true;
        }
        else
        {
            Iswallsliding = false;
        }
    }


}
