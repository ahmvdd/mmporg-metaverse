# 🎮 MetaVerse Multiplayer — MMPORG Project

Jeu massivement multijoueur en temps réel développé avec Unity 6 et un serveur TCP custom en C# / .NET.

Projet réalisé dans le cadre du cours MMPORG — Glassworks Tech.

---
<img width="1470" height="923" alt="Capture d’écran 2026-06-09 à 19 42 47" src="https://github.com/user-attachments/assets/45099ed9-3419-47b3-9c68-1065aabc59da" />

## 📋 Description

MetaVerse est un jeu multijoueur en temps réel où plusieurs joueurs (4+) 
évoluent dans une ville 3D. Chaque joueur contrôle son personnage depuis 
sa propre machine. Les positions, actions et objets collectés sont 
synchronisés en temps réel via un protocole TCP custom.

---
Video avant qu'on configure le tunel Tcp 
https://youtu.be/jTDujALA998

## 🏗️ Architecture
mmporg-metaverse/
├── server/          → Serveur TCP .NET (source de vérité)
└── client/          → Projet Unity (client du jeu)

### Schéma réseau
Joueur 1 ──┐
Joueur 2 ──┼──► SERVEUR TCP ──► Broadcast à tous
Joueur 3 ──┤
Joueur 4 ──┘

---

## 🔧 Prérequis

### Serveur
- .NET 8 ou supérieur
- OS : Windows / Mac / Linux

### Client
- Unity 6 (6000.4.8f1)
- OS : Windows / Mac

---

## 🚀 Lancer le projet

### 1. Cloner le repo

```bash
git clone https://github.com/ahmvdd/mmporg-metaverse.git
cd mmporg-metaverse
```

### 2. Lancer le serveur

```bash
cd server
dotnet run
```

Le serveur démarre sur le port **5555**.
Serveur MetaVerse démarré sur le port 5555...

### 3. Lancer le client Unity

- Ouvre Unity Hub
- Open → sélectionne le dossier `client/`
- Ouvre la scène : `Assets/Demos/MetaVerse/MetaVerse.unity`
- Appuie sur **Play ▶️**
- Entre l'IP du serveur et le port **5555**
- Clique **Rejoindre**

### 4. Tester avec plusieurs joueurs

Chaque joueur lance le client Unity sur sa machine et se connecte 
à l'IP de la machine qui héberge le serveur.

---

## 📡 Protocole

Les messages sont envoyés en texte via TCP, séparés par `|`.
Chaque message se termine par `\n`.

| Message | Direction | Description |
|---|---|---|
| `CONNECT\|player_id` | Client → Serveur | Joueur rejoint |
| `DISCONNECT\|player_id` | Client → Serveur | Joueur quitte |
| `MOVE\|player_id\|x\|y\|z\|rotY` | Client → Serveur | Position du joueur |
| `COLLECT\|player_id\|objet_id` | Client → Serveur | Collecte un objet |
| `COLLECT_OK\|player_id\|objet_id` | Serveur → Client | Collecte validée |
| `COLLECT_DENIED\|player_id\|objet_id` | Serveur → Client | Collecte refusée |

---

## 🎮 Contrôles

| Touche | Action |
|---|---|
| Z / Flèche haut | Avancer |
| S / Flèche bas | Reculer |
| Q / Flèche gauche | Tourner à gauche |
| D / Flèche droite | Tourner à droite |

---

## 👥 Équipe

| Rôle | Responsabilité |
|---|---|
| P1 | Serveur TCP robuste |
| P2 | Client Unity + UI connexion |
| P3 | Joueurs distants + interpolation |
| P4 | Objets collectables + race condition |

---

## 📁 Structure du code
client/Assets/Demos/MetaVerse/
├── CharacterController.cs   → Déplacement joueur local + envoi position
├── NetworkManager.cs        → Connexion TCP + queue thread-safe
├── RemotePlayerManager.cs   → Spawn + synchro joueurs distants
├── CollectableManager.cs    → Gestion objets collectables
├── ConnectionUI.cs          → UI connexion (IP/port)
└── ScoreManager.cs          → Scores des joueurs
server/
└── Program.cs               → Serveur TCP + broadcast + déconnexion

---

## ✅ Fonctionnalités

### MVP
- [x] Serveur TCP multi-clients
- [ ] Connexion via IP/port
- [ ] Synchronisation positions en temps réel
- [ ] Spawn des autres joueurs
- [ ] Gestion déconnexion

### Bonus
- [ ] Interpolation des positions (mouvement fluide)
- [ ] Objets collectables partagés
- [ ] Synchronisation état initial
- [ ] Race condition (arbitrage serveur)
- [ ] Serveur déployé sur internet

---

## 🔗 Liens

- [Support de cours](https://learn.glassworks.tech/mmporg/)
- [Projet de base du prof](https://dev.glassworks.tech/courses/mmporg/mmporg-sample)
