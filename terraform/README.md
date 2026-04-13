# INFRASTRUCTURE DEVSECOPS

## Documentation Technique — Projet Final Bachelor Cybersecurite 2025

|                            |                                |
| -------------------------- | ------------------------------ |
| Responsable infrastructure | Gomez                          |
| Cloud provider             | AWS (eu-west-3 - Paris)        |
| Acces console              | SSM Session Manager (sans SSH) |
| IaC                        | Terraform >= 1.3 + backend S3  |
| Instance                   | i-04505dbb91806e12a            |

---

## 1. Vue d'ensemble de l'architecture

L'infrastructure repose sur une instance EC2 deployee sur AWS dans la region eu-west-3 (Paris). Toute la configuration est definie en Infrastructure as Code via Terraform, ce qui garantit la reproductibilite et la tracabilite de chaque choix technique.

L'acces a l'instance ne passe pas par SSH. A la place, AWS Systems Manager Session Manager est utilise comme seul canal d'acces a la console. Cette decision est au coeur de l'approche securisee du projet.

### 1.1 Schema d'architecture

```
Internet  (HTTP port 80 / HTTPS port 443)
       |
       v
[ Security Group : web-and-ssh-terraform ]
  - port 22 : FERME (aucune regle SSH)
  - port 80  : ouvert (0.0.0.0/0)
  - port 443 : ouvert (0.0.0.0/0)
       |
       v
[ EC2 Instance - Amazon Linux 2 ]  id: i-04505dbb91806e12a
  - IAM Role          : devsecops-ssm-role
  - EBS 32 Go gp3     : chiffre AES-256
  - IMDSv2            : obligatoire
       ^
       |  (canal SSM chiffre TLS, aucun port ouvert)
[ SSM Session Manager - AWS API ]
       ^
       |
[ Collaborateurs  (AWS CLI + plugin SSM) ]
```

### 1.2 Composants deployes

| Composant            | Identifiant AWS         | Role                            |
| -------------------- | ----------------------- | ------------------------------- |
| EC2 Instance         | i-04505dbb91806e12a     | Serveur applicatif principal    |
| Security Group       | sg-0dc679e5c4997802d    | Controle du trafic reseau       |
| IAM Role             | devsecops-ssm-role      | Identite de l'instance pour SSM |
| IAM Instance Profile | devsecops-ssm-profile   | Pont entre le role et l'EC2     |
| S3 Bucket            | devsecops-tfstate-ajele | Stockage du state Terraform     |
| DynamoDB Table       | terraform-lock          | Verrouillage du state Terraform |

---

## 2. Mesures de securite en place

### 2.1 Aucun port SSH expose

Le security group ne contient aucune regle autorisant le port 22. Ce choix est volontaire et constitue la principale mesure de reduction de surface d'attaque. Un attaquant qui scanne l'instance ne trouvera aucun port d'administration accessible depuis Internet.

### 2.2 Chiffrement du disque au repos

Le volume EBS est configure avec `encrypted = true`. AWS utilise AES-256 via le service KMS. Meme en cas d'acces physique ou de snapshot non autorise, les donnees restent illisibles sans la cle de chiffrement.

### 2.3 IMDSv2 obligatoire

Le service de metadonnees de l'instance (IMDS) est configure en version 2 avec `http_tokens = required`. IMDSv1 est vulnerable aux attaques SSRF : un attaquant exploitant une faille applicative pouvait interroger `http://169.254.169.254` pour voler les credentials IAM de l'instance. IMDSv2 impose un token de session qui casse ce vecteur d'attaque.

### 2.4 IAM Least Privilege

Le role IAM attache a l'instance dispose uniquement de la politique `AmazonSSMManagedInstanceCore`, qui contient exactement les permissions necessaires au fonctionnement de SSM et rien d'autre. L'instance ne peut pas creer d'autres ressources AWS, acceder a S3 ou effectuer des actions non prevues.

### 2.5 State Terraform chiffre et centralise

Le fichier `terraform.tfstate` est stocke dans un bucket S3 avec chiffrement active et acces public totalement bloque. La table DynamoDB `terraform-lock` empeche deux personnes d'appliquer des changements simultanement, evitant toute corruption de l'etat.

### 2.6 Tracabilite des acces via CloudTrail

Chaque session SSM ouverte par un collaborateur est automatiquement enregistree dans AWS CloudTrail. Il est possible de savoir qui s'est connecte, a quelle heure et depuis quelle identite. C'est une tracabilite impossible a obtenir avec SSH sans configuration supplementaire complexe.

---

## 3. Comparaison SSH vs SSM Session Manager

### 3.1 Ce qu'aurait necesssite une approche SSH

Si l'acces avait ete configure via SSH, voici ce qui aurait ete necessaire :

- Ouvrir le port 22 dans le security group, exposant un service d'administration directement sur Internet
- Generer une paire de cles RSA (`aws ec2 create-key-pair`) et stocker la cle privee `.pem`
- Distribuer la cle privee a chaque collaborateur de maniere securisee
- Chaque collaborateur aurait du conserver sa cle localement, avec le risque de perte ou de vol
- Configurer les permissions de la cle (`chmod 400`) sur chaque machine
- Gerer la rotation des cles en cas de compromission : regenerer, redistribuer, reconfigurer
- Aucune tracabilite native des commandes executees sans configuration de logs supplementaire

> La gestion des cles SSH dans une equipe de 5 personnes est une source recurrente d'incidents de securite : cles partagees, cles oubliees dans des depots Git, absence de rotation.

