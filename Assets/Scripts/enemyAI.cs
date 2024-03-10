using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("Components")]
    [SerializeField] Renderer model;
    [SerializeField] Animator animator;
    [SerializeField] Collider weaponCollider;
    [SerializeField] NavMeshAgent meshAgent;
    [SerializeField] Transform headPos;
    [SerializeField] Transform shootPos;

    [Header("Enemy Stats")]
    [SerializeField, Range(1, 10)] float HP;
    [SerializeField] float enemySpeed;
    [SerializeField] int playerFaceSpeed;
    [SerializeField] int viewConeAngle;
    [SerializeField, Range(1, 100)] int roamDistance;
    [SerializeField, Range(0, 10)] float roamTimer;

    [Header("Enemy Stats")]
    [SerializeField] float shootRate;
    [SerializeField] GameObject bulletPrefab;

    public bool playerInRange;
    Vector3 playerDir;
    Vector3 startingPos;
    float stoppingDistanceOriginal;
    float angleToPlayer;
    bool isShooting;
    bool destinationChosen;

    private void Start()
    {
        startingPos = transform.position;
        stoppingDistanceOriginal = meshAgent.stoppingDistance;
    }

    private void Update()
    {
        if (meshAgent.isActiveAndEnabled)
        {
            animator.SetFloat("Speed", meshAgent.velocity.normalized.magnitude);

            if (playerInRange && !CanSeePlayer())
                StartCoroutine(Roam());
            else if (meshAgent.destination != GameManager.instance.player.transform.position)
                StartCoroutine(Roam());
        }
    }

    IEnumerator Shoot()
    {
        isShooting = true;
        animator.SetTrigger("Shoot");


        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    public void CreateBullet()
    {
        Instantiate(bulletPrefab, shootPos.position, transform.rotation);
    }
    public void WeaponColliderOn()
    {
        weaponCollider.enabled = true;
    }
    public void WeaponColliderOff()
    {
        weaponCollider.enabled = false;
    }

    IEnumerator Roam()
    {
        if (!destinationChosen && meshAgent.remainingDistance < 0.05f)
        {
            destinationChosen = true;
            meshAgent.stoppingDistance = 0;

            yield return new WaitForSeconds(roamTimer);

            Vector3 randomPos = Random.insideUnitSphere * roamDistance;
            randomPos += startingPos;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomPos, out hit, roamDistance, 1);
            meshAgent.SetDestination(hit.position);

            destinationChosen = false;
        }
    }

    bool CanSeePlayer()
    {
        meshAgent.stoppingDistance = stoppingDistanceOriginal;

        playerDir = GameManager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(new Vector3(playerDir.x, 0, playerDir.z), transform.forward);

        Debug.DrawRay(headPos.position, playerDir);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= viewConeAngle)
            {
                meshAgent.SetDestination(GameManager.instance.player.transform.position);
                if (meshAgent.remainingDistance <= meshAgent.stoppingDistance)
                    FacePlayer();
                if (!isShooting)
                {
                    StartCoroutine(Shoot());
                }

                return true;
            }
        }

        meshAgent.stoppingDistance = 0;
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    void FacePlayer()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * playerFaceSpeed);
    }
    public void TakeDamage(float dmg)
    {
        HP -= dmg;
        WeaponColliderOff();

        if (HP <= 0)
        {
            StopAllCoroutines();
            GameManager.instance.UpdateGameGoal(-1);
            animator.SetBool("Dead", true);
            meshAgent.enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;
        }
        else
        {
            meshAgent.SetDestination(GameManager.instance.player.transform.position);
            StartCoroutine(FlashColor());
            // If you find a damage animation, call it here.
        }
    }
    IEnumerator FlashColor()
    {
        model.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);
        model.material.color = Color.white;
    }

}
