# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Tetris 2D in C# (.NET 8) with OpenTK 4 (OpenGL 4 / GLSL 330) and SkiaSharp, built for a Computer Graphics class. All identifiers, comments, and UI text are in **Spanish** — keep new code consistent (e.g. `Renderizar`, `Actualizar`, `Pieza`, `Tablero`). Doc comments are Spanish `/// <summary>` blocks, mostly written without accents.

## Commands

```bash
dotnet build      # compile
dotnet run        # build and launch the game window (800x600)
```

There is no test project and no linter configured. `PartidaTetris` and `GeneradorPiezas` accept an optional `Random` so a seeded instance can be used if tests are added.

## Architecture

Layers with one-way dependencies: `Pantallas → UI / Graficos → Logica`.

- **Logica/** — pure game rules, no OpenGL or UI. `PartidaTetris` is the engine (gravity, scoring, combos, skips, pause, game over). `Tablero` is the 10×18 grid (row 0 = top). `Pieza` is **immutable**: `Movida`/`Rotada` return new pieces that are tested with `Tablero.Cabe` before being accepted. Rotation uses a simplified wall-kick list in `PartidaTetris.DesplazamientosRotacion`. Piece order comes from a 7-bag in `GeneradorPiezas`. Scoring events are published through the `PuntosObtenidos` event (`EventoPuntuacion`).
- **Graficos/** — the only layer that talks to OpenGL. `GestorShader` compiles two inline GLSL programs (solid color and alpha-textured text). `DibujadorCuadros` draws each rectangle/line right away through one dynamic VBO. `DibujadorBloques` builds the beveled Tetris blocks on top of it.
- **UI/** — visual components that receive read-only data (`ITableroLectura`, `DatosMarcador`, `DescripcionControl`) and draw it; they never change game state. Text rendering: `GeneradorFuenteAtlas` rasterizes a fixed character set with SkiaSharp into a GL texture at startup, and `RenderizadorTexto` draws quads from it. Characters missing from `GeneradorFuenteAtlas.Caracteres` won't render. All colors, including per-piece colors, live in `TemaArcade`.
- **Pantallas/** — controllers. `Pantalla` is the base class. Screens never switch themselves: they raise `JugarSolicitado`/`MenuSolicitado`, and `TetrisGame.CambiarPantalla` swaps in the new screen. `PantallaInicio` (Portada → Nombre) shows a start message chosen by `Logica/MensajesBienvenida` that never repeats the previous one; `Logica/RegistroMensajes` saves the last index in `ApplicationData/Tetris2D/ultimo_mensaje.txt` so this holds across launches. `PantallaJuego` has an internal state machine (Controles → Jugando → Terminado), reads `PartidaTetris`, and passes data to the UI components. Layout is computed every frame, proportional to the framebuffer size.
- **`MapaControles`** is the single source of truth for key bindings: each entry pairs the on-screen description with key→action lambdas. Change a key here and the help panels update automatically.

### Rendering conventions

- `TetrisGame` owns the shared `GestorShader`, `DibujadorCuadros`, and `RenderizadorTexto` (created once in `OnLoad`, disposed in `OnUnload`) and passes them to each screen.
- Coordinates are **framebuffer pixels with Y pointing down**: an orthographic projection is set on both shader programs every frame. Mouse positions are scaled from window to framebuffer space for HiDPI (`PuntoRaton`), so use `FramebufferSize`, not `Size`, when doing layout.
- Holding a key relies on OpenTK's key-repeat `OnKeyDown` events for continuous movement; there is no separate input polling.
