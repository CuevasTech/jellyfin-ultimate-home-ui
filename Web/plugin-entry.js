/**
 * UHUI — Plugin Entry Point
 * Loaded as a <script type="module"> injected into Jellyfin's index.html.
 * Dynamically imports the interceptor and starts the DOM observer.
 */
import('./interceptor.js')
  .then(({ init }) => init())
  .catch(e => console.error('[UHUI] Failed to initialize:', e));
