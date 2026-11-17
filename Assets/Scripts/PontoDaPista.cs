using UnityEngine;

public class PontoDaPista : MonoBehaviour
{
    [Tooltip("Distância que o bot reconhece este ponto e passa para o próximo")]
    public float raioDeCaptura = 15f;

    // Essa mágica desenha uma esfera colorida no Unity para você VER o tamanho do raio!
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, raioDeCaptura);
    }
}