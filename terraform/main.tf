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

# Função Lambda que será integrada ao API Gateway
resource "aws_lambda_function" "api_function_webapi" {
  filename         = "../src/Presentation/bin/Release/net8.0/BetaApi.zip"
  function_name    = "BetaApiWebApi"
  handler          = "BetaApi::BetaApi.Function::FunctionHandler"
  runtime          = "dotnet8"
  role             = aws_iam_role.lambda_exec_role.arn
  source_code_hash = filebase64sha256("../src/Presentation/bin/Release/net8.0/BetaApi.zip")
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

# Criando a fila SQS
resource "aws_sqs_queue" "loan_simulation_queue" {
  name = "loanSimulationQueue"
}

# Permissão para que a função Lambda envie mensagens para a fila SQS
resource "aws_iam_policy" "lambda_sqs_policy" {
  name        = "LambdaSQSSendMessagePolicy"
  description = "Permite à função Lambda enviar mensagens para a fila SQS"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action   = "sqs:SendMessage"
        Effect   = "Allow"
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

# Permissão para que o SQS invoque a Lambda (se necessário, dependendo do fluxo)
resource "aws_lambda_permission" "allow_sqs" {
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.api_function_webapi.function_name
  principal     = "sqs.amazonaws.com"
  source_arn    = aws_sqs_queue.loan_simulation_queue.arn
}