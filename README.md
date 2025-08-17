## MCP - Projeto Base

Este repositório é um template para iniciar projetos .NET, contendo:

- **MCP**: Um módulo básico de contexto/modelo.
- **API**: Uma API conectada ao MCP.
- **CONSOLECHAT**: Um chat interativo via console.
- **Docker**: Todos os serviços rodam em containers Docker, facilitando o setup e deploy.

### Tecnologias Utilizadas
- .NET
- Docker / Docker Compose
- Entity Framework Core

### Estrutura do Projeto

```
MCP/           # Módulo de contexto/modelo
API/           # API principal
CONSOLECHAT/   # Chat via console
docker-compose.yml
```

### Como Executar
1. Clone o repositório:
	```sh
	git clone https://github.com/ArthurSilv4/MCP.git
	```
2. Execute os containers com Docker Compose:
	```sh
	docker-compose run --rm consolechat
	```
3. Acesse a API em `http://localhost:8000/scalar` (ou porta configurada).
4. Use o chat pelo console conforme instruções do terminal.

### Personalização
Este projeto serve como base para outros projetos. Basta clonar e adaptar os módulos conforme sua necessidade.