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
  default     = "devesecops_project_final_ajele"
}

variable "ami_id" {
  description = "AMI Amazon Linux 2023 — eu-west-3"
  type        = string
  default     = "ami-00d73b8937cc56758"
}

variable "instance_type" {
  description = "Type d'instance EC2"
  type        = string
  default     = "t3.micro"
}

variable "collaborator_usernames" {
  description = "Liste des IAM usernames des 4 collaborateurs"
  type        = list(string)
  default = [
    "esteban",
    "leo",
    "erwin",
    "anthony"
  ]
}