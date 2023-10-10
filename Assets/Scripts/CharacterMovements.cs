using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovements : MonoBehaviour
{
	[SerializeField]
	private float moveSpeed = 5f;
	[SerializeField]
	private Rigidbody rb;
	[SerializeField]
	private GameObject cameraLookAt;
	[SerializeField]
	private float VerticalMinAngle = 90f, VerticalMaxAngle = 90f;
	[SerializeField]
	private Camera mainCamera;
	[SerializeField]
	private ParticleSystem shootPS;
	[SerializeField]
	private Cinemachine.CinemachineVirtualCamera executeCamera;
	[SerializeField]
	private Animator characterAnimator;

	Vector2 inputs = Vector2.zero;

	private float shootTimer = 0f;

	private bool isExecuting = false;
	private void Reset()
	{
		rb = GetComponent<Rigidbody>();
		rb.constraints = RigidbodyConstraints.FreezeRotation;
	}


	// Start is called before the first frame update
	void Start()
	{
		Application.targetFrameRate = 60;
	}

	private void Update()
	{
		shootTimer += Time.deltaTime * 7f;
		if (isExecuting)
			return;

		if (Input.GetMouseButton(0))
		{
			if (shootTimer >= 1f)
			{
				Shoot();
				shootTimer = 0f;
			}
		}
		characterAnimator.SetFloat("MoveY", Input.GetAxis("Vertical"));

	}

	private void LateUpdate()
	{
		if (isExecuting)
			return;

		Enemy en = GetClosestStunnedEnemy();
		if (en != null)
		{
			en.ActiveWorldCanvas();
			if (Input.GetKeyDown(KeyCode.E))
			{
				StartCoroutine(ExecuteCoroutine(en));
			}
		}
	}

	void FixedUpdate()
	{
		if (isExecuting)
			return;
		//transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X") * Time.deltaTime * 500f, 0f));
		Vector3 rot = transform.localRotation.eulerAngles;
		rot.y += Input.GetAxis("Mouse X") * Time.fixedDeltaTime * 500f;
		transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(rot), 1f);


		rot = cameraLookAt.transform.localRotation.eulerAngles;
		rot.x += Input.GetAxis("Mouse Y") * Time.fixedDeltaTime * 500f;
		if (rot.x > 180f)
			rot.x -= 360f;
		rot.x = Mathf.Clamp(rot.x, VerticalMinAngle, VerticalMaxAngle);

		cameraLookAt.transform.localRotation = Quaternion.Slerp(cameraLookAt.transform.localRotation, Quaternion.Euler(rot), 1f);
		Vector3 moveXZ = transform.rotation * Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")), 1f) * moveSpeed;
		rb.velocity = new Vector3(moveXZ.x, rb.velocity.y, moveXZ.z);

	}

	private void Shoot()
	{
		Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
		if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~(1 << LayerMask.NameToLayer("Player"))))
		{
			ParticleSystem ps = Instantiate(shootPS, hit.point, Quaternion.LookRotation(hit.normal));
		}
	}

	private Enemy GetClosestStunnedEnemy()
	{
		//METHODE 1
		Collider[] colliders = Physics.OverlapSphere(transform.position, 2f);
		IEnumerable<Enemy> enemies = colliders.Where(o => o.GetComponent<Enemy>() != null).Select(o => o.GetComponent<Enemy>());

		//METHODE 2
		//IEnumerable<Enemy> enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None)
		//	.Where(o => Vector3.Distance(o.transform.position, transform.position) <= 2f);

		if (enemies.Count() == 0)
			return null;

		return enemies.Where(o => o.isStunned == true).OrderBy(o => Vector3.Distance(o.transform.position, transform.position)).FirstOrDefault();
	}

	private IEnumerator ExecuteCoroutine(Enemy enemy)
	{
		isExecuting = true;
		executeCamera.Follow = enemy.transform;
		executeCamera.LookAt = enemy.transform;
		executeCamera.gameObject.SetActive(true);
		yield return new WaitForSeconds(3f);
		executeCamera.gameObject.SetActive(false);
		yield return new WaitForSeconds(2f);
		Destroy(enemy.gameObject);
		isExecuting = false;
	}
}
