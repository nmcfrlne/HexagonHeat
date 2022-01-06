//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class LobbyStart : MonoBehaviour
//{
//    public List<GameObject> playerList;
//    public HexagonGameManager gameManager;
//    private int playersRequired = 2;

//    private float minX = -5f;
//    private float maxX = 5f;
//    private float minY = -5f;
//    private float maxY = 5f;

//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if(playerList.Count == playersRequired)
//        {
//            gameManager.SetGamePlayers(playerList);

//            foreach (GameObject player in playerList)
//            {
//                Vector3 randomPos = new Vector3(Random.Range(minX, maxX), 1.2f, Random.Range(minY, maxY));
//                player.transform.position = randomPos;
//            }

//            gameManager.startGame = true;

//            playerList.Clear();
//        }
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.gameObject.tag == "Player")
//        {
//            playerList.Add(other.gameObject);
//        }
//    }
//}
