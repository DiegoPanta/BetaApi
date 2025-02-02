provider "aws" {
  region = "us-east-1"
}

# Criando a tabela DynamoDB
resource "aws_dynamodb_table" "loan_simulation_table" {
  name         = "LoanSimulationEntity"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"

  attribute {
    name = "Id"
    type = "S" # String (GUID)
  }

  attribute {
    name = "LoanAmount"
    type = "N"
  }

  attribute {
    name = "Installments"
    type = "N"
  }

  global_secondary_index {
    name            = "LoanAmountIndex"
    hash_key        = "LoanAmount"
    range_key       = "Installments"
    projection_type = "ALL"
  }

  tags = {
    Name = "LoanSimulationDynamoDB"
  }
}

output "dynamodb_table_name" {
  value = aws_dynamodb_table.loan_simulation_table.name
}

# Criando o IAM Role para Lambda
resource "aws_iam_role" "lambda_exec_role" {
  name = "lambda_exec_role"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
        Effect = "Allow"
      },
    ]
  })
}

resource "null_resource" "build_dotnet_lambda" {
  provisioner "local-exec" {
    command     = <<EOT
      dotnet restore ../src/Presentation/Presentation.csproj
      dotnet publish ../src/Presentation/Presentation.csproj -c Release -r win-x64 --self-contained false -o ../src/Presentation/publish
    EOT
    interpreter = ["PowerShell", "-Command"]
  }
  triggers = {
    always_run = "${timestamp()}"
  }
}

## Archiving the Artifacts
data "archive_file" "lambda" {
  type        = "zip"
  source_dir  = "../src/Presentation/publish/"
  output_path = "./BetaApi.zip"
  depends_on  = [null_resource.build_dotnet_lambda]
}

## AWS Lambda Resources
resource "aws_lambda_function" "api_function_webapi" {
  filename         = "BetaApi.zip"
  function_name    = "BetaApi"
  role             = aws_iam_role.beta_api_role.arn
  handler          = "BetaApi"
  source_code_hash = data.archive_file.lambda.output_base64sha256
  runtime          = "dotnet8"
  depends_on       = [data.archive_file.lambda]
  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT = "Development"
    }
  }
}
resource "aws_lambda_function_url" "hello_api_url" {
  function_name      = aws_lambda_function.api_function_webapi.function_name
  authorization_type = "NONE"
}
# Criando a API Gateway
resource "aws_apigatewayv2_api" "http_api" {
  name          = "beta-api"
  protocol_type = "HTTP"
}

# Integração entre Lambda e API Gateway
resource "aws_apigatewayv2_integration" "lambda_integration" {
  api_id           = aws_apigatewayv2_api.http_api.id
  integration_type = "AWS_PROXY"
  integration_uri  = aws_lambda_function.api_function_webapi.arn
}

# Criando uma rota na API Gateway
resource "aws_apigatewayv2_route" "http_route" {
  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = "GET /Simulation"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_integration.id}"
}

# Criando o stage da API
resource "aws_apigatewayv2_stage" "api_stage" {
  api_id      = aws_apigatewayv2_api.http_api.id
  name        = "$default"
  auto_deploy = true
}

output "api_url" {
  value = aws_apigatewayv2_api.http_api.api_endpoint
}

## IAM Permissions and Roles related to Lambda
data "aws_iam_policy_document" "assume_role" {
  statement {
    effect = "Allow"
    principals {
      type        = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }
    actions = ["sts:AssumeRole"]
  }
}

resource "aws_iam_role" "beta_api_role" {
  name               = "beta_api_role"
  assume_role_policy = data.aws_iam_policy_document.assume_role.json
}

# Criando a fila SQS principal com Dead Letter Queue (DLQ)
resource "aws_sqs_queue" "loan_simulation_queue" {
  name                       = "loanSimulationQueue"
  message_retention_seconds  = 86400 # Mensagens são retidas por 1 dia
  visibility_timeout_seconds = 30    # Tempo para processamento da mensagem
  delay_seconds              = 0     # Mensagem disponível imediatamente
  receive_wait_time_seconds  = 10    # Long polling para reduzir custos

  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.loan_simulation_dlq.arn
    maxReceiveCount     = 5 # Envia para DLQ após 5 tentativas falhas
  })
}

# Criando uma Dead Letter Queue (DLQ) para mensagens que falharem
resource "aws_sqs_queue" "loan_simulation_dlq" {
  name                      = "loanSimulationQueue-DLQ"
  message_retention_seconds = 1209600 # Mantém mensagens por 14 dias
}

# Criando a política para que a Lambda possa enviar mensagens para a fila SQS
resource "aws_iam_policy" "lambda_sqs_policy" {
  name        = "LambdaSQSSendMessagePolicy"
  description = "Permite à função Lambda enviar mensagens para a fila SQS"

  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect   = "Allow",
        Action   = "sqs:SendMessage",
        Resource = aws_sqs_queue.loan_simulation_queue.arn
      }
    ]
  })
}

# Anexando a política ao role da Lambda
resource "aws_iam_role_policy_attachment" "lambda_sqs_policy_attachment" {
  role       = aws_iam_role.lambda_exec_role.name
  policy_arn = aws_iam_policy.lambda_sqs_policy.arn
}

# Permissão para que o SQS invoque a Lambda
resource "aws_lambda_permission" "allow_sqs" {
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.api_function_webapi.function_name
  principal     = "sqs.amazonaws.com"
  source_arn    = aws_sqs_queue.loan_simulation_queue.arn
}

output "sqs_queue_url" {
  value = aws_sqs_queue.loan_simulation_queue.url
}

output "sqs_dlq_url" {
  value = aws_sqs_queue.loan_simulation_dlq.url
}
