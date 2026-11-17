# Font source

Family: JetBrainsMono Nerd Font Mono. JetBrains Mono 2.304, Nerd Fonts 3.5.1.

Official archive:
https://github.com/ryanoasis/nerd-fonts/releases/download/v3.5.1/JetBrainsMono.tar.xz

Archive SHA-256: `04d5e8f903693f9dd13e16f867e994834e681eb3c72c0d337a770dcda09010cf`.

Mono Regular (400), Medium (500), and SemiBold (600) are subsetted into the
`*-Web.woff2` files with fontTools 4.65.0 / Brotli 1.2.1. All non-private-use
characters are preserved, including Latin, Cyrillic, Greek, and mathematical
symbols. The only retained private-use icons are U+F00D (close/remove) and
U+F09B (GitHub). Add any new private-use icons to the subset before using them.

Names, retained outlines, metrics, hinting, and OpenType layout features are
preserved. The x-height to em ratio remains 0.55. FontForge editor metadata
(`PfEd`) is dropped; it is not needed for rendering.

To reproduce from each original Mono TTF in the archive:

```sh
pyftsubset JetBrainsMonoNerdFontMono-Regular.ttf \
    --unicodes=U+0000-DFFF,U+F900-EFFFF,U+F00D,U+F09B \
    --layout-features='*' --drop-tables+=PfEd --flavor=woff2 \
    --output-file=JetBrainsMonoNerdFontMono-Regular-Web.woff2
```

Repeat for Medium and SemiBold. These temporary conversion tools are not
application dependencies. Regular is preloaded; other weights load on demand.

Font license: OFL.txt. Upstream information and icon sources/licenses:
UPSTREAM.md. Keep these documents with the fonts.
