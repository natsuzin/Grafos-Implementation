using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using System.Linq;
using System;

namespace GrafoWPF
{
    public partial class MainWindow : Window
    {
        private Grafo grafo = new Grafo();
        private Vertice verticeSelecionado = null;
        private List<UIElement> caminhosDestacados = new List<UIElement>();
        private Dictionary<Vertice, int> coloracaoAtual = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private readonly Brush[] coresDisponiveis = new Brush[]
        {
            Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Yellow,
            Brushes.Orange, Brushes.Purple, Brushes.Pink, Brushes.Cyan,
            Brushes.Lime, Brushes.Magenta, Brushes.Brown, Brushes.Navy,
            Brushes.Teal, Brushes.Olive, Brushes.Maroon, Brushes.Aqua,
            Brushes.Fuchsia, Brushes.Silver, Brushes.Gold, Brushes.Coral
        };

        private void AdicionarMensagem(string mensagem)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string mensagemFormatada = $"[{timestamp}] {mensagem}";
            MensagensListBox.Items.Add(mensagemFormatada);
            MensagensListBox.ScrollIntoView(MensagensListBox.Items[MensagensListBox.Items.Count - 1]);
            if (MensagensListBox.Items.Count > 100)
                MensagensListBox.Items.RemoveAt(0);
        }

        private void DesenharGrafo()
        {
            GrafoCanvas.Children.Clear();
            if (grafo.Vertices.Count == 0) return;

            DirecaoCheckBox.IsEnabled = !(grafo.Vertices.Count > 1);
            AdicionarMensagem($"Lista de adjacência atualizada:\n{grafo.ListarAdjacencias()}");

            // Estrutura para controlar múltiplas arestas entre os mesmos vértices
            var contagemArestas = new Dictionary<(Vertice, Vertice), int>();

            // Conta quantas arestas existem entre cada par de vértices
            foreach (var v in grafo.Vertices)
            {
                foreach (var (vizinho, peso) in v.Adjacentes)
                {
                    var chave = (v, vizinho);
                    if (!contagemArestas.ContainsKey(chave))
                        contagemArestas[chave] = 0;
                    contagemArestas[chave]++;
                }
            }

            // Desenha arestas primeiro
            var arestasDesenhadas = new Dictionary<(Vertice, Vertice), int>();

            foreach (var v in grafo.Vertices)
            {
                for (int i = 0; i < v.Adjacentes.Count; i++)
                {
                    var (vizinho, peso) = v.Adjacentes[i];
                    var chave = (v, vizinho);

                    if (!arestasDesenhadas.ContainsKey(chave))
                        arestasDesenhadas[chave] = 0;

                    int indiceAresta = arestasDesenhadas[chave];
                    int totalArestas = contagemArestas[chave];

                    // Verifica se existe aresta reversa
                    bool existeReversa = !grafo.Dirigido ? false :
                        contagemArestas.ContainsKey((vizinho, v));

                    DesenharAresta(v, vizinho, peso, Brushes.Black, 2, false,
                                 indiceAresta, totalArestas, existeReversa);

                    arestasDesenhadas[chave]++;
                }
            }

            // Desenha vértices por último
            foreach (var v in grafo.Vertices)
                DesenharVertice(v);
        }

