using UnityEngine;

public class LookATPlayer : MonoBehaviour
{
    public Transform camera;


    void LateUpdate()
    {
        transform.LookAt(camera);
    }
}
