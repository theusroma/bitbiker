using UnityEngine;

// Essa linha mágica cria um botão no Unity para você gerar novos pilotos!
[CreateAssetMenu(fileName = "Novo Piloto", menuName = "Corrida/Ficha de Piloto")]
public class PilotoData : ScriptableObject
{
    [Header("Informações do Piloto")]
    public string nomeDoPiloto;
    public string nomeDaEquipe;
    public Color corDaMoto = Color.white; // Para mudar a cor da moto automaticamente!

    [Header("Motor da Moto (Status)")]
    public float velocidadeMaxima = 175f;
    public float aceleracaoMotor = 35f;
    public float forcaFreio = 100f;

    [Header("Habilidade (1 a 10)")]
    [Tooltip("10 = Curvas perfeitas. 1 = Erra muito e sai da pista.")]
    [Range(1, 10)]
    public int habilidadeNaCurva = 5;
}