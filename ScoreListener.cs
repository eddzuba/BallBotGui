using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BallBotGui
{
    internal class ScoreListener
    {
        private readonly StateManager _stateManager;
        private readonly int _port;
        private TcpListener? _listener;
        private bool _isRunning;

        public ScoreListener(StateManager stateManager, int port)
        {
            _stateManager = stateManager;
            _port = port;
        }

        /// <summary>Вызывается после каждого успешного обновления счёта. Подписчик отвечает за маршал в UI поток.</summary>
        public Action? OnScoreUpdated { get; set; }

        /// <summary>Вызывается ПРИ КАЖДОМ входящем сообщении (сырой JSON), полезно для отладки.</summary>
        public Action<string>? OnRawMessageReceived { get; set; }

        public void Start()
        {
            if (_isRunning) return;

            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                _isRunning = true;

                Task.Run(() => ListenAsync());
                Logger.Log($"ScoreListener started on port {_port}. Listening for POST /score/");
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to start ScoreListener on port {_port}", ex);
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
        }

        private async Task ListenAsync()
        {
            while (_isRunning)
            {
                try
                {
                    if (_listener == null) break;
                    var client = await _listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClientAsync(client));
                }
                catch (SocketException) when (!_isRunning)
                {
                    // Нормальное завершение при остановке
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                        Logger.Log("Error in ScoreListener loop", ex);
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            {
                try
                {
                    using var stream = client.GetStream();
                    using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
                    // Используем UTF8 без BOM для совместимости с протоколом HTTP
                    var encodingBuffer = new UTF8Encoding(false);
                    using var writer = new StreamWriter(stream, encodingBuffer, leaveOpen: true) { AutoFlush = true };

                    // Устанавливаем таймаут на чтение (10 секунд)
                    client.ReceiveTimeout = 10000;

                    // Читаем первую строку (Request Line)
                    string? requestLine = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(requestLine)) return;

                    // Обработка CORS Preflight (OPTIONS)
                    if (requestLine.StartsWith("OPTIONS", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Log("Received OPTIONS request (CORS Preflight)");
                        await writer.WriteAsync(
                            "HTTP/1.1 204 No Content\r\n" +
                            "Access-Control-Allow-Origin: *\r\n" +
                            "Access-Control-Allow-Methods: POST, OPTIONS\r\n" +
                            "Access-Control-Allow-Headers: Content-Type\r\n" +
                            "Connection: close\r\n" +
                            "\r\n");
                        return;
                    }

                    if (!requestLine.Contains("POST"))
                    {
                        Logger.Log("Received non-POST or empty request: " + requestLine);
                        await writer.WriteAsync(
                            "HTTP/1.1 405 Method Not Allowed\r\n" +
                            "Access-Control-Allow-Origin: *\r\n" +
                            "Content-Length: 0\r\n" +
                            "Connection: close\r\n\r\n");
                        return;
                    }

                    // Читаем HTTP-заголовки
                    int contentLength = 0;
                    string? line;
                    while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
                    {
                        if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                        {
                            int.TryParse(line.Substring("Content-Length:".Length).Trim(), out contentLength);
                        }
                    }

                    // Ограничиваем тело запроса (например, 50 КБ), чтобы защититься от атак/переполнения
                    if (contentLength <= 0 || contentLength > 51200)
                    {
                        Logger.Log("Invalid or too large Content-Length received: " + contentLength);
                        await writer.WriteAsync("HTTP/1.1 413 Payload Too Large\r\nAccess-Control-Allow-Origin: *\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
                        return;
                    }

                    // Читаем тело запроса
                    string json = "";
                    var buffer = new char[contentLength];
                    int read = await reader.ReadAsync(buffer, 0, contentLength);
                    json = new string(buffer, 0, read);

                    // Уведомляем о сыром сообщении (для отладки)
                    OnRawMessageReceived?.Invoke(json);

                    // Обрабатываем JSON
                    ScoreUpdateDto? scoreData = null;
                    string responseBody = "OK";

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            scoreData = JsonConvert.DeserializeObject<ScoreUpdateDto>(json);
                        }
                    }
                    catch (JsonException ex)
                    {
                        Logger.Log("Invalid JSON received for score update: " + json, ex);
                        responseBody = "Invalid JSON";
                    }

                    if (scoreData != null)
                    {
                        UpdateScore(scoreData);
                    }
                    else if (string.IsNullOrEmpty(responseBody) || responseBody == "OK")
                    {
                        responseBody = "Empty or Invalid Data";
                        Logger.Log("Received empty or unusable score update data.");
                    }

                    // Отправляем HTTP-ответ
                    await writer.WriteAsync(
                        $"HTTP/1.1 200 OK\r\n" +
                        $"Access-Control-Allow-Origin: *\r\n" +
                        $"Content-Type: text/plain; charset=utf-8\r\n" +
                        $"Content-Length: {encodingBuffer.GetByteCount(responseBody)}\r\n" +
                        $"Connection: close\r\n" +
                        $"\r\n" +
                        responseBody);
                }
                catch (Exception ex)
                {
                    Logger.Log("Error handling score client", ex);
                }
            }
        }

        private void UpdateScore(ScoreUpdateDto scoreData)
        {
            long adminId = _stateManager.AdminId;

            // Санитизация: счёт не может быть отрицательным (здравый смысл)
            if (scoreData.Team1Score < 0 || scoreData.Team2Score < 0)
            {
                Logger.Log($"Ignoring score update with negative values: {scoreData.Team1Score}-{scoreData.Team2Score}");
                return;
            }

            // Сначала пытаемся найти опрос, в котором играет админ (важно при двух играх в один день).
            // Если админ не найден ни в одном опросе — берём ближайший по времени.
            var currentPoll = _stateManager.GetClosestApprovedPollForToday(adminId)
                              ?? _stateManager.GetClosestApprovedPollForToday();

            // Если пришло 0:0 — это сброс или начало новой партии.
            if (scoreData.Team1Score == 0 && scoreData.Team2Score == 0)
            {
                if (currentPoll != null && currentPoll.GameScores.Any())
                {
                    // Находим индекс последнего завершенного сета
                    int lastFinishedIdx = currentPoll.GameScores.FindLastIndex(gs => gs.IsFinished);
                    
                    if (lastFinishedIdx == -1)
                    {
                        // Стов еще не было, очищаем всю историю
                        currentPoll.GameScores.Clear();
                    }
                    else
                    {
                        // Удаляем всё, что было после последнего завершенного сета
                        int countToRemove = currentPoll.GameScores.Count - (lastFinishedIdx + 1);
                        if (countToRemove > 0)
                        {
                            currentPoll.GameScores.RemoveRange(lastFinishedIdx + 1, countToRemove);
                        }
                    }

                    Logger.Log($"Score reset to 0:0 for poll {currentPoll.idPoll}. Current set history cleared.");
                    _stateManager.SaveState();
                    OnScoreUpdated?.Invoke();
                }
                return;
            }

            // Определяем, какая команда (в терминах опроса) соответствует Team1Score и Team2Score на табло.
            // ...
            int score1 = scoreData.Team1Score;
            int score2 = scoreData.Team2Score;

            if (currentPoll != null)
            {
                var lastComposition = currentPoll.TeamCompositions
                    .Where(tc => tc.Timestamp.Date == DateTime.Today)
                    .OrderByDescending(tc => tc.Timestamp)
                    .FirstOrDefault();

                if (lastComposition != null)
                {
                    int adminTeamIndex = -1;
                    for (int i = 0; i < lastComposition.Teams.Count; i++)
                    {
                        if (lastComposition.Teams[i].Contains(adminId))
                        {
                            adminTeamIndex = i;
                            break;
                        }
                    }

                    // Если Team1Score всегда счёт моей команды:
                    // Если я во второй команде (индекс 1, желтые), то мой счёт (Team1Score) должен пойти в score2.
                    if (adminTeamIndex == 1)
                    {
                        score1 = scoreData.Team2Score; // оппонент
                        score2 = scoreData.Team1Score; // я
                    }
                    // Если я в первой команде (индекс 0, зеленые) или индекс не найден, оставляем как есть.
                }

                var newScore = new GameScore(score1, score2, scoreData.IsFinished);
                currentPoll.GameScores.Add(newScore);

                string statusText = scoreData.IsFinished ? " (Finished)" : "";
                Logger.Log($"Score updated for poll {currentPoll.idPoll}: {score1}-{score2}{statusText} (Admin score was in Team1Score)");

                _stateManager.SaveState();
                OnScoreUpdated?.Invoke();
            }
            else
            {
                // Если нет одобренного опроса, сохраняем счёт как есть
                var newScore = new GameScore(score1, score2, scoreData.IsFinished);
                _stateManager.state.StandaloneScores.Add(newScore);
                
                Logger.Log($"Stored score standalone: {score1}-{score2} (No active poll found, MySide={scoreData.MySide})");
                
                _stateManager.SaveState();
                OnScoreUpdated?.Invoke();
            }
        }

        private class ScoreUpdateDto
        {
            public int Team1Score { get; set; }
            public int Team2Score { get; set; }
            public bool IsFinished { get; set; }
            public string? MySide { get; set; }
        }
    }
}
