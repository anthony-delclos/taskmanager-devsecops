# ============================================================
# PROJET FINAL DEVSECOPS - INFRASTRUCTURE AWS
# Auteur : Gomez (Infrastructure)
# ============================================================

terraform {
  required_version = ">= 1.3.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = ">= 5.0"
    }
  }

  backend "s3" {
    bucket         = "devsecops-tfstate-ajele"
    key            = "projet-final/terraform.tfstate"
    region         = "eu-west-3"
    dynamodb_table = "terraform-lock"
    encrypt        = true
    profile        = "devesecops_project_final_ajele"
  }
}

provider "aws" {
  region  = var.aws_region
  profile = var.aws_profile
}

# ============================================================
# 1. DATA SOURCE - VPC et Subnet par defaut
# ============================================================
data "aws_vpc" "default" {
  default = true
}

data "aws_subnets" "default" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.default.id]
  }
}

# ============================================================
# 2. SECURITY GROUP
# Port 22 ferme - acces console exclusivement via SSM
# ============================================================
resource "aws_security_group" "web_and_ssh" {
  name        = "web-and-ssh-terraform"
  description = "HTTP/HTTPS only - console access via SSM (no SSH)"
  vpc_id      = data.aws_vpc.default.id

  ingress {
    description = "HTTP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    description = "All outbound traffic - required for SSM endpoints"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name    = "devsecops-sg"
    Project = "DevSecOps-Final"
  }
}

# ============================================================
# 3. IAM - ROLE EC2 POUR SSM
# ============================================================
resource "aws_iam_role" "ssm_role" {
  name        = "devsecops-ssm-role"
  description = "IAM role for EC2 instance - SSM access"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action    = "sts:AssumeRole"
      Effect    = "Allow"
      Principal = { Service = "ec2.amazonaws.com" }
    }]
  })

}

resource "aws_iam_role_policy_attachment" "ssm_policy" {
  role       = aws_iam_role.ssm_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
}

resource "aws_iam_instance_profile" "ssm_profile" {
  name = "devsecops-ssm-profile"
  role = aws_iam_role.ssm_role.name
}

# ============================================================
# 5. INSTANCE EC2
# ============================================================
resource "aws_instance" "main" {
  ami                  = var.ami_id
  instance_type        = var.instance_type
  iam_instance_profile = aws_iam_instance_profile.ssm_profile.name

  vpc_security_group_ids = [aws_security_group.web_and_ssh.id]
  subnet_id              = data.aws_subnets.default.ids[0]

  user_data = <<-EOF
    #!/bin/bash
    set -e
    systemctl enable amazon-ssm-agent
    systemctl start amazon-ssm-agent
    yum update -y --security
  EOF

  root_block_device {
    volume_size = 32
    volume_type = "gp3"
    encrypted   = true
  }

  metadata_options {
    http_endpoint               = "enabled"
    http_tokens                 = "required"
    http_put_response_hop_limit = 1
  }

  tags = {
    Name    = "DevSecOps-TP-SSM"
    Project = "DevSecOps-Final"
  }
}