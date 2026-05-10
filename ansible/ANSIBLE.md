# Ansible — Documentation Technique Complète

## Projet Final DevSecOps — Bachelor Cybersécurité 2025

| | |
|---|---|
| Responsable Ansible | Gomez |
| Transport | AWS SSM Session Manager (sans SSH) |
| OS cible | Amazon Linux 2023 |
| Instance EC2 | i-04505dbb91806e12a |
| Région AWS | eu-west-3 (Paris) |
| Date de rédaction | Avril 2026 |

---

## Table des matières

1. [Vue d'ensemble](#1-vue-densemble)
2. [Pourquoi Ansible ?](#2-pourquoi-ansible-)
3. [Transport SSM — fonctionnement et justification](#3-transport-ssm--fonctionnement-et-justification)
4. [Structure des fichiers](#4-structure-des-fichiers)
5. [Configuration globale — ansible.cfg](#5-configuration-globale--ansiblecfg)
6. [Inventaire — inventory/hosts.yml](#6-inventaire--inventoryhostsyml)
7. [Rôle security — durcissement du système](#7-rôle-security--durcissement-du-système)
8. [Rôle docker — installation et hardening](#8-rôle-docker--installation-et-hardening)
9. [Playbook principal — deploy.yml](#9-playbook-principal--deployyml)
10. [Ansible Vault — gestion des secrets](#10-ansible-vault--gestion-des-secrets)
11. [Résultats Docker Bench Security](#11-résultats-docker-bench-security)
12. [Ce qui reste à faire](#12-ce-qui-reste-à-faire)
13. [Ce que les collaborateurs doivent fournir](#13-ce-que-les-collaborateurs-doivent-fournir)
14. [Variables à modifier selon le contexte](#14-variables-à-modifier-selon-le-contexte)
15. [Commandes de référence](#15-commandes-de-référence)
16. [Annexe — Glossaire](#16-annexe--glossaire)

---

## 1. Vue d'ensemble

Ansible est l'outil de gestion de configuration utilisé pour **automatiser le durcissement et le déploiement** de l'instance EC2. Une fois l'infrastructure provisionnée par Terraform, Ansible prend le relais pour :

1. **Durcir le système d'exploitation** (rôle `security`) : mises à jour, SSH, auditd, SELinux
2. **Installer et sécuriser Docker** (rôle `docker`) : daemon CIS, Docker Compose, ECR, Docker Bench

L'ensemble est **idempotent** : relancer le playbook plusieurs fois produit le même résultat sans créer de doublons ni écraser de configurations existantes.

### Schéma de fonctionnement global

```
Nœud de contrôle (machine locale)
    ansible-playbook deploy.yml
          |
          | (HTTPS → AWS API → SSM)
          v
    AWS Systems Manager
          |
          | (canal SSM chiffré TLS — aucun port 22)
          v
    EC2 i-04505dbb91806e12a (Amazon Linux 2023)
          |
          |-- Rôle security : updates, SSH, auditd, SELinux
          |-- Rôle docker   : Docker, daemon.json, Compose, ECR, Bench
```

---

## 2. Pourquoi Ansible ?

### 2.1 Automatisation reproductible

Sans Ansible, chaque configuration (SSH hardening, auditd, Docker) devrait être appliquée manuellement à la main via SSM. Cela crée :
- Des configurations inconsistantes entre runs
- Une absence de traçabilité des changements
- Une impossibilité de recréer l'environnement à l'identique

Ansible résout cela : **un seul playbook, un résultat garanti**.

### 2.2 Idempotence

Chaque tâche Ansible vérifie l'état actuel avant d'agir. Si Docker est déjà installé, la tâche dit `ok` et ne fait rien. Si la règle SSH est déjà en place, elle n'est pas réécrite. C'est la propriété fondamentale qui rend Ansible sûr à relancer sans risque.

### 2.3 Infrastructure as Code pour la configuration

De même que Terraform gère l'infrastructure, Ansible gère la **configuration de l'OS et des services**. Les deux ensemble forment la chaîne complète :

```
Terraform → Instance EC2 vide mais sécurisée
Ansible   → Instance EC2 configurée et durcie
```

---

## 3. Transport SSM — fonctionnement et justification

### 3.1 Le problème avec SSH dans ce projet

Ansible se connecte par défaut aux machines distantes via SSH. Cela aurait nécessité :
- Ouvrir le port 22 dans le Security Group — ce que nous avons **délibérément refusé**
- Distribuer une clé privée `.pem` à tous les collaborateurs
- Gérer la rotation des clés

### 3.2 La solution : community.aws.aws_ssm

Le plugin `community.aws.aws_ssm` permet à Ansible de se connecter à l'instance **sans SSH**, en passant exclusivement par l'API AWS Systems Manager. Le flux de connexion est :

```
ansible-playbook
    → boto3 (Python)
    → API AWS SSM (HTTPS)
    → SSM Agent sur l'instance EC2
    → Exécution des commandes
```

**Avantages concrets :**
- Port 22 reste fermé → surface d'attaque nulle
- Authentification via identité IAM/SSO → pas de clés à distribuer
- Chaque connexion est tracée dans CloudTrail automatiquement
- Fonctionne depuis n'importe quel réseau, sans IP fixe

### 3.3 Prérequis sur le nœud de contrôle

Pour que le transport SSM fonctionne, la machine qui exécute Ansible doit avoir :

```bash
# 1. Collections Ansible
ansible-galaxy collection install community.aws amazon.aws

# 2. SDK Python AWS
pip install boto3 botocore

# 3. Plugin AWS CLI Session Manager
# Linux : https://docs.aws.amazon.com/systems-manager/latest/userguide/
#         session-manager-working-with-install-plugin.html

# 4. Profil AWS configuré
aws configure --profile devesecops_project_final_ajele
# ou via SSO :
aws sso login --profile devesecops_project_final_ajele
```

### 3.4 Fichier temporaire via S3

Le plugin SSM utilise un bucket S3 comme canal de transfert de fichiers temporaires entre le nœud de contrôle et l'instance EC2. C'est pourquoi :
- Le bucket `devsecops-tfstate-ajele` est utilisé avec le préfixe `ansible-ssm-tmp/`
- L'instance EC2 dispose de la policy `AmazonS3FullAccess` dans son rôle IAM

> En production : restreindre à une policy personnalisée limitée au seul préfixe `ansible-ssm-tmp/*` du bucket.

---

## 4. Structure des fichiers

```
ansible/
├── ansible.cfg                          # Configuration globale (transport, chemins, logs)
├── inventory/
│   ├── hosts.yml                        # Inventaire : instance EC2 ciblée par ID SSM
│   └── group_vars/
│       └── vault.yml                    # Secrets chiffrés (Ansible Vault AES256)
├── playbooks/
│   └── deploy.yml                       # Point d'entrée unique du déploiement
└── roles/
    ├── security/                        # Rôle 1 : durcissement OS
    │   ├── defaults/
    │   │   └── main.yml                 # Variables par défaut (SELinux, auditd, SSH, fail2ban)
    │   ├── handlers/
    │   │   └── main.yml                 # Handlers : restart sshd, reload auditd
    │   ├── meta/
    │   │   └── main.yml                 # Métadonnées du rôle (platform, version)
    │   └── tasks/
    │       └── main.yml                 # Tâches de hardening système
    └── docker/                          # Rôle 2 : Docker + sécurité conteneurs
        ├── defaults/
        │   └── main.yml                 # Variables : version Compose, ECR URL, région
        ├── handlers/
        │   └── main.yml                 # Handlers : start/restart/reload docker
        ├── meta/
        │   └── main.yml                 # Métadonnées (pas de dépendance — ordre via playbook)
        └── tasks/
            └── main.yml                 # Installation Docker, daemon.json, ECR, Bench
```

---

## 5. Configuration globale — ansible.cfg

```ini
[defaults]
inventory      = inventory/hosts.yml
remote_user    = ec2-user
roles_path     = ./roles
host_key_checking = False
log_path       = /tmp/ansible-devsecops.log
forks          = 5
remote_tmp     = /tmp/.ansible/tmp

[ssh_connection]
ssh_args = -o ControlMaster=no
```

### Points clés

| Paramètre | Valeur | Pourquoi |
|-----------|--------|----------|
| `remote_user` | `ec2-user` | Utilisateur par défaut Amazon Linux 2023 |
| `host_key_checking` | `False` | Sans SSH, le fingerprint ne s'applique pas |
| `remote_tmp` | `/tmp/.ansible/tmp` | **Critique** : le canal SSM s'exécute sous un utilisateur système sans accès à `/home/ec2-user`. Forcer `/tmp` évite l'erreur `Permission denied` au démarrage du play |
| `log_path` | `/tmp/ansible-devsecops.log` | Traçabilité locale des runs Ansible |
| `ssh_args` | `ControlMaster=no` | Empêche tout fallback SSH accidentel |

---

## 6. Inventaire — inventory/hosts.yml

```yaml
all:
  children:
    ec2:
      hosts:
        i-04505dbb91806e12a:
      vars:
        ansible_connection: community.aws.aws_ssm
        ansible_aws_ssm_instance_id: "i-04505dbb91806e12a"
        ansible_aws_ssm_region: "eu-west-3"
        ansible_aws_ssm_profile: "devesecops_project_final_ajele"
        ansible_aws_ssm_bucket_name: "devsecops-tfstate-ajele"
        ansible_aws_ssm_bucket_prefix: "ansible-ssm-tmp"
        ansible_aws_ssm_timeout: 60
        ansible_python_interpreter: /usr/bin/python3
        ansible_user: ec2-user
```

### Décision importante : ID d'instance, pas adresse IP

L'inventaire cible l'instance par son **ID AWS** (`i-04505dbb91806e12a`), pas par son IP. C'est obligatoire pour le transport SSM : le plugin a besoin de l'ID pour ouvrir la session via l'API AWS, pas d'une adresse réseau. Une IP privée changerait après un arrêt/redémarrage de l'instance ; l'ID, lui, est permanent.

---

## 7. Rôle security — durcissement du système

Ce rôle s'exécute **en premier** dans le playbook. Son objectif est d'établir un socle sécurisé avant tout déploiement applicatif.

### 7.1 Mises à jour système

```yaml
- name: Apply security package updates
  ansible.builtin.dnf:
    name: "*"
    state: latest
    security: "{{ not security_update_all_packages }}"
```

**Pourquoi :** Les CVE sont publiées quotidiennement. Une instance fraîche sans mises à jour de sécurité est vulnérable aux exploits connus. La variable `security_update_all_packages` (défaut : `false`) applique uniquement les correctifs de sécurité marqués, sans risque de casser des dépendances applicatives.

### 7.2 fail2ban — bloc/rescue (AL2023)

fail2ban n'est **pas disponible** dans les dépôts officiels d'Amazon Linux 2023. EPEL (Extra Packages for Enterprise Linux) n'est pas nativement supporté sur AL2023 — Amazon a fait ce choix pour garantir la stabilité et la compatibilité de la distribution.

**Solution mise en place :** pattern `block/rescue` Ansible. Si l'installation échoue, le play ne s'arrête pas. Un message d'avertissement informatif est affiché et les autres mesures de sécurité restent actives.

```yaml
- name: Install and configure fail2ban
  when: security_install_fail2ban
  block:
    - name: Install fail2ban (via dnf)
      ...
  rescue:
    - name: Warn that fail2ban is unavailable on Amazon Linux 2023
      ansible.builtin.debug:
        msg: "fail2ban non disponible sur Amazon Linux 2023..."
```

**Ce n'est pas bloquant car :**
- Le port 22 est fermé au niveau du Security Group Terraform → aucun paquet SSH n'atteint l'instance
- Le durcissement SSH (section 7.3) bloque root et mots de passe si le SG était rouvert
- auditd (section 7.4) journalise toute tentative de connexion

### 7.3 Durcissement SSH

```yaml
- name: Disable SSH root login
  ansible.builtin.lineinfile:
    path: /etc/ssh/sshd_config
    regexp: "^#?PermitRootLogin"
    line: "PermitRootLogin no"
    validate: /usr/sbin/sshd -t -f %s
  notify: Restart sshd

- name: Disable SSH password authentication
  ansible.builtin.lineinfile:
    path: /etc/ssh/sshd_config
    regexp: "^#?PasswordAuthentication"
    line: "PasswordAuthentication no"
    validate: /usr/sbin/sshd -t -f %s
  notify: Restart sshd
```

**Pourquoi ces deux directives :**

| Directive | Valeur | Protection |
|-----------|--------|------------|
| `PermitRootLogin no` | Interdit la connexion directe en root | Empêche l'exploitation root immédiate même avec des credentials valides |
| `PasswordAuthentication no` | Impose les clés SSH uniquement | Rend les attaques par brute-force de mots de passe inopérantes |

La directive `validate:` garantit que la configuration modifiée est syntaxiquement valide avant d'être appliquée — évite de bloquer sshd avec un fichier corrompu.

### 7.4 auditd — journalisation des appels système

auditd est le système de journalisation des événements du noyau Linux. Il enregistre qui a fait quoi, quand, sur quels fichiers.

**Règles déployées :**

| Règle | Cible | Événements capturés |
|-------|-------|---------------------|
| `-w /etc/passwd -p wa` | Base utilisateurs | Toute modification de compte |
| `-w /etc/shadow -p wa` | Mots de passe hashés | Changements de credentials |
| `-w /etc/sudoers -p wa` | Escalade de privilèges | Ajout/suppression de droits sudo |
| `-w /var/log/auth.log -p wa` | Logs d'authentification | Tentatives de connexion |
| `-w /usr/bin/docker -p x` | Binaire Docker | Exécution de Docker |
| `-w /var/lib/docker -p rwxa` | Données Docker | Modifications des images/conteneurs |
| `-w /etc/docker -p rwxa` | Configuration Docker | Changements daemon.json |
| `-a always,exit -S execve` | Tous les processus | Toutes les exécutions de commandes |

Les règles pour `/var/lib/docker` et `/etc/docker` corrigent les WARNs 1.6 et 1.7 du Docker Bench Security.

### 7.5 SELinux — mode enforcing

```yaml
- name: Set SELinux to enforcing mode
  ansible.posix.selinux:
    policy: targeted
    state: "{{ security_selinux_state }}"
  register: selinux_result
```

SELinux (Security-Enhanced Linux) est un système de contrôle d'accès obligatoire (MAC) intégré au noyau Linux. En mode `enforcing`, il applique des politiques de sécurité qui restreignent ce que chaque processus peut faire, même root.

**Politique `targeted` :** seuls les processus réseau à risque (httpd, docker, sshd…) sont confinés. Le reste du système reste non confiné pour éviter les faux positifs.

> Un reboot peut être nécessaire si SELinux passe de `disabled` à `enforcing`. Le module Ansible le détecte et affiche un avertissement.

### 7.6 Variables par défaut (security/defaults/main.yml)

| Variable | Valeur par défaut | Description |
|----------|-------------------|-------------|
| `security_update_all_packages` | `false` | `true` = toutes les mises à jour, `false` = sécurité uniquement |
| `security_install_fail2ban` | `true` | Tente l'installation (block/rescue si indisponible) |
| `security_fail2ban_maxretry` | `5` | Tentatives avant bannissement |
| `security_fail2ban_bantime` | `600` | Durée de bannissement en secondes (10 min) |
| `security_fail2ban_findtime` | `600` | Fenêtre temporelle d'analyse |
| `security_ssh_permit_root_login` | `"no"` | Accès root SSH |
| `security_ssh_password_auth` | `"no"` | Authentification par mot de passe |
| `security_install_auditd` | `true` | Installation et activation auditd |
| `security_selinux_state` | `"enforcing"` | Mode SELinux cible |

---

## 8. Rôle docker — installation et hardening

### 8.1 Ordre des tâches

```
1. Prérequis système     → dnf-plugins-core
2. Installation Docker   → notify: Start and enable docker
3. daemon.json (CIS)     → notify: Restart docker
4. Docker Compose        → téléchargement binaire v2.27.0
5. Permissions Compose   → chmod 755
6. Groupe docker         → ec2-user → notify: Reload user permissions
7. Ensure started        → idempotence (couvre les runs sans changement)
8. ECR login             → aws ecr get-login-password | docker login
9. Docker Bench Security → audit CIS automatique
10. Affichage résultats  → debug stdout_lines
```

### 8.2 daemon.json — durcissement CIS du daemon Docker

```json
{
  "icc": false,
  "live-restore": true,
  "userland-proxy": false,
  "no-new-privileges": true,
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  }
}
```

**Détail de chaque directive :**

| Directive | Valeur | WARN corrigé | Explication |
|-----------|--------|-------------|-------------|
| `icc` | `false` | 2.1 | Désactive la communication inter-conteneurs sur le bridge par défaut. Les conteneurs ne peuvent pas se parler sans réseau explicitement déclaré |
| `live-restore` | `true` | 2.14 | Les conteneurs continuent de tourner si le daemon Docker redémarre (mise à jour, crash). Évite les interruptions de service |
| `userland-proxy` | `false` | 2.15 | Supprime le proxy userland Docker pour le NAT. Utilise iptables directement → meilleure performance et moindre surface d'attaque |
| `no-new-privileges` | `true` | 2.18 | Interdit aux processus dans les conteneurs d'obtenir des privilèges supplémentaires via setuid/setgid — même si le binaire est setuid root |
| `log-driver` | `json-file` | 2.12 | Active la journalisation structurée de tous les conteneurs avec rotation automatique (10 Mo max, 3 fichiers) |

### 8.3 Authentification ECR

```yaml
- name: Authenticate to Amazon ECR
  ansible.builtin.shell: >
    aws ecr get-login-password --region {{ aws_region }}
    | docker login --username AWS --password-stdin {{ ecr_registry_url }}
  changed_when: false
  no_log: true
  ignore_errors: true
```

- `no_log: true` : le token d'authentification ECR (valide 12h) n'apparaît jamais dans les logs Ansible
- `ignore_errors: true` : si ECR n'est pas encore accessible, le play continue
- Le token est obtenu via le rôle IAM de l'instance (`AmazonEC2ContainerRegistryReadOnly`) sans aucune clé en dur

### 8.4 Docker Bench Security

Docker Bench Security est un outil open source de CIS (Center for Internet Security) qui audite la configuration Docker selon le CIS Docker Benchmark. Il s'exécute dans un conteneur Docker avec accès en lecture seule aux fichiers système.

```yaml
- name: Run Docker Bench Security audit
  ansible.builtin.command: >
    docker run --rm --net host --pid host --userns host
    --cap-add audit_control
    -v /etc:/etc:ro -v /var/lib:/var/lib:ro
    -v /var/run/docker.sock:/var/run/docker.sock:ro
    docker/docker-bench-security
```

Le résultat est affiché dans les logs Ansible via la tâche `Display Docker Bench Security results`.

### 8.5 Handlers du rôle docker

| Handler | Déclenché par | Action |
|---------|--------------|--------|
| `Start and enable docker` | Installation Docker | Démarre le service ET l'active au boot |
| `Restart docker` | Modification daemon.json | Redémarre pour charger la nouvelle config |
| `Reload user permissions` | Ajout au groupe docker | `meta: reset_connection` — recharge la session SSM pour que le groupe soit actif immédiatement |

### 8.6 Variables par défaut (docker/defaults/main.yml)

| Variable | Valeur | Description |
|----------|--------|-------------|
| `docker_compose_version` | `"2.27.0"` | Version du binaire Docker Compose téléchargé |
| `docker_user` | `"ec2-user"` | Utilisateur ajouté au groupe docker |
| `aws_region` | `"eu-west-3"` | Région AWS pour ECR et SSM |
| `ecr_registry_url` | `418295718544.dkr.ecr.eu-west-3.amazonaws.com/taskmanager` | URL complète du registre ECR |
| `docker_content_trust` | `"0"` | DCT désactivé (dev) — passer à `"1"` en production |

---

## 9. Playbook principal — deploy.yml

```yaml
- name: Déploiement sécurisé de l'infrastructure applicative sur EC2
  hosts: ec2
  become: true
  vars:
    aws_region: "eu-west-3"
  roles:
    - role: security
      tags: [security, hardening]
    - role: docker
      tags: [docker, containers]
```

**Points importants :**

- `become: true` déclaré **une seule fois** au niveau du playbook — pas répété dans chaque tâche. C'est la bonne pratique Ansible qui évite la redondance.
- L'ordre `security` → `docker` est **garanti** par la déclaration dans ce fichier. La dépendance dans `docker/meta/main.yml` a été **supprimée** pour éviter la double exécution du rôle security.
- Les tags permettent d'exécuter uniquement un rôle : `--tags security` ou `--tags docker`.

---

## 10. Ansible Vault — gestion des secrets

### 10.1 Qu'est-ce qu'Ansible Vault ?

Ansible Vault chiffre les fichiers de variables sensibles avec AES-256. Seul celui qui possède le mot de passe de déchiffrement peut lire le contenu. Le fichier chiffré peut être commité sur Git en toute sécurité.

### 10.2 Fichier vault.yml

`ansible/inventory/group_vars/vault.yml` contient les variables sensibles du projet (credentials, tokens, etc.). Son contenu est chiffré — l'en-tête du fichier ressemble à :

```
$ANSIBLE_VAULT;1.1;AES256
66386134...
```

### 10.3 Utilisation

```bash
# Déployer avec déchiffrement automatique
ansible-playbook playbooks/deploy.yml --vault-password-file ~/.vault_pass

# Éditer le vault (ouvre un éditeur avec le contenu déchiffré)
ansible-vault edit inventory/group_vars/vault.yml --vault-password-file ~/.vault_pass

# Voir le contenu déchiffré
ansible-vault view inventory/group_vars/vault.yml --vault-password-file ~/.vault_pass
```

### 10.4 Fichier ~/.vault_pass

Le mot de passe vault est stocké localement dans `~/.vault_pass`. Ce fichier ne doit **jamais** être commité sur Git.

```bash
# Créer le fichier (noter les guillemets simples pour éviter l'interprétation des caractères spéciaux)
echo 'votre-mot-de-passe' > ~/.vault_pass
chmod 600 ~/.vault_pass
```

> Utiliser **impérativement** des guillemets simples `'` si le mot de passe contient `!`, `$`, ou `\`. Les guillemets doubles causeraient une erreur bash d'historique (`event not found`).

---

## 11. Résultats Docker Bench Security

### 11.1 Évolution du score

| Run | Changements | Score |
|-----|-------------|-------|
| Run initial | Rôle security exécuté 2 fois (bug méta) | 9 |
| Run 2 | Bug méta corrigé, auditd actif | 11 |
| Run 3 (attendu) | daemon.json + auditd /var/lib/docker + /etc/docker | ~18 |

### 11.2 WARNs corrigés par le dernier run

| Check | Description | Fix appliqué |
|-------|-------------|-------------|
| 1.5 | Auditing configured for Docker daemon | Règle auditd `/usr/bin/docker` |
| 1.6 | Auditing for /var/lib/docker | Règle auditd ajoutée |
| 1.7 | Auditing for /etc/docker | Règle auditd ajoutée |
| 2.1 | Inter-container communication restricted | `icc: false` dans daemon.json |
| 2.12 | Centralized logging configured | `log-driver: json-file` dans daemon.json |
| 2.14 | Live restore enabled | `live-restore: true` dans daemon.json |
| 2.15 | Userland Proxy disabled | `userland-proxy: false` dans daemon.json |
| 2.18 | Containers restricted from new privileges | `no-new-privileges: true` dans daemon.json |

### 11.3 WARNs restants (non bloquants)

| Check | Description | Raison non corrigée |
|-------|-------------|---------------------|
| 1.1 | Separate partition for containers | Nécessite un changement Terraform (volume EBS dédié) — hors scope actuel |
| 2.8 | User namespace support | `userns-remap` peut casser l'agent ECS présent sur l'instance |
| 2.11 | Authorization plugin for Docker | Nécessite un authz plugin tiers (OPA, etc.) — complexité élevée |
| 4.5 | Docker Content Trust | Nécessite une infrastructure de signing (Notary) — hors scope |
| 4.6 | HEALTHCHECK in images | Dépend des Dockerfiles applicatifs — à traiter côté app |

---

## 12. Ce qui reste à faire

### 12.1 Déploiement de l'application (priorité haute)

L'instance est prête mais **aucun conteneur applicatif ne tourne** (`No containers running` dans Docker Bench). L'étape suivante est :

1. **Construire l'image Docker** de l'application `taskmanager`
2. **Pousser l'image sur ECR** :
   ```bash
   aws ecr get-login-password --region eu-west-3 --profile <profil> \
     | docker login --username AWS --password-stdin \
       418295718544.dkr.ecr.eu-west-3.amazonaws.com
   
   docker build -t taskmanager .
   docker tag taskmanager:latest 418295718544.dkr.ecr.eu-west-3.amazonaws.com/taskmanager:latest
   docker push 418295718544.dkr.ecr.eu-west-3.amazonaws.com/taskmanager:latest
   ```
3. **Créer un `docker-compose.yml`** pour l'application
4. **Ajouter une tâche Ansible** dans le rôle docker pour déployer via `docker compose up`

### 12.2 CI/CD Pipeline (priorité moyenne)

Intégrer Ansible dans une pipeline GitHub Actions ou GitLab CI pour que le déploiement se déclenche automatiquement sur chaque push sur `main`.

### 12.3 HTTPS / Reverse proxy (priorité moyenne)

L'instance expose les ports 80 et 443. Il faut configurer un reverse proxy (nginx ou Traefik) devant l'application pour :
- Terminer TLS avec un certificat (Let's Encrypt ou ACM)
- Servir l'application en HTTPS

### 12.4 Amélioration Docker Bench (priorité basse)

Pour faire passer le score de ~18 vers ~22+ :
- WARN 1.1 : ajouter un volume EBS dédié `/var/lib/docker` via Terraform
- WARN 4.6 : ajouter `HEALTHCHECK` dans les Dockerfiles applicatifs

---

## 13. Ce que les collaborateurs doivent fournir

### 13.1 Pour le déploiement de l'application

| Élément | Responsable | Description |
|---------|-------------|-------------|
| `Dockerfile` | Équipe applicative | Image Docker de l'application `taskmanager` |
| `docker-compose.yml` | Équipe applicative | Services, volumes, réseaux, variables d'env |
| Variables d'environnement | Tous | Secrets à ajouter dans `vault.yml` (DB password, JWT secret, etc.) |
| Port d'écoute de l'app | Équipe applicative | Pour configurer le reverse proxy |

### 13.2 Variables à ajouter dans vault.yml

Les collaborateurs doivent fournir les valeurs suivantes pour enrichir le vault Ansible :

```yaml
# Exemple de ce que vault.yml devrait contenir
vault_db_password: "..."          # Mot de passe base de données
vault_jwt_secret: "..."           # Clé JWT de l'application
vault_app_secret_key: "..."       # Clé secrète Django/Flask/etc.
```

Pour ajouter une variable :
```bash
ansible-vault edit inventory/group_vars/vault.yml --vault-password-file ~/.vault_pass
```

### 13.3 Pour la connexion SSM (chaque collaborateur)

Chaque collaborateur doit avoir configuré sur sa machine :
- AWS CLI installé
- Plugin SSM Session Manager installé
- Profil SSO AWS configuré avec accès au compte `418295718544`
- Session SSO active : `aws sso login --profile <son-profil>`

---

## 14. Variables à modifier selon le contexte

| Fichier | Variable | Quand modifier |
|---------|----------|----------------|
| `inventory/hosts.yml` | `ansible_aws_ssm_instance_id` | Si l'instance EC2 est recréée (nouvelle AMI, resize, etc.) |
| `docker/defaults/main.yml` | `ecr_registry_url` | Si le compte AWS ou la région change |
| `docker/defaults/main.yml` | `docker_compose_version` | Pour mettre à jour Docker Compose |
| `docker/defaults/main.yml` | `docker_content_trust` | Passer à `"1"` pour activer DCT en production |
| `security/defaults/main.yml` | `security_selinux_state` | `"permissive"` si SELinux casse une app en production |
| `security/defaults/main.yml` | `security_update_all_packages` | `true` pour une mise à jour complète (pas que sécurité) |
| `inventory/hosts.yml` | `ansible_aws_ssm_profile` | Si le nom du profil SSO change |

---

## 15. Commandes de référence

### Déploiement complet

```bash
cd ansible
ansible-playbook playbooks/deploy.yml --vault-password-file ~/.vault_pass
```

### Test de connectivité

```bash
ansible ec2 -m ping
```

### Déployer uniquement le rôle security

```bash
ansible-playbook playbooks/deploy.yml --vault-password-file ~/.vault_pass --tags security
```

### Déployer uniquement le rôle docker

```bash
ansible-playbook playbooks/deploy.yml --vault-password-file ~/.vault_pass --tags docker
```

### Dry-run (simulation sans changement)

```bash
ansible-playbook playbooks/deploy.yml --vault-password-file ~/.vault_pass --check --diff
```

### Vérification post-déploiement

```bash
# Docker actif
ansible ec2 -m shell -a "systemctl is-active docker"

# Version Docker
ansible ec2 -m shell -a "docker --version"

# daemon.json appliqué
ansible ec2 -m shell -a "cat /etc/docker/daemon.json"

# Règles auditd actives
ansible ec2 -m shell -a "auditctl -l"

# SELinux enforcing
ansible ec2 -m shell -a "getenforce"

# ec2-user dans le groupe docker
ansible ec2 -m shell -a "groups ec2-user"
```

### Connexion manuelle à l'instance (hors Ansible)

```bash
aws ssm start-session \
  --target i-04505dbb91806e12a \
  --region eu-west-3 \
  --profile devesecops_project_final_ajele
```

---

## 16. Annexe — Glossaire

### Ansible

**Playbook** : Fichier YAML décrivant une séquence d'opérations à exécuter sur des hôtes distants. Analogue à un script shell, mais déclaratif et idempotent.

**Rôle** : Unité de réutilisation Ansible. Regroupe tasks, handlers, defaults, meta en une structure standardisée. Un rôle fait une chose et la fait bien.

**Task** : Unité élémentaire d'un playbook. Appelle un module Ansible avec des paramètres. Exemple : `ansible.builtin.dnf` pour installer un paquet.

**Handler** : Tâche spéciale qui ne s'exécute qu'une fois en fin de play, uniquement si elle a été notifiée par une task qui a produit un changement. Utilisé pour les redémarrages de services.

**Idempotence** : Propriété d'une opération qui peut être appliquée plusieurs fois sans produire d'effet supplémentaire. `ok` dans les résultats Ansible = tâche déjà dans l'état souhaité, rien changé.

**Inventory** : Fichier décrivant les hôtes cibles (adresses IP, IDs, groupes, variables de connexion).

**group_vars** : Dossier contenant des fichiers de variables automatiquement chargés pour les groupes d'hôtes correspondants.

**Ansible Vault** : Système de chiffrement AES-256 intégré à Ansible pour sécuriser les fichiers de variables sensibles.

**block/rescue** : Structure Ansible équivalente à try/catch. Si une tâche dans `block` échoue, les tâches de `rescue` s'exécutent au lieu d'arrêter le play.

**become** : Équivalent Ansible de `sudo`. Permet d'exécuter les tâches avec des privilèges élevés.

**notify** : Directive dans une task qui déclenche l'exécution d'un handler si la task produit un changement.

**changed_when: false** : Indique à Ansible qu'une tâche ne modifie jamais l'état (même si elle s'exécute). Utilisé sur les tâches en lecture seule comme Docker Bench.

**no_log: true** : Empêche Ansible d'afficher les paramètres et résultats d'une tâche dans les logs. Indispensable pour les tâches manipulant des tokens ou mots de passe.

### AWS / Infrastructure

**SSM Session Manager** : Service AWS permettant d'ouvrir une session interactive sur une instance EC2 sans SSH, via l'API AWS et un agent installé sur l'instance.

**SSM Agent** : Daemon installé par défaut sur Amazon Linux 2023 qui communique avec le service SSM d'AWS et exécute les commandes reçues.

**ECR (Elastic Container Registry)** : Registre Docker managé par AWS. Stocke et distribue les images Docker. Intégré avec IAM pour le contrôle d'accès.

**IMDSv2** : Version 2 du service de métadonnées des instances EC2. Impose un token de session pour accéder aux métadonnées, bloquant les attaques SSRF qui exploitaient IMDSv1.

**IAM Role** : Identité AWS attachée à une ressource (ici EC2) qui lui confère des permissions pour accéder à d'autres services AWS sans clé d'accès explicite.

**CloudTrail** : Service AWS qui enregistre toutes les actions sur les APIs AWS, incluant chaque connexion SSM. Fournit une traçabilité d'audit complète.

**EPEL** : Extra Packages for Enterprise Linux. Dépôt communautaire de paquets supplémentaires pour les distributions Red Hat/CentOS. Non supporté nativement sur Amazon Linux 2023.

### Sécurité

**auditd** : Daemon Linux du sous-système d'audit du noyau. Journalise les appels système, accès aux fichiers, changements de permissions. Complémentaire à CloudTrail (niveau OS vs niveau AWS).

**SELinux** : Security-Enhanced Linux. Système de contrôle d'accès obligatoire (MAC) du noyau. En mode `enforcing`, il bloque les actions non autorisées par sa politique même pour root.

**fail2ban** : Outil qui analyse les logs système et bannit temporairement les IP ayant trop d'échecs d'authentification. Non disponible sur AL2023 (EPEL requis).

**CIS Benchmark** : Standard de sécurité publié par le Center for Internet Security. Définit des configurations recommandées pour les OS, services et conteneurs.

**Docker Content Trust (DCT)** : Mécanisme de signature et vérification des images Docker. Quand activé (`DOCKER_CONTENT_TRUST=1`), seules les images signées peuvent être téléchargées.

**icc (Inter-Container Communication)** : Paramètre Docker contrôlant si les conteneurs sur le même bridge réseau peuvent communiquer directement. `false` les isole.

**userns-remap** : Fonctionnalité Docker mappant l'UID root du conteneur vers un UID non-privilégié sur l'hôte. Réduit l'impact d'une évasion de conteneur.

**Userland proxy** : Processus Docker gérant le forwarding de ports en espace utilisateur. Remplaçable par des règles iptables directes (`userland-proxy: false`).

**no-new-privileges** : Flag Linux empêchant un processus (et ses enfants) d'obtenir des privilèges via setuid, setgid ou capabilities. Bloque une classe entière d'escalades de privilèges.

**AES-256** : Standard de chiffrement symétrique utilisé par AWS KMS, Ansible Vault, et le chiffrement EBS.
