# Ansible — Déploiement Docker sur EC2

## Structure du projet

```
ansible/
├── ansible.cfg                        # Configuration globale Ansible (inventory, user SSH, roles_path)
├── inventory/
│   ├── hosts.yml                      # Inventaire statique : IP de l'instance EC2 cible
│   └── group_vars/
│       └── vault.yml                  # Variables sensibles chiffrées avec Ansible Vault
├── playbooks/
│   └── deploy.yml                     # Playbook principal : applique le rôle docker sur le groupe ec2
└── roles/
    └── docker/
        ├── defaults/
        │   └── main.yml               # Variables par défaut (version Docker Compose, utilisateur)
        ├── handlers/
        │   └── main.yml               # Handlers : démarrage Docker, activation au boot, reset connexion
        └── tasks/
            └── main.yml               # Tâches : installation Docker, Docker Compose, sécurité
```

## Prérequis

- Ansible >= 2.14
- Python >= 3.9
- Instance EC2 Amazon Linux 2023 accessible via SSM Session Manager
- Mot de passe Vault dans `~/.vault_pass` (non commité)

Installer les dépendances Python :

```bash
pip install ansible ansible-lint
```

## Procédure de déploiement

### 1. Vérifier la connectivité

```bash
cd ansible
ansible ec2 -m ping
```

Résultat attendu :

```
<YOUR_INSTANCE_IP> | SUCCESS => { "ping": "pong" }
```

### 2. Vérifier la syntaxe du playbook

```bash
ansible-playbook playbooks/deploy.yml --syntax-check
```

### 3. Lancer le déploiement (dry-run)

```bash
ansible-playbook playbooks/deploy.yml --check --diff
```

### 4. Déployer

```bash
ansible-playbook playbooks/deploy.yml --vault-password-file ~/.vault_pass
```

## Commandes de vérification post-déploiement

```bash
# Vérifier que Docker est actif
ansible ec2 -m shell -a "systemctl is-active docker"

# Vérifier la version Docker installée
ansible ec2 -m shell -a "docker --version"

# Vérifier la version Docker Compose
ansible ec2 -m shell -a "docker-compose --version"

# Vérifier que ec2-user est dans le groupe docker
ansible ec2 -m shell -a "groups ec2-user"

# Idempotence : relancer le playbook sans changement attendu
ansible-playbook playbooks/deploy.yml --vault-password-file ~/.vault_pass
# => toutes les tâches doivent afficher "ok", aucune "changed"
```

## Extrait du rapport Docker Bench Security

Rapport généré automatiquement par la tâche `Run Docker Bench Security` du rôle.

```
Section A - Host Configuration
[WARN] 1.1  - Ensure a separate partition for containers has been created
[PASS] 1.5  - Ensure auditing is configured for the Docker daemon

Section B - Docker daemon configuration
[PASS] 2.1  - Run the Docker daemon as a non-root user (Rootless mode)
[WARN] 2.14 - Ensure Userland Proxy is Disabled
[NOTE] 2.17 - Ensure that the daemon-level seccomp profile is applied

Section C - Docker daemon configuration files
[PASS] 3.1  - Ensure that the docker.service file ownership is set to root:root
[PASS] 3.2  - Ensure that docker.service file permissions are set to 644 or more restrictive

Section E - Container Images and Build Files
[NOTE] 4.1  - Ensure that a user for the container has been created

Score: 16 checks PASS / 4 WARN / 3 NOTE
```

> Les avertissements (WARN) sont documentés et acceptés dans le cadre de ce TP.

## Problèmes rencontrés et solutions

### 1. `ansible.cfg` ignoré dans le runner GitLab CI

**Problème** : Le runner GitLab CI clonant le repo dans un répertoire world-writable (chmod 777), ansible refusait de charger `ansible.cfg` depuis un répertoire world-writable pour des raisons de sécurité.

**Solution** : Nous avons ainsi défini les variables d'environnement dans `.gitlab-ci.yml` :

```yaml
variables:
  ANSIBLE_CONFIG: "${CI_PROJECT_DIR}/ansible/ansible.cfg"
  ANSIBLE_ROLES_PATH: "${CI_PROJECT_DIR}/ansible/roles"
```

### 2. `role-name[path]` — chemin relatif dans le playbook

**Problème** : `ansible-lint` a interdi l'import de rôles par chemin (`- ../roles/docker`).

**Solution** : Nous avons utilisé le nom du rôle (`- docker`) et défini `ANSIBLE_ROLES_PATH`.

### 3. `ansible-lint` — violations de style

**Problème** : Nous avons constaté 10 violations (truthy values, permissions manquantes, noms en minuscules et plus.)

**Solution** : Nous avons appliqué des corrections dans les fichiers du rôle comme suit :

- `yes` / `no` remplacés par `true` / `false`
- `mode: '0755'` ajouté sur les téléchargements
- Noms de handlers capitalisés
- `changed_when: false` ajouté sur les tâches `command`
- Préfixe `docker_` ajouté aux variables du rôle
