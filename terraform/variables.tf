# ============================================================
# VARIABLES — PROJET FINAL DEVSECOPS
# ============================================================

variable "aws_region" {
  description = "Région AWS à utiliser"
  type        = string
  default     = "eu-west-3"
}

variable "aws_profile" {
  description = "Profil AWS CLI à utiliser (configuré dans ~/.aws/credentials)"
  type        = string
  default     = "your-aws-sso-profile"
}

variable "ami_id" {
  description = "AMI Amazon Linux 2023 — eu-west-3"
  type        = string
  default     = "ami-xxxxxxxxxxxxxxxxx"  # AMI Amazon Linux 2023 — eu-west-3 (à remplacer)
}

variable "instance_type" {
  description = "Type d'instance EC2"
  type        = string
  default     = "t3.small"
}

variable "collaborator_usernames" {
  description = "Liste des IAM usernames des 4 collaborateurs"
  type        = list(string)
  default = [
    "user1",
    "user2",
    "user3",
    "user4"
  ]
}