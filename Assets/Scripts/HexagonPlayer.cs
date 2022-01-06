using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (CapsuleCollider))]

public class HexagonPlayer : MonoBehaviour {
	
	public float speed = 5f;
	public float airVelocity = 8f;
	public float gravity = 10.0f;
	public float maxVelocityChange = 10.0f;
	public float jumpHeight = 2.0f;
	public float maxFallSpeed = 20.0f;
	public float rotateSpeed = 25f; //Speed the player rotate
	private Vector3 moveDir;
	[SerializeField] private GameObject cam;
	private Rigidbody rb;

	private float distToGround;

	private bool canMove = true; //If player is not hitted
	private bool isStuned = false;
	private bool wasStuned = false; //If player was stunned before get stunned another time
	private float pushForce;
	private Vector3 pushDir;

	public Vector3 checkPoint;
	private bool slide = false;

	[SerializeField] private PhotonView view;
	[SerializeField] private bool isDead = false;
	public Text playerNameText;
	public Animator anim;
	public Material[] playerColours;
	[SerializeField] private GameObject mainCam;
	[SerializeField] private List<GameObject> specList;

	public AudioSource audioSource;
	public int colour = 0;
	public GameObject psPrefab;

	void  Start (){
		if (view.IsMine)
		{
			cam.SetActive(true);

			playerNameText.text = PhotonNetwork.LocalPlayer.NickName;

			colour = Random.Range(0, 7);

			mainCam = GameObject.FindWithTag("PlayerCam");

			view.RPC("Colour", RpcTarget.AllBuffered, colour);
		}
		else
		{
			//Color nameColor = Random.ColorHSV();
			playerNameText.text = view.Owner.NickName;
			//playerNameText.color = nameColor;
		}

		// get the distance to ground
		//TODO: Doesn't work for some reason with new animated player model
		//distToGround = GetComponent<Collider>().bounds.extents.y;
		distToGround = 0.2f;
	}

	[PunRPC]
	void Colour(int transferredColor)
	{
		transform.GetChild(0).GetComponent<Renderer>().material = playerColours[transferredColor];
	}

	bool IsGrounded (){
		return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
	}
	
	void Awake () {
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
		rb.useGravity = false;

		checkPoint = transform.position;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Confined;
	}
	
