using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("Identidade do Piloto")]
    public PilotoData perfilPiloto;

    [Header("Inteligência Artificial (O GPS)")]
    public Transform rotaParent;
    // A variável "distanciaPraTrocarPonto" foi removida. O bot agora lê do script PontoDaPista!

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

        // ==========================================
        // SMART START: Acha o ponto mais próximo!
        // ==========================================
        if (waypoints != null && waypoints.Length > 0)
        {
            float menorDistancia = Mathf.Infinity;
            for (int i = 0; i < waypoints.Length; i++)
            {
                float distancia = Vector2.Distance(transform.position, waypoints[i].position);
                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    indicePontoAtual = i; // Define o alvo inicial como o ponto mais perto
                }
            }
        }
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

        // ==========================================
        // LEITURA DO RAIO INDIVIDUAL DO PONTO
        // ==========================================
        float distanciaDestePonto = 15f; // Valor padrão de segurança
        PontoDaPista infoDoPonto = alvoAtual.GetComponent<PontoDaPista>();
        if (infoDoPonto != null)
        {
            distanciaDestePonto = infoDoPonto.raioDeCaptura;
        }

        // ==========================================
        // SISTEMA ANTI-RODOPIO (Usa o raio dinâmico)
        // ==========================================
        if (distancia < distanciaDestePonto || (Mathf.Abs(angulo) > 90f && distancia < distanciaDestePonto * 2f))
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
        // FRENAGEM DINÂMICA E ANTECIPADA (LOOK-AHEAD)
        // ==========================================
        float limiteParaFrear = Mathf.Lerp(35f, 50f, habilidadeNaCurva / 10f);
        bool freandoProAlvoAtual = Mathf.Abs(angulo) > limiteParaFrear;

        Vector2 direcaoCurvaFutura = (alvoFuturo.position - alvoAtual.position).normalized;
        float anguloCurvaFutura = Vector2.Angle(direcaoProAlvo, direcaoCurvaFutura);

        // A moto calcula o espaço que precisa baseada na velocidade atual
        float distanciaNecessariaPraFrear = Mathf.Max(50f, velocidadeAtual * 0.4f); 

        bool freandoPraCurvaFutura = (anguloCurvaFutura > limiteParaFrear) && (distancia < distanciaNecessariaPraFrear);

        botFreando = freandoProAlvoAtual || freandoPraCurvaFutura;
    }

    void AplicarFisica()
    {
        foraDaPista = (pedacosDePista <= 0);
        float velMaxAtual = foraDaPista ? velocidadeMaxima * multiplicadorVelocidadeOffTrack : velocidadeMaxima;

        // ==========================================
        // SISTEMA DE ERRO HUMANO (DISTRAÇÃO)
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
        
        // ==========================================
        // CONVERSÃO KM/H PARA METROS POR SEGUNDO
        // ==========================================
        float velocidadeEmMetrosPorSegundo = velocidadeAtual / 3.6f;
        
        rb.linearVelocity = direcaoReal * velocidadeEmMetrosPorSegundo;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Pista")) pedacosDePista++;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Pista")) pedacosDePista--;
    }

    // ==========================================
    // RAIO-X DO DESENVOLVEDOR (Gizmos)
    // ==========================================
    private void OnDrawGizmosSelected()
    {
        // 1. O Radar de Freio (Círculo Vermelho)
        // Se o jogo estiver rodando, ele mostra o tamanho real do freio. Se estiver parado, mostra o mínimo (50).
        float distanciaDeFrenagem = Mathf.Max(50f, velocidadeAtual * 1.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDeFrenagem);

        // 2. O Foco da Visão (Linha Verde) - Só aparece com o jogo rodando (Play)
        if (Application.isPlaying && waypoints != null && waypoints.Length > 0)
        {
            Transform alvoAtual = waypoints[indicePontoAtual];
            if (alvoAtual != null)
            {
                // Calcula o alvo exato com o desvio do corredor dele
                Vector2 direcaoDaPista = alvoAtual.up; 
                Vector2 posicaoComDesvio = (Vector2)alvoAtual.position + (Vector2.Perpendicular(direcaoDaPista) * meuDesvioNaPista);

                // Desenha a linha verde ligando a moto ao alvo
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, posicaoComDesvio);
                Gizmos.DrawSphere(posicaoComDesvio, 1.5f); // Uma bolinha no final do laser
            }
        }
    }

}