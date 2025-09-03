using Grafos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrafoWPF
{
    // cada aresta conecta dois vértices, tem um peso e um nome
    public class Aresta
    {
        public Vertice Origem { get; set; } // vértice de origem
        public Vertice Destino { get; set; } // vértice de destino
        public string Nome { get; set; } // nome da aresta
        public int Peso { get; set; } // peso da aresta
    }
}

