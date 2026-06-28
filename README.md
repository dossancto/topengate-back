# Configuração e Uso do Projeto Opengate

Siga estes passos para configurar e executar o projeto:

## 1. Copie o Arquivo de Ambiente

Copie o arquivo de ambiente de exemplo para criar sua configuração local:

```bash
cp .env.example .env
```

## 2. Inicie os Serviços Docker

Certifique-se de que o Docker está em execução e inicie os containers necessários usando o arquivo Docker Compose fornecido:

```bash
docker compose -f docker/docker-compose.yml up -d
```

## 3. Execute as Migrações do Entity Framework

Navegue até o diretório principal do projeto e aplique as migrações do EF Core para configurar o esquema do banco de dados:

```bash
cd src/Opengate
dotnet ef database update
```

## 4. Inicie a Aplicação

Ainda no diretório `src/Opengate`, execute a aplicação:

```bash
dotnet run
```

Sua aplicação deve estar rodando agora.



