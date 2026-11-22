
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeradorPedidoAleatorio : MonoBehaviour
{
    public enum Distancia
    {
        Curta,
        Longa
    }

    [Serializable]
    public class Produto
    {
        public string nome;
        public GameObject prefab;

        [Tooltip("Imagem da folha para 1 unidade.")]
        public Sprite demanda1;

        [Tooltip("Imagem da folha para 2 unidades.")]
        public Sprite demanda2;

        [Tooltip("Imagem da folha para 3 unidades.")]
        public Sprite demanda3;

        public Sprite Imagem(int qtd)
        {
            return qtd == 1 ? demanda1 :
                   qtd == 2 ? demanda2 :
                   qtd == 3 ? demanda3 : null;
        }
    }

    [Serializable]
    public class Entregador
    {
        public string nome;
        public Sprite imagemCompleta;
        public Sprite fotoMontagem;

        public bool atendeCurta;
        public bool atendeLonga;

        [Min(1)]
        public int tempoPreparacaoSegundos = 90;
    }

    [Serializable]
    public class Item
    {
        public Produto produto;
        public int quantidade;
        public Sprite imagem;
    }

    [Serializable]
    public class FolhaUI
    {
        public TMP_Text nomeCliente;
        public Image imagemDistancia;
        public Image imagemTrajeto;
        public Image demanda1;
        public Image demanda2;
        public Image imagemEntregador;
        public RectTransform demandasContainer;
    }

    private class Pedido
    {
        public string cliente;
        public Distancia distancia;
        public Entregador entregador;

        public List<Item> itens = new List<Item>();
    }

    [Header("Cadastros")]
    [SerializeField] private Produto[] produtos;
    [SerializeField] private Entregador[] entregadores;

    [Header("Arquivo de nomes dos clientes")]
    [Tooltip(
        "Arraste aqui o arquivo NomesClientes.txt. " +
        "Compativel com Windows e WebGL."
    )]
    [SerializeField] private TextAsset arquivoNomesClientes;

    [Header("Imagens")]
    [SerializeField] private Sprite distanciaCurta;
    [SerializeField] private Sprite distanciaLonga;
    [SerializeField] private Sprite trajetoEcologico;

    [Header("Interface")]
    [SerializeField] private FolhaUI[] folhas = new FolhaUI[3];
    [SerializeField] private GameObject panelPedidos;
    [SerializeField] private GameObject panelMontagem;

    [Header("Montagem")]
    [SerializeField] private RectTransform areaProdutosEsquerda;
    [SerializeField] private RectTransform areaProdutosDireita;
    [SerializeField] private RectTransform areaCaixa;
    [SerializeField] private MountingManager mountingManager;

    [Header("Cronometro")]
    [SerializeField] private CronometroPreparacao cronometroPreparacao;

    [Header("Inicializacao")]
    [SerializeField] private bool gerarAoIniciar = true;

    [Header("Substituicao")]
    [SerializeField]
    private SistemaSubstituicaoEntregador sistemaSubstituicao;

    private List<string> clientes = new List<string>();

    private readonly List<Pedido> pedidos =
        new List<Pedido>();

    private readonly List<GameObject> objetosCriados =
        new List<GameObject>();

    private List<Item> pedido = new List<Item>();

    private int indicePedidoAtual = -1;

    public int TotalProdutosEsperados { get; private set; }

    public IReadOnlyList<Item> PedidoAtual => pedido;

    public IReadOnlyList<GameObject> ProdutosCriados =>
        objetosCriados;

    public Distancia DistanciaAtual { get; private set; }

    public Entregador EntregadorAtual { get; private set; }

    public int IndicePedidoAtual => indicePedidoAtual;

    private void Start()
    {
        CarregarClientes();

        // Mantem o funcionamento original.
        // O GameplayManager restaurara os pedidos salvos
        // quando a integracao estiver concluida.
        if (gerarAoIniciar)
            GerarNovoPedido();
    }

    private void CarregarClientes()
    {
        clientes.Clear();

        if (arquivoNomesClientes == null)
        {
            Debug.LogError(
                "NomesClientes.txt nao foi atribuido no Inspector!"
            );
            return;
        }

        try
        {
            clientes = arquivoNomesClientes.text
                .Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries
                )
                .Select(nome => nome.Trim())
                .Where(nome => nome.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (clientes.Count == 0)
            {
                Debug.LogError(
                    "NomesClientes.txt esta vazio ou " +
                    "nao possui nomes validos."
                );
                return;
            }

            Debug.Log(
                "Nomes dos clientes carregados: " +
                clientes.Count
            );
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "Erro ao carregar clientes: " +
                erro.Message
            );
        }
    }

    private List<Entregador> Compativeis(
        Distancia distancia)
    {
        return (entregadores ?? Array.Empty<Entregador>())
            .Where(e =>
                e != null &&
                e.imagemCompleta != null &&
                (distancia == Distancia.Curta
                    ? e.atendeCurta
                    : e.atendeLonga)
            )
            .ToList();
    }

    private List<int> Quantidades(Produto produto)
    {
        GrupoPedidoVisual grupo =
            produto.prefab.GetComponent<GrupoPedidoVisual>();

        int maximo = grupo == null
            ? 1
            : grupo.QuantidadeMaxima;

        return Enumerable.Range(
                1,
                Mathf.Clamp(maximo, 0, 3)
            )
            .Where(q => produto.Imagem(q) != null)
            .ToList();
    }

    private List<Item[]> CriarCombinacoes()
    {
        var validos =
            (produtos ?? Array.Empty<Produto>())
            .Where(p =>
                p != null &&
                p.prefab != null &&
                p.prefab.GetComponent<RectTransform>() != null &&
                p.prefab.GetComponent<ProdutoArrastavel>() != null &&
                Quantidades(p).Count > 0
            )
            .GroupBy(p => p.prefab)
            .Select(g => g.First())
            .ToList();

        var combinacoes = new List<Item[]>();

        for (int a = 0; a < validos.Count; a++)
        {
            for (int b = a + 1; b < validos.Count; b++)
            {
                foreach (int qa in Quantidades(validos[a]))
                {
                    foreach (int qb in Quantidades(validos[b]))
                    {
                        combinacoes.Add(new[]
                        {
                            new Item
                            {
                                produto = validos[a],
                                quantidade = qa,
                                imagem = validos[a].Imagem(qa)
                            },
                            new Item
                            {
                                produto = validos[b],
                                quantidade = qb,
                                imagem = validos[b].Imagem(qb)
                            }
                        });
                    }
                }
            }
        }

        return combinacoes;
    }

    // ==========================================
    // GERACAO NORMAL DOS PEDIDOS
    // ==========================================

    public void GerarNovoPedido()
    {
        if (clientes.Count == 0)
            CarregarClientes();

        if (clientes.Count == 0 ||
            folhas == null ||
            folhas.Length != 3 ||
            folhas.Any(f => f == null))
        {
            Debug.LogError(
                "Verifique os nomes e as tres folhas."
            );
            return;
        }

        List<Item[]> combinacoes = CriarCombinacoes();

        if (combinacoes.Count < 3)
        {
            Debug.LogError(
                "Cadastre produtos suficientes para " +
                "formar tres pedidos diferentes."
            );
            return;
        }

        List<Distancia> distancias =
            new List<Distancia>();

        if (distanciaCurta != null &&
            Compativeis(Distancia.Curta).Count > 0)
        {
            distancias.Add(Distancia.Curta);
        }

        if (distanciaLonga != null &&
            Compativeis(Distancia.Longa).Count > 0)
        {
            distancias.Add(Distancia.Longa);
        }

        if (distancias.Count == 0)
        {
            Debug.LogError(
                "Cadastre distancias e entregadores validos."
            );
            return;
        }

        LimparProdutos();

        if (mountingManager != null)
            mountingManager.ConfigurarProdutos(objetosCriados);

        pedidos.Clear();
        pedido = new List<Item>();

        TotalProdutosEsperados = 0;
        EntregadorAtual = null;
        indicePedidoAtual = -1;

        var nomes = clientes
            .OrderBy(_ => UnityEngine.Random.value)
            .ToList();

        for (int i = 0; i < 3; i++)
        {
            int indice = UnityEngine.Random.Range(
                0,
                combinacoes.Count
            );

            Item[] par = combinacoes[indice];
            combinacoes.RemoveAt(indice);

            Distancia distancia = distancias[
                UnityEngine.Random.Range(
                    0,
                    distancias.Count
                )
            ];

            List<Entregador> aptos =
                Compativeis(distancia);

            Pedido novo = new Pedido
            {
                cliente = nomes[i % nomes.Count],
                distancia = distancia,
                entregador = aptos[
                    UnityEngine.Random.Range(
                        0,
                        aptos.Count
                    )
                ]
            };

            if (UnityEngine.Random.value < 0.5f)
            {
                novo.itens.Add(par[0]);
                novo.itens.Add(par[1]);
            }
            else
            {
                novo.itens.Add(par[1]);
                novo.itens.Add(par[0]);
            }

            pedidos.Add(novo);

            ExibirFolha(folhas[i], novo);
        }

        Debug.Log(
            "Tres pedidos diferentes gerados " +
            "com duas demandas cada."
        );
    }

    private void ExibirFolha(FolhaUI folha, Pedido dados)
    {
        if (folha.nomeCliente != null)
            folha.nomeCliente.text = dados.cliente;

        if (folha.imagemDistancia != null)
        {
            folha.imagemDistancia.sprite =
                dados.distancia == Distancia.Curta
                    ? distanciaCurta
                    : distanciaLonga;

            folha.imagemDistancia.SetNativeSize();
        }

        if (folha.imagemTrajeto != null)
        {
            bool ecologico =
                dados.distancia == Distancia.Curta;

            folha.imagemTrajeto.gameObject.SetActive(
                ecologico
            );

            if (ecologico)
            {
                folha.imagemTrajeto.sprite =
                    trajetoEcologico;
            }
        }

        if (folha.imagemEntregador != null)
        {
            folha.imagemEntregador.sprite =
                dados.entregador.imagemCompleta;
        }

        MostrarDemanda(
            folha.demanda1,
            dados.itens[0]
        );

        MostrarDemanda(
            folha.demanda2,
            dados.itens[1]
        );

        if (folha.demandasContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                folha.demandasContainer
            );
        }
    }

    private void MostrarDemanda(Image imagem, Item item)
    {
        if (imagem == null)
            return;

        imagem.sprite = item.imagem;
        imagem.gameObject.SetActive(true);
        imagem.SetNativeSize();
    }

    // ==========================================
    // SELECAO E MONTAGEM
    // ==========================================

    public void SelecionarPedido(int indice)
    {
        if (indice < 0 ||
            indice >= pedidos.Count ||
            areaProdutosEsquerda == null ||
            areaProdutosDireita == null ||
            areaCaixa == null ||
            mountingManager == null ||
            panelPedidos == null ||
            panelMontagem == null)
        {
            Debug.LogError(
                "Pedido invalido ou referencias ausentes."
            );
            return;
        }

        Pedido escolhido = pedidos[indice];

        pedido = escolhido.itens;
        indicePedidoAtual = indice;

        DistanciaAtual = escolhido.distancia;
        EntregadorAtual = escolhido.entregador;

        if (sistemaSubstituicao != null)
        {
            sistemaSubstituicao.PrepararNovoPedido();
        }

        TotalProdutosEsperados =
            pedido.Sum(item => item.quantidade);

        LimparProdutos();

        panelMontagem.SetActive(true);

        mountingManager.ConfigurarProdutos(
            objetosCriados
        );

        for (int i = 0; i < pedido.Count; i++)
        {
            Item item = pedido[i];

            RectTransform destino = i == 0
                ? areaProdutosEsquerda
                : areaProdutosDireita;

            GameObject obj = Instantiate(
                item.produto.prefab,
                destino,
                false
            );

            Debug.Log(
                $"[MONTAGEM] Folha {indice + 1} | " +
                $"Produto pedido: {item.produto.nome} | " +
                $"Quantidade: {item.quantidade} | " +
                $"Prefab usado: {item.produto.prefab.name} | " +
                $"Objeto criado: {obj.name}"
            );

            RectTransform rect =
                obj.GetComponent<RectTransform>();

            ProdutoArrastavel arrastavel =
                obj.GetComponent<ProdutoArrastavel>();

            GrupoPedidoVisual grupo =
                obj.GetComponent<GrupoPedidoVisual>();

            rect.anchoredPosition = Vector2.zero;

            if (grupo != null)
            {
                grupo.ConfigurarQuantidade(
                    item.quantidade
                );
            }

            arrastavel.ConfigurarParaPedido(
                areaCaixa,
                mountingManager,
                item.quantidade
            );

            objetosCriados.Add(obj);
        }

        mountingManager.ConfigurarProdutos(
            objetosCriados
        );

        panelPedidos.SetActive(false);

        if (cronometroPreparacao != null)
        {
            cronometroPreparacao.Iniciar(
                EntregadorAtual
            );
        }
        else
        {
            Debug.LogError(
                "CronometroPreparacao nao foi atribuido!"
            );
        }

        Debug.Log(
            $"Pedido {indice + 1} escolhido: " +
            escolhido.cliente
        );
    }

    private void LimparProdutos()
    {
        foreach (GameObject obj in objetosCriados)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Destroy(obj);
            }
        }

        objetosCriados.Clear();
    }

    // ==========================================
    // SALVAMENTO DOS PEDIDOS
    // ==========================================

    public bool CapturarPedidosParaSalvamento(
        LogiVerySaveManager.Dados dados)
    {
        if (dados == null || pedidos.Count != 3)
        {
            Debug.LogError(
                "LogiVery: nao foi possivel capturar " +
                "os pedidos para salvar."
            );
            return false;
        }

        List<LogiVerySaveManager.Pedido> registros =
            new List<LogiVerySaveManager.Pedido>();

        foreach (Pedido original in pedidos)
        {
            int indiceEntregador = Array.IndexOf(
                entregadores,
                original.entregador
            );

            if (indiceEntregador < 0 ||
                original.itens == null ||
                original.itens.Count != 2)
            {
                Debug.LogError(
                    "LogiVery: pedido com dados invalidos."
                );
                return false;
            }

            LogiVerySaveManager.Pedido registro =
                new LogiVerySaveManager.Pedido
                {
                    cliente = original.cliente,
                    distancia = (int)original.distancia,
                    indiceEntregador = indiceEntregador
                };

            foreach (Item item in original.itens)
            {
                int indiceProduto = Array.IndexOf(
                    produtos,
                    item.produto
                );

                if (indiceProduto < 0)
                {
                    Debug.LogError(
                        "LogiVery: produto nao encontrado " +
                        "no cadastro para salvar."
                    );
                    return false;
                }

                registro.itens.Add(
                    new LogiVerySaveManager.ItemPedido
                    {
                        indiceProduto = indiceProduto,
                        quantidade = item.quantidade
                    }
                );
            }

            registros.Add(registro);
        }

        dados.pedidos = registros;
        dados.pedidoSelecionado = indicePedidoAtual;

        // Preserva tambem um eventual entregador substituto.
        if (dados.substituicao == null)
        {
            dados.substituicao =
                new LogiVerySaveManager.Substituicao();
        }

        dados.substituicao.indiceEntregadorAtual =
            EntregadorAtual == null
                ? -1
                : Array.IndexOf(
                    entregadores,
                    EntregadorAtual
                );

        return true;
    }

    // Recupera as tres folhas sem fazer novos sorteios.
    public bool RestaurarPedidosDoSalvamento(
        LogiVerySaveManager.Dados dados)
    {
        if (dados == null ||
            dados.pedidos == null ||
            dados.pedidos.Count != 3 ||
            folhas == null ||
            folhas.Length != 3 ||
            produtos == null ||
            entregadores == null)
        {
            Debug.LogError(
                "LogiVery: dados de pedidos " +
                "incompletos para restauracao."
            );
            return false;
        }

        // Monta primeiro uma lista temporaria.
        // Se houver erro, nao substitui os pedidos atuais.
        List<Pedido> recuperados = new List<Pedido>();

        foreach (LogiVerySaveManager.Pedido salvo
                 in dados.pedidos)
        {
            if (salvo == null ||
                salvo.indiceEntregador < 0 ||
                salvo.indiceEntregador >= entregadores.Length ||
                entregadores[salvo.indiceEntregador] == null ||
                salvo.itens == null ||
                salvo.itens.Count != 2 ||
                string.IsNullOrWhiteSpace(salvo.cliente) ||
                (salvo.distancia != (int)Distancia.Curta &&
                 salvo.distancia != (int)Distancia.Longa))
            {
                Debug.LogError(
                    "LogiVery: pedido salvo invalido."
                );
                return false;
            }

            Pedido reconstruido = new Pedido
            {
                cliente = salvo.cliente,
                distancia = (Distancia)salvo.distancia,
                entregador =
                    entregadores[salvo.indiceEntregador]
            };

            foreach (LogiVerySaveManager.ItemPedido itemSalvo
                     in salvo.itens)
            {
                if (itemSalvo == null ||
                    itemSalvo.indiceProduto < 0 ||
                    itemSalvo.indiceProduto >= produtos.Length ||
                    produtos[itemSalvo.indiceProduto] == null ||
                    produtos[itemSalvo.indiceProduto].prefab == null)
                {
                    Debug.LogError(
                        "LogiVery: item salvo nao " +
                        "existe mais no cadastro."
                    );
                    return false;
                }

                Produto produtoSalvo =
                    produtos[itemSalvo.indiceProduto];

                Sprite imagem =
                    produtoSalvo.Imagem(itemSalvo.quantidade);

                if (imagem == null)
                {
                    Debug.LogError(
                        "LogiVery: quantidade salva " +
                        "nao possui imagem cadastrada."
                    );
                    return false;
                }

                reconstruido.itens.Add(new Item
                {
                    produto = produtoSalvo,
                    quantidade = itemSalvo.quantidade,
                    imagem = imagem
                });
            }

            recuperados.Add(reconstruido);
        }

        LimparProdutos();

        if (mountingManager != null)
            mountingManager.ConfigurarProdutos(objetosCriados);

        pedidos.Clear();
        pedidos.AddRange(recuperados);

        pedido = new List<Item>();
        indicePedidoAtual = -1;
        EntregadorAtual = null;
        TotalProdutosEsperados = 0;

        for (int i = 0; i < pedidos.Count; i++)
        {
            ExibirFolha(folhas[i], pedidos[i]);
        }

        Debug.Log(
            "LogiVery: os mesmos tres pedidos " +
            "foram restaurados."
        );

        return true;
    }

    // Recupera a escolha do jogador.
    // A posicao dos produtos e o cronometro
    // serao restaurados pelos respectivos sistemas.
    public bool RestaurarPedidoSelecionado(
        LogiVerySaveManager.Dados dados)
    {
        if (dados == null)
            return false;

        if (dados.pedidoSelecionado < 0)
            return true;

        if (dados.pedidoSelecionado >= pedidos.Count)
        {
            Debug.LogError(
                "LogiVery: indice do pedido salvo invalido."
            );
            return false;
        }

        SelecionarPedido(dados.pedidoSelecionado);

        if (indicePedidoAtual != dados.pedidoSelecionado)
            return false;

        // Pode existir um entregador substituto,
        // diferente do entregador inicial da folha.
        if (dados.substituicao != null)
        {
            int indiceEntregador =
                dados.substituicao.indiceEntregadorAtual;

            if (entregadores != null &&
                indiceEntregador >= 0 &&
                indiceEntregador < entregadores.Length &&
                entregadores[indiceEntregador] != null)
            {
                EntregadorAtual =
                    entregadores[indiceEntregador];

                if (cronometroPreparacao != null)
                {
                    cronometroPreparacao.Iniciar(
                        EntregadorAtual
                    );
                }
            }
        }

        Debug.Log(
            "LogiVery: pedido selecionado recuperado: " +
            (indicePedidoAtual + 1)
        );

        return true;
    }

    // ==========================================
    // SUBSTITUICAO DE ENTREGADOR
    // ==========================================

    public bool SortearEntregadorSubstituto(
        out Entregador substituto)
    {
        substituto = null;

        if (EntregadorAtual == null)
            return false;

        List<Entregador> candidatos =
            Compativeis(DistanciaAtual)
            .Where(e =>
                e != EntregadorAtual &&
                e.nome != EntregadorAtual.nome &&
                e.tempoPreparacaoSegundos > 0 &&
                e.fotoMontagem != null
            )
            .ToList();

        if (candidatos.Count == 0)
        {
            Debug.LogError(
                "Nenhum substituto compativel disponivel."
            );
            return false;
        }

        substituto = candidatos[
            UnityEngine.Random.Range(
                0,
                candidatos.Count
            )
        ];

        EntregadorAtual = substituto;

        return true;
    }

    public void ReiniciarCronometroDoEntregadorAtual()
    {
        if (EntregadorAtual == null)
        {
            Debug.LogError(
                "Nenhum entregador atual definido!"
            );
            return;
        }

        if (cronometroPreparacao == null)
        {
            Debug.LogError(
                "CronometroPreparacao nao foi atribuido!"
            );
            return;
        }

        cronometroPreparacao.Iniciar(
            EntregadorAtual
        );
    }
}
