using System.Collections;
using Assets.Scripts.Enemies;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SpawnPoint : MonoBehaviour
{
    public SphereCollider Collider;
    public GameObject[] Enemies;
    public GameObject SpawnParticle;
    public Vector3 Spawnpoint = new Vector3(0, 0, 0);
    private Vector3 _oldEnemyPos = Vector3.zero;
    private bool _spawned = false;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + Spawnpoint, 1);
    }

    private void Start()
    {
        if (Collider == null)
        {
            Collider = GetComponent<SphereCollider>();
            Collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !_spawned)
        {
            _spawned = true;
            MyCharacterController.Instance.RunnerEnable = false;
            MyCharacterController.Instance.SwordButton.gameObject.SetActive(true);
            SpawnEnemies();
        }
    }

    private void SpawnEnemies()
    {
        foreach (var enemy in Enemies)
        {
            var charPos = transform.position + Spawnpoint + new Vector3(Random.Range(0, 5f), 0f, Random.Range(0, 5f));
            Instantiate(SpawnParticle, charPos, Quaternion.identity);
            var relativePos = MyCharacterController.Instance.transform.position - transform.position;
            var enemyToAdd = Instantiate(enemy, charPos, Quaternion.LookRotation(relativePos)) as GameObject;
            if (enemyToAdd != null)
            {
                enemyToAdd.GetComponent<Robot>().SetOperationInfos("Celio 00023", Random.Range(1, 1500));
            }
        }
    }

}
