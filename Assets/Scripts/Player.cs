//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Photon.Pun;
//using UnityEngine.UI;

//public class Player : MonoBehaviour
//{
//    private float speed = 6f;
//    [SerializeField] private bool isDead = false;
//    [SerializeField] private PhotonView view;
//    [SerializeField] private Transform playerCam;
//    [SerializeField] private GameObject mainCam;
//    public Text playerNameText;
//    public Rigidbody rb;

//    public GameObject GroundCollider;
//    private float radius;
//    [SerializeField] private bool isGrounded;
//    public LayerMask groundMask;

//    public float jumpForce = 10f;



//    // Start is called before the first frame update
//    void Start()
//    {
//        if (view.IsMine)
//        {
//            playerCam = gameObject.transform.Find("Camera");
//            playerCam.gameObject.SetActive(true);
//            radius = GroundCollider.GetComponent<SphereCollider>().radius;
//            playerNameText.text = PhotonNetwork.LocalPlayer.NickName;
//            mainCam = GameObject.FindGameObjectWithTag("MainCamera");
//            mainCam.SetActive(false);
//        }
//        else
//        {
//            Color nameColor = Random.ColorHSV();
//            playerNameText.text = view.Owner.NickName;
//            playerNameText.color = nameColor;
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (view.IsMine)
//        {
//            if (!isDead)
//            {
//                isGrounded = Physics.CheckSphere(GroundCollider.transform.position, radius, groundMask);
//                Collider[] platformCheck = Physics.OverlapSphere(GroundCollider.transform.position, radius, groundMask);

//                Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
//                transform.position += input.normalized * speed * Time.deltaTime;

//                if ((isGrounded && Input.GetKeyDown(KeyCode.Space)))
//                {
//                    //isJumping = true;
//                    //airTimeCounter = airTime;
//                    rb.velocity = Vector2.up * jumpForce;
                    
//                    //anim.SetTrigger("Jumping");
//                    //jumpCount = 1;
//                    //dashAble = true;
//                }

//                if(isGrounded)
//                    gameObject.transform.parent = platformCheck[0].gameObject.transform;
//                else
//                    gameObject.transform.parent = null;
//            }
//            else
//            {
//                //Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
//                //playerCamera.position += input.normalized * speed * Time.deltaTime;
//                playerCam.gameObject.SetActive(false);
//                mainCam.SetActive(true);
//            }
//        }   
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        if (view.IsMine)
//        {
//            if (collision.gameObject.name.Contains("Hexagon"))
//            {
//                //transform.parent = collision.transform;
//                //view.RPC("ParentTransformOn", RpcTarget.AllBuffered, collision.gameObject.name);
//            }

//            if (collision.gameObject.name.Equals("Lava"))
//            {
//                isDead = true;
//                view.RPC("Die", RpcTarget.AllBuffered);
//            }
//        }

//    }

//    private void OnCollisionExit(Collision collision)
//    {
//        if (view.IsMine)
//        {
//            if (collision.gameObject.name.Contains("Hexagon"))
//            {
//                //view.RPC("ParentTransformOff", RpcTarget.AllBuffered);
//                //transform.parent = null;
//            }
//        }
//    }

//    //[PunRPC]
//    //private void ParentTransformOn(string collision)
//    //{
//    //    GameObject platform = GameObject.Find(collision);
//    //    gameObject.transform.parent = platform.transform;
//    //}

//    //[PunRPC]
//    //private void ParentTransformOff()
//    //{
//    //    gameObject.transform.parent = null;
//    //}

//    [PunRPC]
//    private void Die()
//    {
//        GetComponent<MeshRenderer>().enabled = false;
//        GetComponent<CapsuleCollider>().enabled = false;
//        playerNameText.enabled = false;
         
//        GameObject map = GameObject.FindWithTag("Map");
//        map.GetComponent<HexagonGameManager>().numberOfPlayersAlive--;
//    }
//}
