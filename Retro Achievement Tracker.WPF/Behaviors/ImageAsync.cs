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
///
/// <para><b>Storage.</b> Downloading, decoding and caching all live in
/// <see cref="Converters.BadgeImageCache"/>, which the overlay converter shares. This behaviour only
/// owns the Border-specific part: newest-load-wins and assigning the brush.</para>
/// </summary>
public static class ImageAsync
{
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
            var image = await Converters.BadgeImageCache.GetOrLoadAsync(uri, cts.Token);
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

}
