using System;
using System.Collections.Generic;
using System.Windows;

namespace GrafoWPF
{
    // cada vértice tem um nome, posição e lista de adjacência
    public class Vertice
    {
        public string Nome { get; set; }
        public Point Posicao { get; set; }

        // Lista de adjacência: cada item é o vértice vizinho + peso da aresta
        public List<(Vertice vizinho, int peso)> Adjacentes { get; set; } = new();
    }
}
