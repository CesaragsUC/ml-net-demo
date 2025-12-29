
# 🏠 ML Imóveis Curitiba – Price Prediction API

A **Machine Learning project in .NET** for **real estate price prediction in Curitiba (Brazil)**, built with **ML.NET**, featuring **automated training**, **scheduled background jobs (Quartz)**, and a **Web API for inference**.

This project follows **production‑grade best practices**, clearly separating **model training** from **model inference**.

---

## 🎯 Goal

- Train a regression model to predict **real estate sale prices**
- Automate **periodic retraining**
- Expose a **REST API** for predictions
- Allow **hot‑reload of the model** without restarting the API

---

## 🧠 Architecture Overview

```
┌──────────────────────────┐
│ Historical CSV Dataset   │
└─────────────┬────────────┘
              │
              ▼
┌──────────────────────────┐
│ TrainJob (Quartz Worker) │
│ - ML.NET                 │
│ - Feature engineering    │
│ - Metrics (R², RMSE)     │
│ - Model generation       │
└─────────────┬────────────┘
              │
              ▼
┌──────────────────────────┐
│ ImovelVenda.mlnet        │  ← Trained model
│ (versionable artifact)   │
└─────────────┬────────────┘
              │
              ▼
┌──────────────────────────┐
│ ASP.NET Core Web API     │
│ - PredictionEnginePool   │
│ - Auto model reload      │
│ - Swagger / OpenAPI      │
└─────────────┬────────────┘
              │
              ▼
        Client / Front‑end
```

---

## 🧩 Solution Structure

```
MLSampleApp
│
├── MLImoveisCuritiba.Shared
│   ├── ModelInput.cs
│   ├── ModelOutput.cs
│
├── MLImoveisCuritiba.TrainJob
│   ├── Program.cs
│   ├── TrainModelJob.cs
│   ├── appsettings.json
│   └── Data/
│       └── curitiba_imoveis_ml_poc_v2.csv
│
├── MLImoveisCuritiba.WebApi
│   ├── Controllers/
│   │   └── PredictPriceController.cs
│   ├── Models/
│   │   ├── PredictRequest.cs
│   │   └── PredictResponse.cs
│   ├── Program.cs
│   └── ImovelVenda.mlnet
```

---

## 🔁 Model Training (TrainJob)

- Implemented as a **Worker Service**
- Scheduled with **Quartz.NET**
- Runs automatically:
  - **Once every 24 hours** (configurable)
- Pipeline:
  - One‑Hot Encoding for categorical features
  - Normalization for numerical features
  - Regression using **FastForest**
- Metrics evaluated:
  - R²
  - RMSE
  - MAE

### Safe model update
The model is first written to a temporary file and then atomically replaced:

```
ImovelVenda.mlnet.tmp → ImovelVenda.mlnet
```

This prevents the API from reading a partially written model.

---

## ⚡ Inference (Web API)

- ASP.NET Core (Controllers‑based, not Minimal API)
- Uses **PredictionEnginePool** (thread‑safe)
- Configured with:
  - `watchForChanges: true`
- The API automatically reloads the model when the file changes
- Swagger enabled

### Endpoint
```
POST /api/v1/predict/price
```

### Sample Request
```json
{
  "cidade": "Curitiba",
  "bairro": "Batel",
  "estado": "PR",
  "cep": "80420-000",
  "qtdQuartos": 3,
  "piscina": "NAO",
  "tipo": "apartamento",
  "anoConstrucao": 2018,
  "anoReferencia": 2025,
  "areaM2": 110,
  "vagasGaragem": 2,
  "condominioMensal": 1500,
  "distCentroKm": 2.1,
  "proxParque": "SIM"
}
```

### Sample Response
```json
{
  "precoPrevisto": 1537639.4,
  "precoPrevistoFormatado": "R$ 1,537,639.40"
}
```

---

## 🧪 Why separate Training and API?

| Reason | Benefit |
|------|--------|
| Performance | API never trains at runtime |
| Stability | No CPU spikes |
| Scalability | API can scale horizontally |
| Maintainability | Training evolves independently |
| Real production pattern | Cloud‑ready architecture |

---

## 🛠️ Technologies Used

- .NET 10
- ML.NET
- Quartz.NET
- ASP.NET Core Web API
- Swagger / OpenAPI
- C#
- CSV dataset (PoC)

---

## 🚀 Possible Improvements

- Model versioning
- Automatic metric validation before replacing the model
- Feature drift detection
- Docker / Kubernetes deployment
- Blob Storage for model artifacts
- Observability (Prometheus / Application Insights)

---

## 📌 Note

This project was designed with a strong focus on **architecture, clarity, and production best practices**, not just training a model, but **deploying it correctly**.

---

## 👨‍💻 Author

**Cesar Augusto Gadelha Santos**  
Software Engineer | .NET | Cloud | DevOps | Machine Learning
