namespace Tetris2D.Logica
{
    /// <summary>
    /// Mensajes que la pantalla de inicio muestra al abrir el juego y como
    /// se elige uno. Solo usan caracteres del atlas de texto (acentos, ñ, ¡ y ¿
    /// si; emojis no).
    /// </summary>
    public static class MensajesBienvenida
    {
        public static IReadOnlyList<string> Todos { get; } = new[]
        {
            "¡Hoy es un gran día para hacer un TETRIS!",
            "La pieza I siempre llega... eventualmente.",
            "Cuatro filas de un golpe valen 800 puntos. ¡Ve por ellas!",
            "Encadena piezas que borren filas y sube el combo hasta x5.",
            "¿Sin salida? Tienes 3 saltos por partida con la tecla C.",
            "La pieza fantasma te dice donde va a caer. ¡Úsala!",
            "Cada 10 líneas sube el nivel y las piezas caen más rápido.",
            "Deja una columna libre a un lado y espera la pieza I.",
            "Soltar con ESPACIO da 2 puntos por cada fila que baja.",
            "Un tablero plano es un tablero feliz.",
            "Respira hondo: las piezas no se acomodan solas.",
            "Si una pieza no gira junto a la pared, prueba del otro lado.",
            "Las S y las Z son difíciles de acomodar. Planea antes de soltarlas.",
            "¡Bienvenido de vuelta a la sala de máquinas!"
        };

        /// <summary>
        /// Elige al azar el indice de un mensaje distinto de "anterior".
        /// Se sortea entre Count - 1 opciones y se salta el anterior, asi nunca
        /// sale dos veces seguidas el mismo.
        /// </summary>
        /// <param name="anterior">Indice mostrado la ultima vez (null = ninguno).</param>
        /// <param name="azar">Generador aleatorio; se puede fijar la semilla para pruebas.</param>
        public static int ElegirIndice(int? anterior, Random azar)
        {
            bool anteriorValido = anterior is int a && a >= 0 && a < Todos.Count;
            if (!anteriorValido || Todos.Count < 2)
                return azar.Next(Todos.Count);

            int indice = azar.Next(Todos.Count - 1);
            return indice >= anterior!.Value ? indice + 1 : indice;
        }
    }
}
