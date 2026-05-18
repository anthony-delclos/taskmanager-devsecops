# ============================================================
# PROJET FINAL DEVSECOPS - INFRASTRUCTURE AWS

# ============================================================

terraform {
  # use_lockfile (natif S3) requiert >= 1.10 — on aligne la contrainte
  required_version = ">= 1.10.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = ">= 5.0"
    }
  }

  backend "s3" {
    bucket  = "your-terraform-state-bucket"
    key     = "projet-final/terraform.tfstate"
    region  = "eu-west-3"
    encrypt = true
    profile = "your-aws-sso-profile"

    # Remplace dynamodb_table (déprécié depuis Terraform 1.10).
    # Utilise un fichier .tflock dans S3 au lieu d'une table DynamoDB.
    # La table "terraform-lock" peut être supprimée si elle n'est
    # plus utilisée par d'autres projets.
    use_lockfile = true
  }
}

provider "aws" {
  region  = var.aws_region
  profile = var.aws_profile
}

# ============================================================
# 1. DATA SOURCES
# ============================================================
data "aws_vpc" "default" {
  default = true
}

# Récupère l'account ID AWS courant dynamiquement.
# Utilisé pour construire les ARNs des ressources sans valeur en dur.
data "aws_caller_identity" "current" {}

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

# Permet à l'instance EC2 de puller les images depuis ECR
resource "aws_iam_role_policy_attachment" "ecr_policy" {
  role       = aws_iam_role.ssm_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly"
}

# Managed policy S3 restreinte au préfixe ansible-ssm-tmp/.
# Utilise aws_iam_policy (managed) + aws_iam_role_policy_attachment
# car iam:PutRolePolicy (inline) est bloqué par PowerUserAccess SSO,
# alors que iam:CreatePolicy + iam:AttachRolePolicy sont autorisés.
# Policy S3 pour le plugin community.aws.aws_ssm d'Ansible.
# Le plugin nécessite s3:PutObject + s3:GetObject pour transférer
# les fichiers temporaires entre le nœud de contrôle et l'instance.
#
# Contrainte : le profil SSO PowerUserAccess ne permet ni
# iam:CreatePolicy ni iam:PutRolePolicy — seul iam:AttachRolePolicy
# sur des policies AWS managées existantes est autorisé.
# AmazonS3FullAccess est la seule policy managée AWS disponible
# couvrant le besoin read+write S3.
#
# En production : remplacer par une policy inline créée par un admin
# avec droits IAM, restreinte au bucket your-terraform-state-bucket/ansible-ssm-tmp/*
resource "aws_iam_role_policy_attachment" "s3_ssm_ansible" {
  role       = aws_iam_role.ssm_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonS3FullAccess"
}

resource "aws_iam_instance_profile" "ssm_profile" {
  name = "devsecops-ssm-profile"
  role = aws_iam_role.ssm_role.name
}

# ============================================================
# 4. ECR — ELASTIC CONTAINER REGISTRY
# Stocke les images Docker de l'application taskmanager.
# L'instance EC2 pull depuis ce registre via la policy ECR ci-dessous.
# ============================================================
resource "aws_ecr_repository" "taskmanager" {
  name                 = "taskmanager"
  image_tag_mutability = "MUTABLE"

  # Scan automatique des vulnérabilités CVE à chaque push d'image
  image_scanning_configuration {
    scan_on_push = true
  }

  # Chiffrement des images au repos avec la clé AWS managée
  encryption_configuration {
    encryption_type = "AES256"
  }

  tags = {
    Name    = "taskmanager-ecr"
    Project = "DevSecOps-Final"
  }
}

# Politique de cycle de vie : supprime les images non-taguées
# après 14 jours pour limiter les coûts de stockage ECR
resource "aws_ecr_lifecycle_policy" "taskmanager" {
  repository = aws_ecr_repository.taskmanager.name

  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Expire untagged images after 14 days"
      selection = {
        tagStatus   = "untagged"
        countType   = "sinceImagePushed"
        countUnit   = "days"
        countNumber = 14
      }
      action = { type = "expire" }
    }]
  })
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
    dnf update -y --security
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