        private void DesenharVertice(Vertice vertice)
        {
            Brush corVertice = Brushes.LightBlue;
            if (coloracaoAtual != null && coloracaoAtual.ContainsKey(vertice))
            {
                int indiceCor = coloracaoAtual[vertice];
                corVertice = coresDisponiveis[indiceCor % coresDisponiveis.Length];
            }

            var elipse = new Ellipse
            {
                Width = 35,
                Height = 35,
                Fill = corVertice,
                Stroke = Brushes.Black,
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

        private string GerarNomeVertice(Grafo grafo)
        {
            int i = 1;
            while (grafo.Vertices.Any(v => v.Nome == $"V{i}"))
                i++;
            return $"V{i}";
        }

        // NOVO: Desenha aresta com suporte a laços e curvas
        private void DesenharAresta(Vertice origem, Vertice destino, int peso, Brush cor,
                                   double espessura, bool caminhoTemporario = false,
                                   int indiceAresta = 0, int totalArestas = 1,
                                   bool existeReversa = false)
        {
            // LAÇO: quando origem e destino são o mesmo vértice
            if (origem == destino)
            {
                DesenharLaco(origem, peso, cor, espessura, caminhoTemporario, indiceAresta);
                return;
            }

            // Calcula curvatura se houver múltiplas arestas ou aresta reversa
            double curvatura = 0;

            if (grafo.Dirigido) { 
                if (totalArestas > 1 || existeReversa)
                {
                    // Distribui as curvaturas
                    double espacamento = 50; // espaço entre curvas
                    if (totalArestas > 1)
                    {
                        // Múltiplas arestas na mesma direção
                        curvatura = (indiceAresta - (totalArestas - 1) / 2.0) * espacamento;
                    }
                    else if (existeReversa)
                    {
                        // Apenas uma aresta em cada direção - curva para não sobrepor
                        curvatura = 30;
                    }
                }
            }

            if (Math.Abs(curvatura) < 0.1)
            {
                // Aresta reta
                DesenharArestaReta(origem, destino, peso, cor, espessura, caminhoTemporario);
            }
            else
            {
                // Aresta curva
                DesenharArestaCurva(origem, destino, peso, cor, espessura,
                                   caminhoTemporario, curvatura);
            }
        }

        // Desenha aresta reta (código original)
        private void DesenharArestaReta(Vertice origem, Vertice destino, int peso,
                                       Brush cor, double espessura, bool caminhoTemporario)
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

            if (caminhoTemporario)
            {
                linha.Tag = "temp";
                caminhosDestacados.Add(linha);
            }
            GrafoCanvas.Children.Add(linha);

            if (grafo.Dirigido)
                DesenharSetaReta(origem.Posicao, destino.Posicao, cor, caminhoTemporario);

            DesenharPesoAresta(origem.Posicao, destino.Posicao, peso, cor,
                             caminhoTemporario, 0);
        }

        // Desenha aresta curva usando Bézier
        private void DesenharArestaCurva(Vertice origem, Vertice destino, int peso,
                                        Brush cor, double espessura,
                                        bool caminhoTemporario, double curvatura)
        {
            Point p1 = origem.Posicao;
            Point p2 = destino.Posicao;

            // Calcula ponto de controle para a curva
            Point meio = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);

            // Vetor perpendicular
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double comprimento = Math.Sqrt(dx * dx + dy * dy);

            double perpX = -dy / comprimento;
            double perpY = dx / comprimento;

            Point controle = new Point(
                meio.X + perpX * curvatura,
                meio.Y + perpY * curvatura
            );

            // Cria curva Bézier quadrática
            var curva = new Path
            {
                Stroke = cor,
                StrokeThickness = espessura,
                Fill = null
            };

            var geometria = new PathGeometry();
            var figura = new PathFigure { StartPoint = p1 };

            var bezier = new QuadraticBezierSegment
            {
                Point1 = controle,
                Point2 = p2
            };

            figura.Segments.Add(bezier);
            geometria.Figures.Add(figura);
            curva.Data = geometria;

            if (caminhoTemporario)
            {
                curva.Tag = "temp";
                caminhosDestacados.Add(curva);
            }
            GrafoCanvas.Children.Add(curva);

            if (grafo.Dirigido)
                DesenharSetaCurva(p1, controle, p2, cor, caminhoTemporario);

            // Peso na curva
            Point posicaoPeso = CalcularPontoBezier(p1, controle, p2, 0.5);
            DesenharPesoAresta(posicaoPeso, posicaoPeso, peso, cor,
                             caminhoTemporario, curvatura);
        }

