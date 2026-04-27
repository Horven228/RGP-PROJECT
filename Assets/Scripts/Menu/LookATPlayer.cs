using UnityEngine;

// чтобы полоска здоровья врага всегда смотрела на игрока
public class LookATPlayer : MonoBehaviour
{
    public Transform camera;


    void LateUpdate()
    {
        transform.LookAt(camera);
    }
}
