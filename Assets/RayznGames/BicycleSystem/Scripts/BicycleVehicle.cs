using UnityEngine;
using UnityEditor;
using System.Drawing;
using DG.Tweening;
using NaughtyAttributes;
using Unity.Netcode;

namespace rayzngames
{
    public class BicycleVehicle : NetworkBehaviour
    {
        //debugInfo	
        public NetworkVariable<float> _horizontalInput = new NetworkVariable<float>(0f);

        //NetworkVariable<float> _verticalInput = new NetworkVariable<float>(0f);
        NetworkVariable<bool> _braking = new NetworkVariable<bool>(false);

        [SerializeField] Rigidbody rb;

        [Header("Power/Braking")] [Space(5)] [SerializeField]
        float motorForce;

        [Header("Power/Braking")] [Space(5)] [SerializeField]
        private float motorForceInstant = 70f;

        [Header("Power/Braking")] [Space(5)] [SerializeField]
        private float pedalInterval = .4f;

        [SerializeField] float brakeForce;
        public Vector3 COG;

        [Space(20)]
        [Header("Steering")]
        [Space(5)]
        [Tooltip("Defines the maximum steering angle for the bike")]
        [SerializeField]
        float maxSteeringAngle;

        [Tooltip("Sets how current_MaxSteering is reduced based on the speed of the RB, (0 - No effect) (1 - Full)")]
        [Range(0f, 1f)]
        [SerializeField]
        float steerReductorAmmount;

        [Tooltip("Sets the Steering sensitivity [Steering Stiffness] 0 - No turn, 1 - FastTurn)")]
        [Range(0.001f, 1f)]
        [SerializeField]
        float turnSmoothing;

        [Space(20)]
        [Header("Lean")]
        [Space(5)]
        [Tooltip("Defines the maximum leaning angle for this bike")]
        [SerializeField]
        float maxLeanAngle = 45f;

        [Tooltip("Sets the Leaning sensitivity (0 - None, 1 - full")] [Range(0.001f, 1f)] [SerializeField]
        float leanSmoothing;

        float targetLeanAngle;

        [Space(20)] [Header("Object References")]
        public Transform handle;

        public Transform pedalPivot;

        [Space(10)] [SerializeField] WheelCollider frontWheel;
        [SerializeField] WheelCollider backWheel;
        [Space(10)] [SerializeField] Transform frontWheelTransform;
        [SerializeField] Transform backWheelTransform;
        [Space(10)] [SerializeField] TrailRenderer frontTrail;
        [SerializeField] TrailRenderer rearTrail;
        ContactProvider frontContact;
        ContactProvider rearContact;

        [Space(20)] [HeaderAttribute("Info")] [SerializeField]
        float currentSteeringAngle;

        [Tooltip("Dynamic steering angle baed on the speed of the RB, affected by sterReductorAmmount")]
        [SerializeField]
        float current_maxSteeringAngle;

        [Tooltip("The current lean angle applied")] [Range(-45, 45)]
        public float currentLeanAngle;

        [Space(20)] [HeaderAttribute("Speed M/s")] [SerializeField]
        public float currentSpeed;

        private float pedalIntervalTimer;
        private Tweener pedalTween;

        // Start is called before the first frame update
        void Start()
        {
            if (!IsServer)
                return;

            frontContact = frontTrail.transform.GetChild(0).GetComponent<ContactProvider>();
            rearContact = rearTrail.transform.GetChild(0).GetComponent<ContactProvider>();
            //Important to stop bike from Jittering
            frontWheel.ConfigureVehicleSubsteps(5, 12, 15);
            backWheel.ConfigureVehicleSubsteps(5, 12, 15);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            EmitTrail();
            UpdateHandles();
            UpdateWheels();
            Speed_O_Meter();
            HandleSteering();

            if (!IsServer)
                return;

            HandleEngine();
            HandleEngineNew();
            LeanOnTurn();
            //DebugInfo();
        }


