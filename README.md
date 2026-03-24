# FCG.Api.Users

**Tech Challenge - Fase 3**
API de gerenciamento de usuários e autenticação.

> **⚠️ Este microsserviço faz parte de um sistema maior.**  
> Para executar toda a plataforma (Docker Compose ou Kubernetes), veja: [FCG.Infra.Orchestration](../FCG.Infra.Orchestration/README.md)

## Sobre o Projeto

A FCG API Users é responsável por gerenciar usuários, autenticação e autorização da plataforma FCG. Utiliza **AWS Cognito** para autenticação segura e publica eventos para notificar outros microsserviços sobre ações de usuários.

## Arquitetura

O projeto segue **Clean Architecture** com as seguintes camadas:

```
├── FCG.Api.Users/                          # Controllers, Middlewares
├── FCG.Api.Users.Application/              # Commands, Queries (CQRS)
├── FCG.Api.Users.Domain/                   # Entidades, Regras de Negócio
├── FCG.Api.Users.Infrastructure.Data/      # EF Core, Repositories
└── FCG.Api.Users.Infrastructure.AWS/       # AWS Cognito Integration
```

## Tecnologias

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- **AWS Cognito** - Autenticação e autorização
- **MassTransit** - Messaging com RabbitMQ
- MediatR + FluentValidation
- xUnit + FluentAssertions

## Variáveis de Ambiente

### Obrigatórias

```bash
# Banco de Dados
ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=FCGUsersDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"

# JWT
Authentication__JwtBearer__Authority="https://cognito-idp.<REGION>.amazonaws.com/<USER_POOL_ID>"

# AWS Credentials
AWS__Region="us-east-1"
AWS__AccessKeyId="YOUR_ACCESS_KEY"
AWS__SecretAccessKey="YOUR_SECRET_KEY"
AWS__SessionToken="YOUR_SESSION_TOKEN"  # Se usar AWS Academy

# AWS Cognito
AWS__Cognito__UserPoolId="us-east-1_XXXXXXXXX"
AWS__Cognito__ClientId="your-client-id"
AWS__Cognito__ClientSecret="your-client-secret"

# RabbitMQ
Messaging__RabbitMQ__Host="localhost"
Messaging__RabbitMQ__Username="guest"
Messaging__RabbitMQ__Password="guest"

# Admin Seed (criado automaticamente no startup)
Admin__Email="admin@fcg.com"
Admin__Password="Admin@123"
```

## Como Executar

### Localmente
```bash
cd src/FCG.Api.Users
dotnet run
```

Acesse: http://localhost:5001/swagger

### Docker
```bash
docker build -t fcg-users .
docker run -p 5001:80 fcg-users
```

## Testes

```bash
dotnet test
```

Testes unitários implementados em `FCG.Api.Users.Domain.Tests`.

---

## Fase 3 — Novidades

### Lambda de Notificação
Após confirmação de e-mail, o handler `ConfirmSignUpCommandHandler` invoca diretamente a função `FCG.Lambda.Notification` via AWS SDK (fire-and-forget) com o evento `UserCreated`.

### Temporal Tables
A entidade `Users` usa `IsTemporal()` via EF Core — o SQL Server mantém automaticamente o histórico de alterações na tabela `UsersHistory`.

### Observabilidade
- **AWS X-Ray**: middleware `app.UseXRay("fcg-users-api")` habilitado — rastreamento distribuído via CloudWatch

### CI/CD (GitHub Actions)
- **CI** (`.github/workflows/ci.yml`): build + testes em push/PR na `main`
- **CD** (`.github/workflows/cd.yml`): build Docker → push ECR → `kubectl set image` no EKS

**Secrets obrigatórios no repositório GitHub:**
- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`

### Kubernetes
Manifests em `k8s/`:
- `deployment.yaml` — Deployment com 2 réplicas
- `service.yaml` — Service NLB interno (integrado ao AWS API Gateway via VPC Link)
- `configmap.yaml` — Variáveis não sensíveis
- `secret.yaml` — Connection string, credenciais AWS, Cognito