// Tailwind v4 runs as a PostCSS plugin. This file tells PostCSS to process our
// CSS through Tailwind (which reads the @theme/@import directives in index.css)
// and autoprefixer (adds vendor prefixes for older browser compatibility).
export default {
  plugins: {
    '@tailwindcss/postcss': {},
    autoprefixer: {},
  },
}
