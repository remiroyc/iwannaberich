using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeleteStaticObjectOnCamera : MonoBehaviour
{

    public LayerMask Mask;
    public Material DisabledObjetMaterial;

    private Dictionary<Transform, Material> TransformDisabled = new Dictionary<Transform, Material>();

    void Update()
    {
        var camRef = (transform.position - transform.up * 0.5f) - transform.forward;
        var ray = new Ray(camRef, transform.forward);
        Debug.DrawRay(ray.origin, ray.direction, Color.green);
        var distance = Vector3.Distance(camRef, MyCharacterController.Instance.transform.position - MyCharacterController.Instance.transform.forward * 0.1f);
        var hits = Physics.RaycastAll(ray, distance, Mask);

        foreach (var transformDisabled in TransformDisabled.ToList())
        {
            if (!hits.Select(h => h.transform).Contains(transformDisabled.Key))
            {
                var meshRenderer = transformDisabled.Key.gameObject.GetComponent<Renderer>();
                meshRenderer.material = transformDisabled.Value;
                TransformDisabled.Remove(transformDisabled.Key);
            }
        }

        foreach (var hit in hits.Where(h => h.transform.tag != "SolidGround"))
        {
            if (!TransformDisabled.ContainsKey(hit.transform))
            {
                var meshRenderer = hit.transform.gameObject.GetComponent<Renderer>();
                TransformDisabled.Add(hit.transform, meshRenderer.material);
                meshRenderer.material = DisabledObjetMaterial;
            }
        }
    }
}
