using System.Collections.Concurrent;
using static NuGet.Packaging.PackagingConstants;

namespace client.App_Code
{
    public static class NewUser
    {

        public class newUserEntry
        {
            public required Models.User value { get; set; }
            public DateTime exp { get; set; }
        }

        private static readonly ConcurrentDictionary<string, newUserEntry> _storage = new ConcurrentDictionary<string, newUserEntry>();

        public static void Store(string key, Models.User value, TimeSpan duration)
        {
            _storage[key] = new newUserEntry { value = value, exp = DateTime.Now.Add(duration) };
        }

        public static ConcurrentDictionary<string, newUserEntry> Storage
        {
            get { return _storage; }
        }

        public static void Remove(string key)
        {
            _storage.TryRemove(key, out _);
        }

        public static Models.User? get(string key)
        {
            if (_storage.TryGetValue(key, out var data))
            {
                if (data.exp > DateTime.Now)
                {
                    return data.value;
                }
                _storage.TryRemove(key, out _);
            }
            return null;
        }

        public static Task Siivoa()
        {
            foreach (var entry in _storage)
            {
                if (entry.Value.exp <= DateTime.Now)
                {
                    _storage.TryRemove(entry.Key, out _);
                }
            }
            return Task.CompletedTask;
        }
    }

    public static class RecoverPassword
    {
        public class recoverEntry
        {
            public required client.Controllers.RecoverUser value { get; set; }
            public DateTime exp { get; set; }
        }

        private static readonly ConcurrentDictionary<string, recoverEntry> _storage = new ConcurrentDictionary<string, recoverEntry>();

        public static void Store(string key, client.Controllers.RecoverUser value, TimeSpan duration)
        {
            _storage[key] = new recoverEntry { value = value, exp = DateTime.Now.Add(duration) };
        }

        public static ConcurrentDictionary<string, recoverEntry> Storage
        {
            get { return _storage; }
        }

        public static void Remove(string key)
        {
            _storage.TryRemove(key, out _);
        }

        public static client.Controllers.RecoverUser? get(string key)
        {
            if (_storage.TryGetValue(key, out var data))
            {
                if (data.exp > DateTime.Now)
                {
                    return data.value;
                }
                _storage.TryRemove(key, out _);
            }
            return null;
        }

        public static Task Siivoa()
        {
            foreach (var entry in _storage)
            {
                if (entry.Value.exp <= DateTime.Now)
                {
                    _storage.TryRemove(entry.Key, out _);
                }
            }
            return Task.CompletedTask;
        }
    }

    public class CleanupUserService : IHostedService, IDisposable
    {
        private Timer? _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            
            NewUser.Siivoa();

        
            _timer = new Timer(DoCleanup, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        private void DoCleanup(object state)
        {
            NewUser.Siivoa();
            RecoverPassword.Siivoa();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }




}
