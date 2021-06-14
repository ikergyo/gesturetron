using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadeMotorController : MotorController
{
    [SerializeField]
    private float topSpeed=50f;

    [SerializeField]
    private float defaultSpeed = 28.0f;

    [SerializeField]
    private float breakedSpeed = 20f;

    [SerializeField]
    private float accelPower = 1.005f;

    [SerializeField]
    private float breakPower = 0.95f;

    [SerializeField]
    private float defaultBreakPower = 0.98f;


    private Rigidbody m_Rigidbody;


    

    // Use this for initialization
    void Awake()
    {
        
    }
    private void OnEnable()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.velocity = transform.forward * defaultSpeed;
    }
    /*IEnumerator reachDefaultSpeed()
    {   while (m_Rigidbody.velocity.magnitude < defaultSpeed)
        {

            yield return new WaitForSeconds(0.5f);
        }
    }*/

    // Update is called once per frame
    public override void Move(float steering, float accel, float footbrake, float handbrake)
    {
        Vector3 forwardVector = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        float accelPlus = accel + 1; //Ez azért kell mert ha 0 és 1 közötti az érték akkor szorzásnál szar számolni, lassul
        if (accel > 0)
        {
            if (m_Rigidbody.velocity.magnitude < topSpeed)
            {
                Vector3 nw = m_Rigidbody.velocity.magnitude * forwardVector * accelPower * accelPlus;
                m_Rigidbody.velocity = nw;
            }
            else
            {
                m_Rigidbody.velocity = forwardVector * topSpeed;
            }
        }
        else
        {
            if(accel == 0)
            {
                
                float num = Mathf.Abs(m_Rigidbody.velocity.magnitude - defaultSpeed);
                if(num < 1.2f)
                {
                    m_Rigidbody.velocity = forwardVector * defaultSpeed;
                }
                else
                {
                    if (m_Rigidbody.velocity.magnitude > defaultSpeed)
                    {
                        Vector3 nw = forwardVector * (m_Rigidbody.velocity.magnitude * defaultBreakPower);
                        m_Rigidbody.velocity = nw;
                    }
                    else
                    {
                        Vector3 nw = forwardVector * (m_Rigidbody.velocity.magnitude * accelPower);
                        m_Rigidbody.velocity = nw;
                    }
                }
            }
            else
            {
                if(m_Rigidbody.velocity.magnitude > breakedSpeed)
                {
                    Vector3 nw = m_Rigidbody.velocity.magnitude * forwardVector * breakPower;
                    m_Rigidbody.velocity = nw ;
                }
                else
                {
                    m_Rigidbody.velocity = forwardVector * breakedSpeed;
                }
            }
        }


        
        m_Rigidbody.velocity += transform.forward * accel;
        if (steering < 0)
        {
            transform.Rotate(Vector3.down, 1.3f);
            transform.rotation = Quaternion.Euler(0,transform.eulerAngles.y,0);
        }
        else if (steering > 0)
        {
            transform.Rotate(Vector3.up, 1.3f);
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        }

    }
    public override void Init()
    {
        m_Rigidbody.velocity = Vector3.zero;
    }
}
