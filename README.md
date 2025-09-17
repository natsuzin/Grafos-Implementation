# Projeto de Grafos – T1

**UNIVALI – Politécnica Kobrasol**  
**Curso:** Ciência da Computação – 2025/2  
**Disciplina:** Grafos  
**Profª:** Fernanda Cunha  

## Descrição do Projeto

Este projeto tem como objetivo implementar uma estrutura de **grafos** utilizando **Lista de Adjacência**, contemplando tanto grafos **dirigidos** quanto **não dirigidos**, com qualquer número de vértices e arestas.  

Um grafo \(G = (V, A)\) é definido por:  
- **V**: conjunto de vértices, cada um com um identificador único;  
- **A**: conjunto de arestas ou arcos, cada um com um identificador e um valor numérico (custo/peso).  

O projeto também define relações de **adjacência**, **incidência** e operações essenciais sobre o grafo, conforme detalhado abaixo.

## Funcionalidades Implementadas

O programa permite realizar as seguintes operações:  

1. **Inserir vértice isolado** – Adiciona um novo vértice ao grafo.  
2. **Inserir aresta/arco** – Conecta dois vértices com identificador e valor definido.  
3. **Remover vértice** – Remove um vértice e todas as arestas/arcos incidentes.  
4. **Remover ligação** – Remove uma aresta ou arco específico do grafo.  
5. **Visualização do grafo** – Mostra graficamente os vértices e as ligações.
6. **Algoritmo de Prim** – Calcula a Árvore Geradora Mínima (AGM) e apresenta graficamente.  
7. **Busca em Largura (BFS)** – Realiza BFS a partir de um vértice e mostra a árvore gerada.  
8. **Busca em Profundidade (DFS)** – Realiza DFS a partir de um vértice e mostra a árvore gerada.  
9. **Componentes Conexas/Fortemente Conexas** – Identifica e apresenta na tela os conjuntos de vértices conectados.

> Cada funcionalidade está alinhada aos requisitos de avaliação da disciplina e possui pontuação específica para fins de nota.

## Estrutura do Projeto

- **Classes/Tipos principais**:  
  - `Grafo` – Estrutura principal do grafo com atributos essenciais (vértices, arestas/arcos, lista de adjacência).  
  - `Vertice` – Representa cada vértice com identificador único.  
  - `Aresta` / `Arco` – Representa conexões entre vértices, com identificador e peso.  

- **Interface do Usuário**:  
  - Tela básica com moldura e menu para acesso às operações do grafo ativo.  
  - Permite selecionar operações, inserir/remover vértices e arestas, e visualizar resultados graficamente.  

