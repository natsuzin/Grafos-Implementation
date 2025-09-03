using System;
using System.Collections.Generic;
using System.Windows;

namespace GrafoWPF
{
    // Cada vértice tem um nome, posição e lista de adjacência
    public class Vertice
    {
        public string Nome { get; set; } // nome do vértice
        public Point Posicao { get; set; } // posição do vértice na tela
        public List<(Vertice vizinho, int peso)> Adjacentes { get; set; } = new(); // inicializa a lista de adjacentes vazia
    }
}
