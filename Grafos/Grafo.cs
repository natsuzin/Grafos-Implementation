using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GrafoWPF
{
    // Representa um grafo com vértices e arestas/arcos
    public class Grafo
    {
        // Lista principal: cada posição é um vértice
        public List<Vertice> Vertices { get; set; } = new(); // lista de vértices do grafo, onde cada vértice tem sua própria lista de adjacência
        public bool Dirigido { get; set; } = false; // se o grafo é dirigido

        /*
         * ===== LISTA DE ADJACÊNCIA =====
         * Forma de armazenar o grafo onde cada vértice mantém uma lista dos seus vizinhos (adjacentes) e os pesos das arestas que os conectam.
         */
        public string ListarAdjacencias()
        {
            var sb = new StringBuilder();
            foreach (var v in Vertices)
            {
                sb.Append($"{v.Nome} → ");
                if (v.Adjacentes.Count == 0)
                {
                    sb.Append("(isolado)");
                }
                else
                {
                    foreach (var (vizinho, peso) in v.Adjacentes)
                    {
                        sb.Append($"{vizinho.Nome}({peso}) ");
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
        
        // Verifica se dois vértices são adjacentes
        public bool SaoAdjacentes(string v1Nome, string v2Nome)
        {
            if (string.IsNullOrWhiteSpace(v1Nome) || string.IsNullOrWhiteSpace(v2Nome)) // verifica se os nomes são válidos
                return false;

            // remove espaços em branco
            v1Nome = v1Nome.Trim(); 
            v2Nome = v2Nome.Trim();

            // procura os vértices na lista
            var v1 = Vertices.FirstOrDefault(v => v.Nome.Equals(v1Nome, StringComparison.OrdinalIgnoreCase));
            var v2 = Vertices.FirstOrDefault(v => v.Nome.Equals(v2Nome, StringComparison.OrdinalIgnoreCase));
            if (v1 == null || v2 == null) return false;

            // verifica se v2 está na lista de adjacência de v1
            bool adj = v1.Adjacentes.Any(a =>
                a.vizinho == v2 || a.vizinho.Nome.Equals(v2.Nome, StringComparison.OrdinalIgnoreCase)
            );

            return adj;
        }

        /* ===== ALGORITMO DE PRIM =====
         * Utilizado em grafos não dirigidos e ponderados.
         * É usado para encontrar a árvore geradora mínima (MST).
         * Ele seleciona um subconjunto de arestas que conecta todos os vértices com o menor peso total possível, sem formar ciclos.
         * 
         * Entrada: Um grafo ponderado e conectado.
         * Saída: Um conjunto de arestas que formam a árvore geradora mínima.
         * Estratégia: Começa com um vértice inicial e expande a árvore adicionando a aresta de menor peso que conecta um vértice na árvore a um vértice fora dela.
         */
        public List<Aresta> GerarArvoreGeradoraMinimaPrim()
        {
            // armazena as arestas da árvore geradora mínima
            var mst = new List<Aresta>(); // lista final da MST
            if (Vertices.Count == 0) return mst;
            

            var visitados = new HashSet<Vertice>(); // vértices já incluídos na MST - garante que não formem ciclos
            var arestasDisponiveis = new List<Aresta>(); // arestas que podem ser adicionadas à MST - 

            // inicialização da árvore
            // var inicial = Vertices[0];
            var inicial = Vertices
                .OrderBy(v => v.Adjacentes.Sum(a => a.peso))
                .First();

            visitados.Add(inicial); // pega o vértice inicial

            // roda pela lista de adjacência do vértice inicial
            foreach (var (vizinho, peso) in inicial.Adjacentes)
            {
                // adiciona todas as arestas do vértice inicial à lista de disponíveis
                arestasDisponiveis.Add(new Aresta
                {
                    Origem = inicial,
                    Destino = vizinho,
                    Peso = peso,
                    Nome = $"{inicial.Nome}-{vizinho.Nome}" 
                });
            }

            // crescimento da árvore
            while (visitados.Count < Vertices.Count && arestasDisponiveis.Count > 0)
            {
                // pega a aresta de menor peso que conecta um vértice visitado a um não visitado
                var menor = arestasDisponiveis.OrderBy(a => a.Peso).FirstOrDefault(a =>
                    (visitados.Contains(a.Origem) && !visitados.Contains(a.Destino)) ||
                    (visitados.Contains(a.Destino) && !visitados.Contains(a.Origem)));

                if (menor == null) break;

                mst.Add(menor); // adiciona a aresta menor à árvore geradora mínima

                var novo = visitados.Contains(menor.Origem) ? menor.Destino : menor.Origem; // novo vértice a ser adicionado
                visitados.Add(novo); // marca o novo vértice como visitado

                // atualização das arestas disponíveis
                foreach (var (vizinho, peso) in novo.Adjacentes)
                {
                    // para cada vizinho do novo vértice, se não foi visitado, adiciona a aresta à lista de disponíveis
                    if (!visitados.Contains(vizinho))
                    {
                        arestasDisponiveis.Add(new Aresta
                        {
                            Origem = novo,
                            Destino = vizinho,
                            Peso = peso,
                            Nome = $"{inicial.Nome}-{vizinho.Nome}"
                        });
                    }
                }
            }

            return mst;
        }

        /* ===== ALGORITMO DE DIJKSTRA PARA CAMINHOS MÍNIMOS =====
         * Utilizado em grafos dirigidos ou não dirigidos e ponderados.
         * É usado para encontrar o caminho mais curto de um vértice de origem para todos os outros vértices em um grafo ponderado.
         */
        public (List<Aresta> arvore, List<Vertice> inalcançaveis) GerarArvoreCaminhosMinimos(Vertice origem)
        {
            var dist = new Dictionary<Vertice, int>();
            var anterior = new Dictionary<Vertice, Vertice>();
            var pendentes = new HashSet<Vertice>(Vertices);

            foreach (var v in Vertices)
                dist[v] = int.MaxValue;

            dist[origem] = 0;

            while (pendentes.Count > 0)
            {
                // pega o vértice não processado com menor distância
                var atual = pendentes.OrderBy(v => dist[v]).First();
                pendentes.Remove(atual);

                // Se a distância ainda for infinita, os vértices restantes são inalcançáveis
                if (dist[atual] == int.MaxValue)
                    break;

                foreach (var (vizinho, peso) in atual.Adjacentes)
                {
                    // aqui garante que só seguimos a direção da seta
                    int alt = dist[atual] + peso;
                    if (alt < dist[vizinho])
                    {
                        dist[vizinho] = alt;
                        anterior[vizinho] = atual;
                    }
                }
            }

            // monta a árvore de caminhos mínimos
            var resultado = new List<Aresta>();
            foreach (var kv in anterior)
            {
                var u = kv.Value;
                var v = kv.Key;
                var aresta = u.Adjacentes.First(a => a.vizinho == v);
                resultado.Add(new Aresta
                {
                    Origem = u,
                    Destino = v,
                    Peso = aresta.peso,
                    Nome = $"{u.Nome}->{v.Nome}"
                });
            }

            // verifica vértices inalcançáveis
            var inalcançaveis = Vertices.Where(v => dist[v] == int.MaxValue && v != origem).ToList();

            return (resultado, inalcançaveis);
        }


        /*
        * ===== BUSCA POR LARGURA =====
        * Essa busca explora todos os vizinhos de um vértice antes de avançar para os vizinhos dos vizinhos.
        */
        public List<Aresta> BuscaLargura(Vertice origem)
        {
            var arvore = new List<Aresta>(); // armazena as arestas da árvore de busca em largura
            var visitados = new HashSet<Vertice>(); // armazena os vértices já visitados
            var fila = new Queue<Vertice>(); // fila para gerenciar a ordem de visita

            visitados.Add(origem); // marca o vértice de origem como visitado
            fila.Enqueue(origem); // adiciona o vértice de origem à fila

            while (fila.Count > 0)
            {
                var atual = fila.Dequeue(); // remove o vértice da frente da fila

                // explora todos os vizinhos do vértice atual
                foreach (var (vizinho, peso) in atual.Adjacentes)
                {
                    // se o vizinho ainda não foi visitado, marca como visitado e adiciona à fila
                    if (!visitados.Contains(vizinho))
                    {
                        visitados.Add(vizinho); // marca como visitado
                        fila.Enqueue(vizinho); // adiciona à fila

                        // adiciona a aresta à árvore de busca em largura
                        arvore.Add(new Aresta
                        {
                            Origem = atual,
                            Destino = vizinho,
                            Peso = peso,
                            Nome = $"{atual.Nome}-{vizinho.Nome}"
                        });
                    }
                }
            }

            return arvore;
        }

        /*
         * ===== BUSCA POR PROFUNDIDADE =====
         * Essa busca explora o máximo possível ao longo de cada ramo antes de retroceder.
         */
        public List<Aresta> BuscaProfundidade(Vertice origem)
        {
            var arvore = new List<Aresta>(); // armazena as arestas da árvore de busca em profundidade
            var visitados = new HashSet<Vertice>(); // armazena os vértices já visitados

            // função recursiva para realizar a busca em profundidade
            void DFS(Vertice v)
            {
                visitados.Add(v); // marca o vértice atual como visitado

                // explora todos os vizinhos do vértice atual
                foreach (var (vizinho, peso) in v.Adjacentes)
                {
                    // se o vizinho ainda não foi visitado, adiciona a aresta e continua a busca recursivamente
                    if (!visitados.Contains(vizinho))
                    {
                        // adiciona a aresta à árvore de busca em profundidade
                        arvore.Add(new Aresta
                        {
                            Origem = v,
                            Destino = vizinho,
                            Peso = peso,
                            Nome = $"{v.Nome}-{vizinho.Nome}"
                        });
                        DFS(vizinho);
                    }
                }
            }

            DFS(origem);
            return arvore;
        }

        /*
        *   ===== ALGORITMO DE ROY CORRIGIDO =====
        *   Este algoritmo encontra as componentes fortemente conexas de um grafo dirigido
        *   através das relações de alcançabilidade (sucessores e antecessores)
        */
        public (List<List<Aresta>> componentes, String mensagem) Roy()
        {
            List<List<Aresta>> componentes = new List<List<Aresta>>();
            String mensagem = "";

            // Função recursiva do algoritmo de Roy
            void algoritmoRoy(List<Vertice> vertices)
            {
                if (vertices.Count == 0 || vertices == null) return;

                var v = vertices[0]; // Vértice inicial
                var sucessores = new HashSet<Vertice>(); // Vértices alcançáveis a partir de v
                var antecessores = new HashSet<Vertice>(); // Vértices que podem alcançar v
                var verticesComponente = new List<Vertice>();
                var componente = new List<Aresta>();

                // Função para encontrar todos os sucessores de um vértice (busca em profundidade)
                void EncontrarSucessores(Vertice vp, HashSet<Vertice> visitados)
                {
                    if (visitados.Contains(vp)) return;
                    visitados.Add(vp);
                    sucessores.Add(vp);

                    // Explora todos os vizinhos diretos
                    foreach (var (vizinho, peso) in vp.Adjacentes)
                    {
                        if (!visitados.Contains(vizinho))
                        {
                            EncontrarSucessores(vizinho, visitados);
                        }
                    }
                }

                // Função para encontrar todos os antecessores de um vértice
                void EncontrarAntecessores(Vertice vn)
                {
                    antecessores.Add(vn);

                    // Procura por todos os vértices que têm vn como vizinho
                    foreach (var vertice in vertices)
                    {
                        if (!antecessores.Contains(vertice))
                        {
                            // Verifica se este vértice tem uma aresta para vn
                            if (vertice.Adjacentes.Any(adj => adj.vizinho == vn))
                            {
                                EncontrarAntecessores(vertice);
                            }
                        }
                    }
                }

                // Encontra sucessores a partir de v
                EncontrarSucessores(v, new HashSet<Vertice>());

                // Encontra antecessores de v
                EncontrarAntecessores(v);

                // A componente fortemente conexa é a interseção entre sucessores e antecessores
                verticesComponente = sucessores.Intersect(antecessores).ToList();

                // Cria as arestas do componente
                foreach (var vc in verticesComponente)
                {
                    foreach (var (vizinho, peso) in vc.Adjacentes)
                    {
                        if (verticesComponente.Contains(vizinho))
                        {
                            // Para grafos não dirigidos, evita arestas duplicadas
                            if (!Dirigido && vc.Nome.CompareTo(vizinho.Nome) > 0)
                                continue;

                            // Cria arestas do componente
                            componente.Add(new Aresta
                            {
                                Origem = vc,
                                Destino = vizinho,
                                Peso = peso,
                                Nome = Dirigido ? $"{vc.Nome}->{vizinho.Nome}" : $"{vc.Nome}-{vizinho.Nome}"
                            });
                        }
                    }
                }

                // Só adiciona componentes com mais de um vértice ou pelo menos uma aresta
                if (verticesComponente.Count > 1 || componente.Count > 0)
                {
                    // Verifica se o grafo é fortemente conexo (todos os vértices estão na mesma componente)
                    if (verticesComponente.Count == Vertices.Count)
                    {
                        mensagem = Dirigido ? "Grafo Fortemente Conexo" : "Grafo Conexo";
                    }

                    componentes.Add(componente);
                }

                // Remove os vértices já processados e continua com os restantes
                vertices = vertices.Except(verticesComponente).ToList();
                algoritmoRoy(vertices);
            }

            // Cria uma cópia da lista de vértices para não modificar a original
            var verticesCopia = new List<Vertice>(Vertices);

            // Inicializa algoritmo de Roy
            algoritmoRoy(verticesCopia);

            // Se não encontrou nenhuma componente, mas há vértices, significa que são vértices isolados
            if (componentes.Count == 0 && Vertices.Count > 0)
            {
                mensagem = "Grafo contém apenas vértices isolados (sem arestas)";
            }

            return (componentes, mensagem);
        }

        /*
         * ===== MATRIZES DE ADJACÊNCIA =====
         * A matriz ed adjacencia é uma representação do grafo onde uma matriz 2D é usada para indicar a presença e o peso das arestas entre os vértices.
         */
        public int[,] GerarMatrizAdjacencia()
        {
            int n = Vertices.Count;
            int[,] matriz = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matriz[i, j] = 0;
                }
            }

            for (int i = 0; i < n; i++)
            {
                foreach (var (vizinho, peso) in Vertices[i].Adjacentes)
                {
                    int j = Vertices.IndexOf(vizinho);
                    matriz[i, j] = peso;
                }
            }

            return matriz;
        }

        /*
         * ===== MATRIZES DE INCIDÊNCIA =====
         * A matriz de incidência é uma representação do grafo onde uma matriz 2D é usada para indicar a relação entre vértices e arestas.
         */
        public (int[,] matriz, List<Aresta> arestas) GerarMatrizIncidencia()
        {
            var arestas = new List<Aresta>();

            foreach (var v in Vertices)
            {
                foreach (var (vizinho, peso) in v.Adjacentes)
                {
                    if (Dirigido || v.Nome.CompareTo(vizinho.Nome) < 0) 
                    {
                        arestas.Add(new Aresta
                        {
                            Origem = v,
                            Destino = vizinho,
                            Peso = peso,
                            Nome = $"{v.Nome}-{vizinho.Nome}"
                        });
                    }
                }
            }

            int[,] matriz = new int[Vertices.Count, arestas.Count];

            for (int j = 0; j < arestas.Count; j++)
            {
                var a = arestas[j];
                int iOrigem = Vertices.IndexOf(a.Origem);
                int iDestino = Vertices.IndexOf(a.Destino);
                if (Dirigido)
                {
                    matriz[iOrigem, j] = 1; // saída
                    matriz[iDestino, j] = -1; // entrada
                }
                else
                {
                    matriz[iOrigem, j] = 1;
                    matriz[iDestino, j] = 1;
                }
            }

            return (matriz, arestas);
        }
    }
}
