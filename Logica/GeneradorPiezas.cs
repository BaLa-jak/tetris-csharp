namespace Tetris2D.Logica
{
    /// <summary>
    /// Genera la secuencia de piezas con el sistema de "bolsa de 7": se mete
    /// una pieza de cada tipo en una bolsa, se revuelven y se van sacando.
    /// Al vaciarse se llena otra bolsa. Asi nunca pasan demasiadas piezas
    /// sin que salga alguna en particular (por ejemplo, la I).
    /// </summary>
    public class GeneradorPiezas
    {
        private readonly Random _azar;
        private readonly Queue<TipoPieza> _cola = new();

        /// <param name="azar">Generador aleatorio; se puede fijar la semilla para pruebas.</param>
        public GeneradorPiezas(Random? azar = null)
        {
            _azar = azar ?? new Random();
            RellenarSiHaceFalta();
        }

        /// <summary>Tipo de la proxima pieza, sin sacarla de la cola.</summary>
        public TipoPieza Siguiente => _cola.Peek();

        /// <summary>Saca la proxima pieza de la cola.</summary>
        public TipoPieza Tomar()
        {
            TipoPieza tipo = _cola.Dequeue();
            RellenarSiHaceFalta();
            return tipo;
        }

        /// <summary>Si la cola quedo vacia, agrega una bolsa nueva revuelta.</summary>
        private void RellenarSiHaceFalta()
        {
            if (_cola.Count > 0)
                return;

            TipoPieza[] bolsa = Enum.GetValues<TipoPieza>();
            _azar.Shuffle(bolsa);
            foreach (TipoPieza tipo in bolsa)
                _cola.Enqueue(tipo);
        }
    }
}
