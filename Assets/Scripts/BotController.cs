using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("Identidade do Piloto")]
    public PilotoData perfilPiloto; // A caixinha onde você vai jogar a Ficha!

    [Header("Inteligência Artificial (O GPS)")]
    public Transform rotaParent; // A caixinha do "RotaBots" com os pontos da pista
    public float distanciaPraTrocarPonto = 15f; 

    // Variáveis do motor (Escondidas porque vão ser preenchidas pela ficha)
    private float velocidadeMaxima; 
    private float aceleracaoMotor;
    private float forcaFreio;
    private int habilidadeNaCurva;

    [Header("Penalidades da Grama")]
    public float multiplicadorVelocidadeOffTrack = 0.3f;
    public float multiplicadorEstercoOffTrack = 0.4f;

    // Variáveis de estado
    private float velocidadeAtual = 0f;
    private int pedacosDePista = 0;
    private bool foraDaPista = true;
    private Rigidbody2D rb;

    // Variáveis de GPS
    private Transform[] waypoints;
    private int indicePontoAtual = 0;
    private float inputLateralAI = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 1. O Bot "lê" a ficha antes de acelerar
        CarregarPerfil();

        // 2. O Bot "decora" onde estão os pontos da pista
        if (rotaParent != null)
        {
            waypoints = new Transform[rotaParent.childCount];
            for (int i = 0; i < rotaParent.childCount; i++)
            {
                waypoints[i] = rotaParent.GetChild(i);
            }
        }
    }

    void CarregarPerfil()
    {
        // Confere se você não esqueceu de colocar a ficha no Unity
        if (perfilPiloto != null)
        {
            // Puxa os status da Ficha para o Motor
            velocidadeMaxima = perfilPiloto.velocidadeMaxima;
            aceleracaoMotor = perfilPiloto.aceleracaoMotor;
            forcaFreio = perfilPiloto.forcaFreio;
            habilidadeNaCurva = perfilPiloto.habilidadeNaCurva;

            // Renomeia o objeto do Bot automaticamente
            gameObject.name = "Bot_" + perfilPiloto.nomeDoPiloto;

            // Pinta a moto com a cor da equipe
            SpriteRenderer spriteMoto = GetComponent<SpriteRenderer>();
            if (spriteMoto != null)
            {
                spriteMoto.color = perfilPiloto.corDaMoto;
            }
        }
    }

    void FixedUpdate()
    {
        PensarPraOndeIr();
        AplicarFisica();
    }

    void PensarPraOndeIr()
    {
        // Se não tem pista, ele não faz nada
        if (waypoints == null || waypoints.Length == 0) return;

        Transform alvoAtual = waypoints[indicePontoAtual];

        // Mede a distância até a próxima "migalha de pão"
        float distancia = Vector2.Distance(transform.position, alvoAtual.position);
        if (distancia < distanciaPraTrocarPonto)
        {
            // Pula para o próximo ponto
            indicePontoAtual = (indicePontoAtual + 1) % waypoints.Length;
            alvoAtual = waypoints[indicePontoAtual];
        }

        // Vira o guidão na direção do ponto
        Vector2 direcaoProAlvo = (alvoAtual.position - transform.position).normalized;
        float angulo = Vector2.SignedAngle(transform.up, direcaoProAlvo);

        // Define a força de virar a moto (-1 pra esquerda, 1 pra direita)
        inputLateralAI = Mathf.Clamp(-angulo / 45f, -1f, 1f);
    }

    void AplicarFisica()
    {
        foraDaPista = (pedacosDePista <= 0);
        float velMaxAtual = foraDaPista ? velocidadeMaxima * multiplicadorVelocidadeOffTrack : velocidadeMaxima;

        // O Bot acelera sozinho sem parar
        velocidadeAtual += aceleracaoMotor * Time.fixedDeltaTime;
        velocidadeAtual = Mathf.Clamp(velocidadeAtual, 0, velMaxAtual);

        if (velocidadeAtual > 1f)
        {
            // Para simplificar, o bot vira num valor fixo de 80 graus, mas reduz na grama
            float estercoAtual = foraDaPista ? 80f * multiplicadorEstercoOffTrack : 80f;
            float rotacao = -inputLateralAI * estercoAtual * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation + rotacao);
        }

        // Move a moto
        rb.linearVelocity = transform.up * velocidadeAtual;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Pista")) pedacosDePista++;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Pista")) pedacosDePista--;
    }
}