        // Desenha laço (self-loop)
        private void DesenharLaco(Vertice vertice, int peso, Brush cor,
                                 double espessura, bool caminhoTemporario,
                                 int indiceLaco = 0)
        {
            Point centro = vertice.Posicao;
            double raio = 25 + (indiceLaco * 15); // Aumenta raio para múltiplos laços
            double anguloInicio = 45; // Graus

            // Converte para radianos
            double radInicio = anguloInicio * Math.PI / 180;
            double radFim = (anguloInicio + 270) * Math.PI / 180;

            // Pontos de início e fim no círculo do vértice
            Point pInicio = new Point(
                centro.X + 17.5 * Math.Cos(radInicio),
                centro.Y + 17.5 * Math.Sin(radInicio)
            );

            Point pFim = new Point(
                centro.X + 17.5 * Math.Cos(radFim),
                centro.Y + 17.5 * Math.Sin(radFim)
            );

            // Cria o laço usando arco
            var laco = new Path
            {
                Stroke = cor,
                StrokeThickness = espessura,
                Fill = null
            };

            var geometria = new PathGeometry();
            var figura = new PathFigure { StartPoint = pInicio };

            // Usa arco para criar o laço
            var arco = new ArcSegment
            {
                Point = pFim,
                Size = new Size(raio, raio),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = true
            };

            figura.Segments.Add(arco);
            geometria.Figures.Add(figura);
            laco.Data = geometria;

            if (caminhoTemporario)
            {
                laco.Tag = "temp";
                caminhosDestacados.Add(laco);
            }
            GrafoCanvas.Children.Add(laco);

            if (grafo.Dirigido)
            {
                // Seta no final do laço
                double angulo = radFim - 0.3; // Ajuste para posicionar melhor a seta
                Point pontoSeta = new Point(
                    centro.X + 17.5 * Math.Cos(angulo),
                    centro.Y + 17.5 * Math.Sin(angulo)
                );

                DesenharSetaLaco(pontoSeta, radFim, cor, caminhoTemporario);
            }

            // Peso do laço
            Point posicaoPeso = new Point(
                centro.X + (raio + 10) * Math.Cos((radInicio + radFim) / 2),
                centro.Y + (raio + 10) * Math.Sin((radInicio + radFim) / 2)
            );

            var txtPeso = new TextBlock
            {
                Text = peso.ToString(),
                Foreground = cor,
                FontWeight = FontWeights.Bold,
                Background = Brushes.White,
                Padding = new Thickness(2)
            };
            Canvas.SetLeft(txtPeso, posicaoPeso.X - 8);
            Canvas.SetTop(txtPeso, posicaoPeso.Y - 8);

            if (caminhoTemporario)
            {
                txtPeso.Tag = "temp";
                caminhosDestacados.Add(txtPeso);
            }
            GrafoCanvas.Children.Add(txtPeso);
        }

        // Desenha seta para curva
        private void DesenharSetaCurva(Point inicio, Point controle, Point fim,
                                       Brush cor, bool caminhoTemporario)
        {
            // Calcula direção no ponto final da curva
            Point pontoAntes = CalcularPontoBezier(inicio, controle, fim, 0.9);

            double dx = fim.X - pontoAntes.X;
            double dy = fim.Y - pontoAntes.Y;
            double comprimento = Math.Sqrt(dx * dx + dy * dy);

            if (comprimento == 0) return;

            dx /= comprimento;
            dy /= comprimento;

            double offset = 20;
            Point pontoFinal = new Point(fim.X - dx * offset, fim.Y - dy * offset);
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
        }

        // Desenha seta para laço
        private void DesenharSetaLaco(Point posicao, double angulo, Brush cor,
                                     bool caminhoTemporario)
        {
            double tamanhoSeta = 8;

            // Direção tangente ao círculo
            double dx = Math.Cos(angulo);
            double dy = Math.Sin(angulo);

            Point p1 = new Point(
                posicao.X - dy * tamanhoSeta - dx * tamanhoSeta,
                posicao.Y + dx * tamanhoSeta - dy * tamanhoSeta
            );
            Point p2 = new Point(
                posicao.X + dy * tamanhoSeta - dx * tamanhoSeta,
                posicao.Y - dx * tamanhoSeta - dy * tamanhoSeta
            );

            var seta = new Polygon
            {
                Points = new PointCollection { posicao, p1, p2 },
                Fill = cor,
                Stroke = cor
            };

            if (caminhoTemporario)
            {
                seta.Tag = "temp";
                caminhosDestacados.Add(seta);
            }
            GrafoCanvas.Children.Add(seta);
        }

        // Desenha seta reta (código original)
        private void DesenharSetaReta(Point origem, Point destino, Brush cor,
                                     bool caminhoTemporario)
        {
            double dx = destino.X - origem.X;
            double dy = destino.Y - origem.Y;
            double comprimento = Math.Sqrt(dx * dx + dy * dy);
            if (comprimento == 0) return;

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
        }

        // NOVO: Desenha peso da aresta
        private void DesenharPesoAresta(Point p1, Point p2, int peso, Brush cor,
                                       bool caminhoTemporario, double curvatura)
        {
            Point posicao;

            if (Math.Abs(curvatura) < 0.1)
            {
                // Posição normal para aresta reta
                posicao = new Point(
                    (p1.X + p2.X) / 2,
                    (p1.Y + p2.Y) / 2
                );
            }
            else
            {
                // Para curvas, usa o ponto já calculado
                posicao = p1;
            }

            var txtPeso = new TextBlock
            {
                Text = peso.ToString(),
                Foreground = cor,
                FontWeight = FontWeights.Bold,
                Background = Brushes.White,
                Padding = new Thickness(2)
            };
            Canvas.SetLeft(txtPeso, posicao.X - 5);
            Canvas.SetTop(txtPeso, posicao.Y - 8);

            if (caminhoTemporario)
            {
                txtPeso.Tag = "temp";
                caminhosDestacados.Add(txtPeso);
            }
            GrafoCanvas.Children.Add(txtPeso);
        }

