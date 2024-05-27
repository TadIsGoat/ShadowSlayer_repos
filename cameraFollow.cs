// Tadeáš Vykopal, 3.B, PVA, Shadow Slayer

using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    public Transform target;
    [SerializeField] float followSpeed = 100f;
    public float yOffset = 1f;

    void Update()
    {
        try
        {
            Vector3 newPos = new Vector3(target.position.x, target.position.y + yOffset, -10f);
            transform.position = Vector3.Slerp(transform.position, newPos, followSpeed * Time.deltaTime);
        }
        catch
        {

        }
    }
}
