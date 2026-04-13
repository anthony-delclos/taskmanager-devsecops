output "instance_id" {
  description = "EC2 instance ID (needed for SSM connection)"
  value       = aws_instance.main.id
}

output "instance_private_ip" {
  description = "Private IP of the instance"
  value       = aws_instance.main.private_ip
}

output "ssm_connect_command" {
  description = "Command to share with collaborators to connect"
  value       = "aws ssm start-session --target ${aws_instance.main.id} --region ${var.aws_region} --profile <your-sso-profile>"
}

output "security_group_id" {
  description = "Security group ID"
  value       = aws_security_group.web_and_ssh.id
}