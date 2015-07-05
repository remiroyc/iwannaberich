using UnityEngine;

public class BasicFollowCamera : MonoBehaviour
{
    public Transform Target;

    void Update()
    {
        transform.position = MyCharacterController.Instance.transform.position - 2 * MyCharacterController.Instance.transform.forward + Vector3.up;
        transform.LookAt(Target);
    }
}
