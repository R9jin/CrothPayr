using UnityEngine;

public class RoomSpawnPoint : MonoBehaviour
{
    [Tooltip("Room index (1 to 4)")]
    public int roomIndex = 1;

    [Tooltip("Descriptive name of the room")]
    public string roomName = "Room 1";

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.8f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}
