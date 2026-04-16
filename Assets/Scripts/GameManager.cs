using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Referências")]
    public Rigidbody2D motoRb; 
    public TextMeshProUGUI textoVelocidade;
    public TextMeshProUGUI textoTempo;

    private float tempoDecorrido = 0f;
    private bool corridaRolando = false;

    void Start()
    {
        // Força a corrida a começar assim que der o Play para testarmos!
        IniciarCorrida();
    }

    void Update()
    {
        // 1. Atualiza o Velocímetro
        if (motoRb != null && textoVelocidade != null)
        {
            // Usando linearVelocity (padrão do Unity 6 que você está usando)
            float velKmH = motoRb.linearVelocity.magnitude * 3.6f;
            textoVelocidade.text = Mathf.RoundToInt(velKmH).ToString() + " km/h";
        }

        // 2. Atualiza o Cronômetro
        if (corridaRolando && textoTempo != null)
        {
            tempoDecorrido += Time.deltaTime;
            
            int minutos = Mathf.FloorToInt(tempoDecorrido / 60F);
            int segundos = Mathf.FloorToInt(tempoDecorrido - minutos * 60);
            int milissegundos = Mathf.FloorToInt((tempoDecorrido - minutos * 60 - segundos) * 1000);
            
            textoTempo.text = string.Format("{0:00}:{1:00}:{2:000}", minutos, segundos, milissegundos);
        }
    }

    public void IniciarCorrida() 
    { 
        corridaRolando = true; 
        tempoDecorrido = 0f; 
    }
    
    public void PararCorrida() 
    { 
        corridaRolando = false; 
    }
}