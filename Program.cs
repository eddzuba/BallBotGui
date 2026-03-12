namespace BallBotGui
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Принудительно заставляем WinForms перехватывать все исключения в UI потоке
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Глобальная обработка ошибок для UI потока
            Application.ThreadException += (sender, e) =>
            {
                Logger.Log("UI Thread Exception", e.Exception);
                MessageBox.Show($"Произошла ошибка в интерфейсе: {e.Exception.Message}\n\nОшибка записана в log.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            // Глобальная обработка ошибок для фоновых потоков
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                Logger.Log("Fatal Background Exception", ex);
                MessageBox.Show($"Произошла критическая ошибка в фоновом потоке: {ex?.Message}\n\nПриложение может работать нестабильно. Ошибка записана в log.", "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            Application.Run(new Form1());
        }
    }
}