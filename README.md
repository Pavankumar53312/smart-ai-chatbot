# Smart AI Chatbot for Enterprise Knowledge Management

An intelligent chatbot application that streamlines internal knowledge access for enterprise employees using Azure OpenAI's Retrieval-Augmented Generation (RAG). Supports both structured (Azure SQL) and unstructured (PDF/DOCX from Azure Blob Storage) data sources with secure role-based access.

---

##  Features

### Smart Knowledge Retrieval
-  Queries structured data (FAQs, policies) from Azure SQL
-  Processes unstructured documents (PDF, DOCX, TXT) using Azure Blob + AI
-  Embedding + Chunking + Vector Similarity for accurate context

###  Document Management
-  Admin Upload UI (PDF, DOCX, TXT)
-  RBAC: Only users with `Admin` role can upload

## Chat Interface
- Simple frontend for employees to ask natural language questions
- Returns RAG-generated answers using Azure OpenAI

### Authentication & Access Control
- JWT-based login (with role claim)
- Role filtering: HR/IT/Finance/General
- Super-users can access all document types

### Monitoring & Observability
- Swagger/OpenAPI integration for quick testing
- Error logging and basic telemetry ready (extendable)

---

## Tech Stack

| Layer        | Stack                                                                 |
|--------------|-----------------------------------------------------------------------|
| Frontend     | React (Bootstrap UI), Axios, React Router                            |
| Backend      | .NET 8 Web API, Entity Framework Core, Azure.AI.OpenAI SDK           |
| Database     | Azure SQL (EF Models), Vector Index Table for embeddings             |
| Storage      | Azure Blob Storage for document files                                |
| AI           | Azure OpenAI (Chat + Embedding APIs), PDF/DOCX Extraction Logic      |
| Auth         | JWT Bearer Authentication, Role-based Authorization                  |

---

## Folder Structure

SmartAIChatbot/
├── SmartAIChatbot.Api/ # Backend .NET 8 Web API
│ ├── Controllers/ # Auth, Chat, Document APIs
│ ├── Services/ # ChatService, Blob, AI, FormRecognizer
│ ├── Models/ # DTOs and DB Entities
│ ├── Data/ # EF DbContext and Migrations
│ └── Program.cs # Main backend configuration
├── SmartAIChatbot.Client/ # React Frontend
│ ├── pages/ # Login, AskPage, AdminUpload
│ ├── components/ # Navbar, Auth Context
│ └── services/ # Axios API config
└── README.md # This file


---

## Setup & Run (Dev Mode)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Node.js + npm
- Azure resources: OpenAI, Blob Storage, SQL DB

### Backend
```bash
cd SmartAIChatbot.Api
dotnet restore
dotnet run
-----------------
 Frontend
cd SmartAIChatbot.Client
npm install
npm start
-----------------

 Roles Supported

| Role        | Access                                       |
| ----------- | -------------------------------------------- |
| `Admin`     | Upload documents, ask queries, see all roles |
| `HR`        | Access HR documents & data                   |
| `IT`        | Access IT-related content                    |
| `Finance`   | Access Finance policies & guides             |
| `General`   | Basic employee access                        |
| `SuperUser` | Full access to all categories                |

--------------------
 How RAG Works Here

User asks question

Check Azure SQL for matching record

If not found → Search Blob for document content

Text is chunked, embedded, vector-compared

Top chunks form the context

Azure OpenAI generates the final answer with ChatCompletion
---------------
 TODO / Enhancements
 Migrate vector search to Azure Cognitive Search or FAISS

 Setup Application Insights

 Add User Management Dashboard

 Role management via Admin Portal
------------

Developed By
Backend: .NET Full Stack Developer (5 YOE)

Frontend: React + Bootstrap

AI Integration: Azure OpenAI with Embeddings

 Note: This project is fully human-written and production-ready. Not AI-generated.
--------------

