using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [Header("Ships")]
    public GameObject[] ships;
    public EnemyScript enemyScript;
    private ShipScript shipScript;
    private List<int[]> enemyShips;
    private int shipIndex = 0;
    public List<TileScript> allTileScripts;    

    [Header("HUD")]
    public Button nextButton;
    public Button rotateButton;
    public Button replayButton;
    public Image copyrightBackGround;
    public Text messageHeader;
    public Text playerShipCountText;
    public Text enemyShipCountText;
    public Button confirmPrompt;
    public Text inputText;
    public Text timerHeader;

    [Header("Objects")]
    public GameObject barrelPrefab;
    public GameObject enemyBarrelPrefab;
    public GameObject firePrefab;
    public int countdownTime; 

    private bool setupComplete = false;
    private bool playerTurn = true;
    private bool playerClicked = false;
    private bool checkInput = false;
    private bool timerStarted = false;
    
    private List<GameObject> playerFires = new List<GameObject>();
    private List<GameObject> enemyFires = new List<GameObject>();
    
    private int enemyShipCount = 5;
    private int playerShipCount = 5;
    private int randomNumber;
    private string input;

    private GameObject currentTile;
    private InputField inputfield;

    // Start is called before the first frame update
    void Start()
    {
        shipScript = ships[shipIndex].GetComponent<ShipScript>();
        nextButton.onClick.AddListener(() => NextShipClicked());
        rotateButton.onClick.AddListener(() => RotateClicked());
        replayButton.onClick.AddListener(() => ReplayClicked());
        enemyShips = enemyScript.PlaceEnemyShips();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void NextShipClicked()
    {
        if (!shipScript.OnGameBoard())
        {
            shipScript.FlashColor(Color.red);
        } else
        {
            if(shipIndex <= ships.Length - 2)
            {
                shipIndex++;
                shipScript = ships[shipIndex].GetComponent<ShipScript>();
                shipScript.FlashColor(Color.yellow);
            }
            else
            {
                rotateButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(false);
                messageHeader.text = "Click a tile to target the enemy!";
                setupComplete = true;
                for (int i = 0; i < ships.Length; i++) ships[i].SetActive(false);
            }
        }
    }
    
    IEnumerator CountdownStart()
    {
        while (countdownTime > 0 && timerStarted) 
        {
            timerHeader.text = countdownTime.ToString();
            yield return new WaitForSeconds(1f);
            countdownTime--;
        }
        if (countdownTime == 0) 
        {
        messageHeader.text = "Times up!";
        confirmPrompt.gameObject.SetActive(false);
        playerTurn = false;
        checkInput = true;
        timerStarted = false;
        timerHeader.gameObject.SetActive(false);
        playerClicked = false;
        Invoke("EndPlayerTurn", 1.0f);
        }
    }

    public void TileClicked(GameObject tile)
    {
        if(setupComplete && playerTurn && !playerClicked)
        {
            checkInput = false;
            playerClicked = true;
            randomNumber = UnityEngine.Random.Range(100, 999);
            messageHeader.text = "Coordinates are " + randomNumber.ToString() + "!";
            confirmPrompt.gameObject.SetActive(true);
            timerHeader.gameObject.SetActive(true);
            timerStarted = true;
            StartCoroutine(CountdownStart());
            currentTile = tile;            
        } else if (!setupComplete)
        {
            PlaceShip(tile);
            shipScript.SetClickedTile(tile);
        }
    }

    public void ReadStringInput(string pInput)
    {
        confirmPrompt.gameObject.SetActive(false);
        inputText.text = "#";
        input = pInput;
        Debug.Log(input);
        inputfield = inputText.GetComponent<InputField>()   ;
        inputfield.Select();
        inputfield.text = "";
        if (input == randomNumber.ToString())
        {
            timerHeader.gameObject.SetActive(false);
            Vector3 tilePos = currentTile.transform.position;
            tilePos.y += 15;
            checkInput = true;
            playerTurn = false;
            timerStarted = false;
            Instantiate(barrelPrefab, tilePos, barrelPrefab.transform.rotation);
        }
        else
        {
            messageHeader.text = "Wrong coordinates Admiral!";
            timerHeader.gameObject.SetActive(false);
            timerStarted = false;
            playerTurn = false;
            checkInput = true;
            playerClicked = false;
            Invoke("EndPlayerTurn", 1.0f);
        }
        countdownTime = 5;
    }

    private void PlaceShip(GameObject tile)
    {
        shipScript = ships[shipIndex].GetComponent<ShipScript>();
        shipScript.ClearTileList();
        Vector3 newVec = shipScript.GetOffsetVec(tile.transform.position);
        ships[shipIndex].transform.localPosition = newVec;
    }

    void RotateClicked()
    {
        shipScript.RotateShip();
    }

    public void CheckHit(GameObject tile)
    {
        print(tile);
        int tileNum = Int32.Parse(Regex.Match(tile.name, @"\d+").Value);
        int hitCount = 0;
        foreach(int[] tileNumArray in enemyShips)
        {
            if (tileNumArray.Contains(tileNum))
            {
                for (int i = 0; i < tileNumArray.Length; i++)
                {
                    if (tileNumArray[i] == tileNum)
                    {
                        tileNumArray[i] = -5;
                        hitCount++;
                    }
                    else if (tileNumArray[i] == -5)
                    {
                        hitCount++;
                    }
                }
                if (hitCount == tileNumArray.Length)
                {
                    enemyShipCount--;
                    messageHeader.text = "Enemy magazine detonated!";
                    enemyFires.Add(Instantiate(firePrefab, tile.transform.position, Quaternion.identity));
                    tile.GetComponent<TileScript>().SetTileColor(1, new Color32(68, 0, 0, 255));
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                else
                {
                    messageHeader.text = "Direct hit!";
                    tile.GetComponent<TileScript>().SetTileColor(1, new Color32(255, 0, 0, 255));
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                break;
            }
            
        }
        if(hitCount == 0)
        {
            tile.GetComponent<TileScript>().SetTileColor(1, new Color32(38, 57, 76, 255));
            tile.GetComponent<TileScript>().SwitchColors(1);
            messageHeader.text = "We missed!";
        }
        playerClicked = false;
        Invoke("EndPlayerTurn", 1.0f);
    }

    public void EnemyHitPlayer(Vector3 tile, int tileNum, GameObject hitObj)
    {
        enemyScript.MissileHit(tileNum);
        tile.y += 0.2f;
        playerFires.Add(Instantiate(firePrefab, tile, Quaternion.identity));
        messageHeader.text = "They landed a hit on us!";
        if (hitObj.GetComponent<ShipScript>().HitCheckSank())
        {
            playerShipCount--;
            messageHeader.text = "One of our ships just blew!";
            playerShipCountText.text = playerShipCount.ToString();
            enemyScript.SunkPlayer();
        }
       Invoke("EndEnemyTurn", 2.0f);
    }

    private void EndPlayerTurn()
    {
        for (int i = 0; i < ships.Length; i++) ships[i].SetActive(true);
        foreach (GameObject fire in playerFires) fire.SetActive(true);
        foreach (GameObject fire in enemyFires) fire.SetActive(false);
        enemyShipCountText.text = enemyShipCount.ToString();
        messageHeader.text = "Brace for impact!";
        enemyScript.NPCTurn();
        ColorAllTiles(0);
        if (playerShipCount < 1) GameOver("Our ships reside within Davy Jone's Locker...");
    }

    public void EndEnemyTurn()
    {
        for (int i = 0; i < ships.Length; i++) ships[i].SetActive(false);
        foreach (GameObject fire in playerFires) fire.SetActive(false);
        foreach (GameObject fire in enemyFires) fire.SetActive(true);
        playerShipCountText.text = playerShipCount.ToString();
        messageHeader.text = "Select a tile";
        playerTurn = true;
        ColorAllTiles(1);
        if (enemyShipCount < 1) GameOver("Our ships rule the waves Admiral!");
    }

    private void ColorAllTiles(int colorIndex)
    {
        foreach (TileScript tileScript in allTileScripts)
        {
            tileScript.SwitchColors(colorIndex);
        }
    }

    void GameOver(string winner)
    {
        messageHeader.text = "Naval Action Over: " + winner;
        replayButton.gameObject.SetActive(true);
        copyrightBackGround.gameObject.SetActive(true);
        playerTurn = false;
    }

    void ReplayClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


}
