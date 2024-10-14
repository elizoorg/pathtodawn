using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class ObstacleManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    public GameObject ObstaclePrefab;
    [SerializeField]
    private List<Mesh> meshes = new List<Mesh>();
    [SerializeField]
    public float FlySpeed, CurFlySpeed;
    [SerializeField]
    private float acceleration, spawnDistance, spawnInterval, maxSpeed;
    [SerializeField]
    GameObject PlayerPrefab;
    [SerializeField]
    GameObject MenuCamera;
    [SerializeField]
    Image EndGameColoredOverlay;
    [SerializeField]
    GameObject ScoreText, DeathScoreText;
    [SerializeField]
    GameObject HighscoreText, DeathHighscoreText;

    GameObject localPlayer;

    [SerializeField]
    GameObject MenuPanel, GamePanel, EndGamePanel;

    public static ObstacleManager Instance;

    int score = 0;
    int highscore = 0;
    bool isGameActive = false;
    public bool IsGameActive { get => isGameActive; }
    private List<GameObject> obstacles = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        highscore = PlayerPrefs.GetInt("highscore");
        HighscoreText.GetComponent<TextMeshProUGUI>().SetText("Highscore: " + highscore.ToString());

        Instance = this;
    }

    public void StartGame()
    {
        MenuCamera.SetActive(false);
        MenuPanel.SetActive(false);
        EndGamePanel.SetActive(false);
        GamePanel.SetActive(true);

        if (PhotonNetwork.CurrentRoom != null)
            localPlayer = PhotonNetwork.Instantiate(PlayerPrefab.name, Vector3.zero, Quaternion.Euler(90, 0, 0));
        else
            localPlayer = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0));

        isGameActive = true;
        CurFlySpeed = FlySpeed;
        score = 0;
        ScoreText.GetComponent<TextMeshProUGUI>().SetText(score.ToString());
    }
    public void ExitGame()
    {
        //if (PhotonNetwork.IsConnected && PhotonNetwork.CurrentRoom != null)
        //    PhotonNetwork.Destroy(localPlayer);
        //else
        //    Destroy(localPlayer);
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0;  i < players.Length; i++)
            Destroy(players[i]);
        localPlayer = null;

        foreach (GameObject obstacle in obstacles)
            Destroy(obstacle);

        obstacles.Clear();

        MenuCamera.SetActive(true);
        MenuPanel.SetActive(true);
        EndGamePanel.SetActive(false);
        GamePanel.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (!isGameActive)
            return;

        CurFlySpeed = Mathf.Min(maxSpeed, CurFlySpeed + acceleration * Time.deltaTime);
        
        Ray ray = new Ray(localPlayer.transform.position + Vector3.up * 2.0f, Vector3.down);

        if (Physics.Raycast(ray, CurFlySpeed * Time.deltaTime + 2.0f))
        {
            EndGame(false);
            return;
        }

        for (int i = obstacles.Count - 1; i >= 0; i--)
        {
            obstacles[i].transform.position += Vector3.up * CurFlySpeed * Time.deltaTime;
            if (obstacles[i].transform.position.y >= 0)
            {
                Destroy(obstacles[i]);

                obstacles.RemoveAt(i);
                score++;
                ScoreText.GetComponent<TextMeshProUGUI>().SetText(score.ToString());
            }
        }

        if (PhotonNetwork.CurrentRoom != null && !PhotonNetwork.IsMasterClient)
            return;

        float spawnLocation = obstacles.Count > 0 ? obstacles.Last().transform.position.y - spawnInterval : - spawnDistance;
        Quaternion spawnRotation = obstacles.Count > 0 ? obstacles.Last().transform.rotation * new Quaternion(0, Mathf.Sin(Mathf.PI / 4.0f), 0, Mathf.Cos(Mathf.PI / 4.0f)) : Quaternion.identity;
        while (spawnLocation >= -spawnDistance)
        {
            if (PhotonNetwork.CurrentRoom == null)
                spawnObstacle(spawnLocation, spawnRotation, Random.Range(0, meshes.Count));
            else
            {
                if (PhotonNetwork.IsMasterClient)
                    photonView.RPC("spawnObstacle", RpcTarget.All, spawnLocation, spawnRotation, Random.Range(0, meshes.Count));
            }

            spawnLocation -= spawnInterval;
            spawnRotation *= new Quaternion(0, Mathf.Sin(Mathf.PI / 4.0f), 0, Mathf.Cos(Mathf.PI / 4.0f));
        }
    }
    [PunRPC]
    private void spawnObstacle(float spawnLocation, Quaternion spawnRotation, int meshIndex)
    {
        GameObject newObstacle = Instantiate(ObstaclePrefab, new Vector3(0, spawnLocation, 0), spawnRotation);
        Mesh curMesh = meshes[meshIndex];
        newObstacle.GetComponent<MeshFilter>().mesh = curMesh;
        newObstacle.GetComponent<MeshCollider>().sharedMesh = curMesh;
        obstacles.Add(newObstacle);
    }

    public void OnStartClicked()
    {
        StartGame();
    }
    public void EndGame(bool win)
    {
        if (!IsGameActive)
            return;

        isGameActive = false;

        if (PhotonNetwork.CurrentRoom != null)
            PhotonNetwork.LeaveRoom();

        DeathScoreText.GetComponent<TextMeshProUGUI>().SetText("score: " + score.ToString());
        if (score > highscore)
        {
            highscore = score;
            PlayerPrefs.SetInt("highscore", highscore);
        }

        DeathHighscoreText.GetComponent<TextMeshProUGUI>().SetText("high score: " + highscore.ToString());

        GamePanel.SetActive(false);
        EndGamePanel.SetActive(true);
        EndGameColoredOverlay.color = win ? new Color(0.0f, 1.0f, 0.0f, 0.15f) : new Color(1.0f, 0.0f, 0.0f, 0.15f);
    }
}
