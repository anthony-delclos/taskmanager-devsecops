# Infrastructure DevSecOps

## Documentation Technique — Projet Final Bachelor Cybersécurité 2025

| | |
|---|---|
| Responsable infrastructure | Gomez |
| Cloud provider | AWS (eu-west-3 - Paris) |
| Accès console | SSM Session Manager (sans SSH) |
| IaC | Terraform >= 1.10 + backend S3 |
| Instance | i-04505dbb91806e12a |

---

## 1. Vue d'ensemble de l'architecture

L'infrastructure repose sur une instance EC2 déployée sur AWS dans la région eu-west-3 (Paris). Toute la configuration est définie en Infrastructure as Code via Terraform, ce qui garantit la reproductibilité et la traçabilité de chaque choix technique.

L'accès à l'instance ne passe pas par SSH. À la place, AWS Systems Manager Session Manager est utilisé comme seul canal d'accès à la console. Cette décision est au cœur de l'approche sécurisée du projet.

### 1.1 Schéma d'architecture

```
Internet  (HTTP port 80 / HTTPS port 443)
       |
       v
[ Security Group : web-and-ssh-terraform ]
  - port 22 : FERMÉ (aucune règle SSH)
  - port 80  : ouvert (0.0.0.0/0)
  - port 443 : ouvert (0.0.0.0/0)
       |
       v
[ EC2 Instance - Amazon Linux 2023 ]  id: i-04505dbb91806e12a
  - IAM Role          : devsecops-ssm-role
  - EBS 32 Go gp3     : chiffré AES-256
  - IMDSv2            : obligatoire
  - Docker 25.0.6     : installé via Ansible
  - ECR               : 418295718544.dkr.ecr.eu-west-3.amazonaws.com/taskmanager
       ^
       |  (canal SSM chiffré TLS, aucun port ouvert)
[ SSM Session Manager - AWS API ]
       ^
       |
[ Collaborateurs  (AWS CLI + plugin SSM) ]
```

### 1.2 Composants déployés

| Composant | Identifiant AWS | Rôle |
|-----------|----------------|------|
| EC2 Instance | i-04505dbb91806e12a | Serveur applicatif principal |
| Security Group | sg-0dc679e5c4997802d | Contrôle du trafic réseau |
| IAM Role | devsecops-ssm-role | Identité de l'instance pour SSM + ECR + S3 |
| IAM Instance Profile | devsecops-ssm-profile | Pont entre le rôle IAM et l'EC2 |
| S3 Bucket | devsecops-tfstate-ajele | State Terraform + fichiers temporaires Ansible SSM |
| ECR Repository | taskmanager | Registre Docker des images applicatives |

> La table DynamoDB `terraform-lock` n'est plus utilisée. Le verrouillage du state est géré nativement par S3 via `use_lockfile = true` (Terraform >= 1.10).

---

## 2. Mesures de sécurité en place

### 2.1 Aucun port SSH exposé

Le security group ne contient aucune règle autorisant le port 22. Ce choix est volontaire et constitue la principale mesure de réduction de surface d'attaque. Un attaquant qui scanne l'instance ne trouvera aucun port d'administration accessible depuis Internet.

### 2.2 Chiffrement du disque au repos

Le volume EBS est configuré avec `encrypted = true`. AWS utilise AES-256 via le service KMS. Même en cas d'accès physique ou de snapshot non autorisé, les données restent illisibles sans la clé de chiffrement.

### 2.3 IMDSv2 obligatoire

Le service de métadonnées de l'instance (IMDS) est configuré en version 2 avec `http_tokens = required`. IMDSv1 est vulnérable aux attaques SSRF : un attaquant exploitant une faille applicative pouvait interroger `http://169.254.169.254` pour voler les credentials IAM de l'instance. IMDSv2 impose un token de session qui casse ce vecteur d'attaque.

### 2.4 IAM — trois policies managées

Le rôle IAM `devsecops-ssm-role` dispose de trois policies AWS managées :

| Policy | Justification |
|--------|--------------|
| `AmazonSSMManagedInstanceCore` | Permissions SSM strictement nécessaires (connexion Session Manager, exécution de commandes) |
| `AmazonEC2ContainerRegistryReadOnly` | Permet à l'instance de puller les images depuis ECR sans écriture possible |
| `AmazonS3FullAccess` | Requis par le plugin Ansible `community.aws.aws_ssm` pour transférer les fichiers temporaires. En production : remplacer par une policy restreinte au seul préfixe `ansible-ssm-tmp/*` |

