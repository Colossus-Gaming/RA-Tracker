using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RATracker.WPF.Converters;

/// <summary>
/// The app's single store of decoded badge images, shared by both image paths:
/// <see cref="StringToImageSourceConverter"/> (the overlay <c>&lt;Image&gt;</c> bindings) and
/// <see cref="Behaviors.ImageAsync"/> (the dashboard <c>Border.Background</c> badges). Keeping one
/// store means a badge fetched for one surface is already decoded for the other.
///
/// <para><b>Why bytes-then-decode.</b> Binding a remote URL straight to a <see cref="BitmapImage"/>
/// downloads asynchronously: <c>CacheOption.OnLoad</c> does not decode synchronously, and
/// <c>Freeze()</c> throws while <c>IsDownloading</c> is still true. Fetching the bytes first and
/// decoding from a <c>StreamSource</c> is the only way to get a complete, frozen image — which is
/// what makes an unlock reveal swap to the colour badge instantly instead of popping in late.</para>
/// </summary>
public static class BadgeImageCache
{
    // One shared client for the whole app. PooledConnectionLifetime lets a long-running streaming
    // session pick up DNS changes; Timeout prevents a hung CDN connection from pinning a load forever.
    private static readonly HttpClient _http = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    })
    {
        Timeout = TimeSpan.FromSeconds(20)
    };

    // Bounded LRU so browsing back and forth doesn't re-download and the cache can't grow without
    // bound across a long session. A large multiset game can define 2000+ badges.
    private const int CacheCapacity = 512;

    /// <summary>Maximum number of decoded images retained. Callers use this to cap bulk prefetches.</summary>
    public static int Capacity => CacheCapacity;

    private static readonly object _cacheLock = new();
    private static readonly Dictionary<string, LinkedListNode<KeyValuePair<string, ImageSource>>> _cache = new();
    private static readonly LinkedList<KeyValuePair<string, ImageSource>> _lru = new();

    private static readonly ConcurrentDictionary<string, byte> _inFlight = new();
    private static readonly SemaphoreSlim _gate = new(6); // be polite: cap concurrent downloads

    /// <summary>
    /// Returns the decoded image for a URL if it is already cached, otherwise null. Synchronous and
    /// allocation-free on a miss, so it is safe to call from a value converter.
    /// </summary>
    public static ImageSource? Get(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        lock (_cacheLock)
        {
            if (_cache.TryGetValue(url!, out var node))
            {
                _lru.Remove(node);
                _lru.AddFirst(node);
                return node.Value.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the decoded image for a URL, downloading it if it isn't cached yet.
    /// </summary>
    public static async Task<ImageSource?> GetOrLoadAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        var cached = Get(url);
        if (cached != null) return cached;

        var bytes = await _http.GetByteArrayAsync(url, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        var image = Decode(bytes);
        Add(url, image);
        return image;
    }

    /// <summary>
    /// Kicks off background downloads for any of the given URLs not already cached or in flight.
    /// Fire-and-forget: callers use this to warm the cache ahead of an unlock.
    /// </summary>
    public static void Prefetch(IEnumerable<string?> urls)
    {
        foreach (var url in urls)
        {
            if (string.IsNullOrWhiteSpace(url)) continue;
            if (Get(url) != null) continue;
            if (!_inFlight.TryAdd(url!, 0)) continue; // already downloading
            _ = PrefetchOneAsync(url!);
        }
    }

    private static async Task PrefetchOneAsync(string url)
    {
        try
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var bytes = await _http.GetByteArrayAsync(url).ConfigureAwait(false);
                Add(url, Decode(bytes));
            }
            finally
            {
                _gate.Release();
            }
        }
        catch
        {
            // Network or decode failure (404, DNS, timeout, malformed bytes). Not fatal: the live
            // binding just falls back to loading the URL normally.
        }
        finally
        {
            _inFlight.TryRemove(url, out _);
        }
    }

    private static ImageSource Decode(byte[] bytes)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        using (var stream = new MemoryStream(bytes))
        {
            bitmap.StreamSource = stream;
            bitmap.EndInit(); // synchronous decode from the in-memory stream
        }
        bitmap.Freeze(); // now immutable and usable from any thread

        return bitmap;
    }

    private static void Add(string url, ImageSource image)
    {
        lock (_cacheLock)
        {
            if (_cache.ContainsKey(url)) return;

            var node = new LinkedListNode<KeyValuePair<string, ImageSource>>(
                new KeyValuePair<string, ImageSource>(url, image));
            _lru.AddFirst(node);
            _cache[url] = node;

            while (_cache.Count > CacheCapacity && _lru.Last != null)
            {
                var oldest = _lru.Last;
                _lru.RemoveLast();
                _cache.Remove(oldest.Value.Key);
            }
        }
    }
}
