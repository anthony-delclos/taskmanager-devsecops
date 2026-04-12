provider "aws" {
  region  = "eu-west-3"
  profile = "devesecops_project_final_ajele"
}

# 2. Groupe de sécurité 
resource "aws_security_group" "web_and_ssh" {
  name        = "web-and-ssh-terraform"
  description = "HTTP/HTTPS uniquement (Acces console via SSM)"

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
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# 3. CONFIGURATION IAM POUR SSM
# Création du rôle pour l'instance
resource "aws_iam_role" "ssm_role" {
  name = "devsecops-ssm-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = { Service = "ec2.amazonaws.com" }
    }]
  })
}

# Attachement de la politique standard AWS pour SSM
resource "aws_iam_role_policy_attachment" "ssm_policy" {
  role       = aws_iam_role.ssm_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
}
# Création du profil d'instance (ce qui fait le pont entre le rôle et l'EC2)
resource "aws_iam_instance_profile" "ssm_profile" {
  name = "devsecops-ssm-profile"
  role = aws_iam_role.ssm_role.name
}

# 4. Instance EC2 avec le profil IAM
resource "aws_instance" "devsecops_project_final_ajele_instance" {
  ami                  = "ami-00d73b8937cc56758"
  instance_type        = "t3.micro"
  
  # On ajoute cette ligne cruciale :
  iam_instance_profile = aws_iam_instance_profile.ssm_profile.name

  vpc_security_group_ids = [aws_security_group.web_and_ssh.id]

  root_block_device {
    volume_size = 32
    volume_type = "gp3"
    encrypted   = true  
  }

  tags = {
    Name = "DevSecOps-TP-SSM"
  }
}