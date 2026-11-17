using UnityEngine;

public class MotoController : MonoBehaviour
{
    [Header("Motor e Transmissão (Auto-Runner)")]
    public float velocidadeMaxima = 180f; 
    public float aceleracaoMotor = 40f;   // O quão rápido ela atinge os 180
    public float forcaFreio = 100f;       // Freio precisa ser mais forte agora que ela acelera sozinha

    [Header("Direção Dinâmica")]
    public float estercoBaixaVelocidade = 120f; 
    public float estercoAltaVelocidade = 50f;   

    [Header("Penalidades da Grama")]
    public float multiplicadorVelocidadeOffTrack = 0.3f;
    public float multiplicadorEstercoOffTrack = 0.4f;

    // Variáveis de estado
    private float velocidadeAtual = 0f;
    private float inputLateral = 0f;
    private bool freando = false;

    private int pedacosDePista = 0;
    private bool foraDaPista = true;

    private Rigidbody2D rb;
    private float inputLateralMobile = 0f;
    private bool freandoMobile = false;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

void Update()
    {
        // HÍBRIDO: Permite testar no PC com teclado OU jogar no celular com os toques
        inputLateral = Input.GetAxisRaw("Horizontal"); 
        if (inputLateral == 0) // Se não tocou no teclado, usa o mobile
        {
            inputLateral = inputLateralMobile;
        }
        
        // Freia se apertar Espaço OU se o dedo estiver no botão na tela
        freando = Input.GetKey(KeyCode.Space) || freandoMobile; 
    }

    void FixedUpdate()
    {
        AplicarFisica();
    }

    void AplicarFisica()
    {
        foraDaPista = (pedacosDePista <= 0);

        float velMaxAtual = foraDaPista ? velocidadeMaxima * multiplicadorVelocidadeOffTrack : velocidadeMaxima;

        // ==========================================
        // ETAPA 1: O PEDAL DE FREIO DO JOGADOR
        // ==========================================
        if (freando)
        {
            velocidadeAtual -= forcaFreio * Time.fixedDeltaTime;
        }
        else
        {
            velocidadeAtual += aceleracaoMotor * Time.fixedDeltaTime;
        }

        // ==========================================
        // ETAPA 2: O FREIO MOTOR (Atrito na Curva)
        // ==========================================
        // Quando você vira o guidão no máximo (inputLateral = 1 ou -1), a moto perde um pouco do embalo pela física
        velocidadeAtual *= 1f - (Mathf.Abs(inputLateral) * 0.2f * Time.fixedDeltaTime);

        // Trava a velocidade (O mínimo é 30 para a moto não parar do nada no meio da curva)
        velocidadeAtual = Mathf.Clamp(velocidadeAtual, 30f, velMaxAtual);


        // ==========================================
        // ETAPA 3: ESTERÇO (VIRAR O GUIDÃO)
        // ==========================================
        if (velocidadeAtual > 1f)
        {
            float porcentagemVelocidade = velocidadeAtual / velocidadeMaxima;
            float estercoAtual = Mathf.Lerp(estercoBaixaVelocidade, estercoAltaVelocidade, porcentagemVelocidade);

            if (foraDaPista) 
            {
                estercoAtual *= multiplicadorEstercoOffTrack;
            }

            float rotacao = -inputLateral * estercoAtual * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation + rotacao);
        }

        // ==========================================
        // ETAPA 4: INÉRCIA E ADERÊNCIA (O SIMULADOR)
        // ==========================================
        
        // Descobre pra onde a moto estava "voando" por inércia
        Vector2 direcaoInercia = rb.linearVelocity.normalized;
        if (rb.linearVelocity.magnitude < 0.1f) direcaoInercia = transform.up;

        // GRIP DO PNEU: Tente valores entre 3 (escorrega como gelo) e 8 (gruda bem).
        float aderenciaPneu = 6f; 

        // Mistura a inércia com o guidão
        Vector2 direcaoReal = Vector3.Slerp(direcaoInercia, transform.up, aderenciaPneu * Time.fixedDeltaTime);

        // Converte a velocidade de Km/h para Metros por Segundo pro Unity entender a física realista
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
    // ETAPA 4: FUNÇÕES PARA A TELA DE TOQUE
    // ==========================================
    public void TocarEsquerda() { inputLateralMobile = -1f; }
    public void TocarDireita() { inputLateralMobile = 1f; }
    public void SoltarDirecao() { inputLateralMobile = 0f; }

    public void TocarFreio() { freandoMobile = true; }
    public void SoltarFreio() { freandoMobile = false; }

}