# ============================================================
# 6. SSM — CONTRÔLE D'ACCÈS ET JOURNALISATION
#
# Deux mécanismes complémentaires :
#   A. aws_ssm_document  → journalisation obligatoire de toutes
#      les sessions (contrôle compensatoire, toujours applicable)
#   B. aws_iam_policy    → restreint ssm:StartSession à cette
#      instance uniquement (nécessite iam:CreatePolicy — peut
#      échouer avec PowerUserAccess SSO, voir commentaire)
# ============================================================

# ------------------------------------------------------------------
# A. JOURNALISATION DES SESSIONS SSM
#    Le document "SSM-SessionManagerRunShell" est le document par
#    défaut utilisé par Session Manager sur toutes les instances
#    du compte. Le surcharger force la journalisation CloudWatch
#    sur chaque connexion, quelle que soit l'identité du connecteur.
# ------------------------------------------------------------------

resource "aws_cloudwatch_log_group" "ssm_sessions" {
  name              = "/aws/ssm/devsecops-sessions"
  retention_in_days = 90

  tags = {
    Name    = "devsecops-ssm-session-logs"
    Project = "DevSecOps-Final"
  }
}

resource "aws_ssm_document" "session_preferences" {
  name          = "SSM-SessionManagerRunShell"
  document_type = "Session"

  content = jsonencode({
    schemaVersion = "1.0"
    description   = "Session Manager preferences — journalisation CloudWatch obligatoire pour toutes les sessions"
    sessionType   = "Standard_Stream"
    inputs = {
      cloudWatchLogGroupName      = aws_cloudwatch_log_group.ssm_sessions.name
      cloudWatchEncryptionEnabled = false
      cloudWatchStreamingEnabled  = true
      idleSessionTimeout          = "20"
      runAsEnabled                = false
      shellProfile = {
        linux = ""
      }
    }
  })

  tags = {
    Name    = "devsecops-session-prefs"
    Project = "DevSecOps-Final"
  }
}

# ------------------------------------------------------------------
# B. RESTRICTION ssm:StartSession À CETTE INSTANCE UNIQUEMENT
#
#    Requiert iam:CreatePolicy — disponible depuis le passage du profil
#    SSO de PowerUserAccess → AdministratorAccess.
#
#    À attacher manuellement aux rôles SSO des collaborateurs
#    (Erwin, Leo, Anthony, Esteban) après terraform apply.
# ------------------------------------------------------------------

resource "aws_iam_policy" "ssm_session_restricted" {
  name        = "devsecops-ssm-session-restricted"
  description = "Restreint ssm:StartSession à l'instance du projet DevSecOps uniquement"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "AllowSSMSessionOnProjectInstance"
        Effect = "Allow"
        Action = [
          "ssm:StartSession",
          "ssm:TerminateSession",
          "ssm:ResumeSession",
          "ssm:DescribeSessions",
          "ssm:GetConnectionStatus"
        ]
        Resource = [
          "arn:aws:ec2:eu-west-3:${data.aws_caller_identity.current.account_id}:instance/${aws_instance.main.id}",
          "arn:aws:ssm:eu-west-3::document/SSM-SessionManagerRunShell"
        ]
      },
      {
        Sid      = "DenySSMSessionOnOtherInstances"
        Effect   = "Deny"
        Action   = "ssm:StartSession"
        Resource = "arn:aws:ec2:*:*:instance/*"
        Condition = {
          StringNotEquals = {
            "aws:ResourceTag/Project" = "DevSecOps-Final"
          }
        }
      }
    ]
  })

  tags = {
    Name    = "devsecops-ssm-restricted"
    Project = "DevSecOps-Final"
  }
}