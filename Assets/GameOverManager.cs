using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{

    public int nbOfLives = 3;
    public Text lb_gameOver;
    public Text lb_vie_value;
    private float initLife;

    private static GameOverManager _instance;
    public static GameOverManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindObjectOfType<GameOverManager>();

                //Tell unity not to destroy this object when loading a new scene!
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            //If I am the first instance, make me the Singleton
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            //If a Singleton already exists and you find
            //another reference in scene, destroy it!
            if (this != _instance)
                Destroy(this.gameObject);
        }
    }

    public void Init()
    {
        initLife = MyCharacterController.Instance.Life;
        lb_vie_value.text = nbOfLives.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (MyCharacterController.Instance.IsDied)
        {
            Camera.main.transform.LookAt(MyCharacterController.Instance.transform);
        }
    }

    public void GameOver(GameObject respawnPoint = null)
    {
        Debug.Log("GameOVer");

        MyCharacterController.Instance.Life = 0;

        if (PlayerPlanManager.Instance.PlayerPlanSub.Any(o => o.Plan == PlayerPlan.AssuranceVie && o.Enable))
        {
            StartCoroutine(respawnPoint == null ? PerformRespawn(GameObject.Find("RespawnPoint1")) : PerformRespawn(respawnPoint));
            return;
        }

        nbOfLives--;
        lb_vie_value = GameObject.Find("lb_vie_value").GetComponent<Text>();
        lb_vie_value.text = nbOfLives.ToString(CultureInfo.InvariantCulture);

        StartCoroutine(respawnPoint == null ? PerformRespawn(GameObject.Find("RespawnPoint1")) : PerformRespawn(respawnPoint));
    }

    private IEnumerator PerformRespawn(GameObject respawnPoint)
    {

        var mainCam = Camera.main;
        mainCam.GetComponent<BasicFollowCamera>().enabled = false;

        lb_gameOver = GameObject.Find("lb_GameOver").GetComponent<Text>();
        lb_gameOver.enabled = true;

        if (nbOfLives > 0)
            lb_gameOver.text = "Try again !";
        else
            lb_gameOver.text = "Game Over";

        yield return new WaitForSeconds(3);

        if (nbOfLives == 0)
        {
            nbOfLives = 3;
            Application.LoadLevel(0);
        }


        lb_gameOver.enabled = false;
        MyCharacterController.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;

        MyCharacterController.Instance.Life = initLife;
        MyCharacterController.Instance.transform.position = respawnPoint.transform.position;
        MyCharacterController.Instance.CharAnimator.Play("Idle");

        MyCharacterController.Instance.transform.rotation = Quaternion.identity;
        mainCam.GetComponent<BasicFollowCamera>().enabled = true;
        // MyCharacterController.Instance.OrbitCameraScript.enabled = true;
    }
}
