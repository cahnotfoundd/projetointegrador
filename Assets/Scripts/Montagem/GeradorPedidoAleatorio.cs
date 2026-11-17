
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeradorPedidoAleatorio : MonoBehaviour
{
    public enum Distancia { Curta, Longa }

    [Serializable]
    public class Produto
    {
        public string nome;
        public GameObject prefab;

        // Imagens que aparecem no papel, de acordo
        // com a quantidade solicitada.
        [Tooltip("Imagem da folha para 1 unidade.")]
        public Sprite demanda1;

        [Tooltip("Imagem da folha para 2 unidades.")]
        public Sprite demanda2;

        [Tooltip("Imagem da folha para 3 unidades.")]
        public Sprite demanda3;

        public Sprite Imagem(int qtd) =>
            qtd == 1 ? demanda1 :
            qtd == 2 ? demanda2 :
            qtd == 3 ? demanda3 : null;
    }

    
    [Serializable]
    public class Entregador
    {
        public string nome;
        public Sprite imagemCompleta;

        // Foto individual usada na tela de montagem.
        public Sprite fotoMontagem;

        public bool atendeCurta;
        public bool atendeLonga;

        // Tempo de preparação em segundos.
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

    // Referências dos elementos de UMA folha.
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

    // Informações sorteadas para um pedido.
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

    private List<string> clientes = new List<string>();

    private readonly List<Pedido> pedidos =
        new List<Pedido>();

    private readonly List<GameObject> objetosCriados =
        new List<GameObject>();

    private List<Item> pedido = new List<Item>();

    public int TotalProdutosEsperados { get; private set; }
    public IReadOnlyList<Item> PedidoAtual => pedido;
    public IReadOnlyList<GameObject> ProdutosCriados => objetosCriados;
    public Distancia DistanciaAtual { get; private set; }
    public Entregador EntregadorAtual { get; private set; }

    private void Start()
    {
        CarregarClientes();

        if (gerarAoIniciar)
            GerarNovoPedido();
    }

    // Lê os nomes de clientes do arquivo TXT.
    private void CarregarClientes()
    {
        string caminho = Path.Combine(
            Application.streamingAssetsPath,
            "NomesClientes.txt");

        if (!File.Exists(caminho))
        {
            Debug.LogError(
                "NomesClientes.txt nao encontrado.");
            return;
        }

        try
        {
            clientes = File.ReadAllLines(caminho)
                .Select(n => n.Trim())
                .Where(n => n.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "Erro ao carregar clientes: " + erro.Message);
        }
    }

    // Escolhe entregadores compatíveis com a distância.
    private List<Entregador> Compativeis(Distancia distancia)
    {
        return (entregadores ?? Array.Empty<Entregador>())
            .Where(e => e != null &&
                e.imagemCompleta != null &&
                (distancia == Distancia.Curta
                    ? e.atendeCurta
                    : e.atendeLonga))
            .ToList();
    }

    // Descobre quantas unidades o produto pode ter.
    private List<int> Quantidades(Produto produto)
    {
        GrupoPedidoVisual grupo =
            produto.prefab.GetComponent<GrupoPedidoVisual>();

        // Produtos sem variações representam 1 unidade.
        int maximo = grupo == null
            ? 1
            : grupo.QuantidadeMaxima;

        return Enumerable.Range(
                1, Mathf.Clamp(maximo, 0, 3))
            .Where(q => produto.Imagem(q) != null)
            .ToList();
    }

    // Monta opções de pedidos com duas demandas.
    private List<Item[]> CriarCombinacoes()
    {
        var validos = (produtos ?? Array.Empty<Produto>())
            .Where(p => p != null &&
                p.prefab != null &&
                p.prefab.GetComponent<RectTransform>() != null &&
                p.prefab.GetComponent<ProdutoArrastavel>() != null &&
                Quantidades(p).Count > 0)
            .GroupBy(p => p.prefab)
            .Select(g => g.First())
            .ToList();

        var combinacoes = new List<Item[]>();

        // Combina dois tipos diferentes de produto.
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

    // Sorteia os três pedidos.
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
                "Verifique os nomes e as tres folhas.");
            return;
        }

        List<Item[]> combinacoes = CriarCombinacoes();

        // Precisamos de 3 opções diferentes.
        if (combinacoes.Count < 3)
        {
            Debug.LogError(
                "Cadastre produtos suficientes para " +
                "formar tres pedidos diferentes.");
            return;
        }

        List<Distancia> distancias = new List<Distancia>();

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
                "Cadastre distancias e entregadores validos.");
            return;
        }

        LimparProdutos();

        if (mountingManager != null)
            mountingManager.ConfigurarProdutos(objetosCriados);

        pedidos.Clear();
        pedido = new List<Item>();
        TotalProdutosEsperados = 0;
        EntregadorAtual = null;

        // Sorteia nomes sem repetir, se houver nomes suficientes.
        var nomes = clientes
            .OrderBy(_ => UnityEngine.Random.value)
            .ToList();

        for (int i = 0; i < 3; i++)
        {
            // Retira uma combinação para não repetir
            // o mesmo pedido em outra folha.
            int indice = UnityEngine.Random.Range(
                0, combinacoes.Count);

            Item[] par = combinacoes[indice];
            combinacoes.RemoveAt(indice);

            // Sorteia a distância.
            Distancia distancia = distancias[
                UnityEngine.Random.Range(
                    0, distancias.Count)];

            // Escolhe a ficha de um entregador adequado.
            List<Entregador> aptos = Compativeis(distancia);

            Pedido novo = new Pedido
            {
                cliente = nomes[i % nomes.Count],
                distancia = distancia,
                entregador = aptos[
                    UnityEngine.Random.Range(
                        0, aptos.Count)]
            };

            // Pode inverter qual demanda fica primeiro.
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

            // Preenche apenas a folha correspondente.
            ExibirFolha(folhas[i], novo);
        }

        Debug.Log(
            "Tres pedidos diferentes gerados com duas demandas cada.");
    }

    // Coloca os dados sorteados na folha.
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

        // Curta = elétrica = ecológico.
        // Longa = combustível = não ecológico.
        if (folha.imagemTrajeto != null)
        {
            bool ecologico =
                dados.distancia == Distancia.Curta;

            folha.imagemTrajeto.gameObject.SetActive(
                ecologico);

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

        // As duas imagens das demandas.
        MostrarDemanda(folha.demanda1, dados.itens[0]);
        MostrarDemanda(folha.demanda2, dados.itens[1]);

        if (folha.demandasContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                folha.demandasContainer);
        }
    }

    // Exibe a imagem de quantidade correta.
    private void MostrarDemanda(Image imagem, Item item)
    {
        if (imagem == null)
            return;

        imagem.sprite = item.imagem;
        imagem.gameObject.SetActive(true);
        imagem.SetNativeSize();
    }

    // Chamado quando o jogador clica em uma folha.
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
                "Pedido invalido ou referencias ausentes.");
            return;
        }

        Pedido escolhido = pedidos[indice];

        pedido = escolhido.itens;
        DistanciaAtual = escolhido.distancia;
        EntregadorAtual = escolhido.entregador;

        TotalProdutosEsperados =
            pedido.Sum(item => item.quantidade);

        // Remove produtos de uma seleção anterior.
        LimparProdutos();

        // Abre a tela antes de criar os objetos.
        panelMontagem.SetActive(true);
        mountingManager.ConfigurarProdutos(objetosCriados);

        // Uma demanda por lado, um prefab por demanda.
        for (int i = 0; i < pedido.Count; i++)
        {
            Item item = pedido[i];

            RectTransform destino = i == 0
                ? areaProdutosEsquerda
                : areaProdutosDireita;

            GameObject obj = Instantiate(
                item.produto.prefab,
                destino,
                false);

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

            // Não modifica os desenhos internos.
            rect.anchoredPosition = Vector2.zero;

            // Mostra a quantidade de variações solicitada.
            if (grupo != null)
            {
                grupo.ConfigurarQuantidade(
                    item.quantidade);
            }

            arrastavel.ConfigurarParaPedido(
                areaCaixa,
                mountingManager,
                item.quantidade);

            objetosCriados.Add(obj);
        }

        // O gerenciador acompanha os dois prefabs.
        mountingManager.ConfigurarProdutos(
            objetosCriados);

        
        panelPedidos.SetActive(false);

        // Atualiza a foto e inicia o tempo do entregador escolhido.
        if (cronometroPreparacao != null)
        {
            cronometroPreparacao.Iniciar(EntregadorAtual);
        }
        else
        {
            Debug.LogError(
                "CronometroPreparacao não foi atribuído!");
        }

        Debug.Log(
            $"Pedido {indice + 1} escolhido: " +
            escolhido.cliente);

    }

    // Apaga os produtos criados anteriormente.
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
}
