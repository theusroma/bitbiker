# BitBiker — Jogo de Corrida 2D Top-Down

Bem-vindo ao repositório do **BitBiker**! Este é um projeto de corrida 2D com visão superior (*top-down*) desenvolvido na **Unity**, inspirado em circuitos reais (como o traçado de Interlagos). O jogador pilota uma moto contra adversários controlados pela IA.

Este guia foi feito para que qualquer pessoa consiga baixar, rodar e contribuir com o desenvolvimento do projeto de forma rápida e organizada.

---

## Pré-requisitos

Antes de abrir o projeto, certifique-se de ter instalado no seu computador:
1. **Git** — Para baixar e enviar atualizações do código ([Baixar Git](https://git-scm.com/)).
2. **Unity Hub & Unity Editor** — O projeto utiliza a **Unity 6 (6000.3.10f1)** com **Universal Render Pipeline (URP)** e o pacote **New Input System**. Ao abrir pelo Unity Hub, ele indicará se você precisa instalar exatamente essa versão.

---

## Como Rodar o Projeto na Sua Máquina

Siga este passo a passo para abrir o jogo no seu computador:

### 1. Clonar o repositório
Abra o terminal (CMD, PowerShell ou Git Bash) na pasta onde você costuma salvar seus projetos e rode:
```bash
git clone https://github.com/theusroma/bitbiker.git
```
*(Verifique se o link acima corresponde exatamente à URL do repositório no GitHub).*

### 2. Abrir no Unity Hub
1. Abra o **Unity Hub**.
2. Clique no botão **Add** (ou *Open* > *Add project from disk*).
3. Selecione a pasta `bitbiker` que você acabou de baixar.
4. Clique no projeto na lista para abri-lo. *(A primeira abertura pode demorar alguns minutos enquanto a Unity gera a pasta `Library` e compila os shaders).*

### 3. Abrir a Cena Principal e Jogar
1. Dentro da Unity, vá na aba **Project** (parte inferior) e entre em: `Assets > Scenes`.
2. Dê um duplo clique no arquivo **`SampleScene`**.
3. Aperte o botão **Play (▶)** no topo da tela para testar!

---

## Estrutura do Projeto (Onde está cada coisa)

Para facilitar a navegação de quem está chegando agora, os arquivos principais estão organizados dentro da pasta `Assets/`:

* **`Assets/Scenes/`**: Contém a cena principal do jogo (`SampleScene.unity`).
* **`Assets/Scripts/`**: Toda a programação em C# do jogo:
  * `MotoController.cs`: Controla a movimentação, aceleração, curvas e física da moto do jogador.
  * `BotController.cs`: Controla o comportamento e a navegação dos pilotos adversários (IA).
  * `PilotoData.cs`: Estrutura de dados (*ScriptableObject*) que define os atributos individuais de cada rival (velocidade, agressividade, cor, etc.).
  * `PontoDaPista.cs` & `RotaVisualizador.cs`: Sistema de *waypoints* (pontos de referência) que guia os bots pelo traçado da pista.
  * `GameManager.cs`: Gerencia o estado geral da corrida e o cronômetro.
  * `CameraFollow.cs`: Faz a câmera acompanhar a moto do jogador suavemente.
* **`Assets/Pilotos/`**: Fichas individuais dos 9 rivais (`Rival_Amarelo`, `Rival_Azul`, `Rival_Preto`, etc.), onde é possível ajustar a dificuldade/velocidade de cada um direto pelo *Inspector* da Unity.
* **`Assets/Sprites/`**: Artes da moto, asfalto, zebras e bandeira quadriculada.
* **`Assets/InputSystem_Actions.inputactions`**: Configuração dos controles (teclado/controle).

---

##  O que já funciona vs. O que falta para completar o jogo

O núcleo jogável já está de pé: a pista está montada, o jogador controla a moto, a câmera segue o movimento, o cronômetro funciona e os 9 bots já seguem os pontos da pista.

Abaixo está o **Roadmap (Lista de Tarefas)** do que precisa ser ajustado ou criado para o jogo ficar completo:

### 1. Física e Dirigibilidade (`MotoController.cs`)
* [ ] **Calibrar Aceleração, Freio e Inércia:** Ajustar o peso da moto para que as curvas e frenagens pareçam mais naturais e responsivas (evitar que pareça deslizar demais no gelo ou parar instantaneamente).
* [ ] **Detecção e Penalidade de Grama (Off-Road):** Configurar corretamente o atrito e a redução de velocidade máxima quando a moto sair do asfalto e pisar na grama/brita.
* [ ] **Limites da Pista:** Adicionar colisores nas barreiras/muros externos para impedir que as motos saiam voando para fora do mapa.

### 2. Inteligência Artificial dos Bots (`BotController.cs` e `PontoDaPista.cs`)
* [ ] **Melhorar o Mapeamento da Pista:** Refinar o posicionamento dos *waypoints* (`PontoDaPista`) nas curvas fechadas (como o S do Senna e o Laranjinha) para os bots fazerem tangências melhores.
* [ ] **Frenagem Antes das Curvas:** Fazer os bots reduzirem a velocidade antes de curvas fechadas em vez de passarem reto ou virarem de forma robótica.
* [ ] **Desvio e Ultrapassagem:** Evitar que os bots fiquem enfileirados exatamente na mesma linha ou batam uns nos outros sem tentar desviar.

### 3. Regras de Corrida (`GameManager.cs`)
* [ ] **Sistema de Voltas e Checkpoints:** Criar setores invisíveis ao longo da pista para validar a volta completa (impedindo que o jogador corte caminho) e encerrar a corrida após X voltas.
* [ ] **Ranking em Tempo Real:** Calcular e exibir quem está em 1º, 2º, 3º... até 10º lugar durante a corrida.
* [ ] **Largada:** Adicionar semáforo de largada (luzes vermelhas apagando) para segurar as motos antes do início oficial.

### 4. Interface de Usuário (UI / HUD)
* [ ] **HUD de Corrida:** Adicionar velocímetro, contador de voltas (`Volta 1/3`), posição atual (`1º/10`) e melhorar o visual do cronômetro.
* [ ] **Menus:** Criar Menu Inicial (Jogar / Sair), Menu de Pausa (`ESC`) e Tela de Resultados / Pódio ao cruzar a linha de chegada na última volta.

### 5. Efeitos Visuais (VFX) e Sonoros (SFX)
* [ ] **Áudio:** Adicionar som de motor (variando o *pitch*/rotação conforme a velocidade), som de derrapagem/freio, colisão entre motos e música de fundo/menu.
* [ ] **Partículas e Feedback Visual:** Criar rastros de pneu no asfalto (*Trail Renderer*), fumaça/poeira ao passar na grama e faíscas ou tremor leve de câmera em batidas.

---

## Como Contribuir (Fluxo Básico de Git)

Para evitar conflitos nos arquivos da Unity, siga sempre esta ordem ao trabalhar no projeto:

1. **Antes de começar a mexer**, baixe o que seus amigos fizeram:
   ```bash
   git pull
   ```
2. Faça suas alterações na Unity e lembre-se de salvar a cena (`Ctrl + S`).
3. **Para enviar suas mudanças para o GitHub:**
   ```bash
   git add .
   git commit -m "Descreva brevemente o que voce fez"
   git push
   ```