        // NOVO: Calcula ponto em curva Bézier quadrática
        private Point CalcularPontoBezier(Point p0, Point p1, Point p2, double t)
        {
            double x = (1 - t) * (1 - t) * p0.X + 2 * (1 - t) * t * p1.X + t * t * p2.X;
            double y = (1 - t) * (1 - t) * p0.Y + 2 * (1 - t) * t * p1.Y + t * t * p2.Y;
            return new Point(x, y);
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

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(GrafoCanvas);

            foreach (var v in grafo.Vertices)
            {
                double dx = pos.X - v.Posicao.X;
                double dy = pos.Y - v.Posicao.Y;
                if (dx * dx + dy * dy <= 20 * 20)
                {
                    if (verticeSelecionado == null)
                    {
                        verticeSelecionado = v;
                        AdicionarMensagem($"Vértice {v.Nome} selecionado para conexão");
                        DesenharGrafo(); // Redesenha para mostrar borda destacada
                    }
                    else
                    {
                        AdicionarMensagem($"Criando conexão entre {verticeSelecionado.Nome} e {v.Nome}");

                        string input = Interaction.InputBox("Digite o peso da aresta:", "Peso da aresta", "1");
                        if (!int.TryParse(input, out int peso)) peso = 1;

                        // MODIFICADO: Permite criar laços e múltiplas arestas
                        verticeSelecionado.Adjacentes.Add((v, peso));

                        if (verticeSelecionado == v)
                        {
                            // É um laço
                            AdicionarMensagem($"Laço criado em {v.Nome} com peso {peso}");
                        }
                        else
                        {
                            string nomeA = $"{verticeSelecionado.Nome}-{v.Nome}";

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

                        DesenharGrafo();
                        verticeSelecionado = null;
                    }
                    return;
                }
            }

            if (verticeSelecionado != null)
            {
                verticeSelecionado = null;
                AdicionarMensagem("Seleção cancelada");
                DesenharGrafo();
                return;
            }

            var novoVertice = new Vertice
            {
                Nome = GerarNomeVertice(grafo),
                Posicao = pos
            };

            AdicionarMensagem($"Novo vértice {novoVertice.Nome} criado");
            grafo.Vertices.Add(novoVertice);
            DesenharGrafo();
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(GrafoCanvas);

            if (verticeSelecionado != null)
            {
                verticeSelecionado = null;
                DesenharGrafo();
                AdicionarMensagem("Seleção cancelada");
            }

            foreach (var v in grafo.Vertices.ToList())
            {
                double dx = pos.X - v.Posicao.X;
                double dy = pos.Y - v.Posicao.Y;

                if (dx * dx + dy * dy <= 20 * 20)
                {
                    foreach (var v2 in grafo.Vertices)
                        v2.Adjacentes.RemoveAll(a => a.vizinho == v);

                    grafo.Vertices.Remove(v);
                    AdicionarMensagem($"Vértice {v.Nome} removido do grafo");
                    DesenharGrafo();
                    return;
                }
            }

            foreach (var v in grafo.Vertices)
            {
                for (int i = v.Adjacentes.Count - 1; i >= 0; i--)
                {
                    var viz = v.Adjacentes[i];

                    // MODIFICADO: Melhor detecção para laços
                    if (v == viz.vizinho)
                    {
                        // É um laço - verifica distância do centro
                        double distCentro = Math.Sqrt(
                            (pos.X - v.Posicao.X) * (pos.X - v.Posicao.X) +
                            (pos.Y - v.Posicao.Y) * (pos.Y - v.Posicao.Y)
                        );

                        if (distCentro > 25 && distCentro < 50)
                        {
                            v.Adjacentes.RemoveAt(i);
                            AdicionarMensagem($"Laço removido de {v.Nome}");
                            DesenharGrafo();
                            return;
                        }
                    }
                    else if (EstaPertoDaLinha(v.Posicao, viz.vizinho.Posicao, pos, 10))
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

        // [Resto dos métodos permanecem iguais]
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

        private void GerarArvore_Click(object sender, RoutedEventArgs e)
        {
            if (grafo.Vertices.Count == 0)
            {
                AdicionarMensagem("Grafo está vazio. Adicione vértices primeiro.");
                return;
            }

            if (!grafo.Dirigido)
            {
                var mst = grafo.GerarArvoreGeradoraMinimaPrim();
                if (mst.Count == 0)
                {
                    AdicionarMensagem("Não foi possível gerar árvore geradora mínima (grafo desconexo).");
                    return;
                }

                int pesoTotal = 0;
                foreach (var aresta in mst)
                {
                    DesenharArestaReta(aresta.Origem, aresta.Destino, aresta.Peso, Brushes.Green, 4, true);
                    pesoTotal += aresta.Peso;
                }

                AdicionarMensagem($"Árvore Geradora Mínima (Prim) gerada! Peso total: {pesoTotal}");
            }
            else
            {
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
                    DesenharArestaReta(aresta.Origem, aresta.Destino, aresta.Peso, Brushes.Purple, 4, true);

                if (inalcançaveis.Count > 0)
                    AdicionarMensagem($"Alguns vértices não foram alcançáveis a partir de {raiz.Nome}: {string.Join(", ", inalcançaveis.Select(v => v.Nome))}");
                else
                    AdicionarMensagem($"Árvore de Caminhos Mínimos (Dijkstra) de {raiz.Nome} gerada com sucesso!");
            }
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

            sb.Append("     ");
            foreach (var v in grafo.Vertices)
            {
                sb.Append($"{v.Nome,4}");
            }
            sb.AppendLine();

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

            var dadosIncidencia = grafo.GerarMatrizIncidencia();
            int[,] matriz = dadosIncidencia.matriz;
            var arestas = dadosIncidencia.arestas;
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("MATRIZ DE INCIDÊNCIA");
            sb.AppendLine("".PadRight(50, '='));

            int numArestas = matriz.GetLength(1);

            sb.Append("     ");
            foreach (var aresta in arestas)
            {
                sb.Append($"{aresta.Nome}");
            }
            sb.AppendLine();

            for (int i = 0; i < grafo.Vertices.Count; i++)
            {
                sb.Append($"{grafo.Vertices[i].Nome,3}: ");
                for (int j = 0; j < numArestas; j++)
                {
                    var resultado = $"  {matriz[i, j]} ";
                    if (matriz[i, j] >= 0) resultado += " ";
                    sb.Append(resultado);
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

            string input = Interaction.InputBox("Digite o nome do vértice inicial para BFS:", "Vértice inicial", "");
            if (string.IsNullOrWhiteSpace(input))
            {
                AdicionarMensagem("BFS cancelada - nenhum vértice especificado.");
                return;
            }

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
                DesenharArestaReta(aresta.Origem, aresta.Destino, aresta.Peso, Brushes.Orange, 4, true);
            }

            AdicionarMensagem($"Busca em Largura (BFS) executada a partir de {origem.Nome}.");
        }

        private void ExecutarDFS_Click(object sender, RoutedEventArgs e)
        {
            if (grafo.Vertices.Count == 0)
            {
                AdicionarMensagem("Grafo vazio - não é possível executar DFS.");
                return;
            }

            string input = Interaction.InputBox("Digite o nome do vértice inicial para DFS:", "Vértice inicial", "");
            if (string.IsNullOrWhiteSpace(input))
            {
                AdicionarMensagem("DFS cancelada - nenhum vértice especificado.");
                return;
            }

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
                DesenharArestaReta(aresta.Origem, aresta.Destino, aresta.Peso, Brushes.Blue, 4, true);
            }

            AdicionarMensagem($"Busca em Profundidade (DFS) executada a partir de {origem.Nome}.");
        }

        private void ExecutarRoy_Click(object sender, RoutedEventArgs e)
        {
            var totalVertices = grafo.Vertices.Count;
            if (totalVertices == 0)
            {
                AdicionarMensagem("Grafo vazio - não é possível executar algoritmo de Roy.");
                return;
            }

            var resultado = grafo.Roy();

            if (!string.IsNullOrWhiteSpace(resultado.mensagem))
            {
                AdicionarMensagem(resultado.mensagem);
            }

            if (resultado.componentes.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(resultado.mensagem))
                {
                    AdicionarMensagem("Não foram encontradas componentes conexas. O grafo contém apenas vértices isolados.");
                }
                return;
            }

            var cores = new List<Brush> { Brushes.Red, Brushes.Pink };
            int corIndex = 0;

            for (int i = 0; i < resultado.componentes.Count; i++)
            {
                var componente = resultado.componentes[i];

                var verticesDaComponente = new HashSet<string>();
                foreach (var aresta in componente)
                {
                    verticesDaComponente.Add(aresta.Origem.Nome);
                    verticesDaComponente.Add(aresta.Destino.Nome);
                }

                string tipoComponente = grafo.Dirigido ? "fortemente conexa" : "conexa";
                AdicionarMensagem($"Componente {tipoComponente} {i + 1}: " +
                                 $"Vértices [{string.Join(", ", verticesDaComponente.OrderBy(v => v))}]");

                Brush corAtual = cores[corIndex % cores.Count];
                foreach (var aresta in componente)
                {
                    DesenharArestaReta(aresta.Origem, aresta.Destino, aresta.Peso, corAtual, 4, true);
                }

                corIndex++;
            }

            string resumo = grafo.Dirigido ?
                $"Análise de Roy concluída: {resultado.componentes.Count} componente(s) fortemente conexa(s) encontrada(s)" :
                $"Análise de Roy concluída: {resultado.componentes.Count} componente(s) conexa(s) encontrada(s)";

            AdicionarMensagem(resumo);
        }

        private void ExecutarWelshPowell_Click(object sender, RoutedEventArgs e)
        {
            if (grafo.Vertices.Count == 0)
            {
                AdicionarMensagem("Grafo vazio - não é possível executar coloração.");
                return;
            }

            if (grafo.Vertices.Count == 1)
            {
                AdicionarMensagem("Grafo com apenas 1 vértice - coloração trivial (1 cor).");
                return;
            }

            coloracaoAtual = grafo.ColoracaoWelshPowell();

            if (coloracaoAtual.Count == 0)
            {
                AdicionarMensagem("Erro ao executar coloração de Welsh-Powell.");
                return;
            }

            DesenharGrafo();

            string estatisticas = grafo.ObterEstatisticasColoracao(coloracaoAtual);
            AdicionarMensagem(estatisticas);

            int numeroCores = coloracaoAtual.Values.Distinct().Count();
            AdicionarMensagem($"✓ Coloração aplicada com sucesso! Número cromático: {numeroCores}");
        }

        private void LimparCaminhos_Click(object sender, RoutedEventArgs e)
        {
            int removed = 0;

            foreach (var elem in caminhosDestacados.ToList())
            {
                if (GrafoCanvas.Children.Contains(elem))
                {
                    GrafoCanvas.Children.Remove(elem);
                    removed++;
                }
            }
            caminhosDestacados.Clear();

            for (int i = GrafoCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (GrafoCanvas.Children[i] is FrameworkElement fe && fe.Tag != null && fe.Tag.ToString() == "temp")
                {
                    GrafoCanvas.Children.RemoveAt(i);
                    removed++;
                }
            }

            if (coloracaoAtual != null)
            {
                coloracaoAtual = null;
                DesenharGrafo();
                AdicionarMensagem("Coloração removida.");
            }

            AdicionarMensagem($"Limpeza de caminhos executada.");
        }

        private Vertice EscolherVerticeValido()
        {
            foreach (var v in grafo.Vertices)
            {
                if (v.Adjacentes.Count > 0)
                    return v;
            }
            return null;
        }

        private void LimparLog_Click(object sender, RoutedEventArgs e)
        {
            MensagensListBox.Items.Clear();
            AdicionarMensagem("Log de atividades limpo");
        }

        private void LimparGrafo_Click(object sender, RoutedEventArgs e)
        {
            grafo.Vertices.Clear();
            verticeSelecionado = null;
            GrafoCanvas.Children.Clear();
            MensagensListBox.Items.Clear();
            DirecaoCheckBox.IsEnabled = true;
            coloracaoAtual = null;

            AdicionarMensagem("Grafo completamente limpo!");
        }

        private void Dirigido_Checked(object sender, RoutedEventArgs e)
        {
            grafo.Dirigido = true;
            AdicionarMensagem("Modo dirigido ativado (DIJKSTRA)");
            GerarArvoreButton.Content = "🌳 DIJKSTRA";
            DesenharGrafo();
        }

        private void Dirigido_Unchecked(object sender, RoutedEventArgs e)
        {
            grafo.Dirigido = false;
            AdicionarMensagem("Modo não-dirigido ativado (PRIM)");
            GerarArvoreButton.Content = "🌳 PRIM";
            DesenharGrafo();
        }
    }
}