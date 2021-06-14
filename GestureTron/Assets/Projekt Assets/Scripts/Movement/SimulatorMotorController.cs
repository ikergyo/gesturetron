using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class SimulatorMotorController : MotorController
{

    internal enum Roll
    {
        Right,
        Left,
        None
    }
    //SerializeFieldek amiket a unityn belül lehet állitani az adott objektumhoz, itt meg megkapja a defaultat.
    [SerializeField] private float m_Topspeed = 200; //Max sebesség
    [SerializeField] private float m_Downforce = 100f; //Leszoritó erő
    [SerializeField] private float m_MaximumSteerAngle;
    [SerializeField] private WheelCollider[] m_WheelColliders = new WheelCollider[2]; //Collider a kerékhez
    [SerializeField] private float m_SlipLimit; 
    [Range(0, 1)][SerializeField] private float m_TractionControl;
    [SerializeField] private float m_FullTorqueOverAllWheels;
    [SerializeField]
    private float DefaultAllTorque = 3988;
	[SerializeField]
	private float BonusTorqueValue = 100;
	[SerializeField]
	private float MaximumBonusTorque = 3000;
    private float transformZPercent;
    private float transformActualZRotate;

    private float BonusTorque = 0;

	private Stopwatch boostStopper;
	private Stopwatch boostDecreaseStopper;

    [SerializeField] private float m_BrakeTorque;
    [SerializeField] private float m_ReverseTorque;


    private Roll actualRoll = Roll.None; //Éppen merre dől.
    private float m_CurrentTorque; //Aktuális forgatónyomaték
    private Rigidbody m_Rigidbody; //Fizikai test (?) 
    private const float k_ReversingThreshold = 0.01f; //forditott küszöbérték (?)

    public bool Skidding { get; private set; } //csúszás (?)
    public float BrakeInput { get; private set; }
    //public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * 2.23693629f; } } //Ha mérföldel számolunk
    public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * 3.6f; } }
    public float MaxSpeed { get { return m_Topspeed; } }
    public float Revs { get; private set; }
    public float AccelInput { get; private set; }
    

    private void OnEnable(){
        m_Rigidbody = GetComponent<Rigidbody>();
        boostStopper = new Stopwatch ();
		boostDecreaseStopper = new Stopwatch ();
	}

	void Start () {
        

		boostStopper.Start ();

		m_CurrentTorque = DefaultAllTorque;

        //m_CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl * m_FullTorqueOverAllWheels);
    }
    private void SpeedLimitController()  
    {
        float speed = m_Rigidbody.velocity.magnitude;
        speed *= 3.6f;
        if (speed > m_Topspeed)
        {
           // Debug.Log("Normalized: " + m_Rigidbody.velocity.normalized);
            m_Rigidbody.velocity = (m_Topspeed / 3.6f) * m_Rigidbody.velocity.normalized;
        }
    }
    public override void Move(float steering, float accel, float footbrake, float handbrake)
    {
        float actualEulerTransfomrZ = this.transform.eulerAngles.z;
        if (actualEulerTransfomrZ > 0 && actualEulerTransfomrZ < 180)
        {
            actualRoll = Roll.Left;
        }
        else if (actualEulerTransfomrZ > 180 && actualEulerTransfomrZ < 360)
        {
            actualRoll = Roll.Right;
        }

        float origiSteering = steering;
        steering = Mathf.Clamp(steering, -1, 1); //Clamp: ha kisebb az érték mint aminimum akkor minimum, ha nagyobb mint max akkor maximum, különben meg az érték.
        steering = steering * m_MaximumSteerAngle;
        AccelInput = accel = Mathf.Clamp(accel, 0, 1); //0 és 1 közötti szám
        BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0); //0 és 1 közötti szám
        //float steerY = steering / (CurrentSpeed/100);

        //Ez vizsgálja mennyire dől be és nem engedi tovább dőlni. Ki lehetne szervezni külön dllbe vagy csak fvbe
        float steerHelper = this.transform.eulerAngles.z + steering;

        if (transform.eulerAngles.z > 180)
        {
            transformActualZRotate = (transform.eulerAngles.z - 360);
        }
        else
        {
            transformActualZRotate = transform.eulerAngles.z;
        }

        if (steering == 0 && transform.eulerAngles.z != 0)
        {
            
            if (!(Mathf.Abs(transformActualZRotate) <= 2))
            {
                transformZPercent = transformActualZRotate * 0.05f;
                transform.Rotate(new Vector3(0, 0, -transformZPercent));
            }
            else
            {
                transform.Rotate(new Vector3(0, 0, -transformActualZRotate));
            }     
        }
        if (transformActualZRotate < 25 && transformActualZRotate > -25)
        {
            transform.Rotate(new Vector3(0, 0, -steering)); 
        }
        else
        {
            if (transformActualZRotate >= 25)
            {
                transform.Rotate(new Vector3(0, 0, -0.05f));
            }
            else if(transformActualZRotate <= -25)
            {
                transform.Rotate(new Vector3(0, 0, 0.05f));
            }

            if(actualRoll == Roll.Left && origiSteering > 0)
            {
                transform.Rotate(new Vector3(0, 0, -steering));
            }
            else if (actualRoll == Roll.Left && origiSteering < 0)
            {
                transform.Rotate(new Vector3(0, origiSteering, 0));
            }
            else if(actualRoll == Roll.Right && origiSteering < 0)
            {
                transform.Rotate(new Vector3(0, 0, -steering));
            }
            else if (actualRoll == Roll.Right && origiSteering > 0)
            {
                transform.Rotate(new Vector3(0, origiSteering, 0));
            }
        }
		BoostListener ();
        SpeedController(accel, footbrake);
        SpeedLimitController();
        GroundControl();
    }
    private void SpeedController(float accel, float footbrake)
    {

        float thrustTorque;

		thrustTorque = accel * ((m_CurrentTorque / 2f) + (BonusTorque / 2f)); //ez adódik hozzá, ez a tolóerő (gáz). Azért van osztva kettővel mert kettő kerék van ami. Kétkerék meghajtásos

        m_WheelColliders[0].motorTorque = m_WheelColliders[1].motorTorque = thrustTorque;

       for (int i = 0; i < 2; i++)
        {
            //ha az aktuáli ssebesség nagyobb mint 5 és a Rigidbody sebességvektorának és a forward treansformnak a közbezárt szöge kissebb mint 50
           if (CurrentSpeed > 5 /*&& Vector3.Angle(transform.forward, m_Rigidbody.velocity) < 50f*/)
            {
                m_WheelColliders[1].brakeTorque = m_BrakeTorque * footbrake;
            }
            else if (footbrake > 0) //ellenkező esetben ha nyomjuk a féklet(hátrafele nyil)
            {
                m_WheelColliders[i].brakeTorque = 0f; //a féket 0 ra állitja
                m_WheelColliders[i].motorTorque = -m_ReverseTorque * footbrake; // tolatni fog, reversetorque erősségel (minusz a motortorque)
            }
        }
    }
    
    private void AdjustTorque(float forwardSlip)
    {
        if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
        {
            m_CurrentTorque -= 10 * m_TractionControl; //lassitja ha a csúszás limit túllépte a slipLimitet ( kb mindig belép iyg)
        }
        else//Különben plusszolja az aktuális nyomatékot
        {
            m_CurrentTorque += 10 * m_TractionControl;
            if (m_CurrentTorque > m_FullTorqueOverAllWheels)
            {
                m_CurrentTorque = m_FullTorqueOverAllWheels; //Korlátozza ne lépje túl a megadottat
            }
        }

    }
    
    private void GroundControl() 
    {
        WheelHit wheelHit;
        //Visszaadja hogy azt adott kerék hozzáér-e valamihez(planehez)
        m_WheelColliders[0].GetGroundHit(out wheelHit); 
        AdjustTorque(wheelHit.forwardSlip);
        
        m_WheelColliders[1].GetGroundHit(out wheelHit);
        AdjustTorque(wheelHit.forwardSlip);
    }
	private void BoostListener(){
		if(Input.GetKeyDown(KeyCode.LeftControl)){
			boostStopper.Reset ();
			boostStopper.Start ();
			if (BonusTorque <= MaximumBonusTorque) {
				BonusTorque += BonusTorqueValue;
			}
		}
		if (boostStopper.ElapsedMilliseconds >= 500) {
			if (boostDecreaseStopper.IsRunning && (boostDecreaseStopper.ElapsedMilliseconds % 20) == 0) {
				
				if (BonusTorque == 0) {
					boostStopper.Reset ();
					boostStopper.Stop();
					boostDecreaseStopper.Reset ();
					boostDecreaseStopper.Stop ();
				} else {
					BonusTorque -= BonusTorqueValue;
				}
			} else {
				boostDecreaseStopper.Reset ();
				boostDecreaseStopper.Start ();
			}

		}
	}
    public override void Init()
    {
        m_Rigidbody.velocity = Vector3.zero;
    }
}
