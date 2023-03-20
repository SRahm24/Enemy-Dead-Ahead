using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMissileScript : MonoBehaviour
{
    GameManager gameManager;
    EnemyScript enemyScript;
    public Vector3 targetTileLocation;
    private int targetTile = -1;

    void Start()
    {
        gameManager = Camera.main.GetComponent<GameManager>();
        enemyScript = Camera.main.GetComponent<EnemyScript>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ship"))
        {
            gameManager.EnemyHitPlayer(targetTileLocation, targetTile, collision.gameObject);
        }
        else
        {
            enemyScript.PauseAndEnd(targetTile);
        }
        Destroy(gameObject);
    }
    public void SetTarget(int target)
    {
        targetTile = target;
    }
}
