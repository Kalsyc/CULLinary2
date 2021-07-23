using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PlayerAction
{
    public delegate void PlayerMoveDelegate(Vector3 direction, float speed, bool isGrounded);
    public delegate void PlayerRunDelegate(Vector3 direction, float speed, bool isGrounded);
    public delegate void PlayerStopDelegate(Vector3 direction, bool isGrounded);
    public delegate void PlayerRotateDelegate(Vector3 direction, float speed);
    public delegate void PlayerRotateToLocationDelegate(Vector3 worldPosition, float speed);
    public delegate void PlayerJumpDelegate(Vector3 direction, bool isGrounded);
    public event PlayerMoveDelegate OnPlayerMove;
    public event PlayerRunDelegate OnPlayerRun;
    public event PlayerStopDelegate OnPlayerStop;
    public event PlayerRotateDelegate OnPlayerRotate;
    public event PlayerRotateToLocationDelegate OnPlayerInteract;
    public event PlayerJumpDelegate OnPlayerJump;

    [SerializeField] private float walkSpeed = 1.0f;
    [SerializeField] private float runSpeed = 2.0f;
    [SerializeField] private float turnSpeed = 8.0f;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundMask;
    private bool isGrounded = true;
    private KeyCode runKeyCode;
    private PlayerMelee playerMelee;
    private void Awake()
    {
        playerMelee = GetComponent<PlayerMelee>();
        runKeyCode = PlayerKeybinds.GetKeybind(KeybindAction.Run);
    }

    private void Update()
    {

        float moveVertical = Input.GetAxisRaw("Vertical");
        float moveHorizontal = Input.GetAxisRaw("Horizontal");

        Vector3 direction = new Vector3(moveHorizontal, 0.0f, moveVertical);
        float targetAngle = Mathf.Atan2(direction.normalized.x, direction.normalized.z) * Mathf.Rad2Deg;
        Vector3 moveDirection = Quaternion.Euler(0.0f, targetAngle, 0.0f) * Vector3.forward;

        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);

        bool isMeleeInvoked = playerMelee ? playerMelee.GetIsInvoking() : false;

        if (!isMeleeInvoked)
        {
            this.SetIsInvoking(true);

            if (Input.GetKey(KeyCode.Space))
            {
                OnPlayerJump?.Invoke(moveDirection.normalized, isGrounded);
            }

            if (direction == Vector3.zero)
            {
                OnPlayerStop?.Invoke(moveDirection.normalized, isGrounded);
            }
            else
            {
                if (Input.GetKey(runKeyCode))
                {
                    OnPlayerRun?.Invoke(moveDirection.normalized, runSpeed, isGrounded);
                }
                else
                {
                    OnPlayerMove?.Invoke(moveDirection.normalized, walkSpeed, isGrounded);
                }
            }

            if (direction != Vector3.zero)
            {
                OnPlayerRotate?.Invoke(direction.normalized, turnSpeed);
            }
        }
        else
        {
            this.SetIsInvoking(false);
        }


    }

}
