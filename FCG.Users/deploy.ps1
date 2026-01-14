# Define a raiz como a pasta onde este arquivo está salvo
$root = $PSScriptRoot

Write-Host "INICIANDO DEPLOY AUTOMATIZADO DA USERS-API..." -ForegroundColor Cyan

# ------------------------------------------------------------------
# 1. BUILD DA IMAGEM DOCKER
# ------------------------------------------------------------------
Write-Host "1. Construindo imagem Docker users-api:latest ..." -ForegroundColor Yellow

# Atenção: O comando assume que o Dockerfile está na pasta FCG.Users.API
docker build -t users-api:latest -f FCG.Users.API/Dockerfile .

# Se o build falhar, para o script aqui
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Falha no Build do Docker. Verifique se o Dockerfile está na pasta correta."
    exit
}
Write-Host "✅ Imagem construída com sucesso!" -ForegroundColor Green

# ------------------------------------------------------------------
# 2. INFRAESTRUTURA (BANCO E RABBITMQ)
# ------------------------------------------------------------------
Write-Host "2. Subindo Infraestrutura Base..." -ForegroundColor Yellow

# SQL Server (infrastructure-sqlserver.yaml)
kubectl apply -f (Join-Path $root "k8s/infrastructure-sqlserver.yaml")
Write-Host "⏳ Aguardando SQL Server ficar pronto..."
kubectl wait --for=condition=ready pod -l app=sqlserver --timeout=600s

# RabbitMQ (infrastructure-rabbitmq.yaml)
kubectl apply -f (Join-Path $root "k8s/infrastructure-rabbitmq.yaml")
Write-Host "⏳ Aguardando RabbitMQ ficar pronto..."
kubectl wait --for=condition=ready pod -l app=rabbitmq --timeout=300s

# ------------------------------------------------------------------
# 3. CONFIGURAÇÕES E SEGREDOS
# ------------------------------------------------------------------
Write-Host "3. Aplicando ConfigMaps e Secrets..." -ForegroundColor Yellow
kubectl apply -f (Join-Path $root "k8s/configmap.yaml")
kubectl apply -f (Join-Path $root "k8s/secret.yaml")

# ------------------------------------------------------------------
# 4. APLICAÇÃO (USERS API)
# ------------------------------------------------------------------
Write-Host "4. Implantando UsersAPI..." -ForegroundColor Yellow

# Service (service.yaml)
kubectl apply -f (Join-Path $root "k8s/service.yaml")

# Deleta pods antigos para forçar atualização da imagem
kubectl delete pod -l app=users-api --ignore-not-found

# Deployment (deployment.yaml)
kubectl apply -f (Join-Path $root "k8s/deployment.yaml")

# ------------------------------------------------------------------
# 5. VERIFICAÇÃO FINAL
# ------------------------------------------------------------------
Write-Host "⏳ Aguardando a API subir e rodar as Migrations..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app=users-api --timeout=600s

Write-Host "🎉 DEPLOY CONCLUIDO COM SUCESSO!" -ForegroundColor Green
Write-Host ""
Write-Host "--- STATUS DOS PODS ---"
kubectl get pods
Write-Host ""
Write-Host "--- STATUS DOS SERVICES ---"
kubectl get services

Write-Host ""
Write-Host "ACESSE A DOCUMENTACAO:"
Write-Host "👉 http://localhost/swagger"