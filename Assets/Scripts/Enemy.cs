using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
	[System.Flags]
	private enum AIType : byte
	{
		STATIC = 2,
		PATROL = 4,
		WANDER = 8,
		FOLLOW = 16,
		FLEE = 32
	}

	[SerializeField] private NavMeshAgent agent;
	[SerializeField] private CharacterMovements player;
	[SerializeField] private float maxDistance;
	private float sqrMaxDistance;
	[SerializeField] private float fieldOfView;
	[SerializeField] private AIType aiType;

	private Coroutine rotationCoroutine = null;

	public bool isStunned { get;  private set; }

	[SerializeField]
	private float currentHealth = 50f;
	[SerializeField]
	private Canvas worldCanvas;

	private void Reset()
	{
		agent = GetComponent<NavMeshAgent>();
	}

	// Start is called before the first frame update
	void Start()
	{
		sqrMaxDistance = maxDistance * maxDistance;
		if (GetBaseBehaviour() == AIType.WANDER)
		{
			agent.SetDestination(GetRandomPoint(20, 180, 5f));
		}

	}

	// Update is called once per frame
	void Update()
	{
		if (currentHealth <= 0f)
		{
			isStunned = true;
		}
		worldCanvas.gameObject.SetActive(false);
		if (isStunned)
			return;
		if (GetBaseBehaviour() == AIType.WANDER)
		{
			if (Vector3.Distance(agent.destination, transform.position) < 1.5f)
			{
				agent.SetDestination(GetRandomPoint(20, 180, 5f));
			}
		}
		else if (GetBaseBehaviour() == AIType.STATIC)
		{
			if (rotationCoroutine == null)
			{
				rotationCoroutine = StartCoroutine(RotateCoroutine(90f, 60f, 5f));
			}
		}
		Vector3 agentToPlayer = player.transform.position - transform.position;
		float sqrDistance = agentToPlayer.sqrMagnitude;
		if (sqrDistance <= sqrMaxDistance)
		{
			if (Vector3.Dot(transform.forward, agentToPlayer.normalized) > Mathf.Cos(fieldOfView * Mathf.Deg2Rad / 2f))
			{
				if (rotationCoroutine != null)
				{
					StopCoroutine(rotationCoroutine);
					rotationCoroutine = null;
				}
			}
		}
	}

	private Vector3 GetRandomPoint(int limit, float fieldOfSearch, float radius)
	{
		Vector3 randomPos = Random.insideUnitSphere * radius + transform.position;
		if (Vector3.Dot(transform.forward, (randomPos - transform.position).normalized) < Mathf.Cos(fieldOfSearch * Mathf.Deg2Rad / 2f))
		{
			if (limit == 0)
			{
				return GetRandomPoint(20, fieldOfSearch * 1.5f, radius);
			}
			Debug.Log("BREAK");
			return GetRandomPoint(--limit, fieldOfSearch, radius);
		}

		if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, radius, -1))
		{
			return hit.position;
		}

		if (limit == 0)
		{
			return GetRandomPoint(20, fieldOfSearch * 1.5f, radius);
		}
		return GetRandomPoint(--limit, fieldOfSearch, radius);
	}

	private AIType GetBaseBehaviour()
	{
		if ((aiType & AIType.STATIC) == AIType.STATIC)
		{
			return AIType.STATIC;
		}
		else if ((aiType & AIType.PATROL) == AIType.PATROL)
		{
			return AIType.PATROL;
		}

		return AIType.WANDER;
	}

	private AIType GetTriggeredBehaviour()
	{
		if ((aiType & AIType.FOLLOW) == AIType.FOLLOW)
		{
			return AIType.FOLLOW;
		}

		return AIType.FLEE;
	}

	private IEnumerator RotateCoroutine(float rotationDegrees, float rotationSpeed, float delayBetweenRotations)
	{
		yield return new WaitForSeconds(delayBetweenRotations);
		float currentRotationApplied = 0f;
		while (currentRotationApplied < rotationDegrees)
		{
			currentRotationApplied += rotationSpeed * Time.deltaTime;
			transform.Rotate(transform.up, rotationSpeed * Time.deltaTime);
			yield return null; //Wait for one frame to continue the while loop
		}

		rotationCoroutine = null;
	}

	public void ActiveWorldCanvas()
	{
		worldCanvas.gameObject.SetActive(true);
	}
}