	void FixedUpdate () {
		if (view.IsMine)
		{
			if (!isDead)
			{
				if(!IsGrounded())
				{
					anim.SetBool("isJumping", true);
				}
				else
				{
					anim.SetBool("isJumping", false);
				}

				if (canMove)
				{
					if (moveDir.x != 0 || moveDir.z != 0)
					{
						Vector3 targetDir = moveDir; //Direction of the character

						targetDir.y = 0;
						if (targetDir == Vector3.zero)
							targetDir = transform.forward;
						Quaternion tr = Quaternion.LookRotation(targetDir); //Rotation of the character to where it moves
						Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, Time.deltaTime * rotateSpeed); //Rotate the character little by little
						transform.rotation = targetRotation;

						anim.SetBool("isWalking", true);
						if (!audioSource.isPlaying)
							audioSource.Play();
					}
					else
					{
						anim.SetBool("isWalking", false);
						audioSource.Stop();
					}

					if (IsGrounded())
					{
						// Calculate how fast we should be moving
						Vector3 targetVelocity = moveDir;
						targetVelocity *= speed;

						// Apply a force that attempts to reach our target velocity
						Vector3 velocity = rb.velocity;
						if (targetVelocity.magnitude < velocity.magnitude) //If I'm slowing down the character
						{
							targetVelocity = velocity;
							rb.velocity /= 1.1f;
						}
						Vector3 velocityChange = (targetVelocity - velocity);
						velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
						velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
						velocityChange.y = 0;
						if (!slide)
						{
							if (Mathf.Abs(rb.velocity.magnitude) < speed * 1.0f)
								rb.AddForce(velocityChange, ForceMode.VelocityChange);
						}
						else if (Mathf.Abs(rb.velocity.magnitude) < speed * 1.0f)
						{
							rb.AddForce(moveDir * 0.15f, ForceMode.VelocityChange);
							//Debug.Log(rb.velocity.magnitude);
						}

						// Jump
						if (IsGrounded() && Input.GetButton("Jump"))
						{
							//anim.SetTrigger("Jumping");
							rb.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
						}
					}
					else
					{
						
						if (!slide)
						{

							Vector3 targetVelocity = new Vector3(moveDir.x * airVelocity, rb.velocity.y, moveDir.z * airVelocity);
							Vector3 velocity = rb.velocity;
							Vector3 velocityChange = (targetVelocity - velocity);
							velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
							velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
							rb.AddForce(velocityChange, ForceMode.VelocityChange);
							if (velocity.y < -maxFallSpeed)
								rb.velocity = new Vector3(velocity.x, -maxFallSpeed, velocity.z);
						}
						else if (Mathf.Abs(rb.velocity.magnitude) < speed * 1.0f)
						{
							
							rb.AddForce(moveDir * 0.15f, ForceMode.VelocityChange);
						}
						//anim.SetTrigger("Jumping");
					}
				}
				else
				{
					rb.velocity = pushDir * pushForce;
				}
				// We apply gravity manually for more tuning control
				rb.AddForce(new Vector3(0, -gravity * GetComponent<Rigidbody>().mass, 0));
			}
		}
	}


	private void Update()
	{
		if (view.IsMine)
		{
			if (!isDead)
			{
				float h = Input.GetAxis("Horizontal");
				float v = Input.GetAxis("Vertical");

				Vector3 v2 = v * cam.transform.forward; //Vertical axis to which I want to move with respect to the camera
				Vector3 h2 = h * cam.transform.right; //Horizontal axis to which I want to move with respect to the camera
				moveDir = (v2 + h2).normalized; //Global position to which I want to move in magnitude 1

				RaycastHit hit;
				if (Physics.Raycast(transform.position, -Vector3.up, out hit, distToGround + 0.1f))
				{
					if (hit.transform.tag == "Slide")
					{
						slide = true;
					}
					else
					{
						slide = false;
					}
				}
			}
		}
	}

	float CalculateJumpVerticalSpeed () {
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * jumpHeight * gravity);
	}

	public void HitPlayer(Vector3 velocityF, float time)
	{
		rb.velocity = velocityF;

		pushForce = velocityF.magnitude;
		pushDir = Vector3.Normalize(velocityF);
		StartCoroutine(Decrease(velocityF.magnitude, time));
	}

	public void LoadCheckPoint()
	{
		transform.position = checkPoint;
	}

	private IEnumerator Decrease(float value, float duration)
	{
		if (isStuned)
			wasStuned = true;
		isStuned = true;
		canMove = false;

		float delta = 0;
		delta = value / duration;

		for (float t = 0; t < duration; t += Time.deltaTime)
		{
			yield return null;
			if (!slide) //Reduce the force if the ground isnt slide
			{
				pushForce = pushForce - Time.deltaTime * delta;
				pushForce = pushForce < 0 ? 0 : pushForce;
				//Debug.Log(pushForce);
			}
			rb.AddForce(new Vector3(0, -gravity * GetComponent<Rigidbody>().mass, 0)); //Add gravity
		}

		if (wasStuned)
		{
			wasStuned = false;
		}
		else
		{
			isStuned = false;
			canMove = true;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (view.IsMine)
		{
			if (collision.gameObject.name.Equals("Lava"))
			{
				PhotonNetwork.Instantiate(psPrefab.name, transform.position, Quaternion.identity);
				SoundManager.soundInstance.audioSource.PlayOneShot(SoundManager.soundInstance.burn);
				isDead = true;
				rb.velocity = new Vector3(0, 0, 0);
				anim.SetBool("isWalking", false);
				anim.SetBool("isJumping", false);
				audioSource.Stop();

				cam.SetActive(false);

				mainCam.transform.GetChild(0).gameObject.SetActive(true);
				mainCam.transform.GetChild(0).rotation = new Quaternion(0f, 0f, 0f, 0f);

				view.RPC("Die", RpcTarget.AllBuffered);
			}
		}
	}

	public void RevivePlayer()
	{
		if (view.IsMine)
		{
			cam.SetActive(true);

			mainCam.transform.GetChild(0).gameObject.SetActive(false);

			mainCam.transform.GetChild(0).position = new Vector3(0, 4.5f, -12.5f);
			mainCam.transform.GetChild(0).rotation = new Quaternion(0f, 0f, 0f, 0f);

			isDead = false;

			view.RPC("Revive", RpcTarget.AllBuffered);
		}
	}

	[PunRPC]
	private void Die()
	{
		//playerNameText.enabled = false;
		gameObject.transform.position = new Vector3(Random.Range(-5f, 5f), 80f, Random.Range(-5f, 5f));

		GameObject map = GameObject.FindWithTag("Map");
		map.GetComponent<HexagonGameManager>().RemovePlayerFromAliveList(gameObject);
		//specList = map.GetComponent<HexagonGameManager>().aliveList;

		//if(specList.Count > 0)
		//	cam.GetComponent<HexagonCamera>().SetTarget(specList[0].transform);
	}

	[PunRPC]
	private void Revive()
	{
		//playerNameText.enabled = true;
		//cam.GetComponent<HexagonCamera>().SetTarget(gameObject.transform);
	}
}
