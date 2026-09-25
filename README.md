# Tetris2D

Base del juego **Tetris 2D** hecha con **C# y OpenTK** (OpenGL 4) para la clase de
Graficación por Computadora.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows, macOS o Linux (el atlas de texto se genera con SkiaSharp)

## Compilar y ejecutar

```bash
dotnet build
dotnet run
```

## Cómo jugar

1. En la pantalla de inicio aparece un mensaje distinto cada vez que abres el juego.
   Haz clic en **INICIAR** (o presiona **Enter**).
2. Escribe tu nombre y presiona **Enter** o haz clic en **JUGAR** (**VOLVER** / Esc regresa a la portada).
3. Antes de empezar se muestran los controles unos segundos (Enter para saltarlos).
4. Arriba del tablero se ve la **siguiente pieza**.

| Tecla        | Acción                          |
|--------------|---------------------------------|
| Izq / Der    | Mover pieza                     |
| Arriba / X   | Rotar a la derecha              |
| Z            | Rotar a la izquierda            |
| Abajo        | Bajar más rápido (+1 punto)     |
| Espacio      | Soltar pieza (+2 puntos/fila)   |
| C            | Saltar pieza (máximo 3 por partida) |
| P / Esc      | Pausa                           |

**Puntos:** 1 fila = 100, 2 = 300, 3 = 500, 4 = 800, multiplicado por el nivel.
**Combos:** cada pieza seguida que borra filas sube el multiplicador (x2, x3… hasta x5);
se anuncia con un texto animado debajo del tablero. El nivel sube cada 10 líneas.

## Estructura del proyecto

El código está separado en capas; las dependencias van en un solo sentido:
`Pantallas → UI / Graficos → Logica`.

- **Logica/**: reglas del juego. No dibuja nada y no conoce la interfaz.
- **Graficos/**: lo único que usa OpenGL (shaders y dibujo de figuras y bloques).
- **UI/**: componentes visuales. Reciben datos de solo lectura y los dibujan.
- **Pantallas/**: controladores. Leen la lógica, pasan datos a la UI y traducen el teclado.

```
Program.cs                         Punto de entrada (crea la ventana y arranca el bucle)
TetrisGame.cs                      Ventana principal: loop, cámara y cambio de pantallas
Logica/TipoPieza.cs                Las 7 piezas (I, O, T, S, Z, J, L)
Logica/FormasPiezas.cs             Forma y rotaciones de cada pieza
Logica/Pieza.cs                    Pieza en juego (posición y rotación, inmutable)
Logica/GeneradorPiezas.cs          Secuencia aleatoria con "bolsa de 7"
Logica/Tablero.cs                  Cuadrícula 10×18, colisiones y borrado de filas
Logica/ITableroLectura.cs          Vista de solo lectura del tablero para las vistas
Logica/PartidaTetris.cs            Reglas: gravedad, puntos, combos, skips, pausa y fin
Logica/EventoPuntuacion.cs         Datos de una jugada que borró filas
Logica/MensajesBienvenida.cs       Mensajes de inicio y elección sin repetir el anterior
Logica/RegistroMensajes.cs         Guarda el último mensaje mostrado (carpeta AppData/Tetris2D)
Graficos/GestorShader.cs           Compila los shaders GLSL (forma sólida y texto)
Graficos/DibujadorCuadros.cs       Dibuja rectángulos, bordes y líneas
Graficos/DibujadorBloques.cs       Dibuja bloques con relieve y piezas completas
UI/GeneradorFuenteAtlas.cs         Genera el atlas de letras con SkiaSharp → textura GL
UI/RenderizadorTexto.cs            Dibuja texto en pantalla
UI/TemaArcade.cs                   Paleta de colores neón arcade (incluye color de cada pieza)
UI/Boton.cs                        Botón interactivo (hover y desactivado)
UI/CuadroTexto.cs                  Caja de entrada del nombre
UI/FondoEstrellas.cs               Fondo con estrellas compartido por las pantallas
UI/VistaTablero.cs                 Tablero, bloques fijos, pieza actual y fantasma
UI/PanelSiguientePieza.cs          Vista previa de la siguiente pieza
UI/MarcadorPuntos.cs               Puntos, nivel, líneas, combo y skips (DatosMarcador)
UI/PanelAyuda.cs                   Resumen de controles durante la partida
UI/PanelControles.cs               Controles y cuenta regresiva antes de jugar
UI/DescripcionControl.cs           Texto de un control (tecla y acción)
UI/TextoAnimado.cs                 Texto animado para combos y multiplicadores
UI/PanelMensaje.cs                 Panel de pausa y fin del juego
UI/TituloNeon.cs                   Título con resplandor neón
UI/TarjetaMensaje.cs               Panel del mensaje del día (con ajuste de líneas)
Pantallas/Pantalla.cs              Clase base de las pantallas
Pantallas/PantallaInicio.cs        Pantalla de inicio: portada con mensaje → pedir nombre
Pantallas/PantallaJuego.cs         Controlador del juego (une la lógica con los componentes)
Pantallas/MapaControles.cs         Única definición de teclas, textos y acciones
```
