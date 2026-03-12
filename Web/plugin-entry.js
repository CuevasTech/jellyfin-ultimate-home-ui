/**
 * UHUI — Plugin Entry Point
 * Loaded by Jellyfin plugin API. Initializes the MutationObserver interceptor.
 */
const UHUI_VERSION = '1.0.0';

export default {
  name: 'Jellyfin Ultimate Home UI',
  version: UHUI_VERSION,

  initialize() {
    import('./interceptor.js').then(m => m.init());
  }
};
