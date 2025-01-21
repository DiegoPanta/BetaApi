# **BetaApi**

Este projeto é um estudo para aprimorar meu conhecimento no desenvolvimento de APIs, utilizando tecnologias como C#, AWS, e Terraform. A API simula empréstimos bancários, ajustando as taxas de acordo com os dados obtidos a partir de uma API pública do Banco Central.

### **Tecnologias e Recursos Utilizadas**

* C#: Linguagem de programação principal para o desenvolvimento da API.
* Terraform: Infraestrutura como código para provisionamento e gerenciamento de recursos na AWS.
* AWS: Plataforma em nuvem para hospedar e escalar a aplicação.
* AWS Lambda Web API: Funções Lambda para criar uma API sem servidor.
* AWS DynamoDB: Banco de dados NoSQL gerenciado para armazenar dados da aplicação.
* AWS SQS: Serviço de filas para comunicação assíncrona entre serviços.

## **Dependências e Versões Necessárias**

Antes de rodar o projeto, certifique-se de que as dependências abaixo estão instaladas:

- **.NET**: 8.0
- **AWS CLI**: 2.22.22
- **Terraform**: 1.11.0

## **Como rodar o projeto**

1. **Restaurar Dependências NuGet**

Para instalar todas as dependências do projeto especificadas no arquivo .csproj, execute:


```bash
dotnet restore
```

2. **Compilar o Projeto**

Utilize o comando abaixo para compilar o código-fonte e gerar os arquivos binários necessários:

```bash
dotnet build
```
3. **Formatar Arquivos do Terraform**

Para garantir que os arquivos de configuração do Terraform sigam o estilo esperado, use o comando:

```bash
terraform fmt
```

4. **Inicializar o Terraform**

Execute o comando a seguir para inicializar o ambiente do Terraform. Isso irá baixar os provedores necessários e configurar o backend:

```bash
terraform init
```

5. **Planejar a Execução do Terraform**

Para visualizar o que será alterado na infraestrutura sem aplicá-las de fato:

```bash
terraform plan
```

6. **Aplicar as Mudanças do Terraform**

Depois de revisar o plano, use este comando para aplicar as alterações e provisionar a infraestrutura:

```bash
terraform apply
```

7. **Rodar o Projeto .NET**

Para compilar e executar a aplicação em um único comando:

```bash
dotnet run
```

8. **Destruir Recursos do Terraform**

Caso queira remover todos os recursos provisionados pelo Terraform, execute:

```bash
terraform destroy
```

## 📌 Informações importantes sobre a aplicação 📌

* **Status de Desenvolvimento**: Este projeto ainda está em desenvolvimento. Espera-se que haja mudanças na organização, arquitetura e novos recursos a serem implementados.

* **Consulta a Serviços Externos:** A API utiliza taxas de juros obtidas de uma API pública do Banco Central para realizar simulações de empréstimos.

## ⚠️ Problemas enfrentados

**Em construção**

## ⏭️ Próximos passos

* **Organizar e Melhorar a Arquitetura:** Refatoração da arquitetura da aplicação para maior modularidade e escalabilidade.

* **Modularizar o Terraform:** Organizar os arquivos de configuração do Terraform em módulos com responsabilidades bem definidas. 