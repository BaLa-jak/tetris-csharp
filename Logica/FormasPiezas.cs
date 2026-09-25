using OpenTK.Mathematics;

namespace Tetris2D.Logica
{
    /// <summary>
    /// Catalogo de formas de las piezas.
    ///
    /// Cada pieza se define una sola vez en su rotacion inicial, dentro de una
    /// caja cuadrada (2x2 para la O, 4x4 para la I y 3x3 para las demas).
    /// Las otras 3 rotaciones se calculan girando esa caja 90° en sentido
    /// horario: la celda (columna, fila) pasa a (lado - 1 - fila, columna).
    ///
    /// Las celdas usan X = columna y Y = fila (la fila crece hacia abajo).
    /// </summary>
    public static class FormasPiezas
    {
        /// <summary>Numero de rotaciones distintas de cada pieza.</summary>
        public const int Rotaciones = 4;

        // [tipo][rotacion] -> las 4 celdas de la pieza dentro de su caja.
        private static readonly Vector2i[][][] _formas = CrearFormas();

        /// <summary>Celdas de la pieza en la rotacion indicada (0..3).</summary>
        public static IReadOnlyList<Vector2i> Celdas(TipoPieza tipo, int rotacion)
        {
            return _formas[(int)tipo][NormalizarRotacion(rotacion)];
        }

        /// <summary>Lado de la caja cuadrada que contiene a la pieza.</summary>
        public static int LadoCaja(TipoPieza tipo)
        {
            return tipo switch
            {
                TipoPieza.I => 4,
                TipoPieza.O => 2,
                _ => 3
            };
        }

        /// <summary>
        /// Esquinas minima y maxima (inclusivas) de las celdas ocupadas.
        /// Sirve para centrar la pieza en la vista previa y para colocarla
        /// pegada al borde superior del tablero al aparecer.
        /// </summary>
        public static (Vector2i Minimo, Vector2i Maximo) Limites(TipoPieza tipo, int rotacion)
        {
            IReadOnlyList<Vector2i> celdas = Celdas(tipo, rotacion);
            Vector2i minimo = celdas[0];
            Vector2i maximo = celdas[0];
            foreach (Vector2i c in celdas)
            {
                minimo = Vector2i.ComponentMin(minimo, c);
                maximo = Vector2i.ComponentMax(maximo, c);
            }
            return (minimo, maximo);
        }

        /// <summary>Lleva cualquier entero (incluso negativo) al rango 0..3.</summary>
        public static int NormalizarRotacion(int rotacion)
        {
            return ((rotacion % Rotaciones) + Rotaciones) % Rotaciones;
        }

        private static Vector2i[][][] CrearFormas()
        {
            TipoPieza[] tipos = Enum.GetValues<TipoPieza>();
            var formas = new Vector2i[tipos.Length][][];

            foreach (TipoPieza tipo in tipos)
            {
                int lado = LadoCaja(tipo);
                var rotaciones = new Vector2i[Rotaciones][];
                rotaciones[0] = FormaInicial(tipo);

                for (int r = 1; r < Rotaciones; r++)
                {
                    rotaciones[r] = rotaciones[r - 1]
                        .Select(c => new Vector2i(lado - 1 - c.Y, c.X))
                        .ToArray();
                }
                formas[(int)tipo] = rotaciones;
            }
            return formas;
        }

        /// <summary>Forma de cada pieza en su rotacion inicial (columna, fila).</summary>
        private static Vector2i[] FormaInicial(TipoPieza tipo)
        {
            return tipo switch
            {
                //  ....
                //  ####
                TipoPieza.I => new Vector2i[] { new(0, 1), new(1, 1), new(2, 1), new(3, 1) },
                //  ##
                //  ##
                TipoPieza.O => new Vector2i[] { new(0, 0), new(1, 0), new(0, 1), new(1, 1) },
                //  .#.
                //  ###
                TipoPieza.T => new Vector2i[] { new(1, 0), new(0, 1), new(1, 1), new(2, 1) },
                //  .##
                //  ##.
                TipoPieza.S => new Vector2i[] { new(1, 0), new(2, 0), new(0, 1), new(1, 1) },
                //  ##.
                //  .##
                TipoPieza.Z => new Vector2i[] { new(0, 0), new(1, 0), new(1, 1), new(2, 1) },
                //  #..
                //  ###
                TipoPieza.J => new Vector2i[] { new(0, 0), new(0, 1), new(1, 1), new(2, 1) },
                //  ..#
                //  ###
                TipoPieza.L => new Vector2i[] { new(2, 0), new(0, 1), new(1, 1), new(2, 1) },
                _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null)
            };
        }
    }
}
