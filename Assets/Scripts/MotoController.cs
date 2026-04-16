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
        // ETAPA 1: ACELERAÇÃO AUTOMÁTICA E FRENAGEM
        // ==========================================
        
        if (freando)
        {
            // O jogador apertou o freio, a moto luta contra o motor
            velocidadeAtual -= forcaFreio * Time.fixedDeltaTime;
        }
        else
        {
            // ACELERADOR TRAVADO! Se não está freando, ganha velocidade o tempo todo.
            velocidadeAtual += aceleracaoMotor * Time.fixedDeltaTime;
        }

        // Trava a velocidade entre 0 e a máxima
        velocidadeAtual = Mathf.Clamp(velocidadeAtual, 0, velMaxAtual);


        // ==========================================
        // ETAPA 2: ESTERÇO BASEADO NA VELOCIDADE
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
        // ETAPA 3: MOVE A MOTO
        // ==========================================
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

    // ==========================================
    // ETAPA 4: FUNÇÕES PARA A TELA DE TOQUE
    // ==========================================
    public void TocarEsquerda() { inputLateralMobile = -1f; }
    public void TocarDireita() { inputLateralMobile = 1f; }
    public void SoltarDirecao() { inputLateralMobile = 0f; }

    public void TocarFreio() { freandoMobile = true; }
    public void SoltarFreio() { freandoMobile = false; }

}


