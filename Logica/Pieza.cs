using OpenTK.Mathematics;

namespace Tetris2D.Logica
{
    /// <summary>
    /// Pieza en juego: su tipo, su rotacion y la posicion de su caja dentro
    /// del tablero (columna y fila de la esquina superior izquierda).
    ///
    /// Es inmutable: mover o rotar devuelve una pieza nueva. Asi se puede
    /// "probar" un movimiento contra el tablero y descartarlo si no cabe.
    /// </summary>
    public sealed class Pieza
    {
        public TipoPieza Tipo { get; }
        public int Rotacion { get; }
        public int Columna { get; }
        public int Fila { get; }

        public Pieza(TipoPieza tipo, int rotacion, int columna, int fila)
        {
            Tipo = tipo;
            Rotacion = FormasPiezas.NormalizarRotacion(rotacion);
            Columna = columna;
            Fila = fila;
        }

        /// <summary>
        /// Crea la pieza centrada horizontalmente y con su primera fila de
        /// bloques pegada al borde superior del tablero.
        /// </summary>
        public static Pieza CrearInicial(TipoPieza tipo, int columnasTablero)
        {
            int columna = (columnasTablero - FormasPiezas.LadoCaja(tipo)) / 2;
            int fila = -FormasPiezas.Limites(tipo, 0).Minimo.Y;
            return new Pieza(tipo, 0, columna, fila);
        }

        /// <summary>Posiciones absolutas (columna, fila) de los 4 bloques en el tablero.</summary>
        public IEnumerable<Vector2i> Celdas()
        {
            foreach (Vector2i c in FormasPiezas.Celdas(Tipo, Rotacion))
                yield return new Vector2i(Columna + c.X, Fila + c.Y);
        }

        /// <summary>Copia desplazada dx columnas y dy filas.</summary>
        public Pieza Movida(int dx, int dy)
        {
            return new Pieza(Tipo, Rotacion, Columna + dx, Fila + dy);
        }

        /// <summary>Copia girada 90°: sentido = +1 horario, -1 antihorario.</summary>
        public Pieza Rotada(int sentido)
        {
            return new Pieza(Tipo, Rotacion + sentido, Columna, Fila);
        }
    }
}
