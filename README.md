# Jellyfin Ultimate Home UI

[![Jellyfin](https://img.shields.io/badge/Jellyfin-10.11+-00A4DC?logo=jellyfin&logoColor=white)](https://jellyfin.org)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![GitHub Release](https://img.shields.io/github/v/release/CuevasTech/jellyfin-ultimate-home-ui?label=Release)](https://github.com/CuevasTech/jellyfin-ultimate-home-ui/releases)

**Un plugin que transforma la pantalla de inicio de Jellyfin en una experiencia premium estilo Netflix.** Desarrollado por **CuevasTech**.

---

## Qué hace este plugin

Reemplaza por completo la pantalla de inicio de Jellyfin con un layout moderno, ultra-personalizable y responsive que funciona en cualquier dispositivo: navegador, móvil, tablet y Android TV.

### Características

- **Hero Media Slider** — Banner a pantalla completa con autoplay de tráiler en mute, fallback a fanart, degradado con título, sinopsis, rating y botones de Reproducir y Favorito.
- **7 secciones inteligentes** — Continuar viendo (con barra de progreso), Últimas añadidas, Mis favoritos, Porque viste X, Ver otra vez, Joyas ocultas, Sugerencias por franja horaria.
- **Secciones personalizadas** — Crea secciones con filtros avanzados: género, estudio, año, rating, etc.
- **4 tipos de tarjeta** — Poster (2:3), Cuadrado (1:1), Thumb (16:9), Landscape (16:9 grande).
- **Sistema de pesos con empates** — Asigna prioridades numéricas a cada sección; las que comparten el mismo peso se barajan aleatoriamente en cada recarga.
- **Pestañas de navegación** — Crea, reordena y enlaza pestañas a colecciones, géneros, playlists o Mi Lista.
- **100% responsive** — CSS Grid/Flexbox para Desktop, Mobile y Android TV.
- **Soporte Android TV** — Navegación con D-pad, teclado y Gamepad API.

---

## Instalación

1. Abre el **Panel de administración** de Jellyfin.
2. Ve a **Plugins** → **Repositorios** → **Añadir**.
3. Introduce esta URL:

```
https://raw.githubusercontent.com/CuevasTech/jellyfin-ultimate-home-ui/master/manifest.json
```

4. Ve a **Catálogo** y busca **"Ultimate Home UI"**.
5. Haz clic en **Instalar** y **reinicia Jellyfin**.

---

## Configuración

Tras instalar, ve a **Panel de administración** → **Plugins** → **Ultimate Home UI**:

| Opción | Descripción |
|---|---|
| Activar layout | Activa o desactiva el reemplazo de la home |
| Hero Slider | Fuente del contenido (últimas, aleatorio, tendencia) y número de ítems |
| Ocultar visto | Esconde automáticamente el contenido ya reproducido |
| Pesos de secciones | Asigna prioridad numérica a cada sección |
| Estrategia de empates | Qué hacer cuando varias secciones tienen el mismo peso |
| Secciones visibles | Máximo de secciones a mostrar |

---

Desarrollado por **[CuevasTech](https://github.com/CuevasTech)** · Licencia [MIT](LICENSE)
