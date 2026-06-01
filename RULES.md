# Règles du projet mmporg-metaverse

## Workflow Git

Une branche par feature, personne ne travaille directement sur `main` :

```
main                    ← code stable uniquement
├── feature/reseau-serveur
├── feature/network-manager
├── feature/remote-players
└── feature/chat
```

Quand une feature est finie → **Pull Request** → un coéquipier relit → merge sur `main`.

## Règles d'or

- Ne jamais pusher directement sur `main`
- Commiter souvent avec des messages clairs : `"Ajout broadcast positions"` pas `"update"`
- Toujours `git pull` avant de commencer à coder

## Créer une branche

```bash
git checkout -b feature/ma-feature
```

## Pousser sa branche

```bash
git push origin feature/ma-feature
```

## Ouvrir une Pull Request

Sur GitHub → **New Pull Request** → choisir sa branche → demander une review à un coéquipier.