        [Rpc(SendTo.Server)]
        public void SetHorizontalInputRpc(float horizontalInput)
        {
            _horizontalInput.Value = horizontalInput;
        }

        // [Rpc(SendTo.Server)]
        // public void SetVerticalInputRpc(float verticalInput)
        // {
        //     _verticalInput.Value = verticalInput;
        // }

        [Rpc(SendTo.Server)]
        public void SetBrakingRpc(bool braking)
        {
            //TODO: Brekaing icin  
            // _braking.Value = braking;
        }

        [Rpc(SendTo.Server)]
        public void PedalInstantRpc()
        {
            SetMotorTorque();
        }

        private void SetMotorTorque()
        {
            if (pedalIntervalTimer > 0)
                return;

            Debug.Log("Motor Force set ediddi");
            backWheel.motorTorque = motorForceInstant;
            pedalIntervalTimer = pedalInterval; //Reset the interval
            // Mevcut tween varsa durdur
            if (pedalTween != null && pedalTween.IsActive())
                pedalTween.Kill();

            // Yeni tween başlat
            pedalTween = pedalPivot
                .DOLocalRotate(GetPedalRotationBySpeed(), pedalInterval + .1f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear);
        }

        public RotateMode RotateMode;

        [Button]
        public void rotate90Degrees()
        {
            if (pedalTween != null && pedalTween.IsActive())
                pedalTween.Kill();

            // Yeni tween başlat
            pedalTween = pedalPivot.DOLocalRotate(new Vector3(-90, 0, 0), pedalInterval + .1f, RotateMode)
                .SetEase(Ease.Linear);
        }

        [Button]
        public void rotate180Degrees()
        {
            if (pedalTween != null && pedalTween.IsActive())
                pedalTween.Kill();

            // Yeni tween başlat
            pedalTween = pedalPivot.DOLocalRotate(new Vector3(-180, 0, 0), pedalInterval + .1f, RotateMode)
                .SetEase(Ease.Linear);
        }

        [Button]
        public void Rotate360Degrees()
        {
            if (pedalTween != null && pedalTween.IsActive())
                pedalTween.Kill();

            // Yeni tween başlat
            pedalTween = pedalPivot.DOLocalRotate(new Vector3(360, 0, 0), pedalInterval + .1f, RotateMode)
                .SetEase(Ease.Linear);
        }


        private Vector3 GetPedalRotationBySpeed()
        {
            switch (currentSpeed)
            {
                case < 5:
                    return new Vector3(-90, 0, 0);
                case < 10:
                    return new Vector3(-180, 0, 0);
                case < 15:
                    return new Vector3(-360, 0, 0);
                case < 20:
                    return new Vector3(-540, 0, 0);
                default:
                    return new Vector3(-720, 0, 0);
            }
        }

        private void HandleEngineNew()
        {
            if (pedalIntervalTimer <= 0 && backWheel.motorTorque > 0)
            {
                backWheel.motorTorque = 0f;
            }
            else if (pedalIntervalTimer > 0)
            {
                pedalIntervalTimer -= Time.fixedDeltaTime;
            }

            Debug.Log("Motor Force: " + backWheel.motorTorque);
        }


        private void HandleEngine()
        {
            //TODO: Bunu geri kaldır eski sisteme dönmek için
            //backWheel.motorTorque = _braking.Value ? 0f : _verticalInput.Value * motorForce;


            //If we are braking, ApplyBreaking applies brakeForce conditional is embeded in parameter	
            float force = _braking.Value ? brakeForce : 0f;
            ApplyBraking(force);
        }

        public void ApplyBraking(float brakeForce)
        {
            frontWheel.brakeTorque = brakeForce;
            backWheel.brakeTorque = brakeForce;
        }

