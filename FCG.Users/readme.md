# 👤 FCG Users API

Microsserviço responsável pelo gerenciamento de usuários do ecossistema **FIAP Cloud Games**. 
Este projeto implementa o cadastro de usuários e atua como **Producer**, publicando eventos de integração para outras APIs (como a NotificationsAPI) via RabbitMQ.

## 🚀 Tecnologias Utilizadas

* **Runtime:** .NET 8
* **Banco de Dados:** SQL Server 2022
* **Mensageria:** RabbitMQ (MassTransit)
* **Containerização:** Docker & Kubernetes (K8s)
* **Documentação:** Swagger / OpenAPI

## 🏗️ Arquitetura e Padrões

O projeto segue os princípios da **Clean Architecture** e **CQRS** (sem MediatR), garantindo separação de responsabilidades e testabilidade.

* **API:** Entry point da aplicação (Controllers).
* **Application:** Casos de uso, Handlers, Mappers e Eventos.
* **Domain:** Entidades e regras de negócio puras.
* **Infrastructure:** Implementação de repositórios, configurações do EF Core e MassTransit.

### Fluxo de Criação de Usuário
1.  **API** recebe o POST e chama o Handler.
2.  **Handler** processa a lógica de negócio e gera o `CorrelationId`.
3.  **Repository** persiste no SQL Server.
4.  **MassTransit** publica o evento `UserCreatedEvent` no RabbitMQ.

---

## 📋 Pré-requisitos

Para executar este projeto localmente utilizando a infraestrutura automatizada, você precisará de:

1.  **Docker Desktop** instalado e rodando.
2.  **Kubernetes** habilitado nas configurações do Docker Desktop.
3.  **PowerShell** (para executar o script de deploy).

---

## ⚡ Como Rodar (Deploy Automatizado)

Foi criado um script de automação (`deploy.ps1`) que realiza o build da imagem Docker, aplica as configurações do Kubernetes e executa as migrações de banco de dados automaticamente.

1.  Abra o PowerShell na raiz do projeto.
2.  Execute o script:

```powershell
.\deploy.ps1
```

**O que o script faz:**
* 🐳 **Build:** Cria a imagem `users-api:latest` (incluindo o bundle de migração do EF Core).
* 🏗️ **Infra:** Sobe o **SQL Server** e o **RabbitMQ** no cluster K8s.
* 🔐 **Configs:** Aplica **ConfigMaps** e **Secrets**.
* 🚀 **App:** Sobe a **UsersAPI**.
* 🔄 **Migration:** Executa um `InitContainer` para criar as tabelas do banco automaticamente antes da API iniciar.

---

## 🧪 Como Testar

Após o deploy ser concluído com sucesso (mensagem verde no terminal):

### 1. Acessar a API (Swagger)
A API estará exposta via LoadBalancer na porta 80:
👉 **[http://localhost/swagger](http://localhost/swagger)**

### 2. Acessar o RabbitMQ (Management)
Para visualizar as filas e conexões:
👉 **[http://localhost:15672](http://localhost:15672)**
* **User:** `guest`
* **Pass:** `guest`

---

## 🔍 Observabilidade e Logs

A aplicação implementa **Structured Logging** com foco em rastreabilidade. Cada requisição gera um `CorrelationId` único que perpassa todo o fluxo.

### Padrões de Log Implementados (Requisitos):

1.  ✅ **Log de Sucesso:** Registra a persistência no banco.
    * *Mensagem:* `Cadastro concluído/persistido. UserId: {Guid}, CorrelationId: {Guid}`
2.  ✅ **Log de Erro:** Registra falhas de validação ou banco de dados.
    * *Mensagem:* `Falha crítica no cadastro (Validação/DB). CorrelationId: {Guid}`
3.  ✅ **Log de Publicação:** Registra o envio do evento para o RabbitMQ.
    * *Mensagem:* `UserCreatedEvent publicado. EventId: {Guid}, CorrelationId: {Guid}`
4.  🚫 **Log de Consumo:** **N/A (Não Aplicável)**.
    * *Nota:* Este microsserviço atua apenas como Produtor. O consumo é realizado pela `NotificationsAPI`.

### Como ver os logs no Kubernetes:
Para acompanhar os logs em tempo real via terminal:

```powershell
kubectl logs -l app=users-api -f
```

---

## 📂 Estrutura de Pastas (Kubernetes)

Os arquivos de manifesto do Kubernetes estão localizados na pasta `/k8s`:

* `configmap.yaml`: Variáveis de ambiente não sensíveis.
* `secret.yaml`: Connection Strings e senhas.
* `infrastructure-sqlserver.yaml`: Deployment do Banco de Dados.
* `infrastructure-rabbitmq.yaml`: Deployment do Broker de Mensageria.
* `deployment.yaml`: Deployment da API (contém o InitContainer de migração).
* `service.yaml`: Exposição da API via LoadBalancer.

---

## 📝 Evento de Integração

O contrato de evento publicado para consumo externo (`UserCreatedEvent`) possui a seguinte estrutura para garantir rastreabilidade:

```csharp
public class UserCreatedEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string NickName { get; set; }
    public string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Rastreabilidade
    public Guid EventId { get; set; }       // Gerado automaticamente
    public Guid CorrelationId { get; set; } // ID do fluxo (repassado do Handler)
}
```