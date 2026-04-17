using UnityEngine;

public class RotaVisualizador : MonoBehaviour
{
    // Escolha a cor da linha no Unity!
    public Color corDaLinha = Color.yellow;
    public float tamanhoDaBolinha = 1f;

    // Essa função mágica do Unity desenha coisas que só aparecem na tela de edição
    private void OnDrawGizmos()
    {
        Gizmos.color = corDaLinha;

        Transform[] pontos = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            pontos[i] = transform.GetChild(i);
        }

        // Desenha uma linha ligando o Ponto A ao Ponto B, B ao C...
        for (int i = 0; i < pontos.Length; i++)
        {
            Transform pontoAtual = pontos[i];
            Transform proximoPonto = pontos[(i + 1) % pontos.Length]; // O % faz o último ligar de volta no primeiro

            // Desenha a bolinha e a linha
            Gizmos.DrawSphere(pontoAtual.position, tamanhoDaBolinha);
            Gizmos.DrawLine(pontoAtual.position, proximoPonto.position);
        }
    }
}