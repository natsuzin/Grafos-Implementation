using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using Microsoft.VisualBasic; // para InputBox
using System.Linq;
using System;

namespace GrafoWPF
{
    public partial class MainWindow : Window
    {
        private Grafo grafo = new Grafo();
        private Vertice verticeSelecionado = null;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void AdicionarMensagem(string mensagem)
        {
            // Adiciona timestamp para identificar quando a mensagem foi gerada
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string mensagemFormatada = $"[{timestamp}] {mensagem}";

            MensagensListBox.Items.Add(mensagemFormatada);
            MensagensListBox.ScrollIntoView(MensagensListBox.Items[MensagensListBox.Items.Count - 1]);

            // Limita o número de mensagens para performance
            if (MensagensListBox.Items.Count > 100)
            {
                MensagensListBox.Items.RemoveAt(0);
            }
        }

        private void VerificarAdjacencia_Click(object sender, RoutedEventArgs e)
        {
            string v1 = Vertice1TextBox.Text.Trim();
            string v2 = Vertice2TextBox.Text.Trim();

            if (string.IsNullOrEmpty(v1) || string.IsNullOrEmpty(v2))
            {
                AdicionarMensagem("Digite os dois vértices antes de verificar adjacência.");
                return;
            }

            var foundV1 = grafo.Vertices.FirstOrDefault(v => v.Nome.Equals(v1, StringComparison.OrdinalIgnoreCase));
            var foundV2 = grafo.Vertices.FirstOrDefault(v => v.Nome.Equals(v2, StringComparison.OrdinalIgnoreCase));

            if (foundV1 == null || foundV2 == null)
            {
                AdicionarMensagem($"Vértice {(foundV1 == null ? v1 : v2)} não encontrado no grafo.");
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
                if (dx * dx + dy * dy <= 20 * 20) // clique dentro do círculo (aumentado para melhor usabilidade)
                {
                    if (verticeSelecionado == null)
                    {
                        verticeSelecionado = v; // primeiro vértice selecionado
                        AdicionarMensagem($"Vértice {v.Nome} selecionado para conexão");
                        DestacarVertice(v, true);
                    }
                    else
                    {
                        if (verticeSelecionado == v)
                        {
                            AdicionarMensagem($"Seleção do vértice {v.Nome} cancelada");
                            DestacarVertice(verticeSelecionado, false);
                            verticeSelecionado = null;
                            return;
                        }

                        AdicionarMensagem($"Criando conexão entre {verticeSelecionado.Nome} e {v.Nome}");

                        // Segundo vértice selecionado → cria aresta
                        string input = Interaction.InputBox("Digite o peso da aresta:", "Peso da aresta", "1");
                        if (!int.TryParse(input, out int peso)) peso = 1;

                        // Verifica se já existe conexão
                        bool jaExiste = verticeSelecionado.Adjacentes.Any(adj => adj.vizinho == v);
                        if (!jaExiste)
                        {
                            verticeSelecionado.Adjacentes.Add((v, peso));
                            if (!grafo.Dirigido)
                            {
                                v.Adjacentes.Add((verticeSelecionado, peso));
                            }
                            AdicionarMensagem($"Aresta criada entre {verticeSelecionado.Nome} e {v.Nome} com peso {peso}");
                        }
                        else
                        {
                            AdicionarMensagem($"Conexão entre {verticeSelecionado.Nome} e {v.Nome} já existe!");
                        }

                        DestacarVertice(verticeSelecionado, false);
                        DesenharGrafo();
                        verticeSelecionado = null;
                    }
                    return;
                }
            }

            // Se chegou aqui e há vértice selecionado, cancela seleção
            if (verticeSelecionado != null)
            {
                DestacarVertice(verticeSelecionado, false);
                verticeSelecionado = null;
                AdicionarMensagem("Seleção cancelada");
                DesenharGrafo();
                return;
            }

            // Se não clicou em vértice, cria novo vértice
            var novoVertice = new Vertice
            {
                Nome = GerarNomeVertice(grafo),
                Posicao = pos
            };

            AdicionarMensagem($"Novo vértice {novoVertice.Nome} criado");
            grafo.Vertices.Add(novoVertice);

            DesenharGrafo();
        }

