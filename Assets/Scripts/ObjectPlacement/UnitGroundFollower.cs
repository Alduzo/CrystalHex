using UnityEngine;

public class UnitGroundFollower : MonoBehaviour
{
    [SerializeField] private string terrainLayer = "Terrain";
    [SerializeField] private float heightOffset = 1f;

    [SerializeField] private float rotationSmoothSpeed = 5f; // puedes ajustar el valor


    void LateUpdate()
    {
        Vector3 origin = new Vector3(transform.position.x, 100f, transform.position.z);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 200f, LayerMask.GetMask(terrainLayer)))
        {
            // Colocar sobre el terreno
            transform.position = hit.point + hit.normal * heightOffset;

            // Alinear con la pendiente
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
        }
    }
}