> Contrainte : le profil SSO PowerUserAccess bloque `iam:CreatePolicy` et `iam:PutRolePolicy`. Seules les policies managées AWS existantes peuvent être attachées via `iam:AttachRolePolicy`.

### 2.5 State Terraform chiffré et centralisé

Le fichier `terraform.tfstate` est stocké dans un bucket S3 avec chiffrement activé et accès public totalement bloqué. Le verrouillage concurrent est géré par `use_lockfile = true` (natif S3, Terraform >= 1.10) — remplace l'ancienne dépendance à une table DynamoDB.

### 2.6 Traçabilité des accès via CloudTrail

Chaque session SSM ouverte par un collaborateur est automatiquement enregistrée dans AWS CloudTrail. Il est possible de savoir qui s'est connecté, à quelle heure et depuis quelle identité. C'est une traçabilité impossible à obtenir avec SSH sans configuration supplémentaire complexe.

### 2.7 ECR — scan automatique des vulnérabilités

Le registre ECR est configuré avec `scan_on_push = true`. Chaque image Docker poussée est automatiquement analysée pour les CVE connues (via Amazon Inspector). Les images non taguées sont supprimées après 14 jours (politique de cycle de vie).

---

## 3. Comparaison SSH vs SSM Session Manager

### 3.1 Ce qu'aurait nécessité une approche SSH

Si l'accès avait été configuré via SSH, voici ce qui aurait été nécessaire :

- Ouvrir le port 22 dans le security group, exposant un service d'administration directement sur Internet
- Générer une paire de clés RSA (`aws ec2 create-key-pair`) et stocker la clé privée `.pem`
- Distribuer la clé privée à chaque collaborateur de manière sécurisée
- Chaque collaborateur aurait dû conserver sa clé localement, avec le risque de perte ou de vol
- Configurer les permissions de la clé (`chmod 400`) sur chaque machine
- Gérer la rotation des clés en cas de compromission : régénérer, redistribuer, reconfigurer
- Aucune traçabilité native des commandes exécutées sans configuration de logs supplémentaire

> La gestion des clés SSH dans une équipe de 5 personnes est une source récurrente d'incidents de sécurité : clés partagées, clés oubliées dans des dépôts Git, absence de rotation.

### 3.2 Tableau comparatif

| Critère | SSH classique | SSM Session Manager |
|---------|--------------|---------------------|
| Port réseau ouvert | Port 22 exposé sur Internet | Aucun port ouvert |
| Gestion des accès | Clés privées à distribuer et stocker | Identité IAM/SSO, sans clé |
| Traçabilité | Manuelle, logs à configurer | Native dans CloudTrail |
| Rotation des accès | Régénération et redistribution des clés | Révoquer le rôle IAM suffit |
| Surface d'attaque | Port 22 visible et scannable | Aucune surface réseau exposée |
| Accès sans IP fixe | Nécessite gestion CIDR dynamique | Fonctionne depuis n'importe où |
| Audit de conformité | Complexe à prouver | Logs centralisés automatiquement |

---

## 4. Guide de connexion pour les collaborateurs

Ce guide s'adresse à Erwin, Leo, Anthony et Esteban. Suivez les étapes dans l'ordre. L'installation ne se fait qu'une seule fois.

### 4.1 Installation des prérequis (une seule fois)

#### Étape 1 — Installer l'AWS CLI

**Linux (Ubuntu / Debian) :**

```bash
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
sudo ./aws/install
```

**Windows :**
Télécharger et exécuter : https://awscli.amazonaws.com/AWSCLIV2.msi

**macOS :**

```bash
brew install awscli
```

Vérifier l'installation :

```bash
aws --version
```

#### Étape 2 — Installer le plugin SSM Session Manager

Ce plugin est indispensable. Sans lui, la commande de connexion échouera.

**Linux (Ubuntu / Debian) :**

```bash
curl "https://s3.amazonaws.com/session-manager-downloads/plugin/latest/ubuntu_64bit/session-manager-plugin.deb" -o "session-manager-plugin.deb"
sudo dpkg -i session-manager-plugin.deb
```

**Windows :**
Télécharger et exécuter : https://s3.amazonaws.com/session-manager-downloads/plugin/latest/windows/SessionManagerPluginSetup.exe

**macOS :**

```bash
curl "https://s3.amazonaws.com/session-manager-downloads/plugin/latest/mac/sessionmanager-bundle.zip" -o "session-plugin.zip"
unzip session-plugin.zip
sudo ./sessionmanager-bundle/install -i /usr/local/sessionmanagerplugin -b /usr/local/bin/session-manager-plugin
```