        private void DestacarVertice(Vertice vertice, bool destacar)
        {
            // Encontra e modifica o vértice no canvas
            foreach (UIElement element in GrafoCanvas.Children)
            {
                if (element is Ellipse ellipse)
                {
                    double left = Canvas.GetLeft(ellipse) + 15; // raio do círculo
                    double top = Canvas.GetTop(ellipse) + 15;

                    if (Math.Abs(left - vertice.Posicao.X) < 1 && Math.Abs(top - vertice.Posicao.Y) < 1)
                    {
                        if (destacar)
                        {
                            ellipse.Stroke = Brushes.Orange;
                            ellipse.StrokeThickness = 4;
                            ellipse.Fill = Brushes.LightYellow;
                        }
                        else
                        {
                            ellipse.Stroke = Brushes.Black;
                            ellipse.StrokeThickness = 2;
                            ellipse.Fill = Brushes.LightBlue;
                        }
                        break;
                    }
                }
            }
        }

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

            // Cancela seleção se houver
            if (verticeSelecionado != null)
            {
                DestacarVertice(verticeSelecionado, false);
                verticeSelecionado = null;
                DesenharGrafo();
                AdicionarMensagem("Seleção cancelada");
            }

            // Remove vértice se clicou nele
            foreach (var v in grafo.Vertices.ToList())
            {
                double dx = pos.X - v.Posicao.X;
                double dy = pos.Y - v.Posicao.Y;
                if (dx * dx + dy * dy <= 20 * 20)
                {
                    // Remove todas as referências desse vértice
                    foreach (var v2 in grafo.Vertices)
                        v2.Adjacentes.RemoveAll(a => a.vizinho == v);

                    grafo.Vertices.Remove(v);
                    AdicionarMensagem($"Vértice {v.Nome} removido do grafo");
                    DesenharGrafo();
                    return;
                }
            }

            // Remove aresta se clicou perto da linha
            foreach (var v in grafo.Vertices)
            {
                for (int i = v.Adjacentes.Count - 1; i >= 0; i--)
                {
                    var viz = v.Adjacentes[i];
                    if (EstaPertoDaLinha(v.Posicao, viz.vizinho.Posicao, pos, 8))
                    {
                        v.Adjacentes.RemoveAt(i);
                        if (!grafo.Dirigido)
                        {
                            viz.vizinho.Adjacentes.RemoveAll(a => a.vizinho == v);
                        }
                        AdicionarMensagem($"Aresta entre {v.Nome} e {viz.vizinho.Nome} removida");
                        DesenharGrafo();
                        return;
                    }
                }
            }
        }

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

            if (grafo.Vertices.Count == 0) return;

            AdicionarMensagem($"Lista de adjacência atualizada:\n{grafo.ListarAdjacencias()}");

            HashSet<(Vertice, Vertice)> desenhadas = new();

            // Desenha arestas primeiro (para ficarem atrás dos vértices)
            foreach (var v in grafo.Vertices)
            {
                foreach (var (vizinho, peso) in v.Adjacentes)
                {
                    if (!desenhadas.Contains((vizinho, v)))
                    {
                        DesenharAresta(v, vizinho, peso, Brushes.Black, 2);
                        desenhadas.Add((v, vizinho));
                    }
                }
            }

