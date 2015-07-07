using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class TileManager : MonoBehaviour
{

    public enum TileType { Falling, Killing, Jumping, Moving, Rotating, Bonus };
    public TileType tileType;
    public float jumpingTilePower;
    public Vector3 jumpDirection;
    public int CoinValue;
    public GameObject BonusParticle, KillingParticle;

    private bool _hasMoved;

    void FixedUpdate()
    {
        //this.transform.Rotate(Vector3.down * 10 * Time.deltaTime);
    }


    private void OnCollisionEnter(Collision collision)
    {
        switch (tileType)
        {
            case TileType.Jumping:
                AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audios/buzzer"), transform.position, 1);
                collision.rigidbody.AddForce(jumpDirection * jumpingTilePower * Time.deltaTime);
                break;

            case TileType.Falling:
                Destroy(this);
                break;
            case TileType.Killing:
                if (collision.transform.tag == "Player")
                {
                    MyCharacterController.Instance.CharAnimator.Play("Die");
                    if (BonusParticle != null)
                    {
                        Instantiate(BonusParticle, transform.position, Quaternion.identity);
                    }
                    GameOverManager.instance.GameOver();
                }
                break;
            case TileType.Bonus:
                if (collision.transform.tag == "Player")
                {
                    AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audios/bonus"), transform.position, 1);
                    if (BonusParticle != null)
                    {
                        Instantiate(BonusParticle, transform.position, Quaternion.identity);
                    }

                    GameManager.instance.Coins += int.Parse(CoinValue.ToString(CultureInfo.InvariantCulture));
                    Destroy(gameObject);

                    GameManager.instance.Coins += CoinValue;
                    var obj = (GameObject)Instantiate(MyCharacterController.Instance.SmashTextObject, Vector3.zero, Quaternion.identity);
                    obj.SetActive(true);
                    var text = obj.GetComponent<Text>();
                    text.text = "+ " + CoinValue;
                    text.color = Color.green;
                    obj.transform.parent = MyCharacterController.Instance.MainCanvas.transform;

                }
                break;

        }

    }

    void OnTriggerExit(Collider collider)
    {
        switch (tileType)
        {
            case TileType.Moving:
                if (!_hasMoved)
                {
                    transform.Translate(Vector3.left * 5f);
                    GetComponent<Collider>().isTrigger = false;
                    MyCharacterController.Instance.CurrentCharState = CharacterMoveState.Jumping;
                    _hasMoved = true;
                }
                break;
        }

    }
}
