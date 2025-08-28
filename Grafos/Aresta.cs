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
        public Vertice Origem { get; set; }
        public Vertice Destino { get; set; }
        public int Peso { get; set; }
        public string Nome { get; set; }
    }
}

