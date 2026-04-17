using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("Identidade do Piloto")]
    public PilotoData perfilPiloto;

    [Header("Inteligência Artificial (O GPS)")]
    public Transform rotaParent;
    public float distanciaPraTrocarPonto = 15f; 

    // Variáveis do motor
    private float velocidadeMaxima; 
    private float aceleracaoMotor;
    private float forcaFreio;
    private int habilidadeNaCurva;

    [Header("Direção Dinâmica")]
    public float estercoBaixaVelocidade = 120f;
    public float estercoAltaVelocidade = 50f;

    [Header("Penalidades da Grama")]
    public float multiplicadorVelocidadeOffTrack = 0.3f;
    public float multiplicadorEstercoOffTrack = 0.4f;

    // Variáveis de estado
    private float velocidadeAtual = 0f;
    private int pedacosDePista = 0;
    private bool foraDaPista = true;
    private Rigidbody2D rb;

    // Variáveis da IA e do Freio
    private Transform[] waypoints;
    private int indicePontoAtual = 0;
    private float inputLateralAI = 0f;
    private bool botFreando = false;

    private float meuDesvioNaPista; // Cada bot terá o seu corredor

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        CarregarPerfil();

        if (rotaParent != null)
        {
            waypoints = new Transform[rotaParent.childCount];
            for (int i = 0; i < rotaParent.childCount; i++)
            {
                waypoints[i] = rotaParent.GetChild(i);
            }
        }
        
        meuDesvioNaPista = Random.Range(-2f, 2f);
    }

    void CarregarPerfil()
    {
        if (perfilPiloto != null)
        {
            velocidadeMaxima = perfilPiloto.velocidadeMaxima;
            aceleracaoMotor = perfilPiloto.aceleracaoMotor;
            forcaFreio = perfilPiloto.forcaFreio;
            habilidadeNaCurva = perfilPiloto.habilidadeNaCurva;

            gameObject.name = "Bot_" + perfilPiloto.nomeDoPiloto;

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
        if (waypoints == null || waypoints.Length == 0) return;

        Transform alvoAtual = waypoints[indicePontoAtual];
        int indiceProximo = (indicePontoAtual + 1) % waypoints.Length;
        Transform alvoFuturo = waypoints[indiceProximo];

        Vector2 direcaoDaPista = alvoAtual.up; 
        Vector2 posicaoComDesvio = (Vector2)alvoAtual.position + (Vector2.Perpendicular(direcaoDaPista) * meuDesvioNaPista);

        Vector2 direcaoProAlvo = (posicaoComDesvio - (Vector2)transform.position).normalized;
        float angulo = Vector2.SignedAngle(transform.up, direcaoProAlvo);
        float distancia = Vector2.Distance(transform.position, posicaoComDesvio);

        if (distancia < distanciaPraTrocarPonto || (Mathf.Abs(angulo) > 90f && distancia < distanciaPraTrocarPonto * 2f))
        {
            indicePontoAtual = indiceProximo; 
            alvoAtual = waypoints[indicePontoAtual];
            
            direcaoDaPista = alvoAtual.up; 
            posicaoComDesvio = (Vector2)alvoAtual.position + (Vector2.Perpendicular(direcaoDaPista) * meuDesvioNaPista);

            direcaoProAlvo = (posicaoComDesvio - (Vector2)transform.position).normalized;
            angulo = Vector2.SignedAngle(transform.up, direcaoProAlvo);
        }

        inputLateralAI = Mathf.Clamp(-angulo / 30f, -1f, 1f);

        // ==========================================
        // AJUSTE DE BALANCEAMENTO (Freiam na hora certa)
        // Antes era 15f a 50f. Agora é 35f a 50f.
        // ==========================================
        float limiteParaFrear = Mathf.Lerp(35f, 50f, habilidadeNaCurva / 10f);
        
        bool freandoProAlvoAtual = Mathf.Abs(angulo) > limiteParaFrear;

        Vector2 direcaoCurvaFutura = (alvoFuturo.position - alvoAtual.position).normalized;
        float anguloCurvaFutura = Vector2.Angle(direcaoProAlvo, direcaoCurvaFutura);

        bool freandoPraCurvaFutura = (anguloCurvaFutura > limiteParaFrear) && (distancia < 50f);

        botFreando = freandoProAlvoAtual || freandoPraCurvaFutura;
    }

    void AplicarFisica()
    {
        foraDaPista = (pedacosDePista <= 0);
        float velMaxAtual = foraDaPista ? velocidadeMaxima * multiplicadorVelocidadeOffTrack : velocidadeMaxima;

        // ==========================================
        // SISTEMA DE ERRO HUMANO (DISTRACÃO)
        // ==========================================
        float chanceDeErro = (10f - habilidadeNaCurva) * 0.1f; 
        bool teveUmBranco = Random.Range(0f, 100f) < chanceDeErro;

        // O Pedal de Freio do Bot
        if (botFreando && !teveUmBranco) 
        {
            velocidadeAtual -= forcaFreio * Time.fixedDeltaTime;
        }
        else
        {
            velocidadeAtual += aceleracaoMotor * Time.fixedDeltaTime;
        }

        velocidadeAtual *= 1f - (Mathf.Abs(inputLateralAI) * 0.2f * Time.fixedDeltaTime);
        velocidadeAtual = Mathf.Clamp(velocidadeAtual, 30f, velMaxAtual);

        if (velocidadeAtual > 1f)
        {
            float porcentagemVelocidade = velocidadeAtual / velocidadeMaxima;
            float estercoAtual = Mathf.Lerp(estercoBaixaVelocidade, estercoAltaVelocidade, porcentagemVelocidade);

            if (foraDaPista) estercoAtual *= multiplicadorEstercoOffTrack;

            float rotacao = -inputLateralAI * estercoAtual * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation + rotacao);
        }

        Vector2 direcaoInercia = rb.linearVelocity.normalized;
        if (rb.linearVelocity.magnitude < 0.1f) direcaoInercia = transform.up;

        float aderenciaPneu = 5f; 

        Vector2 direcaoReal = Vector3.Slerp(direcaoInercia, transform.up, aderenciaPneu * Time.fixedDeltaTime);
        rb.linearVelocity = direcaoReal * velocidadeAtual;
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