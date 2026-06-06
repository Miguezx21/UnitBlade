# ⚔️ Unit Blade

Metroidvania-lite 2D de pixel art ambientado en la mitología vikinga. Kaelen, joven
guardián de la forja, debe recuperar las cuatro **runas elementales** (Pira, Isa,
Steinn y Thron) para reforjar la **Unit Blade** y derrotar al hechicero **Morgath**.

- **Motor:** Unity 6.3 LTS (6000.3.16f1) · URP · 2D
- **Entrada:** Input System (paquete nuevo)
- **Asignatura:** ISWZ3411 — UDLA
- **Autores:** Miguel Peña · Carlos Carvajal

---

## 🎮 Controles

| Acción | Tecla |
|--------|-------|
| Moverse | `A` / `D` o `←` `→` |
| Saltar | `Espacio` / `W` / `↑` |
| Atacar | `J` o clic izquierdo |
| Cambiar runa (elemento) | `1` Pira · `2` Isa · `3` Steinn · `4` Thron |
| Ciclar elemento | `Q` |
| Reintentar (Game Over) | `Enter` / `Espacio` o botón |

**Teclas de prueba (dev):** `0` desbloquea todas las runas · `H` recibe daño ·
`G` cura · `R` reinicia runas.

---

## 🧩 Mecánicas

- **Runas / elementos:** empiezas con **Isa**. Las demás se desbloquean recogiendo
  su runa, que aparece al **derrotar a un tipo de enemigo**.
- **Counter elemental:** cada enemigo tiene una debilidad; atacar con el elemento
  correcto hace **daño triple**. Ejemplos: Ignis (fuego) ⇐ Isa · Glacius (hielo) ⇐ Pira.
- **Ataque elemental:** la espada cambia de color según la runa equipada.
- **Vidas:** 3 corazones. Al perderlos todos → Game Over.
- **Caída al vacío:** caer bajo el límite resta una vida y reaparece en el SpawnPoint.
- **Reintentar:** al reintentar se restauran las runas al estado del inicio del nivel.

---

## 🗂️ Estructura

```
Assets/
├─ Art/                Sprites, fondos, tilesets, personajes
├─ Animations/         AnimatorControllers y clips (Kaelen, enemigos, Morgath)
├─ Editor/             Herramientas de editor (menús Tools/UnitBlade)
├─ Resources/HUD/      Sprites de runas y corazón
├─ Scenes/
│  ├─ MainMenu.unity   Menú principal
│  ├─ Levels/          Level_01_Castle, Level_02_Forest
│  └─ Boss/            Boss_Morgath
├─ Scripts/            Lógica de juego
└─ Sounds/             Efectos de sonido (.mp3)
```

### Scripts principales
| Script | Función |
|--------|---------|
| `PlayerController2D` | Movimiento, salto, ataque, muerte y caída |
| `PlayerAnimator` | Sincroniza parámetros del Animator (robusto ante params faltantes) |
| `PlayerAttack` | Daño melee + SFX por elemento |
| `PlayerStats` | Vidas, elemento activo, desbloqueos |
| `GameProgress` | Runas recolectadas (snapshot por nivel) |
| `Health` | Vida de enemigos + counter elemental |
| `RuneRevealTracker` | Oculta la runa hasta matar a su grupo de enemigos |
| `RunePickup` | Recoge la runa al tocarla |
| `HUDManager` | HUD de corazones, elemento y runas |
| `MainMenuController` | Menú (Jugar / Runas / Instrucciones / Historia) |
| `AudioManager` | Música y efectos, persiste entre escenas |
| `GameOverManager` | Pantalla de Game Over (Reintentar / Menú) |

---

## 🛠️ Herramientas de editor (menú `Tools → UnitBlade`)

| Herramienta | Qué hace |
|-------------|----------|
| **Configurar Kaelen (Aseprite)** | Corta el `.ase` en clips y arma el AnimatorController de Kaelen. Ejecutar en cada escena con Kaelen. |
| **Crear Escena Menu Principal** | Crea `MainMenu.unity` y la registra como escena 0. |
| **Menu: Construir-Editar en Escena** | Construye el menú como objetos editables (cambiar fondo/runas). |
| **Menu: Limpiar** | Borra el menú de la escena (se rehace en runtime). |
| **Crear HUD en Escena** | Genera el HUD editable con sprites. |
| **Configurar Runas por Tipo** | Asigna cada tipo de enemigo a la runa más cercana. |

---

## ▶️ Cómo ejecutar

1. Abrir el proyecto con **Unity 6.3 LTS (6000.3.16f1)**.
2. Abrir la escena **`Assets/Scenes/MainMenu.unity`**.
3. Pulsar **Play** y darle a **JUGAR**.

> Juega siempre **desde MainMenu** para que el `AudioManager` (con la música/SFX)
> persista entre niveles.

### Si tocas sprites/animaciones
- Tras importar `KAELENfinal.ase`: ejecutar **Tools → UnitBlade → Configurar Kaelen**
  en cada escena con Kaelen.
- Tras mover/añadir enemigos: ejecutar **Tools → UnitBlade → Configurar Runas por Tipo**
  y guardar (Ctrl+S).

---

## 🎵 Audio

Los efectos viven en `Assets/Sounds/`. Para la música, asigna las pistas al objeto
**AudioManager** de la escena MainMenu (campos *Menu / Level / Boss Music*).

| Sonido | Uso |
|--------|-----|
| Sword | Ataque básico |
| Roblox jump | Salto |
| Fire Explosion | Ataque Pira |
| Ice Crack | Ataque Isa |
| Rocks and Stones | Ataque Steinn |
| Lightning | Ataque Thron |

---

## 🗺️ Niveles

1. **Level 01 — Castillo:** Ignis (débil a Isa) y Glacius (débil a Pira).
2. **Level 02 — Bosque:** enemigos Voltox/Glacius; runa Thron.
3. **Boss — Morgath:** combate final por fases.

---

## 📦 Repositorio

- **GitHub:** https://github.com/Miguezx21/UnitBlade.git (rama `main`)
- Binarios grandes gestionados con **Git LFS**.

🤖 Desarrollado con apoyo de [Claude Code](https://claude.com/claude-code)
