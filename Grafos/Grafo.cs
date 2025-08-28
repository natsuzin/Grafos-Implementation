using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GrafoWPF
{
    public class Grafo
    {
        // Lista principal: cada posição é um vértice
        public List<Vertice> Vertices { get; set; } = new();
        public bool Dirigido { get; set; } = false; // se o grafo é dirigido

        public string ListarAdjacencias()
        {
            var sb = new StringBuilder();
            foreach (var v in Vertices)
            {
                sb.Append($"{v.Nome} → ");
                if (v.Adjacentes.Count == 0)
                {
                    sb.Append(" (isolado)");
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

        public bool SaoAdjacentes(string v1Nome, string v2Nome)
        {
            if (string.IsNullOrWhiteSpace(v1Nome) || string.IsNullOrWhiteSpace(v2Nome))
                return false;

            v1Nome = v1Nome.Trim();
            v2Nome = v2Nome.Trim();

            var v1 = Vertices.FirstOrDefault(v => v.Nome.Equals(v1Nome, StringComparison.OrdinalIgnoreCase));
            var v2 = Vertices.FirstOrDefault(v => v.Nome.Equals(v2Nome, StringComparison.OrdinalIgnoreCase));
            if (v1 == null || v2 == null) return false;

            bool adj = v1.Adjacentes.Any(a =>
                a.vizinho == v2 ||
                a.vizinho.Nome.Equals(v2.Nome, StringComparison.OrdinalIgnoreCase)
            );

            if (!Dirigido)
            {
                adj = adj || v2.Adjacentes.Any(a =>
                    a.vizinho == v1 ||
                    a.vizinho.Nome.Equals(v1.Nome, StringComparison.OrdinalIgnoreCase)
                );
            }

            return adj;
        }
        public List<Aresta> GerarArvoreGeradoraMinimaPrim()
        {
            var mst = new List<Aresta>();
            if (Vertices.Count == 0) return mst;
            
            var visitados = new HashSet<Vertice>();
            var arestasDisponiveis = new List<Aresta>();

            var inicial = Vertices[0];
            visitados.Add(inicial);

            foreach (var (vizinho, peso) in inicial.Adjacentes)
            {
                arestasDisponiveis.Add(new Aresta
                {
                    Origem = inicial,
                    Destino = vizinho,
                    Peso = peso,
                    Nome = $"{inicial.Nome}-{vizinho.Nome}"
                });
            }

            while (visitados.Count < Vertices.Count && arestasDisponiveis.Count > 0)
            {
                var menor = arestasDisponiveis.OrderBy(a => a.Peso).FirstOrDefault(a =>
                    (visitados.Contains(a.Origem) && !visitados.Contains(a.Destino)) ||
                    (visitados.Contains(a.Destino) && !visitados.Contains(a.Origem)));

                if (menor == null) break; 

                mst.Add(menor);

                var novo = visitados.Contains(menor.Origem) ? menor.Destino : menor.Origem;
                visitados.Add(novo);

                foreach (var (vizinho, peso) in novo.Adjacentes)
                {
                    if (!visitados.Contains(vizinho))
                    {
                        arestasDisponiveis.Add(new Aresta
                        {
                            Origem = novo,
                            Destino = vizinho,
                            Peso = peso,
                            Nome = $"{novo.Nome}-{vizinho.Nome}"
                        });
                    }
                }
            }

            return mst;
        }
    }
}
