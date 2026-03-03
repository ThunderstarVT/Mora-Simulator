using Singletons;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public enum CameraMode
    {
        NONE,
        ORBITAL
    }
    
    [SerializeField] private CameraMode cameraMode = CameraMode.NONE;

    [Header("Orbital Camera Settings")]
    [SerializeField] private Transform orbitalTarget;
    [SerializeField, Min(0f)] private float orbitalRadialSpeed = 50f;
    [SerializeField, Min(0f)] private float orbitalRadialSmoothing = 50f;
    [SerializeField, Min(0f)] private float orbitalCollisionRadius = 0.5f;
    [SerializeField] private LayerMask orbitalCollisionLayerMask;
    [SerializeField, Min(0f)] private float orbitalMaxDistance = 10f;
    [SerializeField, Min(0f)] private float orbitalMinDistance = 1.0f;
    [SerializeField, Range(-90f, 90f)] private float orbitalMaxVertical = 80f;
    [SerializeField, Range(-90f, 90f)] private float orbitalMinVertical = -80f;
    
    private float orbitalTargetDistance;
    private float orbitalDistance;
    private float orbitalDistanceSmoothed;

    private Vector2 orbitalAngle;

    
    
    private InputSystem_Actions inputActions;
    
    
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void Start()
    {
        setInputActive(true); // temporary, will be managed by menu once that's made

        inputActions.Player.cameraMovement.performed += onCameraMovement;


        orbitalTargetDistance = (orbitalMaxDistance + orbitalMinDistance) * 0.5f;
        orbitalDistance = orbitalTargetDistance;
        orbitalDistanceSmoothed = orbitalDistance;
    }

    private void Update()
    {
        switch (cameraMode)
        {
            case CameraMode.ORBITAL:
            {
                transform.rotation = Quaternion.Euler(orbitalAngle.y, orbitalAngle.x, 0);
                
                Vector3 targetPosition = orbitalTarget.position - transform.forward * orbitalTargetDistance;

                RaycastHit hit;

                if (Physics.SphereCast(transform.position, orbitalCollisionRadius,
                        targetPosition - transform.position, out hit, 
                        Mathf.Min(orbitalRadialSpeed * Time.deltaTime, 
                            Mathf.Abs(orbitalTargetDistance - orbitalDistance)), 
                        orbitalCollisionLayerMask))
                {
                    orbitalDistance = Vector3.Distance(hit.point, orbitalTarget.position);
                }
                else
                {
                    float distDelta = orbitalRadialSpeed * Time.deltaTime;

                    if (Mathf.Abs(orbitalTargetDistance - orbitalDistance) < distDelta)
                    {
                        orbitalDistance = orbitalTargetDistance;
                    }
                    else
                    {
                        orbitalDistance += orbitalDistance > orbitalTargetDistance ? -distDelta : distDelta;
                    }
                }

                if (Physics.CheckSphere(orbitalTarget.position - transform.forward * orbitalDistance, 
                        orbitalCollisionRadius * 0.99f, orbitalCollisionLayerMask))
                {
                    if (Physics.SphereCast(orbitalTarget.position, orbitalCollisionRadius, 
                            targetPosition - orbitalTarget.position, out hit, orbitalTargetDistance, 
                            orbitalCollisionLayerMask))
                    {
                        orbitalDistance = hit.distance;
                    }
                }
                
                orbitalDistanceSmoothed = Mathf.Lerp(orbitalDistanceSmoothed, orbitalDistance, 
                    orbitalRadialSmoothing * Time.deltaTime);

                transform.position = orbitalTarget.position - transform.forward * orbitalDistanceSmoothed;
                
                break;
            }
        }
    }


    private void onCameraMovement(InputAction.CallbackContext context)
    {
        Vector3 rawInput = context.ReadValue<Vector3>();
        
        float horizontal = rawInput.x * SettingsManager.Instance.MouseSense.x;
        float vertical = rawInput.y * SettingsManager.Instance.MouseSense.y;
        float zoom = rawInput.z * SettingsManager.Instance.MouseSense.z;

        switch (cameraMode)
        {
            case CameraMode.ORBITAL:
            {
                orbitalAngle = new Vector2(orbitalAngle.x + horizontal * Time.deltaTime, 
                    Mathf.Clamp(orbitalAngle.y + vertical * Time.deltaTime, orbitalMinVertical, orbitalMaxVertical));
                
                orbitalTargetDistance = Mathf.Clamp(orbitalTargetDistance + zoom * Time.deltaTime, orbitalMinDistance, orbitalMaxDistance);
                
                break;
            }
        }
    }
    
    
    private void OnDestroy()
    {
        setInputActive(false);
    }
    
    
    public void setInputActive(bool active)
    {
        if (active) inputActions.Player.Enable();
        else inputActions.Player.Disable();
    }
    
    public void setCameraMode(CameraMode mode)
    {
        cameraMode = mode;
    }
}
