using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Enemies
{
    public class Robot : Enemy
    {

        private GameObject _weakHitParticle, _mediumHitParticle;
        public Gradient Gradient;
        public Text lb_opName;
        public Text lb_opValue;
        public Image panel;

        public string OperationName;
        public float OperationValue;

        private AudioSource _audio;
        private Rigidbody _rigidbody;

        private void Start()
        {
            RandomAction();
        }

        protected override void Awake()
        {
            _audio = GetComponent<AudioSource>();
            _rigidbody = GetComponent<Rigidbody>();
            base.Awake();
        }

        protected override void Update()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var childRenderer in renderers)
            {
                childRenderer.material.SetColor("_Color", Gradient.Evaluate(Life / MaxLife));
            }
            base.Update();
        }

        /// <summary>
        /// Permet de tirer au sort la nouvelle action que va effectuer le personnage
        /// </summary>
        public override void RandomAction()
        {
            if (Attacking)
            {
                Attacking = false;
            }

            var val = Random.value;
            if (val <= 0.25f)
            {
                StartCoroutine(Idle(IdleTime));
            }
            ////else if (val <= 0.5f)
            ////{
            ////    if (CanAttack)
            ////    {
            ////        StartCoroutine(Attack());
            ////    }
            ////}
            else
            {
                CurrentAction = EnemyActions.MoveHandToHand;
            }
        }

        public override IEnumerator CastSpecialDistanceAttack()
        {
            yield break;
        }

        /// <summary>
        /// Méthode qui permet de gérer les encaissement de notre personnage ainsi que ses esquives
        /// </summary>
        public override void ApplyDamage(TakeDamageModel model)
        {

            base.ApplyDamage(model);


            var relativePos = MyCharacterController.Instance.transform.position - transform.position;
            var lookRot = Quaternion.LookRotation(relativePos);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, lookRot.eulerAngles.y, transform.rotation.eulerAngles.z);


            if (!model.Default)
            {

                var dodge = (model.CanDodge && Random.value <= DodgeProbability);

                CharAnimator.SetBool("dodge", dodge);
                CharAnimator.SetBool("hit_right_attack", model.RightAttack);

                switch (model.AttackTypes)
                {

                    case AttackTypes.Weak:
                        {
                            CharAnimator.SetTrigger("hit_weak_attack");
                            if (!dodge)
                            {
                                ActivateEndOfHitAnimationTrigger = true;
                                GettingHit = true;
                                Life -= (200 * model.AttackMultiplicator);

                                PlayRandomPainAudioClip();

                                if (_weakHitParticle == null)
                                {
                                    _weakHitParticle = Resources.Load<GameObject>("Particles/CFX_Prefabs_Mobile/Hits/CFXM_Hit_A Red");
                                }

                                Instantiate(_weakHitParticle, transform.position, Quaternion.identity);

                            }
                            else
                            {
                                Dodge();
                            }
                        }
                        break;

                    case AttackTypes.Medium:
                        {
                            CharAnimator.SetTrigger("hit_medium_attack");
                            if (!dodge)
                            {
                                GettingHit = true;
                                Life -= 300;
                                PlayRandomPainAudioClip();

                                if (_mediumHitParticle == null)
                                {
                                    _mediumHitParticle = Resources.Load<GameObject>("Particles/CFX_Prefabs_Mobile/Hits/CFXM_Hit_A Red");
                                }

                                Instantiate(_mediumHitParticle, transform.position, Quaternion.identity);
                                StartCoroutine(WaitAndRandomAction(Random.Range(0.5f, 1.5f)));
                            }
                            else
                            {
                                Dodge();
                            }
                        }


                        break;

                    case AttackTypes.Strong:
                        {
                            print("!!!!!!!!!! STRONG !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                            CharAnimator.SetTrigger("hit_strong_attack");

                            ActivateEndOfHitAnimationTrigger = true;
                            GettingHit = true;
                            Life -= 4000;

                            PlayRandomPainAudioClip();

                            if (_weakHitParticle == null)
                            {
                                _weakHitParticle = Resources.Load<GameObject>("Particles/CFX_Prefabs_Mobile/Hits/CFXM_Hit_A Red");
                            }

                            Instantiate(_weakHitParticle, transform.position, Quaternion.identity);


                        }
                        break;

                    case AttackTypes.Combo:

                        CharAnimator.SetTrigger("hit_punch_combo");

                        ActivateEndOfHitAnimationTrigger = true;
                        GettingHit = true;
                        Life -= 7000;

                        PlayRandomPainAudioClip();

                        if (_weakHitParticle == null)
                        {
                            _weakHitParticle = Resources.Load<GameObject>("Particles/CFX_Prefabs_Mobile/Hits/CFXM_Hit_A Red");
                        }

                        Instantiate(_weakHitParticle, transform.position, Quaternion.identity);
                        break;
                }

            }
            else
            {
                CharAnimator.SetBool("dodge", false);
                // L'attaque possède la propriété "Default" a true, cela signifie qu'elle a été générée probablement via un animation event
                // On va devoir donc retrouver les propriètés de l'attaque uniquement grâce au nom de la capacité.
                switch (model.SkillName)
                {
                    case "dash_attack":
                        {
                            GettingHit = true;
                            Life -= 100;
                            // _animator.Play("ReceivePunchComboAttack");
                            RandomAction();
                        }
                        break;

                    case "sword_combo_1":
                        {
                            GettingHit = true;
                            Life -= 200;
                            CharAnimator.Play("ReceivePunchComboAttack");
                            _rigidbody.AddForce(Vector3.up * 10f);
                            RandomAction();
                        }
                        break;

                    //case "weak_combo":
                    //    {
                    //        StartCoroutine(PlayComboAttackHit());
                    //        Life -= 200;
                    //    }
                    //    break;

                    //case "throw":
                    //    Instantiate(Resources.Load("CFXM_Text Wham"), transform.position, Quaternion.identity);
                    //    CharAnimator.SetBool("swung", true);
                    //    Life -= 200;
                    //    GettingHit = false;
                    //    RandomAction();
                    //    break;

                    //case "kiai":
                    //    Instantiate(Resources.Load("CFXM_Text Wham"), transform.position, Quaternion.identity);
                    //    CharAnimator.SetTrigger("hit_kiai");
                    //    Life -= 30;
                    //    GettingHit = false;
                    //    RandomAction();
                    //    break;

                    //case "low_kick":
                    //    Life -= 50;
                    //    var hit = Resources.Load("CFXPrefabs_Mobile/Hits/CFXM_Hit_C White") as GameObject;
                    //    Instantiate(hit, transform.position, Quaternion.identity);
                    //    _animator.Play("ReceiveLowKick");
                    //    _gettingHit = false;
                    //    RandomAction();
                    //    break;

                    //case "spiral_kick":
                    //    Life -= 300;
                    //    var boum = Resources.Load("CFXPrefabs_Mobile/Texts/CFXM_Text Boom") as GameObject;
                    //    Instantiate(boum, transform.position, Quaternion.identity);
                    //    var dir = (transform.position - MyCharacterController.Instance.transform.position).normalized;
                    //    dir.y = 1;
                    //    rigidbody.AddForce(dir * 350f);
                    //    RandomAction();
                    //    break;

                    //case "projection_up_kick":
                    //    Life -= 300;
                    //    rigidbody.AddForce(transform.up * 1000);
                    //    CharAnimator.Play("Hit Blow Back");
                    //    break;

                    //case "kamehameha":

                    //    Life -= 1000;
                    //    var direction = transform.position - model.Emitter.position;
                    //    rigidbody.AddForce(direction * 100);

                    //    break;


                    default:
                        Debug.LogError(string.Format("Technque inconnue: {0}", model.SkillName));
                        break;
                }
            }

        }

        public void SetOperationInfos(string opName, float opValue)
        {

            OperationName = opName;
            OperationValue = opValue;

            lb_opName.text = opName;
            lb_opValue.text = "-" + opValue;

            //On change la couleur de fond en fonction de la value
            if (opValue < 20)
            {
                panel.color = new Color(0, 0, 0, .4f);
            }
            else if (opValue >= 20 && opValue < 60)
            {
                panel.color = new Color(.9f, .29f, .23f, .4f);
            }
            else if (opValue >= 60 && opValue < 100)
            {
                panel.color = new Color(.20f, .60f, .85f, .4f);
            }
            else if (opValue >= 100 && opValue < 500)
            {
                panel.color = new Color(.18f, .8f, .44f, .4f);
            }
            else if (opValue >= 500 && opValue < 1000)
            {
                panel.color = new Color(.95f, .77f, .06f, .4f);
            }
            else if (opValue >= 1000)
            {
                panel.color = new Color(.607f, .35f, .713f, .4f);
            }
            print(panel.color);
        }

        protected override void Die()
        {
            base.Die();
            var bip = Resources.Load<AudioClip>("Audios/Bip");
            if (bip != null)
            {
                _audio.PlayOneShot(bip);
            }
            GameManager.instance.Coins += OperationValue;
            var obj = (GameObject)Instantiate(MyCharacterController.Instance.SmashTextObject, Vector3.zero, Quaternion.identity);
            obj.SetActive(true);
            var text = obj.GetComponent<Text>();
            text.text = "+ " + OperationValue.ToString(CultureInfo.InvariantCulture);
            text.color = Color.green;
            obj.transform.parent = MyCharacterController.Instance.MainCanvas.transform;
        }
    }
}
