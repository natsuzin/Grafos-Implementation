using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using Microsoft.VisualBasic; // para InputBox

namespace GrafoWPF
{
    public partial class MainWindow : Window
    {
        private Grafo grafo = new Grafo();
        private Vertice verticeSelecionado = null; // para criar arestas


        public MainWindow()
        {
            InitializeComponent();
        }

        private void AdicionarMensagem(string mensagem)
        {
            MensagensListBox.Items.Add(mensagem);
            MensagensListBox.ScrollIntoView(MensagensListBox.Items[MensagensListBox.Items.Count - 1]);
        }

        private void VerificarAdjacencia_Click(object sender, RoutedEventArgs e)
        {
            string v1 = Vertice1TextBox.Text.Trim();
            string v2 = Vertice2TextBox.Text.Trim();

            if (string.IsNullOrEmpty(v1) || string.IsNullOrEmpty(v2))
            {
                AdicionarMensagem("Digite os dois vértices antes de verificar.");
                return;
            }

            var foundV1 = grafo.Vertices.FirstOrDefault(v => v.Nome.Equals(v1, StringComparison.OrdinalIgnoreCase));
            var foundV2 = grafo.Vertices.FirstOrDefault(v => v.Nome.Equals(v2, StringComparison.OrdinalIgnoreCase));
            if (foundV1 == null || foundV2 == null)
            {
                AdicionarMensagem($"Vértice {(foundV1 == null ? v1 : v2)} não encontrado.");
                return;
            }

            bool adj = grafo.SaoAdjacentes(v1, v2);
            AdicionarMensagem(adj ? $"{v1} e {v2} SÃO adjacentes." : $"{v1} e {v2} NÃO são adjacentes.");
        }


        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(GrafoCanvas);

            // Checa se clicou em um vértice existente
            foreach (var v in grafo.Vertices)
            {
                double dx = pos.X - v.Posicao.X;
                double dy = pos.Y - v.Posicao.Y;
                if (dx * dx + dy * dy <= 15 * 15) // clique dentro do círculo
                {
                    if (verticeSelecionado == null)
                    {
                        verticeSelecionado = v; // primeiro vértice selecionado
                        AdicionarMensagem($"Vértice {v.Nome} selecionado");
                    }
                    else
                    {
                        AdicionarMensagem($"Criando conexão entre os vértices {verticeSelecionado.Nome} e {v.Nome}");
                        // Segundo vértice selecionado → cria aresta na lista de adjacência
                        string input = Interaction.InputBox("Digite o peso da aresta:", "Peso da aresta", "1");
                        if (!int.TryParse(input, out int peso)) peso = 1;
                        //  gerar nome identificador da aresta seguindo a lógica do vértice

                        verticeSelecionado.Adjacentes.Add((v, peso));
                        v.Adjacentes.Add((verticeSelecionado, peso)); // se for dirigido, remover esta linha

                        DesenharGrafo();

                        AdicionarMensagem($"Aresta criada entre {verticeSelecionado.Nome} e {v.Nome} com peso {peso}");
                        verticeSelecionado = null;
                    }

                    return;
                }
            }

            // Se não clicou em vértice, cria novo vértice
            var novoVertice = new Vertice
            {
                Nome = GerarNomeVertice(grafo),
                Posicao = pos
            };

            AdicionarMensagem($"Vértice {novoVertice.Nome} criado");
            grafo.Vertices.Add(novoVertice);


            DesenharGrafo();
        }

        // Função auxiliar para gerar o próximo nome disponível
        private string GerarNomeVertice(Grafo grafo)
        {
            int i = 1;
            while (grafo.Vertices.Any(v => v.Nome == $"V{i}"))
            {
                i++;
            }
            return $"V{i}";
        }


        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(GrafoCanvas);

            // Remove vértice se clicou nele
            foreach (var v in grafo.Vertices)
            {
                double dx = pos.X - v.Posicao.X;
                double dy = pos.Y - v.Posicao.Y;
                if (dx * dx + dy * dy <= 15 * 15)
                {
                    // Remove todas as referências desse vértice na lista de adjacência dos vizinhos
                    foreach (var v2 in grafo.Vertices)
                        v2.Adjacentes.RemoveAll(a => a.vizinho == v);

                    grafo.Vertices.Remove(v);
                    AdicionarMensagem($"Vértice {v.Nome} removido");
                    DesenharGrafo();
                    return;
                }

            }

