# CAPY CHAOS

Protótipo de um **runner infinito em Unity/C#**, com uma capivara como protagonista e cenários inspirados no Brasil.

O projeto explora controles por teclado e toque, geração procedural de pista, colisões, coleta de moedas e pontuação. É um projeto de portfólio em desenvolvimento.

![Direção visual do CAPY CHAOS](Assets/Art/Concept/CapyChaos_VisualDirection_v1.png)

## O que encontrar no código

- Separação entre sessão de jogo, entrada, personagem, ambiente e interface.
- Pista procedural e alternância de temas brasileiros.
- Troca de faixas, salto, deslize e habilidade Zoomies.
- Regras de pontuação e validações disponíveis no editor.

## Executar

1. Instale o Unity Hub e a versão **6000.5.8f1**, registrada em `ProjectSettings/ProjectVersion.txt`.
2. Adicione a pasta do repositório como projeto no Unity Hub.
3. Aguarde a importação e a resolução dos pacotes.
4. Abra `Assets/Scenes/Prototype.unity` e pressione **Play**.

| Ação | Teclado |
| --- | --- |
| Trocar de faixa | A/D ou setas esquerda/direita |
| Pular | W ou seta para cima |
| Deslizar | S ou seta para baixo |
| Testar Zoomies | Shift esquerdo |
| Reiniciar após a colisão | R ou Espaço |

No toque, deslize para os lados, para cima ou para baixo.

## Estrutura

```text
Assets/Art/Concept/   direção visual e conceitos
Assets/Scenes/       cena do protótipo
Assets/Scripts/      lógica organizada por responsabilidade
Assets/Editor/       validações do protótipo
Packages/            dependências Unity
ProjectSettings/     configuração versionada do editor
```

## Branches e estado

- `main`: primeira versão jogável do protótipo.
- `codex/gameplay-lock`: evolução com progressão de dificuldade, pool de objetos, persistência de progresso e integração de ranking Unity. Essas mudanças ainda não estão integradas à `main`.

As artes conceituais orientam o visual pretendido; não representam uma captura da qualidade gráfica atual do jogo.

A integração online da branch de evolução depende da configuração dos Unity Gaming Services do responsável. Para utilizar seus próprios serviços, configure seu próprio projeto; identificadores de projeto não substituem autenticação nem são credenciais administrativas.

## Validação

No editor, execute **CAPY CHAOS → Validar Protótipo**. As verificações ficam em `Assets/Editor/PrototypeValidation.cs` e variam conforme a branch.

As instruções descrevem as verificações existentes no código. A preparação deste README não executou o Unity, os testes do editor ou builds Android/iOS.

## Autor

[Lucas Albertinazzi](https://github.com/LucasAlbertinazzi) · [LinkedIn](https://www.linkedin.com/in/LucasAlbertinazzi)
