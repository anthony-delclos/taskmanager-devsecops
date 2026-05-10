output "instance_id" {
  description = "EC2 instance ID (needed for SSM connection)"
  value       = aws_instance.main.id
}

output "instance_private_ip" {
  description = "Private IP of the instance"
  value       = aws_instance.main.private_ip
}

output "instance_public_ip" {
  description = "Public IP of the instance — use this for sslip.io: replace dots with dashes (e.g. 1-2-3-4.sslip.io)"
  value       = aws_instance.main.public_ip
}

output "sslip_domain" {
  description = "Domaine sslip.io prêt à l'emploi pour le .env DOMAIN="
  value       = "${replace(aws_instance.main.public_ip, ".", "-")}.sslip.io"
}

output "ssm_connect_command" {
  description = "Command to share with collaborators to connect"
  value       = "aws ssm start-session --target ${aws_instance.main.id} --region ${var.aws_region} --profile <your-sso-profile>"
}

output "security_group_id" {
  description = "Security group ID"
  value       = aws_security_group.web_and_ssh.id
}

output "ecr_repository_url" {
  description = "URL du registre ECR — à utiliser dans ansible/roles/docker/defaults/main.yml (ecr_registry_url)"
  value       = aws_ecr_repository.taskmanager.repository_url
}

output "ecr_repository_name" {
  description = "Nom du repository ECR"
  value       = aws_ecr_repository.taskmanager.name
}

output "ssm_session_log_group" {
  description = "Groupe CloudWatch où toutes les sessions SSM sont journalisées"
  value       = aws_cloudwatch_log_group.ssm_sessions.name
}

output "ssm_restricted_policy_arn" {
  description = "ARN de la policy IAM à attacher aux rôles SSO des collaborateurs (Erwin, Leo, Anthony, Esteban)"
  value       = aws_iam_policy.ssm_session_restricted.arn
}