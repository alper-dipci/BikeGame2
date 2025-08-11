using NaughtyAttributes;
using rayzngames;
using Scriptables;

namespace DefaultNamespace
{
    using System.Collections;
    using UnityEngine;
    using DG.Tweening;

    namespace rayzngames
    {
        public class BicycleVehicleSingle : MonoBehaviour
        {
            [SerializeField] Rigidbody rb;

            [Header("Power/Braking")] [SerializeField]
            float motorForce;

            [SerializeField] private float motorForceInstant = 70f;
            [SerializeField] private float pedalInterval = 0.4f;

            [SerializeField] float brakeForce;
            public Vector3 COG;

            [Header("Steering")] [SerializeField] float maxSteeringAngle;
            [Range(0f, 1f)] [SerializeField] float steerReductorAmmount;
            [Range(0.001f, 1f)] [SerializeField] float turnSmoothing;

            [Header("Lean")] [SerializeField] float maxLeanAngle = 45f;
            [Range(0.001f, 1f)] [SerializeField] float leanSmoothing;

            float targetLeanAngle;

            [Header("Object References")] public Transform handle;
            public Transform pedalPivot;

            [SerializeField] WheelCollider frontWheel;
            [SerializeField] WheelCollider backWheel;
            [SerializeField] Transform frontWheelTransform;
            [SerializeField] Transform backWheelTransform;
            [SerializeField] TrailRenderer frontTrail;
            [SerializeField] TrailRenderer rearTrail;

            ContactProvider frontContact;
            ContactProvider rearContact;

            [Header("Info")] [SerializeField] float currentSteeringAngle;
            [SerializeField] float current_maxSteeringAngle;
            [Range(-45, 45)] public float currentLeanAngle;

            [Header("Speed M/s")] [SerializeField] public float currentSpeed;
            
            [Header("PowerUps")]
            public float jumpForce = 500f;
            

            private float pedalIntervalTimer;
            private Tweener pedalTween;

            private float horizontalInput;

            void Start()
            {
                frontContact = frontTrail.transform.GetChild(0).GetComponent<ContactProvider>();
                rearContact = rearTrail.transform.GetChild(0).GetComponent<ContactProvider>();

                frontWheel.ConfigureVehicleSubsteps(5, 12, 15);
                backWheel.ConfigureVehicleSubsteps(5, 12, 15);

                if (GameManager.Instance != null)
                    GameManager.Instance.OnPlayerFailed += ResetToLastCheckPoint;
            }
            
            [Button]
            public void DefaultWheelFriction()
            {
                var defaultProfile = AssetManager.Instance.wheelDb.DefaultFrictionProfile;
                UpdateWheelFrictions(defaultProfile);
            }
            [Button]
            public void IceWheelFriction()
            {
                var iceProfile = AssetManager.Instance.wheelDb.IceFrictionProfile;
                UpdateWheelFrictions(iceProfile);
            }
            
            private void UpdateWheelFrictions(WheelFrictionProfile profile)
            {
                var frontForwardFriction = profile.GetWheelFrictionCurve(WheelType.FrontForward);
                var frontSidewaysFriction = profile.GetWheelFrictionCurve(WheelType.FrontSideways);
                var backForwardFriction = profile.GetWheelFrictionCurve(WheelType.BackForward);
                var backSidewaysFriction = profile.GetWheelFrictionCurve(WheelType.BackSideways);
                frontWheel.sidewaysFriction = frontSidewaysFriction;
                frontWheel.forwardFriction = frontForwardFriction;
                backWheel.sidewaysFriction = backSidewaysFriction;
                backWheel.forwardFriction = backForwardFriction;
            }
            
            private void OnDisable()
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.OnPlayerFailed -= ResetToLastCheckPoint;

                if (pedalTween != null && pedalTween.IsActive())
                    pedalTween.Kill();
            }

            void Update()
            {
                ListenInputForTest();
            }

            private void ListenInputForTest()
            {
                horizontalInput = Input.GetAxis("Horizontal");
                if (Input.GetKey(KeyCode.Space))
                    SetMotorTorque();
            }

            void FixedUpdate()
            {
                EmitTrail();
                UpdateHandles();
                UpdateWheels();
                Speed_O_Meter();
                HandleSteering();

                HandleEngineNew();
                LeanOnTurn();
            }

            private void SetMotorTorque()
            {
                if (pedalIntervalTimer > 0)
                    return;

                backWheel.motorTorque = motorForceInstant;
                pedalIntervalTimer = pedalInterval;

                if (pedalTween != null && pedalTween.IsActive())
                    pedalTween.Kill();

                pedalTween = pedalPivot
                    .DOLocalRotate(GetPedalRotationBySpeed(), pedalInterval + 0.1f, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.Linear);
            }

            private Vector3 GetPedalRotationBySpeed()
            {
                if (currentSpeed < 5) return new Vector3(-90, 0, 0);
                if (currentSpeed < 10) return new Vector3(-180, 0, 0);
                if (currentSpeed < 15) return new Vector3(-360, 0, 0);
                if (currentSpeed < 20) return new Vector3(-540, 0, 0);
                return new Vector3(-720, 0, 0);
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
            }

            public void ApplyBraking(float brakeForce)
            {
                frontWheel.brakeTorque = brakeForce;
                backWheel.brakeTorque = brakeForce;
            }

            void MaxSteeringReductor()
            {
                float t = (rb.linearVelocity.magnitude / 30f) * steerReductorAmmount;
                t = Mathf.Clamp01(t);
                current_maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 5f, t);
            }

            public void HandleSteering()
            {
                MaxSteeringReductor();

                currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, current_maxSteeringAngle * horizontalInput,
                    turnSmoothing * 0.1f);
                frontWheel.steerAngle = currentSteeringAngle;

                targetLeanAngle = maxLeanAngle * -horizontalInput;
            }

            public void UpdateHandles()
            {
                Vector3 euler = handle.localEulerAngles;
                handle.localEulerAngles = new Vector3(euler.x, currentSteeringAngle, euler.z);
            }

            private void LeanOnTurn()
            {
                Vector3 currentRot = transform.rotation.eulerAngles;

                if (rb.linearVelocity.magnitude < 1f)
                {
                    currentLeanAngle = Mathf.LerpAngle(currentLeanAngle, 0f, 0.1f);
                    transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, currentLeanAngle);
                    return;
                }

                if (Mathf.Abs(currentSteeringAngle) < 0.5f)
                {
                    currentLeanAngle = Mathf.LerpAngle(currentLeanAngle, 0f, leanSmoothing * 0.1f);
                }
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

            private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
            {
                wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
                wheelTransform.position = pos;
                wheelTransform.rotation = rot;
            }

            private void EmitTrail()
            {
                bool braking = false; // burada default false, istersen ayrıca input ekle
                if (braking)
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

            public void ResetToLastCheckPoint()
            {
                StartCoroutine(ResetToLastCheckPointCoroutine());
            }

            private IEnumerator ResetToLastCheckPointCoroutine()
            {
                yield return new WaitForSeconds(0.4f);
                var lastCheckPoint = CheckPointManager.Instance.LastCheckPointPosition;
                transform.position = lastCheckPoint;
                transform.rotation = Quaternion.identity;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
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
}