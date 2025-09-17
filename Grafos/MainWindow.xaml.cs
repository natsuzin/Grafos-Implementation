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
        private Grafo grafo = new Grafo(); // instância do grafo
        private Vertice verticeSelecionado = null; // para criar arestas
        private List<UIElement> caminhosDestacados = new List<UIElement>(); // guarda referências aos elementos temporários (caminhos desenhados)

        public MainWindow()
        {
            InitializeComponent();

        }

        // Adiciona mensagem ao log com timestamp
        private void AdicionarMensagem(string mensagem)
        {
            // adiciona timestamp para identificar quando a mensagem foi gerada
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string mensagemFormatada = $"[{timestamp}] {mensagem}";

            MensagensListBox.Items.Add(mensagemFormatada);
            MensagensListBox.ScrollIntoView(MensagensListBox.Items[MensagensListBox.Items.Count - 1]);

            // limita o número de mensagens para performance
            if (MensagensListBox.Items.Count > 100)
            {
                MensagensListBox.Items.RemoveAt(0);
            }
        }

        // Redesenha todo o grafo no canvas
        private void DesenharGrafo()
        {
            GrafoCanvas.Children.Clear();

            if (grafo.Vertices.Count == 0) return;

            DirecaoCheckBox.IsEnabled = !(grafo.Vertices.Count > 1);

            AdicionarMensagem($"Lista de adjacência atualizada:\n{grafo.ListarAdjacencias()}");

            // para evitar desenhar arestas duplicadas em grafos não-dirigidos
            HashSet<(Vertice, Vertice)> desenhadas = new();

            // desenha arestas primeiro (para ficarem atrás dos vértices)
            foreach (var v in grafo.Vertices)
            {
                foreach (var (vizinho, peso) in v.Adjacentes)
                {
                    if (!desenhadas.Contains((vizinho, v)))
                    {
                        DesenharAresta(v, vizinho, peso, Brushes.Black, 2, false);
                        desenhadas.Add((v, vizinho));
                    }
                }
            }

            // desenha vértices por último
            foreach (var v in grafo.Vertices)
            {
                DesenharVertice(v);
            }
        }

        // Desenha um vértice como círculo com nome
        private void DesenharVertice(Vertice vertice)
        {
            // desenha círculo
            var elipse = new Ellipse
            {
                Width = 35,
                Height = 35,
                Fill = Brushes.LightBlue,
                StrokeThickness = vertice == verticeSelecionado ? 4 : 2,
                Cursor = Cursors.Hand
            };
            Canvas.SetLeft(elipse, vertice.Posicao.X - 17.5);
            Canvas.SetTop(elipse, vertice.Posicao.Y - 17.5);
            GrafoCanvas.Children.Add(elipse);

            // desenha nome
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

        // gera nome único para novo vértice (V1, V2, ...)
        private string GerarNomeVertice(Grafo grafo)
        {
            int i = 1;
            while (grafo.Vertices.Any(v => v.Nome == $"V{i}"))
            {
                i++;
            }
            return $"V{i}";
        }

        // Desenha uma aresta entre dois vértices com peso e estilo
        private void DesenharAresta(Vertice origem, Vertice destino, int peso, Brush cor, double espessura, bool caminhoTemporario = false)
        {
            // linha
            var linha = new Line
            {
                X1 = origem.Posicao.X,
                Y1 = origem.Posicao.Y,
                X2 = destino.Posicao.X,
                Y2 = destino.Posicao.Y,
                Stroke = cor,
                StrokeThickness = espessura
            };
            if (caminhoTemporario)
            {
                linha.Tag = "temp";
                caminhosDestacados.Add(linha);
            }
            GrafoCanvas.Children.Add(linha);

            // seta (se for dirigido)
            if (grafo.Dirigido)
            {
                DesenharArco(origem.Posicao, destino.Posicao, cor, caminhoTemporario);
            }

            // peso
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

            if (caminhoTemporario)
            {
                txtPeso.Tag = "temp";
                caminhosDestacados.Add(txtPeso);
            }
            GrafoCanvas.Children.Add(txtPeso);
        }

        // Desenha uma seta para indicar direção do arco
        private Polygon DesenharArco(Point origem, Point destino, Brush cor, bool caminhoTemporario = false)
        {
            double dx = destino.X - origem.X;
            double dy = destino.Y - origem.Y;
            double comprimento = Math.Sqrt(dx * dx + dy * dy);
            if (comprimento == 0) return null;

            dx /= comprimento;
            dy /= comprimento;

            double offset = 20;
            Point pontoFinal = new Point(destino.X - dx * offset, destino.Y - dy * offset);
            double tamanhoSeta = 8;

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

            if (caminhoTemporario)
            {
                seta.Tag = "temp";
                caminhosDestacados.Add(seta);
            }

            GrafoCanvas.Children.Add(seta);
            return seta;
        }



        // verifica se o ponto 'clique' está perto da linha entre p1 e p2
        private bool EstaPertoDaLinha(Point p1, Point p2, Point clique, double tolerancia)
        {
            // cálculo da projeção do ponto na linha
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

        // Adiciona ou conecta vértices no canvas
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(GrafoCanvas);

            // checa se clicou em um vértice existente
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
                    }
                    else
                    {
                        // se clicou no mesmo vértice, cancela seleção
                        if (verticeSelecionado == v)
                        {
                            AdicionarMensagem($"Seleção do vértice {v.Nome} cancelada");
                            verticeSelecionado = null;
                            return;
                        }

                        AdicionarMensagem($"Criando conexão entre {verticeSelecionado.Nome} e {v.Nome}");

                        // se clicou em outro vértice, cria aresta
                        string input = Interaction.InputBox("Digite o peso da aresta:", "Peso da aresta", "1"); // valor padrão 1
                        if (!int.TryParse(input, out int peso)) peso = 1; // se inválido, usa 1

                        // verifica se já existe conexão
                        bool jaExiste = verticeSelecionado.Adjacentes.Any(adj => adj.vizinho == v);
                        if (!jaExiste)
                        {
                            verticeSelecionado.Adjacentes.Add((v, peso)); // adiciona aresta

                            string nomeA = $"{verticeSelecionado.Nome}-{v.Nome}";

                            // se não for dirigido, adiciona a conexão inversa
                            if (!grafo.Dirigido)
                            {
                                v.Adjacentes.Add((verticeSelecionado, peso));
                                AdicionarMensagem($"Aresta {nomeA} criada com peso {peso}");
                            }
                            else
                            {
                                AdicionarMensagem($"Arco {nomeA} criado com peso {peso}");
                            }
                            
                        }
                        else
                        {
                            string nomeAresta = $"{verticeSelecionado.Nome}-{v.Nome}";
                            AdicionarMensagem($"Essa conexão já existe!");
                        }


                        DesenharGrafo();
                        verticeSelecionado = null;
                    }
                    return;
                }
            }

            // se chegou aqui e há vértice selecionado, cancela seleção
            if (verticeSelecionado != null)
            {
                verticeSelecionado = null;
                AdicionarMensagem("Seleção cancelada");
                DesenharGrafo();
                return;
            }

            // cria novo vértice
            var novoVertice = new Vertice
            {
                Nome = GerarNomeVertice(grafo),
                Posicao = pos
            };

            AdicionarMensagem($"Novo vértice {novoVertice.Nome} criado");
            grafo.Vertices.Add(novoVertice);

            DesenharGrafo();
        }


        // Remove vértice ou aresta com clique direito
        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(GrafoCanvas);

            // cancela seleção
            if (verticeSelecionado != null)
            {
                verticeSelecionado = null;
                DesenharGrafo();
                AdicionarMensagem("Seleção cancelada");
            }

            // remove vértice se clicou nele
            foreach (var v in grafo.Vertices.ToList())
            {
                // ajuste para clique dentro do círculo
                double dx = pos.X - v.Posicao.X;
                double dy = pos.Y - v.Posicao.Y;

                if (dx * dx + dy * dy <= 20 * 20)
                {
                    // remove todas as conexões desse vértice
                    foreach (var v2 in grafo.Vertices)
                        v2.Adjacentes.RemoveAll(a => a.vizinho == v);

                    grafo.Vertices.Remove(v);
                    AdicionarMensagem($"Vértice {v.Nome} removido do grafo");
                    DesenharGrafo();
                    return;
                }
            }

            // remove aresta se clicou perto da linha
            foreach (var v in grafo.Vertices)
            {
                // itera para trás para evitar problemas ao remover
                for (int i = v.Adjacentes.Count - 1; i >= 0; i--)
                {
                    var viz = v.Adjacentes[i];
                    // checa se o clique está perto da linha entre v e seu vizinho
                    if (EstaPertoDaLinha(v.Posicao, viz.vizinho.Posicao, pos, 8))
                    {
                        v.Adjacentes.RemoveAt(i);
                        string nomeA = $"{v.Nome}-{viz.vizinho.Nome}";
                        if (!grafo.Dirigido)
                        {
                            viz.vizinho.Adjacentes.RemoveAll(a => a.vizinho == v);
                            AdicionarMensagem($"Aresta {nomeA} removida");
                        }
                        else
                        {
                            AdicionarMensagem($"Arco {nomeA} removido");
                        }

                        DesenharGrafo();
                        return;
                    }
                }
            }

        }

        // Verifica se dois vértices são adjacentes
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

        // Gera árvore geradora mínima (Prim) ou árvore de caminhos mínimos (Dijkstra)
        private void GerarArvore_Click(object sender, RoutedEventArgs e)
        {
            if (grafo.Vertices.Count == 0)
            {
                AdicionarMensagem("Grafo está vazio. Adicione vértices primeiro.");
                return;
            }

            // se grafo não dirigido
            if (!grafo.Dirigido)
            {
                // usa Prim
                var mst = grafo.GerarArvoreGeradoraMinimaPrim();
                if (mst.Count == 0)
                {
                    AdicionarMensagem("Não foi possível gerar árvore geradora mínima (grafo desconexo).");
                    return;
                }

                int pesoTotal = 0;

                // desenha arestas da árvore em verde
                foreach (var aresta in mst)
                {
                    DesenharAresta(aresta.Origem, aresta.Destino, aresta.Peso, Brushes.Green, 4, true);
                    pesoTotal += aresta.Peso;
                }

                AdicionarMensagem($"Árvore Geradora Mínima (Prim) gerada! Peso total: {pesoTotal}");
            }
            else
            {
                // se grafo dirigido, usa Dijkstra
                var raiz = EscolherVerticeValido();
                if (raiz == null)
                {
                    AdicionarMensagem("Nenhum vértice possui caminhos para percorrer (grafo dirigido isolado).");
                    return;
                }

                var (arvore, inalcançaveis) = grafo.GerarArvoreCaminhosMinimos(raiz);

                if (arvore.Count == 0)
                {
                    AdicionarMensagem("Não foi possível gerar árvore de caminhos mínimos (nenhum vértice alcançável).");
                    return;
                }

                foreach (var aresta in arvore)
                    DesenharAresta(aresta.Origem, aresta.Destino, aresta.Peso, Brushes.Purple, 4, true);

                if (inalcançaveis.Count > 0)
                    AdicionarMensagem($"Alguns vértices não foram alcançáveis a partir de {raiz.Nome}: {string.Join(", ", inalcançaveis.Select(v => v.Nome))}");
                else
                    AdicionarMensagem($"Árvore de Caminhos Mínimos (Dijkstra) de {raiz.Nome} gerada com sucesso!");

            }
        }

        // Gera e exibe matriz de adjacência
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

            // cabeçalho com nomes dos vértices
            sb.Append("     ");
            foreach (var v in grafo.Vertices)
            {
                sb.Append($"{v.Nome,4}");
            }
            sb.AppendLine();

            // linhas da matriz
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

        // Gera e exibe matriz de incidência
        private void MatrizIncidencia_Click(object sender, RoutedEventArgs e)
        {
            if (grafo.Vertices.Count == 0)
            {
                AdicionarMensagem("Grafo vazio - não há matriz de incidência para exibir.");
                return;
            }

            var dadosIncidencia = grafo.GerarMatrizIncidencia();
            int[,] matriz = dadosIncidencia.matriz;
            var arestas = dadosIncidencia.arestas;
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("MATRIZ DE INCIDÊNCIA");
            sb.AppendLine("".PadRight(50, '='));

            int numArestas = matriz.GetLength(1);

            // cabeçalho com números das arestas
            sb.Append("     ");
            foreach (var aresta in arestas)
            {
                sb.Append($"{aresta.Nome}");
            }
            sb.AppendLine();

            // linhas da matriz
            for (int i = 0; i < grafo.Vertices.Count; i++)
            {
                sb.Append($"{grafo.Vertices[i].Nome,3}: ");
                for (int j = 0; j < numArestas; j++)
                {
                    var resultado = $"  {matriz[i, j]} ";
                    if (matriz[i, j] >= 0) resultado += " "; // para alinhar números positivos
                        sb.Append(resultado);
                }
                sb.AppendLine();
            }

            AdicionarMensagem(sb.ToString());
        }


        // Executa busca em largura (BFS) e destaca a árvore resultante
        private void ExecutarBFS_Click(object sender, RoutedEventArgs e)
        {
            if (grafo.Vertices.Count == 0)
            {
                AdicionarMensagem("Grafo vazio - não é possível executar BFS.");
                return;
            }

            // Pergunta qual vértice deve ser a origem da busca
            string input = Interaction.InputBox("Digite o nome do vértice inicial para BFS:", "Vértice inicial", "");
            if (string.IsNullOrWhiteSpace(input))
            {
                AdicionarMensagem("BFS cancelada - nenhum vértice especificado.");
                return;
            }

            // Procura o vértice especificado
            var origem = grafo.Vertices.FirstOrDefault(v => v.Nome.Equals(input.Trim(), StringComparison.OrdinalIgnoreCase));
            if (origem == null)
            {
                AdicionarMensagem($"Vértice '{input}' não encontrado no grafo. Vértices disponíveis: {string.Join(", ", grafo.Vertices.Select(v => v.Nome))}");
                return;
            }

            var arvore = grafo.BuscaLargura(origem);

            if (arvore.Count == 0)
            {
                AdicionarMensagem($"BFS de {origem.Nome} não encontrou outros vértices conectados.");
                return;
            }

            foreach (var aresta in arvore)
            {
                DesenharAresta(aresta.Origem, aresta.Destino, aresta.Peso, Brushes.Orange, 4, true);
            }

            AdicionarMensagem($"Busca em Largura (BFS) executada a partir de {origem.Nome}.");
        }

        // Executa busca em profundidade (DFS) e destaca a árvore resultante
        private void ExecutarDFS_Click(object sender, RoutedEventArgs e)
        {
            if (grafo.Vertices.Count == 0)
            {
                AdicionarMensagem("Grafo vazio - não é possível executar DFS.");
                return;
            }

            // Pergunta qual vértice deve ser a origem da busca
            string input = Interaction.InputBox("Digite o nome do vértice inicial para DFS:", "Vértice inicial", "");
            if (string.IsNullOrWhiteSpace(input))
            {
                AdicionarMensagem("DFS cancelada - nenhum vértice especificado.");
                return;
            }

            // Procura o vértice especificado
            var origem = grafo.Vertices.FirstOrDefault(v => v.Nome.Equals(input.Trim(), StringComparison.OrdinalIgnoreCase));
            if (origem == null)
            {
                AdicionarMensagem($"Vértice '{input}' não encontrado no grafo. Vértices disponíveis: {string.Join(", ", grafo.Vertices.Select(v => v.Nome))}.");
                return;
            }

            var arvore = grafo.BuscaProfundidade(origem);

            if (arvore.Count == 0)
            {
                AdicionarMensagem($"DFS de {origem.Nome} não encontrou outros vértices conectados.");
                return;
            }

            foreach (var aresta in arvore)
            {
                DesenharAresta(aresta.Origem, aresta.Destino, aresta.Peso, Brushes.Blue, 4, true);
            }

            AdicionarMensagem($"Busca em Profundidade (DFS) executada a partir de {origem.Nome}.");
        }

        // Executa Algoritmo de Roy
        private void ExecutarRoy_Click(object sender, RoutedEventArgs e)
        {
            var totalVertices = grafo.Vertices.Count;
            if (totalVertices == 0)
            {
                AdicionarMensagem("Grafo vazio - não é possível executar algoritmo de Roy.");
                return;
            }

            var resultado = grafo.Roy();

            // Exibe mensagem sobre conectividade geral do grafo
            if (!string.IsNullOrWhiteSpace(resultado.mensagem))
            {
                AdicionarMensagem(resultado.mensagem);
            }

            // Se não houver componentes, significa que há apenas vértices isolados
            if (resultado.componentes.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(resultado.mensagem))
                {
                    AdicionarMensagem("Não foram encontradas componentes conexas. O grafo contém apenas vértices isolados.");
                }
                return;
            }

            // Cores disponíveis para destacar componentes diferentes
            var cores = new List<Brush> { Brushes.Red, Brushes.Pink };
            int corIndex = 0;

            // Processa cada componente encontrada
            for (int i = 0; i < resultado.componentes.Count; i++)
            {
                var componente = resultado.componentes[i];

                // Identifica os vértices únicos da componente
                var verticesDaComponente = new HashSet<string>();
                foreach (var aresta in componente)
                {
                    verticesDaComponente.Add(aresta.Origem.Nome);
                    verticesDaComponente.Add(aresta.Destino.Nome);
                }

                string tipoComponente = grafo.Dirigido ? "fortemente conexa" : "conexa";
                AdicionarMensagem($"Componente {tipoComponente} {i + 1}: " +
                                 $"Vértices [{string.Join(", ", verticesDaComponente.OrderBy(v => v))}]");

                // Desenha as arestas da componente com cor específica
                Brush corAtual = cores[corIndex % cores.Count];
                foreach (var aresta in componente)
                {
                    DesenharAresta(aresta.Origem, aresta.Destino, aresta.Peso, corAtual, 4, true);
                }

                corIndex++;
            }

            // Resumo final
            string resumo = grafo.Dirigido ?
                $"Análise de Roy concluída: {resultado.componentes.Count} componente(s) fortemente conexa(s) encontrada(s)" :
                $"Análise de Roy concluída: {resultado.componentes.Count} componente(s) conexa(s) encontrada(s)";

            AdicionarMensagem(resumo);
        }

        private void LimparCaminhos_Click(object sender, RoutedEventArgs e)
        {
            int removed = 0;

            // tenta remover pelos elementos que guardamos
            foreach (var elem in caminhosDestacados.ToList())
            {
                if (GrafoCanvas.Children.Contains(elem))
                {
                    GrafoCanvas.Children.Remove(elem);
                    removed++;
                }
            }
            caminhosDestacados.Clear();

            // fallback: remove qualquer filho marcado com Tag == "temp" (caso algo tenha saído da lista)
            for (int i = GrafoCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (GrafoCanvas.Children[i] is FrameworkElement fe && fe.Tag != null && fe.Tag.ToString() == "temp")
                {
                    GrafoCanvas.Children.RemoveAt(i);
                    removed++;
                }
            }

            AdicionarMensagem($"Limpeza de caminhos executada.");
        }

        // Retorna o primeiro vértice que tenha pelo menos uma aresta de saída
        private Vertice EscolherVerticeValido()
        {
            foreach (var v in grafo.Vertices)
            {
                if (v.Adjacentes.Count > 0)
                    return v; // primeiro vértice que pode percorrer
            }
            return null; // nenhum vértice válido encontrado
        }

        // Limpa o log de mensagens
        private void LimparLog_Click(object sender, RoutedEventArgs e)
        {
            MensagensListBox.Items.Clear();
            AdicionarMensagem("Log de atividades limpo");
        }

        // Limpa todo o grafo, vértices, arestas e log
        private void LimparGrafo_Click(object sender, RoutedEventArgs e)
        {
            grafo.Vertices.Clear();      // Limpa todos os vértices e, por consequência, as arestas
            verticeSelecionado = null;   // Reseta seleção
            GrafoCanvas.Children.Clear(); // Limpa visual do canvas
            MensagensListBox.Items.Clear(); // Limpa o log
            DirecaoCheckBox.IsEnabled = true; // Reabilita checkbox de direção 

            AdicionarMensagem("Grafo completamente limpo!");
        }

        // Alterna para grafo dirigido
        private void Dirigido_Checked(object sender, RoutedEventArgs e)
        {
            grafo.Dirigido = true;
            AdicionarMensagem("Modo dirigido ativado (DIJKSTRA)");
            GerarArvoreButton.Content = "🌳 Gerar Árvore DIJKSTRA";
            DesenharGrafo();
        }

        // Alterna para grafo não-dirigido
        private void Dirigido_Unchecked(object sender, RoutedEventArgs e)
        {
            grafo.Dirigido = false;
            AdicionarMensagem("Modo não-dirigido ativado (PRIM)");
            GerarArvoreButton.Content = "🌳 Gerar Árvore PRIM";
            DesenharGrafo();
        }

    }
}