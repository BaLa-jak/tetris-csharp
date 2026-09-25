namespace Tetris2D.Logica
{
    /// <summary>
    /// Recuerda en disco el ultimo mensaje de bienvenida que se mostro, para
    /// que al volver a abrir el juego salga uno diferente.
    ///
    /// El indice se guarda en la carpeta de datos del usuario
    /// (%AppData%/Tetris2D en Windows, ~/Library/Application Support/Tetris2D
    /// en macOS, ~/.config/Tetris2D en Linux). Si el archivo no se puede leer
    /// o escribir, el juego sigue normal; solo puede repetirse el mensaje.
    /// </summary>
    public static class RegistroMensajes
    {
        private static readonly string Archivo = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Tetris2D", "ultimo_mensaje.txt");

        /// <summary>
        /// Elige un mensaje distinto al ultimo mostrado, lo guarda como el
        /// nuevo "ultimo" y devuelve su texto.
        /// </summary>
        /// <param name="azar">Generador aleatorio; se puede fijar la semilla para pruebas.</param>
        public static string SiguienteMensaje(Random? azar = null)
        {
            int indice = MensajesBienvenida.ElegirIndice(LeerUltimo(), azar ?? new Random());
            GuardarUltimo(indice);
            return MensajesBienvenida.Todos[indice];
        }

        /// <summary>Indice guardado la ultima vez, o null si no hay (o no se pudo leer).</summary>
        public static int? LeerUltimo()
        {
            try
            {
                if (File.Exists(Archivo) && int.TryParse(File.ReadAllText(Archivo).Trim(), out int indice))
                    return indice;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Sin registro: se elige cualquier mensaje.
            }
            return null;
        }

        /// <summary>Guarda el indice del mensaje mostrado (crea la carpeta si hace falta).</summary>
        public static void GuardarUltimo(int indice)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Archivo)!);
                File.WriteAllText(Archivo, indice.ToString());
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // No es grave: la proxima vez podria repetirse el mensaje.
            }
        }
    }
}
