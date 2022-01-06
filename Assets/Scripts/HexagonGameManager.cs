using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class HexagonGameManager : MonoBehaviour
{
    public GameObject BlueHexagon;
    public GameObject CyanHexagon;
    public GameObject GreenHexagon;
    public GameObject PinkHexagon;
    public GameObject RedHexagon;
    public GameObject WhiteHexagon;
    public GameObject YellowHexagon;
    public GameObject Indicator;
    public GameObject playerPrefab;

    public ParticleSystem ps;
    private ParticleSystem.MainModule psSettings;
    private Color psColor;

    [SerializeField] private PhotonView view;

    public bool startGame = false;
    public bool firstGame = true;

    [SerializeField] private float timeLeft = 0f;
    [SerializeField] private float startTimer = 5f;
    [SerializeField] private float speedMultiplyer = 0.6f;
    [SerializeField] private int choice = 69;
    [SerializeField] private int tempChoice = 69;

    private float minX = -5f;
    private float maxX = 5f;
    private float minY = -5f;
    private float maxY = 5f;

    public bool showPause = false;
    public GameObject pauseMenu;
    public GameObject restartButton;
    public Text winnerText;

    public List<GameObject> playerList;
    public List<GameObject> aliveList;
    public List<int> userIdList;

    //public bool musicMuted = false;
    public GameObject muteButton;
    public GameObject unmuteButton;

    public int requiredPlayers = 99;
    public InputField requiredPlayersInput;
    public GameObject masterMenu;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 randomPos = new Vector3(Random.Range(minX, maxX), 80f, Random.Range(minY, maxY));
        PhotonNetwork.Instantiate(playerPrefab.name, randomPos, Quaternion.identity);

        psSettings = ps.main;

        if (PhotonNetwork.IsMasterClient)
        {
            Cursor.visible = true;
            masterMenu.SetActive(true);
        }
    }

    private void Update()
    {
        if (showPause && Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            pauseMenu.SetActive(false);
            showPause = false;
        }
        else if (!showPause && Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            pauseMenu.SetActive(true);
            showPause = true;
        }

        if (!startGame && !firstGame)
        {
            restartButton.SetActive(true);
        }
        else
        {
            restartButton.SetActive(false);
        }
    }

    public void LeaveRoom()
    {
        //GameObject leavePlayer = new GameObject();

        //foreach (GameObject player in gamePlayers)
        //{
        //    Debug.Log(player.GetComponent<PhotonView>().AmOwner);
        //    if (player.GetComponent<PhotonView>().AmOwner == true)
        //    {
        //        leavePlayer = player;
        //    }
        //}

        //Debug.Log(view.ViewID);
        
        //playersAlive.Remove(leavePlayer);
        //gamePlayers.Remove(leavePlayer);

        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Lobby");
    }

    public void SetRequiredPlayers()
    {
        int value;
        bool isNumber = int.TryParse(requiredPlayersInput.text, out value);

        if(isNumber)
        {
            if (value < 9)
            {
                requiredPlayers = value;
                masterMenu.SetActive(false);
                Cursor.visible = false;
            }
            else
            {
                Debug.LogError("cmon bruh");
            }
        }
        else
        {
            Debug.LogError("cmon bruh");
        }
    }

    public void RestartGame()
    {
        view.RPC("Restart", RpcTarget.AllBuffered);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void MuteMusic()
    {
        BackgroundMusic.musicInstace.audioSource.volume = 0;
        muteButton.SetActive(false);
        unmuteButton.SetActive(true);
    }

    public void UnmuteMusic()
    {
        BackgroundMusic.musicInstace.audioSource.volume = 0.01f;
        muteButton.SetActive(true);
        unmuteButton.SetActive(false);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.LogError(aliveList.Count);
        //Debug.Log(startTimer);

        if (startGame)
        {   
            if(timeLeft > -1f && timeLeft < 0)
            {
                if(aliveList.Count == 1)
                {
                    SoundManager.soundInstance.audioSource.PlayOneShot(SoundManager.soundInstance.success);

                    for(int i =0;i<aliveList.Count;i++)
                    {
                        if(aliveList[i]!=null)
                        {
                            winnerText.text = "Winner is " + aliveList[i].GetComponent<HexagonPlayer>().playerNameText.text + "!";
                        }
                    }
                    
                    winnerText.gameObject.SetActive(true);

                    startGame = false;
                    return;
                }

                if (aliveList.Count == 0)
                {
                    SoundManager.soundInstance.audioSource.PlayOneShot(SoundManager.soundInstance.failure);
                    winnerText.text = "LOSERS!";
                    winnerText.gameObject.SetActive(true);

                    startGame = false;
                    return;
                }
            }

            timeLeft -= Time.deltaTime * speedMultiplyer;
            
            if (timeLeft < -1f)
            {
                if(speedMultiplyer<4.8)
                    speedMultiplyer += 0.3f;

                timeLeft = 15f;
                if (PhotonNetwork.IsMasterClient)
                {
                    //Choose colour and set indicator matieral and play particle effect
                    view.RPC("Choose", RpcTarget.AllBuffered, Random.Range(0, 7));
                }

                view.RPC("Move", RpcTarget.AllBuffered, 2);
            }


            if (timeLeft > 6 && timeLeft < 10)
            {
                view.RPC("Move", RpcTarget.AllBuffered, 0);
            }
            else if (timeLeft > 0 && timeLeft < 4)
            {
                view.RPC("Move", RpcTarget.AllBuffered, 1);
            }
        }
        else
        {
            if (firstGame)
            {
                if (requiredPlayers == PhotonNetwork.PlayerList.Length)
                {
                    view.RPC("GetPlayers", RpcTarget.AllBuffered);

                    startTimer -= Time.deltaTime;

                    if (startTimer < 0f)
                    {
                        view.RPC("StartGame", RpcTarget.AllBuffered);
                    }
                }
            }
        }
    }

    [PunRPC]
    private void Choose(int index)
    {
        tempChoice = index;
        choice = tempChoice;

        if (choice == 0)
        {
            Material newMat = Resources.Load("Blue", typeof(Material)) as Material;
            Indicator.GetComponent<Renderer>().material = newMat;
            psColor = Color.blue;
            psSettings.startColor = new ParticleSystem.MinMaxGradient(psColor);
        }
        //Cyan
        else if (choice == 1)
        {
            Material newMat = Resources.Load("Cyan", typeof(Material)) as Material;
            Indicator.GetComponent<Renderer>().material = newMat;
            psColor = Color.cyan;
            psSettings.startColor = new ParticleSystem.MinMaxGradient(psColor);
        }
        //Green
        else if (choice == 2)
        {
            Material newMat = Resources.Load("Green", typeof(Material)) as Material;
            Indicator.GetComponent<Renderer>().material = newMat;
            psColor = Color.green;
            psSettings.startColor = new ParticleSystem.MinMaxGradient(psColor);
        }
        //Pink
        else if (choice == 3)
        {
            Material newMat = Resources.Load("Pink", typeof(Material)) as Material;
            Indicator.GetComponent<Renderer>().material = newMat;
            psColor = Color.magenta;
            psSettings.startColor = new ParticleSystem.MinMaxGradient(psColor);
        }
        //Red
        else if (choice == 4)
        {
            Material newMat = Resources.Load("Red", typeof(Material)) as Material;
            Indicator.GetComponent<Renderer>().material = newMat;
            psColor = Color.red;
            psSettings.startColor = new ParticleSystem.MinMaxGradient(psColor);
        }
        //White
        else if (choice == 5)
        {
            Material newMat = Resources.Load("White", typeof(Material)) as Material;
            Indicator.GetComponent<Renderer>().material = newMat;
            psColor = Color.white;
            psSettings.startColor = new ParticleSystem.MinMaxGradient(psColor);
        }
        //Yellow
        else if (choice == 6)
        {
            Material newMat = Resources.Load("Yellow", typeof(Material)) as Material;
            Indicator.GetComponent<Renderer>().material = newMat;
            psColor = Color.yellow;
            psSettings.startColor = new ParticleSystem.MinMaxGradient(psColor);
        }

        ps.Play();
    }

    public void RemovePlayerFromAliveList(GameObject pPlayer)
    {
        aliveList.Remove(pPlayer);
    }

    [PunRPC]
    private void Restart()
    {
        timeLeft = 0f;
        speedMultiplyer = 0.3f;
        playerList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));

        aliveList = new List<GameObject>(playerList);

        foreach (GameObject player in playerList)
        {
            player.GetComponent<HexagonPlayer>().RevivePlayer();

            Vector3 randomPos = new Vector3(Random.Range(minX, maxX), 2f, Random.Range(minY, maxY));
            player.transform.position = randomPos;

            player.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        }

        winnerText.gameObject.SetActive(false);

        startTimer = 5f;
        startGame = true;
    }

    [PunRPC]
    private void StartGame()
    {
        foreach (GameObject player in playerList)
        {
            Vector3 randomPos = new Vector3(Random.Range(minX, maxX), 2f, Random.Range(minY, maxY));
            player.gameObject.transform.position = randomPos;
            player.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        }

        startGame = true;
        firstGame = false;
        startTimer = 5f;
    }

    [PunRPC]
    private void GetPlayers()
    {
        playerList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));

        aliveList = new List<GameObject>(playerList);
    }

    [PunRPC]

    private void Move(int direction)
    {
        if (direction == 0)
        {
            //Blue
            if (choice == 0)
            {
                CyanHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                GreenHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                PinkHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                RedHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                WhiteHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                YellowHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
            }
            //Cyan
            else if (choice == 1)
            {
                BlueHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                GreenHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                PinkHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                RedHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                WhiteHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                YellowHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
            }
            //Green
            else if (choice == 2)
            {
                BlueHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                CyanHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                PinkHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                RedHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                WhiteHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                YellowHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
            }
            //Pink
            else if (choice == 3)
            {
                BlueHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                CyanHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                GreenHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                RedHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                WhiteHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                YellowHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
            }
            //Red
            else if (choice == 4)
            {
                BlueHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                CyanHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                GreenHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                PinkHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                WhiteHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                YellowHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
            }
            //White
            else if (choice == 5)
            {
                BlueHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                CyanHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                GreenHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                PinkHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                RedHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                YellowHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
            }
            //Yellow
            else if (choice == 6)
            {
                BlueHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                CyanHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                GreenHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                PinkHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                RedHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                WhiteHexagon.transform.position -= new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
            }
        }

        if (direction == 1)
        {
            //Blue
            if (choice == 0)
            {
                if(CyanHexagon.transform.position.y <= 1.98f)
                {
                    CyanHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (GreenHexagon.transform.position.y <= 1.98f)
                {
                    GreenHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (PinkHexagon.transform.position.y <= 1.98f)
                {
                    PinkHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (RedHexagon.transform.position.y <= 1.98f)
                {
                    RedHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (WhiteHexagon.transform.position.y <= 1.98f)
                {
                    WhiteHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (YellowHexagon.transform.position.y <= 1.98f)
                {
                    YellowHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
            }
            //Cyan
            else if (choice == 1)
            {
                if (BlueHexagon.transform.position.y <= 1.98f)
                {
                    BlueHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (GreenHexagon.transform.position.y <= 1.98f)
                {
                    GreenHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (PinkHexagon.transform.position.y <= 1.98f)
                {
                    PinkHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (RedHexagon.transform.position.y <= 1.98f)
                {
                    RedHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (WhiteHexagon.transform.position.y <= 1.98f)
                {
                    WhiteHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (YellowHexagon.transform.position.y <= 1.98f)
                {
                    YellowHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
            }
            //Green
            else if (choice == 2)
            {
                if (BlueHexagon.transform.position.y <= 1.98f)
                {
                    BlueHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (CyanHexagon.transform.position.y <= 1.98f)
                {
                    CyanHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (PinkHexagon.transform.position.y <= 1.98f)
                {
                    PinkHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (RedHexagon.transform.position.y <= 1.98f)
                {
                    RedHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (WhiteHexagon.transform.position.y <= 1.98f)
                {
                    WhiteHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (YellowHexagon.transform.position.y <= 1.98f)
                {
                    YellowHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
            }
            //Pink
            else if (choice == 3)
            {
                if (BlueHexagon.transform.position.y <= 1.98f)
                {
                    BlueHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (CyanHexagon.transform.position.y <= 1.98f)
                {
                    CyanHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (GreenHexagon.transform.position.y <= 1.98f)
                {
                    GreenHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (RedHexagon.transform.position.y <= 1.98f)
                {
                    RedHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (WhiteHexagon.transform.position.y <= 1.98f)
                {
                    WhiteHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (YellowHexagon.transform.position.y <= 1.98f)
                {
                    YellowHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
            }
            //Red
            else if (choice == 4)
            {
                if (BlueHexagon.transform.position.y <= 1.98f)
                {
                    BlueHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (CyanHexagon.transform.position.y <= 1.98f)
                {
                    CyanHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (GreenHexagon.transform.position.y <= 1.98f)
                {
                    GreenHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (PinkHexagon.transform.position.y <= 1.98f)
                {
                    PinkHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (WhiteHexagon.transform.position.y <= 1.98f)
                {
                    WhiteHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (YellowHexagon.transform.position.y <= 1.98f)
                {
                    YellowHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
            }
            //White
            else if (choice == 5)
            {
                if (BlueHexagon.transform.position.y <= 1.98f)
                {
                    BlueHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (CyanHexagon.transform.position.y <= 1.98f)
                {
                    CyanHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (GreenHexagon.transform.position.y <= 1.98f)
                {
                    GreenHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (PinkHexagon.transform.position.y <= 1.98f)
                {
                    PinkHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (RedHexagon.transform.position.y <= 1.98f)
                {
                    RedHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (YellowHexagon.transform.position.y <= 1.98f)
                {
                    YellowHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
            }
            //Yellow
            else if (choice == 6)
            {
                if (BlueHexagon.transform.position.y <= 1.98f)
                {
                    BlueHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (CyanHexagon.transform.position.y <= 1.98f)
                {
                    CyanHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (GreenHexagon.transform.position.y <= 1.98f)
                {
                    GreenHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (PinkHexagon.transform.position.y <= 1.98f)
                {
                    PinkHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (RedHexagon.transform.position.y <= 1.98f)
                {
                    RedHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
                if (WhiteHexagon.transform.position.y <= 1.98f)
                {
                    WhiteHexagon.transform.position += new Vector3(0f, 1f * speedMultiplyer * Time.deltaTime, 0);
                }
            }
        }

        if (direction == 2)
        {
            BlueHexagon.transform.position = new Vector3(3.25f, 2f, 5.625f);
            CyanHexagon.transform.position = new Vector3(6.5f, 2f, 0f);
            GreenHexagon.transform.position = new Vector3(-6.5f, 2f, 0f);
            PinkHexagon.transform.position = new Vector3(-3.25f, 2f, 5.625f);
            RedHexagon.transform.position = new Vector3(-3.25f, 2f, -5.625f);
            WhiteHexagon.transform.position = new Vector3(0f, 2f, 0f);
            YellowHexagon.transform.position = new Vector3(3.25f, 2f, -5.625f);
        }
    }
}