        //This replaces the (Magic numbers) that controlled an exponential decay function for maxteeringAngle (maxSteering angle was not adjustable)
        //This one alows to customize Default bike maxSteeringAngle parameters and maxSpeed allowing for better scalability for each vehicle	
        /// <summary>
        /// Reduces the current maximum Steering based on the speed of the Rigidbody multiplied by SteerReductionAmmount (0-1)  
        /// </summary>
        void MaxSteeringReductor()
        {
            //30 is the value of MaxSpeed at wich currentMaxSteering will be at its minimum,			
            float t = (rb.linearVelocity.magnitude / 30) * steerReductorAmmount;
            t = t > 1 ? 1 : t;
            current_maxSteeringAngle =
                Mathf.LerpAngle(maxSteeringAngle, 5, t); //5 is the lowest posisble degrees of Steering	
        }

        public void HandleSteering()
        {
            MaxSteeringReductor();

            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, current_maxSteeringAngle * _horizontalInput.Value,
                turnSmoothing * 0.1f);
            frontWheel.steerAngle = currentSteeringAngle;

            //We set the target lean angle to the + or - input value of our steering 
            //We invert our input for rotating in the ocrrect axis
            targetLeanAngle = maxLeanAngle * -_horizontalInput.Value;
        }

        public void UpdateHandles()
        {
            handle.localEulerAngles =
                new Vector3(handle.localEulerAngles.x, currentSteeringAngle, handle.localEulerAngles.z);
            //handle.Rotate(Vector3.up, currentSteeringAngle, Space.Self);		
        }

        private void LeanOnTurn()
        {
            Vector3 currentRot = transform.rotation.eulerAngles;
            //Case: not moving much		
            if (rb.linearVelocity.magnitude < 1)
            {
                currentLeanAngle = Mathf.LerpAngle(currentLeanAngle, 0f, 0.1f);
                transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, currentLeanAngle);
                //return;
            }

            //Case: Not steering or steering a tiny amount
            if (currentSteeringAngle < 0.5f && currentSteeringAngle > -0.5)
            {
                currentLeanAngle = Mathf.LerpAngle(currentLeanAngle, 0f, leanSmoothing * 0.1f);
            }
            //Case: Steering
            else
            {
                currentLeanAngle = Mathf.LerpAngle(currentLeanAngle, targetLeanAngle, leanSmoothing * 0.1f);
                rb.centerOfMass = new Vector3(rb.centerOfMass.x, COG.y, rb.centerOfMass.z);
            }

            transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, currentLeanAngle);
        }

        public void UpdateWheels()
        {
            UpdateSingleWheel(frontWheel, frontWheelTransform);
            UpdateSingleWheel(backWheel, backWheelTransform);
        }

        private void EmitTrail()
        {
            if (_braking.Value)
            {
                frontTrail.emitting = frontContact.GetCOntact();
                rearTrail.emitting = rearContact.GetCOntact();
            }
            else
            {
                frontTrail.emitting = false;
                rearTrail.emitting = false;
            }
        }

        void DebugInfo()
        {
            frontWheel.GetGroundHit(out WheelHit frontInfo);
            backWheel.GetGroundHit(out WheelHit backInfo);

            float backCoefficient = (backInfo.sidewaysSlip / backWheel.sidewaysFriction.extremumSlip);
            float frontCoefficient = (frontInfo.sidewaysSlip / frontWheel.sidewaysFriction.extremumSlip);

            //Debug.Log(" Back Coeficient = " + backCoefficient );
            //Debug.Log(" Front Coeficient = " + frontCoefficient);	
        }

        private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
        {
            Vector3 position;
            Quaternion rotation;
            wheelCollider.GetWorldPose(out position, out rotation);
            wheelTransform.rotation = rotation;
            wheelTransform.position = position;
        }

        void Speed_O_Meter()
        {
            currentSpeed = rb.linearVelocity.magnitude;
        }

        public void SetInstantMotorForce(float force)
        {
            motorForceInstant = force;
        }
    }
}