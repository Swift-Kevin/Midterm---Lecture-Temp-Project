using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class playerController : MonoBehaviour, IDamage, IAmmo
{
    [Header(" --- Components --- ")]
    [SerializeField] CharacterController controller;

    [Header(" --- Player Stats --- ")]
    [SerializeField, Range(0, 100)] float currentHP;
    [SerializeField, Range(3, 10)] float playerSpeed;
    [SerializeField, Range(8, 25)] float jumpHeight;
    [SerializeField, Range(10, 50)] float gravityValue;
    [SerializeField, Range(1, 50)] float sprintMod;
    [SerializeField, Range(1, 50)] float dashSpeed;
    [SerializeField, Range(1, 50)] float dashCooldown;
    [SerializeField, Range(1, 3)] int jumpMax;
    [SerializeField] float crouchHeight;

    [Header(" --- Gun Stats --- ")]
    [SerializeField] List<GunStats> gunList = new List<GunStats>();
    [SerializeField, Range(0.1f, 3)] float shootRate;
    [SerializeField, Range(0, 10)] float shootDamage;
    [SerializeField, Range(25, 1000)] float shootDistance;
    [SerializeField] GameObject hitEffect;
    [SerializeField] GameObject gunModel;
    [SerializeField] float zoomIn;
    [SerializeField] float zoomRate;

    [Header("----- Audio -----")]
    [SerializeField] AudioClip[] audJump;
    [SerializeField][Range(0, 1)] float audJumpVol;
    [SerializeField] AudioClip[] audDamage;
    [SerializeField][Range(0, 1)] float audDamageVol;
    [SerializeField] AudioClip[] audSteps;
    [SerializeField][Range(0, 1)] float audStepsVol;
    [SerializeField] AudioSource aud;

    private float originalHeight;
    private bool groundedPlayer;
    private int jumpTimes;
    private Vector3 playerVelocity;
    private Vector3 move;
    bool isShooting;
    bool isSprinting;
    bool stepsPlaying;
    bool jumpsPlaying;
    bool hurtsPlaying;
    bool isDashing;
    float playerMaxHP;
    int selectedGun;
    public Vector3 dashDir;
    float zoomOrig;

    private void Start()
    {
        originalHeight = controller.height;
        playerMaxHP = currentHP;
        UpdatePlayerUI();
        gameManager.instance.playerFlashUI.gameObject.SetActive(false);
        zoomOrig = Camera.main.fieldOfView;
    }

    void Update()
    {
        ZoomSight();

        if (gameManager.instance.activeMenu == null)
        {
            Movement();
            Crouch();

            if (gunList.Count > 0 && Input.GetButton("Shoot") && !isShooting)
            {
                ChangeGun();
                StartCoroutine(Shoot());
            }
        }
        Sprint();
    }

    void Movement()
    {
        if (!isDashing && Input.GetButtonDown("Dash"))
        {
            StartCoroutine(Dash());
        }

        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            jumpTimes = 0;
        }

        move = (transform.right * Input.GetAxis("Horizontal")) + (transform.forward * Input.GetAxis("Vertical"));
        controller.Move(move * Time.deltaTime * playerSpeed);

        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && jumpTimes < jumpMax)
        {
            playerVelocity.y = jumpHeight;
            ++jumpTimes;

            StartCoroutine(PlayJump());
        }

        if (groundedPlayer)
        {
            if (!stepsPlaying && move.normalized.magnitude > 0.5f)
            {
                StartCoroutine(PlaySteps());
            }

            if (playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
                jumpTimes = 0;
            }
        }

        playerVelocity.y -= gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    IEnumerator Dash()
    {
        isDashing = true;
        dashDir = Camera.main.transform.forward * dashSpeed;
        yield return new WaitForSeconds(dashCooldown);
        dashDir = Vector3.zero;
        isDashing = false;
    }


    void Crouch()
    {
        if (Input.GetButtonDown("Crouch"))
        {
            controller.height = crouchHeight;
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            controller.height = originalHeight;
        }

    }

    void ZoomSight()
    {
        if (Input.GetButton("Zoom"))
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, zoomIn, Time.deltaTime * zoomRate);
        }
        else
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, zoomOrig, Time.deltaTime * 8);
        }
    }

    void Sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
            playerSpeed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
            playerSpeed /= sprintMod;
        }
    }

    IEnumerator Shoot()
    {
        if (gunList[selectedGun].currentAmmo > 0)
        {
            isShooting = true;
            RaycastHit hit;

            gunList[selectedGun].currentAmmo--;
            UpdatePlayerUI();

            if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDistance))
            {
                IDamage damagable = hit.collider.GetComponent<IDamage>();
                if (damagable != null)
                {
                    damagable.TakeDamage(shootDamage);
                }
                Instantiate(gunList[selectedGun].hitEffect, hit.point, Quaternion.identity);
            }

            yield return new WaitForSeconds(shootRate);
            isShooting = false;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;

        StartCoroutine(PlayerFlashScreenDamage());
        if (!hurtsPlaying)
            StartCoroutine(PlayHurt());
        UpdatePlayerUI();

        if (currentHP <= 0)
            gameManager.instance.YouLose();
    }

    public void UpdatePlayerUI()
    {
        gameManager.instance.healthBar.fillAmount = currentHP / playerMaxHP;

        if (gunList.Count > 0)
        {
            gameManager.instance.currentAmmoText.text = gunList[selectedGun].currentAmmo.ToString();
            gameManager.instance.maxAmmoText.text = gunList[selectedGun].maxAmmo.ToString();
        }
    }

    IEnumerator PlayerFlashScreenDamage()
    {
        gameManager.instance.playerFlashUI.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerFlashUI.gameObject.SetActive(false);
    }

    IEnumerator PlaySteps()
    {
        stepsPlaying = true;
        aud.PlayOneShot(audSteps[Random.Range(0, audSteps.Length)], audStepsVol);
        if (isSprinting)
        {
            yield return new WaitForSeconds(.5f);
        }
        else if (!isSprinting)
        {
            yield return new WaitForSeconds(.5f);
        }
        stepsPlaying = false;
    }

    IEnumerator PlayHurt()
    {
        hurtsPlaying = true;
        aud.PlayOneShot(audDamage[Random.Range(0, audDamage.Length)], audDamageVol);
        yield return new WaitForSeconds(0.5f);
        hurtsPlaying = false;
    }

    IEnumerator PlayJump()
    {
        jumpsPlaying = true;
        aud.PlayOneShot(audJump[Random.Range(0, audJump.Length)], audJumpVol);
        yield return new WaitForSeconds(0.5f);
        jumpsPlaying = false;
    }

    public void SpawnPlayer()
    {
        controller.enabled = false;

        transform.position = gameManager.instance.playerSpawnPos.transform.position;
        currentHP = playerMaxHP;
        UpdatePlayerUI();

        controller.enabled = true;
    }

    public void GunPickup(GunStats gunToAdd)
    {
        gunList.Add(gunToAdd);
        ChangeGunStats();
        selectedGun = gunList.Count - 1;
    }

    void ChangeGun()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && selectedGun < gunList.Count - 1)
        {
            ++selectedGun;
            ChangeGunStats();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedGun > 0)
        {
            --selectedGun;
            ChangeGunStats();
        }

        UpdatePlayerUI();
    }

    void ChangeGunStats()
    {
        shootDamage = gunList[selectedGun].shootDamage;
        shootDistance = gunList[selectedGun].shootDist;
        shootRate = gunList[selectedGun].shootRate;
        gunModel.GetComponent<MeshFilter>().mesh = gunList[selectedGun].model.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().material = gunList[selectedGun].model.GetComponent<MeshRenderer>().sharedMaterial;
    }

    public void PickupAmmo(int amt, GameObject obj)
    {
        if (gunList.Count > 0)
        {
            int ammoDif = gunList[selectedGun].maxAmmo - gunList[selectedGun].currentAmmo;
            gunList[selectedGun].currentAmmo += amt;

            if (gunList[selectedGun].currentAmmo > gunList[selectedGun].maxAmmo)
                gunList[selectedGun].currentAmmo = gunList[selectedGun].maxAmmo;

            AmmoPickup pkup = obj.GetComponent<AmmoPickup>();

            pkup.ammoAmount -= ammoDif;

            if (pkup.ammoAmount <= 0)
                Destroy(pkup);
        }

        UpdatePlayerUI();
    }
}
