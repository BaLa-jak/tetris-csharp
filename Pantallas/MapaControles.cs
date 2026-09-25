using OpenTK.Windowing.GraphicsLibraryFramework;
using Tetris2D.Logica;
using Tetris2D.UI;

namespace Tetris2D.Pantallas
{
  /// <summary>
  /// Unica definicion de los controles del juego: que teclas hay, como se
  /// muestran en pantalla y que accion aplican sobre la partida.
  /// Cambiar una tecla aqui actualiza tambien los paneles de ayuda.
  /// </summary>
  public static class MapaControles
  {
    /// <summary>
    /// Un control tal como se muestra en pantalla y la accion de cada una
    /// de sus teclas (ej. IZQ / DER comparten texto pero mueven distinto).
    /// </summary>
    private sealed record Control(DescripcionControl Descripcion, Dictionary<Keys, Action<PartidaTetris>> Acciones);

    private static readonly Control[] _controles = {
            new(new("IZQ / DER", "Mover pieza"), new()
            {
                [Keys.Left] = p => p.MoverIzquierda(),
                [Keys.Right] = p => p.MoverDerecha()
            }),
            new(new("ARRIBA / X", "Rotar a la derecha"), new()
            {
                [Keys.Up] = p => p.Rotar(+1),
                [Keys.X] = p => p.Rotar(+1)
            }),
            new(new("Z", "Rotar a la izquierda"), new() { [Keys.Z] = p => p.Rotar(-1) }),
            new(new("ABAJO", "Bajar más rápido"), new() { [Keys.Down] = p => p.BajarUnPaso() }),
            new(new("ESPACIO", "Soltar pieza"), new() { [Keys.Space] = p => p.CaidaRapida() }),
            new(new("C", $"Saltar pieza (máx. {PartidaTetris.MaxSkips})"), new() { [Keys.C] = p => p.Saltar() }),
            new(new("P / ESC", "Pausa"), new()
            {
                [Keys.P] = p => p.AlternarPausa(),
                [Keys.Escape] = p => p.AlternarPausa()
            })
        };

    /// <summary>Textos de los controles, en el orden en que se muestran.</summary>
    public static IReadOnlyList<DescripcionControl> Descripciones { get; } =
        _controles.Select(c => c.Descripcion).ToArray();

    /// <summary>
    /// Aplica sobre la partida la accion de la tecla presionada.
    /// Devuelve false si la tecla no pertenece a ningun control.
    /// </summary>
    public static bool Ejecutar(Keys tecla, PartidaTetris partida)
    {
      foreach (Control control in _controles)
      {
        if (control.Acciones.TryGetValue(tecla, out Action<PartidaTetris>? accion))
        {
          accion(partida);
          return true;
        }
      }
      return false;
    }
  }
}
