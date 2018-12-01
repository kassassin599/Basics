using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour {

    bool gamehasEnded = false;
    public float restartDelay = 1f;
    public GameObject completeLevel1UI;

    public void CompleteLevel1()
    {
        completeLevel1UI.SetActive(true); 
    }

    public void EndGame()
    {
        if(gamehasEnded== false)
        {
            gamehasEnded = true;
            Debug.Log("GAME OVER");
            Invoke("Restart", restartDelay);
        }
        
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
