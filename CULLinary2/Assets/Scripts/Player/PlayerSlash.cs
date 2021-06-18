using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlash : PlayerAction
{
  private Animator animator;
  private PlayerMelee playerMelee;
  private PlayerLocomotion playerLocomotion;

  [SerializeField] private GameObject weapon;
  [SerializeField] private AudioSource audioSource;
  [SerializeField] private AudioClip[] attackSounds;
  [SerializeField] private GameObject playerBody;
  [SerializeField] private float rotateSpeed = 0.5f;

  private const float MAX_DIST_CAM_TO_GROUND = 100f;
  private const float MELEE_ANIMATION_TIME_SECONDS = 0.10f;

  private Collider weaponCollider;
  private Vector3 rotateToFaceDirection;

  private void Awake()
  {
    playerMelee = GetComponent<PlayerMelee>();
    playerMelee.OnPlayerMelee += Slash;

    weaponCollider = weapon.GetComponent<Collider>();
    weaponCollider.enabled = false;

    playerLocomotion = GetComponent<PlayerLocomotion>();
    animator = GetComponent<Animator>();
  }

  private IEnumerator RotatePlayer()
  {
    for (float i = 0.0f;
         i < MELEE_ANIMATION_TIME_SECONDS;
         i = i + Time.deltaTime * rotateSpeed)
    {
      playerLocomotion.Rotate(rotateToFaceDirection, rotateSpeed);
      yield return null;
    }
  }

  private void Slash()
  {
    RaycastHit hit;
    bool hitGround;

    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    hitGround = Physics.Raycast(ray, out hit, MAX_DIST_CAM_TO_GROUND, LayerMask.NameToLayer("Ground"));

    if (hitGround)
    {
      rotateToFaceDirection = new Vector3(hit.point.x - playerBody.transform.position.x,
          0.0f,
          hit.point.z - playerBody.transform.position.z);
      StartCoroutine(RotatePlayer());
      animator.SetBool("isMelee", true);
    }

  }

  private void OnDestroy()
  {
    playerMelee.OnPlayerMelee -= Slash;
  }

  public void AttackStart()
  {
    weaponCollider.enabled = true;
    audioSource.clip = attackSounds[Random.Range(0, attackSounds.Length)];
    audioSource.Play();
  }

  public void AttackEnd()
  {
    animator.SetBool("isMelee", false);
    weaponCollider.enabled = false;
    playerMelee.StopInvoking();
  }

}