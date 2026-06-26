# Quick Start Guide - OrderIntegrationPOC

Tervetuloa Order Integration Proof of Concept -ratkaisuun! Tämä pika-opas auttaa sinua aloittamaan muutamassa minuutissa.

## 🚀 Aloita 3 minuutissa

### 1. Avaa Ratkaisu Visual Studiossa

```powershell
# Avaa ratkaisu
C:\Users\ttkiv\source\repos\OrderIntegrationPOC\OrderIntegrationPOC.slnx
```

### 2. Käynnistä Azure Functions Paikallisesti

Avaa **PowerShell** ja suorita:

```powershell
cd "C:\Users\ttkiv\source\repos\OrderIntegrationPOC\OrderFunctionApp"
func start
```

Odoita, kunnes näet:
```
Functions:
	ProcessOrder: [POST] http://localhost:7071/api/orders/process
```

### 3. Testaa Onnistunut Tilaus

Avaa toinen PowerShell-ikkuna ja suorita:

```powershell
$body = @{
	orderId = "TEST-001"
	customerId = "CUST-001"
	total = 99.99
} | ConvertTo-Json

$response = Invoke-RestMethod `
	-Uri "http://localhost:7071/api/orders/process" `
	-Method Post `
	-ContentType "application/json" `
	-Body $body

$response
```

**Odotettu vastaus:**
```json
{
  "status": "success",
  "message": "Order processed successfully",
  "orderId": "TEST-001",
  "customerId": "CUST-001",
  "total": 99.99,
  "processedAt": "2024-01-15T10:30:45.123Z",
  "environment": "Local Development"
}
```

✅ **Loistava!** Ratkaisu on toiminnassa!

---

## 📚 Dokumentaatio

| Dokumentti | Sisältö |
|---|---|
| [README.md](README.md) | Yksityiskohtainen arkkitehtuuri ja asennus |
| [Testing-Guide.md](docs/Testing-Guide.md) | 7 erillistä testiskenaariota |
| [API-Reference.md](docs/API-Reference.md) | API-dokumentaatio ja esimerkit |
| [Deployment-Guide.md](docs/Deployment-Guide.md) | Azure-käyttöönotto (pilvi) |

---

## 🏗️ Ratkaisun Rakenne

```
OrderIntegrationPOC/
├── OrderFunctionApp/               # Azure Functions-projekti
│   ├── Functions/
│   │   └── ProcessOrder.cs         # HTTP-kynnistetty funktio
│   ├── Models/
│   │   ├── OrderRequest.cs         # Tilauksen tietomallia
│   │   └── OrderValidationResult.cs
│   ├── Services/
│   │   └── OrderValidator.cs       # Liiketoiminnan validiointi
│   ├── Program.cs                  # Konfiguraatio
│   ├── host.json                   # Ajoympäristön asetukset
│   └── local.settings.json         # Paikalliset asetukset
│
├── docs/
│   ├── sample-payloads.json        # Testausmateriaalit
│   ├── LogicApp-Template.json      # ARM-mallit
│   ├── LogicApp-Definition.json    # Logic App -määritys
│   ├── API-Reference.md            # API-dokumentaatio
│   ├── Testing-Guide.md            # Testausopas
│   └── Deployment-Guide.md         # Käyttöönotonohjeet
│
└── README.md                       # Pääasiakirja

```

---

## 🔧 Arkkitehtuuri

```
ERP-järjestelmä
	↓ (JSON-sanoma)
Azure Service Bus: orders-incoming
	↓ (käynnistyminen)
Azure Logic App
	├─► Parsita JSON
	├─► Kutsua ProcessOrder-funktiota
	└─► Kirjata Application Insightsiin
		 ↓
	ProcessOrder Azure Function
	├─► Deserialisoi JSON
	├─► Validoi tiedot
	└─► Palauta HTTP 200/400
		 ↓
	Application Insights
	(Lokit ja telemetria)
```

---

## 📝 Testausskenaariot

### ✅ Onnistunut tilaus
```powershell
$body = @{
	orderId = "ORD-001"
	customerId = "CUST-001"
	total = 149.90
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/orders/process" `
	-Method Post -ContentType "application/json" -Body $body
