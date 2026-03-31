using UnityEngine;

public class MotoController : MonoBehaviour
{
    [Header("Parâmetros de Referência")]
    public float velocidadeAlvo = 55.5f; // Aprox. 200 km/h
    public float estercoMaximo = 48f;    // Graus por segundo
    public float forcaFreio = 12f;
    public float aceleracao = 10f;

    [Header("Penalidades")]
    public float multiplicadorVelocidadeOffTrack = 0.65f; // -35%
    public float multiplicadorEstercoOffTrack = 0.75f;    // -25%

    // Variáveis de estado
    private float velocidadeAtual = 0f;
    private bool freando = false;
    private float inputLateral = 0f;
    private bool foraDaPista = false;

    private Rigidbody2D rb;

    void Start()
    {
        // Pega o componente de física que estará anexado à moto
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Captura os comandos do jogador (Roda a cada frame da tela)
        // Setas do teclado ou A/D (retorna -1 para esquerda, 1 para direita)
        inputLateral = Input.GetAxisRaw("Horizontal"); 
        
        // Barra de espaço ou S para frear
        freando = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.S); 
    }

    void FixedUpdate()
    {
        // A física deve ser calculada aqui (Roda em intervalos fixos de tempo)
        AplicarFisica();
    }

    void AplicarFisica()
    {
        // 1. Calcula os limites baseados em estar na pista ou na grama
        float velAlvoAtual = foraDaPista ? velocidadeAlvo * multiplicadorVelocidadeOffTrack : velocidadeAlvo;
        float estercoAtual = foraDaPista ? estercoMaximo * multiplicadorEstercoOffTrack : estercoMaximo;

        // 2. Acelera sozinho ou freia
        if (freando)
        {
            velocidadeAtual -= forcaFreio * Time.fixedDeltaTime;
        }
        else
        {
            velocidadeAtual += aceleracao * Time.fixedDeltaTime;
        }

        // Não deixa a velocidade ficar negativa nem passar da velocidade alvo
        velocidadeAtual = Mathf.Clamp(velocidadeAtual, 0, velAlvoAtual);

        // 3. Esterço (só vira se a moto tiver alguma velocidade)
        if (velocidadeAtual > 1f)
        {
            float rotacao = -inputLateral * estercoAtual * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation + rotacao);
        }

        // 4. Move a moto para frente (baseado na direção que o "bico" dela aponta)
        rb.linearVelocity = transform.up * velocidadeAtual;
    }

    // 5. Detecta colisões e saídas de pista usando Triggers
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("OffTrack"))
        {
            foraDaPista = true;
        }
        else if (collision.CompareTag("Barreira"))
        {
            // Perde 40 km/h instantaneamente (aprox. 11 m/s) em batidas
            velocidadeAtual = Mathf.Max(0, velocidadeAtual - 11f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("OffTrack"))
        {
            foraDaPista = false;
        }
    }
}