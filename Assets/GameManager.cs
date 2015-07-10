using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int MaxCoins = 2000, MinCoins = -500;
    public Slider CoinSlider;
    public GameObject FirstRespawnPoint;
    public Text LbCoins;

    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();

                //Tell unity not to destroy this object when loading a new scene!
                // DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    private float _coins;
    public float Coins
    {
        get
        {
            return _coins;
        }
        set
        {
            if (value >= MaxCoins)
            {
                _coins = MaxCoins;
            }
            else if (value <= MinCoins)
            {
                GameOverManager.instance.GameOver(FirstRespawnPoint);
            }
            else
            {
                _coins = value;
            }
        }
    }

    public void Update()
    {
        LbCoins.text = Coins.ToString(CultureInfo.InvariantCulture);
        CoinSlider.value = Coins;
    }

    protected void Awake()
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

    protected void Start()
    {
        CoinSlider.minValue = MinCoins;
        CoinSlider.maxValue = MaxCoins;
    }


    public void GenerateCoins(Transform persoPos)
    {
        float val;
        if (Coins > 0)
        {
            val = 100;
        }
        else
        {
            val = 5;
        }

        Coins -= val;

        var obj = (GameObject)Instantiate(MyCharacterController.Instance.SmashTextObject, Vector3.zero, Quaternion.identity);
        obj.SetActive(true);
        var text = obj.GetComponent<Text>();
        text.text = "- " + val;
        text.color = Color.red;
        obj.transform.parent = MyCharacterController.Instance.MainCanvas.transform;

        var coinPrefab = Resources.Load<GameObject>("Prefabs/Game/Coin");
        var angle = Random.value * 180f;
        var coinObj = (GameObject)Instantiate(coinPrefab, persoPos.position + transform.up * 10, Quaternion.Euler(-90, angle, 0));

        Vector3 coinDir;
        var coinDirRandom = Random.value;
        const int coinDirConst = 30;

        if (coinDirRandom > 0.5f)
        {
            coinDir = -transform.forward * coinDirConst;
        }
        else if (coinDirRandom > 0.75f)
        {
            coinDir = -transform.right * coinDirConst;
        }
        else
        {
            coinDir = transform.right * coinDirConst;
        }

        var force = (Vector3.up * 2) + coinDir;
        coinObj.GetComponent<Rigidbody>().AddForce(force);
    }
}
