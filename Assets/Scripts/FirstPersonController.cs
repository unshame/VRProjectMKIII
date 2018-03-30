using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        public float walkSpeed;
        public MouseLook mouseLook;
        public LayerMask interactLayer;

        public Camera camera;
        private Vector2 input;
        private Vector3 moveDir = Vector3.zero;
        private CharacterController characterController;
        private CollisionFlags collisionFlags;

        private Interactible interactible;

        // Use this for initialization
        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            camera = Camera.main;
			mouseLook.Init(transform , camera.transform);
        }


        // Update is called once per frame
        private void Update()
        {
            RotateView();
        }


        private void FixedUpdate()
        {
            bool clicked;
            GetInput(out clicked);

            if (clicked && !interactible) {
                StartInteract();
            }
            else if(!clicked && interactible) {
                StopInteract();
            }
            
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward*input.y + transform.right*input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
                               characterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            moveDir.x = desiredMove.x * walkSpeed;
            moveDir.z = desiredMove.z * walkSpeed;

            if (!characterController.isGrounded) {
                moveDir += Physics.gravity * Time.fixedDeltaTime;
            }

            collisionFlags = characterController.Move(moveDir*Time.fixedDeltaTime);

            mouseLook.UpdateCursorLock();
        }

        private void StartInteract() {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100, interactLayer.value)) {
                Transform objectHit = hit.transform;

                interactible = objectHit.GetComponent<Interactible>();
                if (interactible) {
                    interactible.StartInteract(transform);
                }
            }
        }

        private void StopInteract() {
            interactible.StopInteract();
            interactible = null;
        }

        private void GetInput(out bool clicked)
        {
            // Read input
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            // set the desired speed to be walking or running
            input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (input.sqrMagnitude > 1)
            {
                input.Normalize();
            }

            clicked = Input.GetMouseButton(0);
        }


        private void RotateView()
        {
            mouseLook.LookRotation (transform, camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (collisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(characterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