### 3.2 Tableau comparatif

| Critere             | SSH classique                           | SSM Session Manager              |
| ------------------- | --------------------------------------- | -------------------------------- |
| Port reseau ouvert  | Port 22 expose sur Internet             | Aucun port ouvert                |
| Gestion des acces   | Cles privees a distribuer et stocker    | Identite IAM/SSO, sans cle       |
| Tracabilite         | Manuelle, logs a configurer             | Native dans CloudTrail           |
| Rotation des acces  | Regeneration et redistribution des cles | Revoquer le role IAM suffit      |
| Surface d'attaque   | Port 22 visible et scannable            | Aucune surface reseau exposee    |
| Acces sans IP fixe  | Necessite gestion CIDR dynamique        | Fonctionne depuis n'importe ou   |
| Audit de conformite | Complexe a prouver                      | Logs centralises automatiquement |

---

## 4. Guide de connexion pour les collaborateurs

Ce guide s'adresse a Erwin, Leo, Anthony et Esteban. Suivez les etapes dans l'ordre. L'installation ne se fait qu'une seule fois.

### 4.1 Installation des prerequis (une seule fois)

#### Etape 1 — Installer l'AWS CLI

**Linux (Ubuntu / Debian) :**

```bash
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
sudo ./aws/install
```

**Windows :**
Telecharger et executer : https://awscli.amazonaws.com/AWSCLIV2.msi

**macOS :**

```bash
brew install awscli
```

Verifier l'installation :

```bash
aws --version
```

#### Etape 2 — Installer le plugin SSM Session Manager

Ce plugin est indispensable. Sans lui, la commande de connexion echouera.

**Linux (Ubuntu / Debian) :**

```bash
curl "https://s3.amazonaws.com/session-manager-downloads/plugin/latest/ubuntu_64bit/session-manager-plugin.deb" -o "session-manager-plugin.deb"
sudo dpkg -i session-manager-plugin.deb
```

**Windows :**
Telecharger et executer : https://s3.amazonaws.com/session-manager-downloads/plugin/latest/windows/SessionManagerPluginSetup.exe

**macOS :**

```bash
curl "https://s3.amazonaws.com/session-manager-downloads/plugin/latest/mac/sessionmanager-bundle.zip" -o "session-plugin.zip"
unzip session-plugin.zip
sudo ./sessionmanager-bundle/install -i /usr/local/sessionmanagerplugin -b /usr/local/bin/session-manager-plugin
```

Verifier l'installation :

```bash
session-manager-plugin --version
```

#### Etape 3 — Verifier votre profil SSO existant

Vous disposez deja d'un acces AWS SSO configure sur votre machine. Verifiez le nom de votre profil :

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

Notez le nom entre crochets apres `profile` — c'est celui a utiliser dans les commandes suivantes.

Si votre session SSO est expiree, reconnectez-vous :

```bash
aws sso login --profile <votre-profil-sso>
```

### 4.2 Se connecter a l'instance

Une fois les prerequis installes, la connexion se fait avec une seule commande en remplacant `<votre-profil-sso>` par le nom de profil trouve a l'etape precedente :

```bash
aws ssm start-session \
  --target i-04505dbb91806e12a \
  --region eu-west-3 \
  --profile <votre-profil-sso>
```

Un shell interactif s'ouvre directement dans votre terminal. Vous etes connecte en tant qu'utilisateur `ssm-user`. Pour obtenir les droits root :

```bash
sudo -i
```

### 4.3 Verifier que l'instance est accessible

Si la connexion echoue, verifiez d'abord que l'instance est visible par SSM :

```bash
aws ssm describe-instance-information \
  --region eu-west-3 \
  --profile <votre-profil-sso>
```

L'instance doit apparaitre avec `PingStatus: Online`. Si ce n'est pas le cas, l'instance est peut-etre arretee ou l'agent SSM n'est pas actif.

### 4.4 Fermer la session

Pour terminer la session, tapez :

```bash
exit
```

La session est automatiquement fermee et l'evenement est enregistre dans CloudTrail.

---

## 5. Ce qu'il ne faut jamais faire

| Action interdite                                                   | Pourquoi                                                     |
| ------------------------------------------------------------------ | ------------------------------------------------------------ |
| Commiter `terraform.tfvars` sur Git                                | Contient des variables sensibles du projet                   |
| Ouvrir le port 22 dans le security group                           | Cree une surface d'attaque SSH directement sur Internet      |
| Partager son token SSO ou se connecter depuis le compte d'un autre | Chaque identite doit rester individuelle pour la tracabilite |
| Stocker le tfstate en local                                        | Le state partage serait ecrase a chaque apply                |
| Desactiver le chiffrement du disque                                | Les donnees seraient lisibles en cas d'acces au volume       |

---

## 6. Informations de reference

| Information         | Valeur                                         |
| ------------------- | ---------------------------------------------- |
| Instance ID         | i-04505dbb91806e12a                            |
| Region AWS          | eu-west-3 (Paris)                              |
| IP privee           | 172.31.41.224                                  |
| Security Group      | sg-0dc679e5c4997802d                           |
| S3 State bucket     | devsecops-tfstate-ajele                        |
| DynamoDB Lock table | terraform-lock                                 |
| Profil AWS CLI      | Votre propre profil SSO (voir `~/.aws/config`) |

> En cas de probleme de connexion, verifiez en premier lieu que le plugin SSM est bien installe (`session-manager-plugin --version`) et que votre profil est correctement configure (`aws configure list --profile <votre-profil-sso>`).
