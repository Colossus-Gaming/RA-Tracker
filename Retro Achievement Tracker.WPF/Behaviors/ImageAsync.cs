using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RATracker.WPF.Behaviors;

/// <summary>
/// Attached property that loads a remote image URL into a <see cref="Border"/>'s
/// <see cref="Border.Background"/> as a fully-decoded, frozen <see cref="ImageBrush"/>.
///
/// <para><b>Why this exists.</b> Binding a raw URL string straight to <c>ImageBrush.ImageSource</c>
/// (WPF's built-in <c>ImageSourceConverter</c>) produces a <see cref="BitmapImage"/> that downloads
/// ASYNCHRONOUSLY. When the download finishes, an <see cref="ImageBrush"/> does not reliably
/// invalidate the on-screen visual, so the badge appears stale until an input event (a mouse move)
/// forces a render pass. This was verified empirically: with a remote <c>UriSource</c>,
/// <c>CacheOption.OnLoad</c> does NOT decode synchronously (<c>IsDownloading</c> stays true) and
/// <c>Freeze()</c> throws while still downloading. The only way to obtain a complete, frozen image is
/// to fetch the bytes first and decode from a <c>StreamSource</c>. Assigning the resulting frozen
/// brush to <c>Border.Background</c> (a render-affecting property) invalidates the Border and schedules
/// an immediate render — no mouse move needed. The Border's own <c>CornerRadius</c> clips its
/// background, so rounded corners are preserved without any extra clip geometry.</para>
///
/// <para><b>Scope.</b> Intended for the dashboard badges that change in place with no animation to mask
/// the bug. It is NOT for the overlay badges, which use an <c>&lt;Image&gt;</c> with a live rounded
/// <c>Image.Clip</c> and show-animations that already force a render pass.</para>
/// </summary>
public static class ImageAsync
{
    // One shared client for the whole app. PooledConnectionLifetime lets a long-running streaming
    // session pick up DNS changes; Timeout prevents a hung CDN connection from pinning a load forever.
    private static readonly HttpClient _http = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    })
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    // Bounded LRU cache of frozen (cross-thread-usable) ImageSources keyed by URL, so browsing back
    // and forth doesn't re-download and the cache can't grow without bound over a long session.
    private const int CacheCapacity = 256;
    private static readonly object _cacheLock = new();
    private static readonly Dictionary<string, LinkedListNode<KeyValuePair<string, ImageSource>>> _cache = new();
    private static readonly LinkedList<KeyValuePair<string, ImageSource>> _lru = new();

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.RegisterAttached(
            "Source", typeof(string), typeof(ImageAsync),
            new PropertyMetadata(null, OnSourceChanged));

    public static void SetSource(DependencyObject d, string? value) => d.SetValue(SourceProperty, value);
    public static string? GetSource(DependencyObject d) => (string?)d.GetValue(SourceProperty);

    // Per-target load token: only the most recent load may assign its result. Doubles as the
    // cancellation source so a superseded download is aborted instead of running to completion.
    private static readonly DependencyProperty LoadTokenProperty =
        DependencyProperty.RegisterAttached(
            "LoadToken", typeof(CancellationTokenSource), typeof(ImageAsync));

    private static async void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Border border) return;
        if (DesignerProperties.GetIsInDesignMode(border)) return;

        // Newest load wins: cancel the prior in-flight load and stamp a fresh token for this one.
        (border.GetValue(LoadTokenProperty) as CancellationTokenSource)?.Cancel();
        var cts = new CancellationTokenSource();
        border.SetValue(LoadTokenProperty, cts);

        var uri = e.NewValue as string;
        if (string.IsNullOrWhiteSpace(uri))
        {
            border.Background = null;
            return;
        }

        try
        {
            var image = await LoadFrozenAsync(uri, cts.Token);
            // The DP callback ran on the UI thread, so this continuation resumes on the UI thread.
            if (!ReferenceEquals(border.GetValue(LoadTokenProperty), cts)) return; // superseded
            border.Background = image == null
                ? null
                : new ImageBrush(image) { Stretch = Stretch.UniformToFill };
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer URL (or window closing) — nothing to do.
        }
        catch
        {
            // Network or decode failure (404, DNS, timeout, malformed bytes): leave the badge blank
            // rather than let an async-void exception crash the app.
            if (ReferenceEquals(border.GetValue(LoadTokenProperty), cts))
                border.Background = null;
        }
    }

    private static async Task<ImageSource?> LoadFrozenAsync(string uri, CancellationToken ct)
    {
        if (TryGetCached(uri, out var cached)) return cached;

        // Download + decode entirely off the UI thread.
        byte[] bytes = await _http.GetByteArrayAsync(uri, ct).ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();

        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        using (var ms = new MemoryStream(bytes))
        {
            bmp.StreamSource = ms;
            bmp.EndInit(); // synchronous decode from the in-memory stream
        }
        bmp.Freeze(); // now immutable and usable from the UI thread

        AddToCache(uri, bmp);
        return bmp;
    }

    private static bool TryGetCached(string uri, out ImageSource? image)
    {
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(uri, out var node))
            {
                _lru.Remove(node);
                _lru.AddFirst(node);
                image = node.Value.Value;
                return true;
            }
        }
        image = null;
        return false;
    }

    private static void AddToCache(string uri, ImageSource image)
    {
        lock (_cacheLock)
        {
            if (_cache.ContainsKey(uri)) return;
            var node = new LinkedListNode<KeyValuePair<string, ImageSource>>(
                new KeyValuePair<string, ImageSource>(uri, image));
            _lru.AddFirst(node);
            _cache[uri] = node;
            while (_cache.Count > CacheCapacity && _lru.Last != null)
            {
                var oldest = _lru.Last;
                _lru.RemoveLast();
                _cache.Remove(oldest.Value.Key);
            }
        }
    }
}
