# Projeto: API RESTful em ASP.NET Core

Este projeto é uma API RESTful desenvolvida em ASP.NET Core que permite gerenciar usuários, incluindo operações de criação, leitura, atualização, exclusão, autenticação e geração de relatórios em formatos CSV e PDF. A API utiliza um banco de dados SQLite para armazenar os dados dos usuários.

## Funcionalidades

1. **Adicionar Usuário**: Permite adicionar um novo usuário.
2. **Listar Usuários**: Retorna uma lista de todos os usuários.
3. **Buscar Usuário por ID**: Retorna os detalhes de um usuário específico.
4. **Atualizar Usuário**: Permite atualizar as informações de um usuário.
5. **Deletar Usuário**: Permite deletar um usuário.
6. **Atualizar Nível de Usuário**: Permite atualizar o nível de um usuário.
7. **Autenticação de Usuário**: Permite que um usuário faça login e obtenha um token JWT.
8. **Gerar Relatório**: Gera um relatório dos usuários em formatos CSV e PDF.

## Dependências

- **ASP.NET Core**: Framework para construir a API.
- **Entity Framework Core**: ORM para acessar o banco de dados SQLite.
- **CsvHelper**: Biblioteca para manipulação de arquivos CSV.
- **PdfSharpCore**: Biblioteca para geração de arquivos PDF.
- **JWT**: Para autenticação e geração de tokens.

## Estrutura do Projeto

- **Data**: Contém o contexto do banco de dados `AppDbContext`.
- **Services**: Contém serviços auxiliares, como `AuthToken`.
- **Users**: Contém modelos de dados e requisições relacionados aos usuários.
- **Repositories**: Contém classes para acessar o banco de dados.

## Como Usar

1. Clone o repositório ('git clone https://github.com/lfcr9311/CreatusBackend.git').
2. Abra o projeto no Visual Studio ou Visual Studio Code.
3. Execute o comando `dotnet run` para iniciar a API.
4. Acesse a API em `https://localhost:5215`.
5. Utilize um cliente HTTP, como Postman ou Insomnia, para testar as rotas.

### Rotas que Requerem Autenticação

As seguintes rotas precisam de autenticação (o usuário precisa estar logado, autenticado e ser um administrador - nível > 3):

1. **/report** (GET)
2. **/users/level/{id}** (PUT)
3. **/delete/{id}** (DELETE)

## Modificações do Escopo Original do Projeto

1. Adicionado um endpoint para trocar somente o 'level' do usuário, que só pode ser acessado por um usuário autenticado e com nível 4 ou 5.
2. No endpoint de Deletar, somente usuários com nível 4 ou 5 podem deletar contas.
3. O endpoint **/report** gera um relatório em PDF e CSV dos usuários cadastrados.

---

Este projeto foi desenvolvido como desafio da empresa Creatus para o cargo de Estágio.