```

### ❌ Puuttuva customerId
```powershell
$body = @{
	orderId = "ORD-002"
	total = 100.00
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/orders/process" `
	-Method Post -ContentType "application/json" -Body $body
```

### ❌ Negatiivinen summa
```powershell
$body = @{
	orderId = "ORD-003"
	customerId = "CUST-003"
	total = -50.00
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/orders/process" `
	-Method Post -ContentType "application/json" -Body $body
```

---

## 🛠️ Kehitysympäristön Asetukset

### Edellytykset
- .NET 8 SDK
- Azure Functions Core Tools (v4+)
- Visual Studio 2022 tai Visual Studio Code
- PowerShell 7+

### Tarkista Versiot
```powershell
dotnet --version
func --version
az --version
```

---

## 📦 Konfiguraatio (local.settings.json)

```json
{
	"IsEncrypted": false,
	"Values": {
		"AzureWebJobsStorage": "UseDevelopmentStorage=true",
		"FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
		"APPINSIGHTS_INSTRUMENTATIONKEY": "YOUR_KEY_HERE",
		"ENVIRONMENT": "Development"
	}
}
```

---

## 🚨 Vianmääritys

### Virhe: "Port 7071 already in use"
```powershell
# Etsi prosessi
netstat -ano | findstr :7071

# Tapa se (korvaa PID)
taskkill /PID <PID> /F
```

### Virhe: "Functions Worker Runtime not found"
```powershell
# Asenna uudelleen
npm uninstall -g azure-functions-core-tools
npm install -g azure-functions-core-tools@4
```

### Virhe: "Invalid JSON format"
- Tarkista JSON-syntaksi
- Käytä `ConvertTo-Json` PowerShellissä
- Varmistu lainausmerkkien oikeellisuudesta

---

## 💡 Seuraavat Vaiheet

1. **Lue README.md** - Yksityiskohtaisempi ohjeistus
2. **Käy Testing-Guide.md** - Kaikki 7 testiskenaariota
3. **Tutustu API-Reference.md** - API-tietojen yksityiskohdat
4. **Testaa Deployment-Guide.md** - Azure-käyttöönotto

---

## ☁️ Azure-käyttöönotto

```powershell
# Kirjaudu Azure-tiliisi
az login

# Luo resurssiryhmä
az group create --name "order-integration-poc" --location "North Europe"

# Käyttöönota resurssit (katso Deployment-Guide.md)
```

---

## 📊 Valvonta ja Lokit

### Paikalliset lokit (func start -pääte)
```
[2024-01-15T10:30:45] ProcessOrder function triggered
[2024-01-15T10:30:46] Order validation passed
[2024-01-15T10:30:47] Order processed successfully
```

### Visual Studio-lokit
- Avaa **Output** → Valitse **Azure Functions**

### Application Insights (Pilvipalvelu)
- Azure Portal → Application Insights → Logs (KQL-kyselyt)

---

## 🔒 Turvallisuus

- ❌ Äläkä tallenna salasanoja `local.settings.json`-tiedostoon versionhallintaan
- ✅ Käytä Azure Key Vault -palvelua tuotannossa
- ✅ Ota käyttöön hallitut identiteetit
- ✅ Salaa arkaluonteiset tiedot siirtymisen ja levossa

---

## 📞 Tuki

Ongelmisi? Katso:
1. [README.md](README.md) - Kattava dokumentaatio
2. [Testing-Guide.md](docs/Testing-Guide.md) - Testausohjeistus
3. [API-Reference.md](docs/API-Reference.md) - API-tiedot
4. [Deployment-Guide.md](docs/Deployment-Guide.md) - Käyttöönotto

---

## ✨ Ominaisuudet

- ✅ HTTP-kynnistetty Azure Function
- ✅ JSON-validointi (orderId, customerId, total)
- ✅ Application Insights -valvonta
- ✅ Kattava virheenkäsittely
- ✅ Logic App -integraatio (mallina)
- ✅ Service Bus -jonon tuki
- ✅ DevOps-valmis konfiguraatio

---

## 🎯 Seuraavat Kehitysideat

- [ ] Tietokanta-integraatio (SQL Server / CosmosDB)
- [ ] Tilauksen seurantajärjestelmä
- [ ] Sähköpostilmoitukset
- [ ] Maksutavan integraatio
- [ ] Hallintapaneeli
- [ ] Tietojen vienti

---

**Hyvää testausta!** 🚀

Katso [Testing-Guide.md](docs/Testing-Guide.md) liittyvää testausskenaarioita ja [README.md](README.md) yksityiskohtaisia ohjeita.

---

*Viimeksi päivitetty: 15.1.2024*