            // Desenha vértices por último
            foreach (var v in grafo.Vertices)
            {
                DesenharVertice(v);
            }
        }

        private void DesenharAresta(Vertice origem, Vertice destino, int peso, Brush cor, double espessura)
        {
            var linha = new Line
            {
                X1 = origem.Posicao.X,
                Y1 = origem.Posicao.Y,
                X2 = destino.Posicao.X,
                Y2 = destino.Posicao.Y,
                Stroke = cor,
                StrokeThickness = espessura
            };
            GrafoCanvas.Children.Add(linha);

            // Desenha seta se for dirigido
            if (grafo.Dirigido)
            {
                DesenharSeta(origem.Posicao, destino.Posicao, cor);
            }

            // Desenha peso
            var txtPeso = new TextBlock
            {
                Text = peso.ToString(),
                Foreground = cor,
                FontWeight = FontWeights.Bold,
                Background = Brushes.White,
                Padding = new Thickness(2)
            };
            Canvas.SetLeft(txtPeso, (origem.Posicao.X + destino.Posicao.X) / 2 - 5);
            Canvas.SetTop(txtPeso, (origem.Posicao.Y + destino.Posicao.Y) / 2 - 8);
            GrafoCanvas.Children.Add(txtPeso);
        }

        private void DesenharVertice(Vertice vertice)
        {
            var elipse = new Ellipse
            {
                Width = 35,
                Height = 35,
                Fill = vertice == verticeSelecionado ? Brushes.LightYellow : Brushes.LightBlue,
                Stroke = vertice == verticeSelecionado ? Brushes.Orange : Brushes.Black,
                StrokeThickness = vertice == verticeSelecionado ? 4 : 2,
                Cursor = Cursors.Hand
            };
            Canvas.SetLeft(elipse, vertice.Posicao.X - 17.5);
            Canvas.SetTop(elipse, vertice.Posicao.Y - 17.5);
            GrafoCanvas.Children.Add(elipse);

            var txtNome = new TextBlock
            {
                Text = vertice.Nome,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Canvas.SetLeft(txtNome, vertice.Posicao.X - 10);
            Canvas.SetTop(txtNome, vertice.Posicao.Y - 8);
            GrafoCanvas.Children.Add(txtNome);
        }

        private void DesenharSeta(Point origem, Point destino, Brush cor)
        {
            double dx = destino.X - origem.X;
            double dy = destino.Y - origem.Y;
            double comprimento = Math.Sqrt(dx * dx + dy * dy);

            if (comprimento == 0) return;

            dx /= comprimento;
            dy /= comprimento;

            double offset = 20;
            Point pontoFinal = new Point(
                destino.X - dx * offset,
                destino.Y - dy * offset
            );

            double tamanhoSeta = 12;
            Point p1 = new Point(
                pontoFinal.X - dy * tamanhoSeta - dx * tamanhoSeta,
                pontoFinal.Y + dx * tamanhoSeta - dy * tamanhoSeta
            );
            Point p2 = new Point(
                pontoFinal.X + dy * tamanhoSeta - dx * tamanhoSeta,
                pontoFinal.Y - dx * tamanhoSeta - dy * tamanhoSeta
            );

            var seta = new Polygon
            {
                Points = new PointCollection { pontoFinal, p1, p2 },
                Fill = cor,
                Stroke = cor
            };
            GrafoCanvas.Children.Add(seta);
        }

        private void GerarArvore_Click(object sender, RoutedEventArgs e)
        {
            if (grafo.Vertices.Count == 0)
            {
                AdicionarMensagem("Grafo está vazio. Adicione vértices primeiro.");
                return;
            }

            if (!grafo.Dirigido)
            {
                // Grafo não dirigido: usa Prim
                var mst = grafo.GerarArvoreGeradoraMinimaPrim();
                if (mst.Count == 0)
                {
                    AdicionarMensagem("Não foi possível gerar árvore geradora mínima (grafo desconexo).");
                    return;
                }

                int pesoTotal = 0;
                foreach (var aresta in mst)
                {
                    DesenharAresta(aresta.Origem, aresta.Destino, aresta.Peso, Brushes.Green, 4);
                    pesoTotal += aresta.Peso;
                }

                AdicionarMensagem($"Árvore Geradora Mínima (Prim) gerada! Peso total: {pesoTotal}");
            }
            else
            {
                // Grafo dirigido: usa Dijkstra
                var raiz = grafo.Vertices[0];
                var arvore = grafo.GerarArvoreCaminhosMinimos(raiz);
                if (arvore.Count == 0)
                {
                    AdicionarMensagem("Não foi possível gerar árvore de caminhos mínimos (vértices inalcançáveis).");
                    return;
                }

                int pesoTotal = 0;
                foreach (var aresta in arvore)
                {
                    DesenharAresta(aresta.Origem, aresta.Destino, aresta.Peso, Brushes.Purple, 4);
                    pesoTotal += aresta.Peso;
                }

                AdicionarMensagem($"rvore de Caminhos Mínimos (Dijkstra) de {raiz.Nome} gerada! Peso total: {pesoTotal}");
            }
        }

        private void Dirigido_Checked(object sender, RoutedEventArgs e)
        {
            grafo.Dirigido = true;
            AdicionarMensagem("Modo dirigido ativado - arestas agora são direcionais");
            DesenharGrafo();
        }

        private void Dirigido_Unchecked(object sender, RoutedEventArgs e)
        {
            grafo.Dirigido = false;
            AdicionarMensagem("Modo não-dirigido ativado - arestas são bidirecionais");
            DesenharGrafo();
        }

        private void MatrizAdjacencia_Click(object sender, RoutedEventArgs e)
        {
            if (grafo.Vertices.Count == 0)
            {
                AdicionarMensagem("Grafo vazio - não há matriz de adjacência para exibir.");
                return;
            }

            int[,] matriz = grafo.GerarMatrizAdjacencia();
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("MATRIZ DE ADJACÊNCIA");
            sb.AppendLine("".PadRight(50, '='));

            // Cabeçalho com nomes dos vértices
            sb.Append("     ");
            foreach (var v in grafo.Vertices)
            {
                sb.Append($"{v.Nome,4}");
            }
            sb.AppendLine();

            // Linhas da matriz
            for (int i = 0; i < grafo.Vertices.Count; i++)
            {
                sb.Append($"{grafo.Vertices[i].Nome,3}: ");
                for (int j = 0; j < grafo.Vertices.Count; j++)
                {
                    sb.Append($"{matriz[i, j],4}");
                }
                sb.AppendLine();
            }

            AdicionarMensagem(sb.ToString());
        }

        private void MatrizIncidencia_Click(object sender, RoutedEventArgs e)
        {
            if (grafo.Vertices.Count == 0)
            {
                AdicionarMensagem("Grafo vazio - não há matriz de incidência para exibir.");
                return;
            }

            int[,] matriz = grafo.GerarMatrizIncidencia();
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("MATRIZ DE INCIDÊNCIA");
            sb.AppendLine("".PadRight(50, '='));

            int numArestas = matriz.GetLength(1);

            // Cabeçalho com números das arestas
            sb.Append("     ");
            for (int j = 0; j < numArestas; j++)
            {
                sb.Append($"E{j + 1,3}");
            }
            sb.AppendLine();

            // Linhas da matriz
            for (int i = 0; i < grafo.Vertices.Count; i++)
            {
                sb.Append($"{grafo.Vertices[i].Nome,3}: ");
                for (int j = 0; j < numArestas; j++)
                {
                    sb.Append($"{matriz[i, j],4}");
                }
                sb.AppendLine();
            }

            AdicionarMensagem(sb.ToString());
        }

        private void ExecutarBFS_Click(object sender, RoutedEventArgs e)
        {
            if (grafo.Vertices.Count == 0)
            {
                AdicionarMensagem("Grafo vazio - não é possível executar BFS.");
                return;
            }

            var origem = grafo.Vertices[0];
            var arvore = grafo.BuscaLargura(origem);

            if (arvore.Count == 0)
            {
                AdicionarMensagem($"BFS de {origem.Nome} não encontrou outros vértices conectados.");
                return;
            }

            foreach (var aresta in arvore)
            {
                DesenharAresta(aresta.Origem, aresta.Destino, aresta.Peso, Brushes.Orange, 4);
            }

            AdicionarMensagem($"Busca em Largura (BFS) executada a partir de {origem.Nome} - {arvore.Count} arestas na árvore");
        }

        private void ExecutarDFS_Click(object sender, RoutedEventArgs e)
        {
            if (grafo.Vertices.Count == 0)
            {
                AdicionarMensagem("Grafo vazio - não é possível executar DFS.");
                return;
            }

            var origem = grafo.Vertices[0];
            var arvore = grafo.BuscaProfundidade(origem);

            if (arvore.Count == 0)
            {
                AdicionarMensagem($"DFS de {origem.Nome} não encontrou outros vértices conectados.");
                return;
            }

            foreach (var aresta in arvore)
            {
                DesenharAresta(aresta.Origem, aresta.Destino, aresta.Peso, Brushes.Blue, 4);
            }

            AdicionarMensagem($"Busca em Profundidade (DFS) executada a partir de {origem.Nome} - {arvore.Count} arestas na árvore");
        }

        private void LimparLog_Click(object sender, RoutedEventArgs e)
        {
            MensagensListBox.Items.Clear();
            AdicionarMensagem("Log de atividades limpo");
        }

        private void LimparGrafo_Click(object sender, RoutedEventArgs e)
        {
            grafo.Vertices.Clear();      // Limpa todos os vértices e, por consequência, as arestas
            verticeSelecionado = null;   // Reseta seleção
            GrafoCanvas.Children.Clear(); // Limpa visual do canvas
            MensagensListBox.Items.Clear(); // Limpa o log

            AdicionarMensagem("Grafo completamente limpo!");
        }

    }
}