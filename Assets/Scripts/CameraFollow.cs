using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform alvo; // A sua moto
    public Vector3 offset = new Vector3(0, 0, -10); // Mantém a câmera recuada em 2D

    void LateUpdate()
    {
        if (alvo != null)
        {
            // A câmera copia a posição da moto, mas mantém o recuo (offset)
            transform.position = alvo.position + offset;
        }
    }
}