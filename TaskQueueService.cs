using System.Collections.Generic;

namespace BasTestApi
{
    public class TaskQueueService
    {
        private readonly Queue<(TaskRequest request, DateTime receivedTime, TaskCompletionSource<TaskResponse> tcs)> _queue = new();
        private readonly object _lock = new();
        private bool _processing = false;

        public async Task<TaskResponse> EnqueueTaskAsync(TaskRequest request)
        {
            var tcs = new TaskCompletionSource<TaskResponse>();
            var receivedTime = DateTime.Now;

            lock (_lock)
            {
                _queue.Enqueue((request, receivedTime, tcs));
                if (!_processing)
                {
                    _processing = true;
                    _ = ProcessQueueAsync();
                }
            }

            return await tcs.Task;
        }

        private async Task ProcessQueueAsync()
        {
            List<(TaskRequest, DateTime, TaskCompletionSource<TaskResponse>)> batch;
            while (true)
            {
                (TaskRequest request, DateTime receivedTime, TaskCompletionSource<TaskResponse> tcs) item;

                lock (_lock)
                {
                    if (_queue.Count == 0)
                    {
                        _processing = false;
                        return;
                    }

                    batch = new List<(TaskRequest, DateTime, TaskCompletionSource<TaskResponse>)>();
                    while (batch.Count < 5 && _queue.Count > 0)
                    {
                        batch.Add(_queue.Dequeue());
                    }
                }

                var tasks = batch.Select(async item =>
                {
                    var response = await ProcessTaskAsync(item.Item1, item.Item2);
                    item.Item3.SetResult(response);
                });

                await Task.WhenAll(tasks);
            }
        }

        private async Task<TaskResponse> ProcessTaskAsync(TaskRequest request, DateTime receivedTime)
        {
            var processedTime = DateTime.Now;
            var filePath = "task_log.txt";

            await File.AppendAllTextAsync(filePath, $"{receivedTime:yyyy-MM-ddTHH:mm:ss} | {processedTime:yyyy-MM-ddTHH:mm:ss} | {request.Message}\n");

            await Task.Delay(new Random().Next(50, 101));

            return new TaskResponse
            {
                RequestTime = receivedTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                WriteTime = processedTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                ProcessingTime = (processedTime - receivedTime).TotalMilliseconds.ToString(),
            };
        }
    }
}
