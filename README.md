# ⚡ Azure Functions Starter — Event-Driven Serverless (.NET 8 + Python)

A production-ready **Azure Functions** project demonstrating event-driven, serverless processing patterns using both **C#** and **Python** — Service Bus triggers, Blob Storage processing, and Event Grid handlers.

## 📐 Architecture

```
┌──────────────┐     OrderCreated      ┌─────────────────────┐
│  Order API   │──── Service Bus ─────▶│ OrderProcessor (C#) │──▶ Email/Notify
└──────────────┘                       └─────────────────────┘
                                                │
                                                ▼ writes receipt
┌──────────────┐     Blob Created      ┌────────────────────────┐
│   Client     │──── Blob Storage ────▶│ BlobProcessor (Python) │──▶ Resize/Process
└──────────────┘                       └────────────────────────┘

┌──────────────┐     Resource Event    ┌─────────────────────────┐
│  Azure Infra │──── Event Grid ──────▶│ EventGridHandler (C#)   │──▶ Audit Log
└──────────────┘                       └─────────────────────────┘
```

## 🚀 Functions Included

| Function | Language | Trigger | Description |
|----------|----------|---------|-------------|
| `OrderProcessor` | C# | Service Bus | Processes new orders, sends notifications |
| `OrderStatusProcessor` | C# | Service Bus | Handles order status change events |
| `BlobProcessor` | Python | Blob Storage | Processes uploaded files/images |
| `EventGridHandler` | C# | Event Grid | Handles infrastructure & domain events |

## 🛠️ Tech Stack

- **Runtime**: .NET 8 Isolated Worker + Python 3.11
- **Triggers**: Azure Service Bus, Blob Storage, Event Grid, HTTP
- **Output Bindings**: Table Storage, Service Bus, Blob
- **Testing**: xUnit, MOQ (C#) · pytest (Python)
- **Infra**: Terraform (see [terraform-azure-modules](https://github.com/yourusername/terraform-azure-modules))

## ⚡ Quick Start

```bash
# Install Azure Functions Core Tools
npm install -g azure-functions-core-tools@4

# C# functions
cd src/OrderProcessor
cp local.settings.json.example local.settings.json
# Fill in your connection strings
func start

# Python functions
cd src/BlobProcessor
pip install -r requirements.txt
func start
```

## 👩‍💻 Author

**Shivani Sharma** — Technical Architect (Azure / .NET)
- Email: shivanish.net@gmail.com