            // Remove aresta se clicou perto da linha (simplificado: raio de 5 pixels)
            foreach (var v in grafo.Vertices)
            {
                for (int i = v.Adjacentes.Count - 1; i >= 0; i--)
                {
                    var viz = v.Adjacentes[i];
                    if (EstaPertoDaLinha(v.Posicao, viz.vizinho.Posicao, pos, 5))
                    {
                        v.Adjacentes.RemoveAt(i);
                        viz.vizinho.Adjacentes.RemoveAll(a => a.vizinho == v); // se não dirigido
                        AdicionarMensagem($"Aresta entre {v.Nome} e {viz.vizinho.Nome} removida");
                        DesenharGrafo();
                        return;
                    }
                }
            }
        }

        // Função auxiliar para detectar clique próximo à linha
        private bool EstaPertoDaLinha(Point p1, Point p2, Point clique, double tolerancia)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double comprimento = Math.Sqrt(dx * dx + dy * dy);
            if (comprimento == 0) return false;
            double proj = ((clique.X - p1.X) * dx + (clique.Y - p1.Y) * dy) / (comprimento * comprimento);
            proj = Math.Max(0, Math.Min(1, proj));
            double x = p1.X + proj * dx;
            double y = p1.Y + proj * dy;
            double distancia = Math.Sqrt((clique.X - x) * (clique.X - x) + (clique.Y - y) * (clique.Y - y));
            return distancia <= tolerancia;
        }

        private void DesenharGrafo()
        {
            GrafoCanvas.Children.Clear();

            AdicionarMensagem("Lista de adjacência:\n" + grafo.ListarAdjacencias());

            HashSet<(Vertice, Vertice)> desenhadas = new(); // evita desenhar aresta duplicada

            foreach (var v in grafo.Vertices)
            {
                foreach (var (vizinho, peso) in v.Adjacentes)
                {
                    if (!desenhadas.Contains((vizinho, v))) // evita duplicidade
                    {
                        var linha = new Line
                        {
                            X1 = v.Posicao.X,
                            Y1 = v.Posicao.Y,
                            X2 = vizinho.Posicao.X,
                            Y2 = vizinho.Posicao.Y,
                            Stroke = Brushes.Black,
                            StrokeThickness = 2
                        };
                        GrafoCanvas.Children.Add(linha);

                        var txtPeso = new TextBlock
                        {
                            Text = peso.ToString(),
                            Foreground = Brushes.Red
                        };
                        Canvas.SetLeft(txtPeso, (v.Posicao.X + vizinho.Posicao.X) / 2);
                        Canvas.SetTop(txtPeso, (v.Posicao.Y + vizinho.Posicao.Y) / 2);
                        GrafoCanvas.Children.Add(txtPeso);

                        desenhadas.Add((v, vizinho));
                    }
                }
            }

            // Desenha vértices
            foreach (var v in grafo.Vertices)
            {
                var elipse = new Ellipse
                {
                    Width = 30,
                    Height = 30,
                    Fill = Brushes.LightBlue,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(elipse, v.Posicao.X - 15);
                Canvas.SetTop(elipse, v.Posicao.Y - 15);
                GrafoCanvas.Children.Add(elipse);

                var txtNome = new TextBlock
                {
                    Text = v.Nome,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(txtNome, v.Posicao.X - 7);
                Canvas.SetTop(txtNome, v.Posicao.Y - 10);
                GrafoCanvas.Children.Add(txtNome);
            }
        }

        private void GerarArvore_Click(object sender, RoutedEventArgs e)
        {
            var mst = grafo.GerarArvoreGeradoraMinimaPrim();
            if (mst.Count == 0)
            {
                AdicionarMensagem("Não foi possível gerar a árvore geradora mínima (grafo vazio ou desconexo).");
                return;
            }

            // Desenha a MST em verde por cima
            foreach (var aresta in mst)
            {
                var linha = new Line
                {
                    X1 = aresta.Origem.Posicao.X,
                    Y1 = aresta.Origem.Posicao.Y,
                    X2 = aresta.Destino.Posicao.X,
                    Y2 = aresta.Destino.Posicao.Y,
                    Stroke = Brushes.Green,
                    StrokeThickness = 3
                };
                GrafoCanvas.Children.Add(linha);

                var txtPeso = new TextBlock
                {
                    Text = aresta.Peso.ToString(),
                    Foreground = Brushes.DarkGreen,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(txtPeso, (aresta.Origem.Posicao.X + aresta.Destino.Posicao.X) / 2);
                Canvas.SetTop(txtPeso, (aresta.Origem.Posicao.Y + aresta.Destino.Posicao.Y) / 2);
                GrafoCanvas.Children.Add(txtPeso);
            }

            AdicionarMensagem("Árvore geradora mínima gerada com sucesso!");
        }

    }
}
