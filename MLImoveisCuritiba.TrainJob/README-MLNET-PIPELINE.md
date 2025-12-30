
# 📘 ML.NET – Entendendo os Componentes do Pipeline

Este documento explica, de forma **prática e aplicada**, os principais **métodos e componentes do ML.NET** utilizados em pipelines de Machine Learning, além de detalhar **quando usar OneHotEncoding vs OneHotHashEncoding**.

O objetivo é servir como **guia de estudo e referência** para projetos reais em produção.

---

## 🧠 Visão Geral do Pipeline

Um pipeline de ML.NET segue o conceito de **linha de montagem**:

1. Carrega os dados
2. Prepara / transforma os dados
3. Treina o modelo
4. Avalia a qualidade
5. Salva o modelo treinado
6. Usa o modelo para predição

Cada etapa é encadeada usando `Append()`.

---

## 🔹 TrainTestSplit()

```csharp
var split = ml.Data.TrainTestSplit(data, testFraction: 0.2);
```

### O que faz
Divide o dataset em dois conjuntos:
- **TrainSet** → usado para treinar o modelo
- **TestSet** → usado para avaliar se o modelo generaliza bem

### Por que usar
Evita que o modelo seja avaliado com os mesmos dados usados no treino, o que geraria métricas irreais.

---

## 🔹 OneHotEncoding()

```csharp
ml.Transforms.Categorical.OneHotEncoding("TipoEnc", nameof(ModelInput.Tipo))
```

### O que faz
- Transforma valores categóricos (strings) em vetores binários.
- Cria uma coluna para cada valor único.
- Aprende essas categorias durante o treino
- Dimensão do vetor = nº de categorias vistas no treino

### Exemplo (Bairro)

```csharp
Batel, Centro, Água Verde
```
### Encoding:
```csharp
Batel       → [1,0,0]
Centro      → [0,1,0]
Água Verde  → [0,0,1]

```
### ⚠️ Problema em produção
Se aparecer:
```csharp
Bairro = "Cabral"
```
- ➡️ Nunca visto no treino
- ➡️ O ML.NET gera um vetor todo zerado
- ➡️ Informação perdida

### Quando usar
- ✔️ Categorias fixas e conhecidas
- ✔️ Ex: Piscina (SIM / NAO), Tipo (casa / apartamento)
- ✔️ Poucas categorias
- ✔️ Estado → PR (ou poucos estados)

---

## 🔹 OneHotHashEncoding()

```csharp
ml.Transforms.Categorical.OneHotHashEncoding("BairroEnc", nameof(ModelInput.Bairro), numberOfBits: 10)
```

### O que faz
- Aplica hashing para mapear categorias dinâmicas em vetores de tamanho fixo.
- Aplica hashing sobre o valor.
- Mapeia a categoria para um índice fixo.
- Não precisa conhecer a categoria antes.

```csharp
Bairro = "Batel"        → hash("Batel") % N
Bairro = "Cabral"      → hash("Cabral") % N
Bairro = "Novo Bairro" → hash("Novo Bairro") % N

```
Sempre cai em algum bucket válido.

### 💡 Vantagem

- Funciona com categorias nunca vistas
- Vetor tem tamanho fixo
- Ótimo para produção

### ⚠️ Trade-off

- Pode ocorrer colisão de hash
- Perde interpretabilidade (“qual coluna é qual bairro?”)

### Quando usar
- Categorias abertas
- Alta cardinalidade
- Alto volume de valores
- Produção

---

## 🔥 Comparação

| Critério | OneHotEncoding | OneHotHashEncoding |
|--------|----------------|--------------------|
| Categorias fixas | ✅ | ⚠️ |
| Categorias novas | ❌ | ✅ |
| Produção | ⚠️ | ✅ |

---

## 🔹 Append()

Encadeia transformações no pipeline.

---

## 🔹 Fit()

Treina o pipeline e gera o modelo final.

---

## 🔹 FastForestRegression

Modelo baseado em Random Forest, robusto para dados tabulares.

---

## 🔹 Transform()

Aplica o modelo treinado e gera previsões.

---

## 🔹 Evaluate()

Calcula métricas como R², RMSE e MAE.

---

## 🔹 Save()

Salva o pipeline treinado em arquivo `.mlnet`.

---