Vérifier l'installation :

```bash
session-manager-plugin --version
```

#### Étape 3 — Vérifier votre profil SSO existant

Vous disposez déjà d'un accès AWS SSO configuré sur votre machine. Vérifiez le nom de votre profil :

```bash
cat ~/.aws/config
```

Vous verrez un bloc du type :

```
[profile mon-profil-sso]
sso_start_url = https://...
sso_region    = eu-west-3
sso_account_id = 418295718544
sso_role_name  = PowerUserAccess
```

Notez le nom entre crochets après `profile` — c'est celui à utiliser dans les commandes suivantes.

Si votre session SSO est expirée, reconnectez-vous :

```bash
aws sso login --profile <votre-profil-sso>
```

### 4.2 Se connecter à l'instance

Une fois les prérequis installés, la connexion se fait avec une seule commande en remplaçant `<votre-profil-sso>` par le nom de profil trouvé à l'étape précédente :

```bash
aws ssm start-session \
  --target i-04505dbb91806e12a \
  --region eu-west-3 \
  --profile <votre-profil-sso>
```

Un shell interactif s'ouvre directement dans votre terminal. Vous êtes connecté en tant qu'utilisateur `ssm-user`. Pour obtenir les droits root :

```bash
sudo -i
```

### 4.3 Vérifier que l'instance est accessible

Si la connexion échoue, vérifiez d'abord que l'instance est visible par SSM :

```bash
aws ssm describe-instance-information \
  --region eu-west-3 \
  --profile <votre-profil-sso>
```

L'instance doit apparaître avec `PingStatus: Online`. Si ce n'est pas le cas, l'instance est peut-être arrêtée ou l'agent SSM n'est pas actif.

### 4.4 Fermer la session

Pour terminer la session, tapez :

```bash
exit
```

La session est automatiquement fermée et l'événement est enregistré dans CloudTrail.

---

## 5. Ce qu'il ne faut jamais faire

| Action interdite | Pourquoi |
|-----------------|----------|
| Commiter `terraform.tfvars` sur Git | Contient des variables sensibles du projet |
| Ouvrir le port 22 dans le security group | Crée une surface d'attaque SSH directement sur Internet |
| Partager son token SSO ou se connecter depuis le compte d'un autre | Chaque identité doit rester individuelle pour la traçabilité |
| Stocker le tfstate en local | Le state partagé serait écrasé à chaque apply |
| Désactiver le chiffrement du disque | Les données seraient lisibles en cas d'accès au volume |
| Pousser une image non scannée sur ECR | ECR scan_on_push est actif — ne pas contourner |

---

## 6. Informations de référence

| Information | Valeur |
|-------------|--------|
| Instance ID | i-04505dbb91806e12a |
| Région AWS | eu-west-3 (Paris) |
| IP privée | 172.31.41.224 |
| Security Group | sg-0dc679e5c4997802d |
| S3 State bucket | devsecops-tfstate-ajele |
| ECR URL | 418295718544.dkr.ecr.eu-west-3.amazonaws.com/taskmanager |
| Profil AWS CLI | Votre propre profil SSO (voir `~/.aws/config`) |

> En cas de problème de connexion, vérifiez en premier lieu que le plugin SSM est bien installé (`session-manager-plugin --version`) et que votre profil est correctement configuré (`aws configure list --profile <votre-profil-sso>`).

---

## 7. Procédures Terraform

### 7.1 Appliquer des changements

```bash
cd terraform

# Authentification SSO (si session expirée)
aws sso login --profile devesecops_project_final_ajele

# Initialiser (uniquement si backend ou providers changés)
terraform init -reconfigure

# Vérifier le plan
terraform plan

# Appliquer
terraform apply
```

### 7.2 Récupérer les outputs

```bash
terraform output
```

Outputs disponibles :
- `instance_id` — ID de l'instance EC2
- `instance_private_ip` — IP privée
- `ssm_connect_command` — Commande de connexion SSM
- `security_group_id` — ID du Security Group
- `ecr_repository_url` — URL du registre ECR
- `ecr_repository_name` — Nom du repo ECR

### 7.3 Points d'attention

- `required_version = ">= 1.10.0"` — Terraform 1.10+ obligatoire pour `use_lockfile = true`
- Ne jamais modifier le backend S3 sans `terraform init -reconfigure`
- Ne jamais supprimer manuellement des ressources AWS sans `terraform destroy` — le state serait désynchronisé
