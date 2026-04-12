# Provider AWS — identique Options A et B
provider "aws" {
  region  = "eu-west-3"
  profile = "tp-devsecops"
}

# Clé SSH
resource "aws_key_pair" "deployer" {
  key_name   = "tp-devsecops-terraform"
  public_key = file("~/.ssh/id_rsa.pub")  # Générez-la avec : ssh-keygen -t rsa -b 4096
}

# Groupe de sécurité
resource "aws_security_group" "web_and_ssh" {
  name        = "web-and-ssh-terraform"
  description = "SSH restreint + HTTP + HTTPS"

  # SSH — à compléter
  ingress {
    description = "SSH"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["163.5.3.69/32"]  # curl ifconfig.me pour obtenir votre IP
  }

  # HTTP — à compléter
  ingress {
    description = "HTTP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # HTTPS — à compléter
  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Trafic sortant
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# Instance EC2
resource "aws_instance" "tp_instance" {
  ami             = "ami-00d73b8937cc56758"  # Amazon Linux 2023 — eu-west-3
  instance_type   = "t3.micro"
  key_name        = aws_key_pair.deployer.key_name

  vpc_security_group_ids = [aws_security_group.web_and_ssh.id]

  root_block_device {
    volume_size = 32
    volume_type = "gp3"
    encrypted   = true  
  }

  tags = {
    Name        = "DevSecOps-TP"
    Environment = "student"
    ManagedBy   = "terraform"
  }
}

output "public_ip" {
  description = "13.53.150.229"
  value       = aws_instance.tp_instance.public_ip
}

output "ssh_command" {
  description = "Commande SSH prête à l'emploi"
  value       = "ssh -i ~/.ssh/id_rsa ec2-user@${aws_instance.tp_instance.public_ip}"
}
