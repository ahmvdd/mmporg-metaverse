# Protocole réseau — mmporg-metaverse

Messages texte séparés par `|`, encodés en UTF-8, envoyés via TCP.

---

## CONNECT

**Direction :** client → serveur → broadcast tous les autres clients

**Format :**
```
CONNECT|player_id
```

**Exemple :**
```
CONNECT|a3f9b2c1
```

**Comportement :** Le serveur broadcaste ce message à tous les joueurs déjà connectés pour qu'ils puissent spawner l'avatar du nouveau joueur.

---

## DISCONNECT

**Direction :** client → serveur → broadcast tous les autres clients

**Format :**
```
DISCONNECT|player_id
```

**Exemple :**
```
DISCONNECT|a3f9b2c1
```

**Comportement :** Le serveur broadcaste ce message à tous. Chaque client détruit l'avatar du joueur déconnecté.

---

## MOVE

**Direction :** client → serveur → broadcast tous sauf l'expéditeur

**Format :**
```
MOVE|player_id|x|y|z|rotY
```

- `x`, `y`, `z` : position en float, 2 décimales
- `rotY` : rotation sur l'axe Y en degrés, 2 décimales
- Fréquence d'envoi : **20 fois par seconde**

**Exemple :**
```
MOVE|a3f9b2c1|3.25|0.00|7.82|180.00
```

**Comportement :** Le serveur relaie le message à tous les autres clients. Chaque client met à jour la position de l'avatar correspondant.

---

## COLLECT

**Direction :** client → serveur

**Format :**
```
COLLECT|player_id|objet_id
```

**Exemple :**
```
COLLECT|a3f9b2c1|bonus_42
```

**Comportement :** Le serveur vérifie si l'objet est encore disponible (race condition possible si deux joueurs le touchent en même temps). Il répond `COLLECT_OK` au gagnant et `COLLECT_DENIED` au perdant.

---

## COLLECT_OK

**Direction :** serveur → client (gagnant uniquement)

**Format :**
```
COLLECT_OK|player_id|objet_id
```

**Exemple :**
```
COLLECT_OK|a3f9b2c1|bonus_42
```

**Comportement :** Le client ajoute le bonus au score du joueur et détruit l'objet dans la scène.

---

## COLLECT_DENIED

**Direction :** serveur → client (perdant uniquement)

**Format :**
```
COLLECT_DENIED|player_id|objet_id
```

**Exemple :**
```
COLLECT_DENIED|b7d1e4f0|bonus_42
```

**Comportement :** Le client ignore la collecte — l'objet a déjà été pris par un autre joueur.

---

## Résumé

| Message        | Émetteur | Destinataire              |
|----------------|----------|---------------------------|
| CONNECT        | Client   | Tous les autres clients   |
| DISCONNECT     | Client   | Tous les autres clients   |
| MOVE           | Client   | Tous sauf l'expéditeur    |
| COLLECT        | Client   | Serveur                   |
| COLLECT_OK     | Serveur  | Client gagnant            |
| COLLECT_DENIED | Serveur  | Client perdant            |
