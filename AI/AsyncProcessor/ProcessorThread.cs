namespace HeartAndHavoc_GoblinLegacy.AI.AsyncProcessor
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public sealed class ProcessorThread : IDisposable
    {
        public readonly struct JobHandle
        {
            internal readonly int Id;
            internal JobHandle(int id) => Id = id;
            public override string ToString() => $"JobHandle({Id})";
        }

        private readonly struct Job
        {
            public readonly int Id;
            public readonly Action Work;
            public Job(int id, Action work) { Id = id; Work = work; }
        }

        private readonly BlockingCollection<Job> _queue = new();
        private readonly ConcurrentDictionary<int, byte> _inFlight = new(); // presence = not completed yet
        private readonly Thread _thread;
        private int _nextId;

        public ProcessorThread(string name = "Processor")
        {
            _thread = new Thread(Run)
            {
                IsBackground = true,
                Name = name
            };
            _thread.Start();
        }

        private void Run()
        {
            foreach (var job in _queue.GetConsumingEnumerable())
            {
                try
                {
                    job.Work();
                }
                catch (Exception ex)
                {
                    // Thread-safe minimal logging; replace with your logger if you have one.
                    Console.WriteLine(ex);
                }
                finally
                {
                    _inFlight.TryRemove(job.Id, out _);
                }
            }
        }

        /// Enqueue a void job. Returns a handle you can poll.
        public JobHandle Enqueue(Action work)
        {
            if (work == null) throw new ArgumentNullException(nameof(work));

            int id = Interlocked.Increment(ref _nextId);
            _inFlight[id] = 0;
            _queue.Add(new Job(id, work));
            return new JobHandle(id);
        }

        /// Convenience overload for your signature style.
        public JobHandle Enqueue(WorldSnapshot ws, KremlitSnapshot ks, Action<WorldSnapshot, KremlitSnapshot> work)
        {
            return work == null ? throw new ArgumentNullException(nameof(work)) : Enqueue(() => work(ws, ks));
        }

        /// Polling API
        public bool IsCompleted(JobHandle handle) => !_inFlight.ContainsKey(handle.Id);

        public void Dispose()
        {
            _queue.CompleteAdding();
            _thread.Join();
            _queue.Dispose();
        }
    }

}
