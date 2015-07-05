
using System.Collections;
//using PigeonCoopToolkit.Effects.Trails;
using UnityEngine;

public class StingerAttack : SkillBase
{

    public float MaxTime = 5;
    public float MaxDistance = 10;
    private float _beginTime = 0;
    private float _initialAcceleration;
    private Vector3 _initialPosition;
    //private TakeDamageModel _tdm;

    //private bool _trailRendererCreated = false;
    public GameObject TrailRenderer;

    //private AudioClip _stingerChargeAudio;
    public AudioClip SwordAudio;
    private bool _maxDistanceReached = false;
    private bool _attacked = false;

    void Start()
    {
        _initialAcceleration = MyCharacterController.Instance.AccelerationSpeed;
        //_stingerChargeAudio = Resources.Load<AudioClip>("Audios/stinger_charge");

        //_tdm = new TakeDamageModel
        //{
        //    Default = false,
        //    SkillName = "stinger_attack",
        //    Emitter = charTransform,
        //    RightAttack = false,
        //    CanDodge = false,
        //    AttackMultiplicator = 1,
        //    AttackTypes = AttackTypes.Strong,
        //    NBAttack = NBAttack.Sword
        //};
    }

    void Update()
    {
        if (MyCharacterController.Instance.CurrentSkill == CurrentSkillType)
        {

            if (TrailRenderer != null && !TrailRenderer.activeSelf)
            {
                TrailRenderer.SetActive(true);
            }

            if (MyCharacterController.Instance.FocusEnemy != null)
            {
                if (MyCharacterController.Instance.CloseCombat || (Time.time - _beginTime) >= MaxTime)
                {
                    FinishAttack();
                }
                else
                {
                    MyCharacterController.Instance.InputMovement = new Vector2(0, 1);
                }
            }
            else
            {
                var distance = Vector3.Distance(_initialPosition, transform.position);
                if (distance >= MaxDistance && !_maxDistanceReached)
                {
                    _maxDistanceReached = true;
                    StartCoroutine(AutoAttack());
                }
                else
                {
                    MyCharacterController.Instance.InputMovement = new Vector2(0, 1);
                }
            }
        }
    }

    IEnumerator AutoAttack(Transform charTransform = null)
    {
        Debug.Log("AutoAttack");

        _attacked = true;

        if (SwordAudio != null)
        {
            audio.PlayOneShot(SwordAudio, 1);
        }

        MyCharacterController.Instance.CanMove = false;

        if (charTransform != null)
        {
            MyCharacterController.Instance.CharAnimator.SetTrigger("stinger_collision");
            charTransform.rigidbody.velocity = Vector3.zero;
            charTransform.LookAt(transform);
            charTransform.rigidbody.AddForce(MyCharacterController.Instance.transform.forward * 500);
        }
        else
        {
            MyCharacterController.Instance.CharAnimator.SetTrigger("stinger");
        }



        yield return new WaitForSeconds(1f);

        FinishAttack();

        yield return new WaitForSeconds(0.5f);

        if (charTransform != null)
        {
            charTransform.GetComponent<Enemy>().Life = 0;
        }
    }

    public void FinishAttack()
    {
        Debug.Log("FinishAttack");
        //MyCharacterController.Instance.FocusEnemy.rigidbody.AddForce(100 * Vector3.up);

        //Time.timeScale = 0.5f;

        MyCharacterController.Instance.CurrentSkill = null;
        MyCharacterController.Instance.Attacking = false;
        MyCharacterController.Instance.InputMovement = Vector2.zero;
        MyCharacterController.Instance.AccelerationSpeed = _initialAcceleration;

        MyCharacterController.Instance.CharAnimator.SetFloat("speed", 0);

        if (TrailRenderer != null && TrailRenderer.activeSelf)
        {
            TrailRenderer.SetActive(false);
        }

        //yield return new WaitForSeconds(0.5f);
        //Time.timeScale = 1;
        //MyCharacterController.Instance.FocusEnemy.rigidbody.AddForce(800 * MyCharacterController.Instance.charTransform.forward);
        //MyCharacterController.Instance.FocusEnemy.SendMessage("ApplyDamage", _tdm);
        //MyCharacterController.Instance.CharAnimator.SetFloat("speed", 0);
        //var navMesh = MyCharacterController.Instance.FocusEnemy.GetComponent<NavMeshAgent>();
        MyCharacterController.Instance.StopAttackCharge();
        MyCharacterController.Instance.CanMove = true;
    }

    public void LaunchStingerAttack()
    {
        _attacked = false;
        _maxDistanceReached = false;
        _initialPosition = transform.position;
        //MyCharacterController.Instance.FocusEnemy.GetComponent<NavMeshAgent>().enabled = false;
        _beginTime = Time.time;


        // MyCharacterController.Instance.CharAnimator.SetFloat("speed", 20);
        // MyCharacterController.Instance.CharAnimator.Play("WalkOrRun");

        MyCharacterController.Instance.CurrentSkill = CurrentSkillType;
        MyCharacterController.Instance.AccelerationSpeed = _initialAcceleration * 2;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (MyCharacterController.Instance.CurrentSkill == CurrentSkillType && !_attacked)
        {
            if (collision.transform.tag == "Enemy")
            {
                MyCharacterController.Instance.StopVelocity();
                MyCharacterController.Instance.transform.LookAt(collision.transform);

                var meshAgent = collision.transform.GetComponent<NavMeshAgent>();
                meshAgent.Stop();
                meshAgent.enabled = false;

                StartCoroutine(AutoAttack(collision.transform));
            }
            else
            {
                StartCoroutine(AutoAttack());
            }
        }
    }

}
