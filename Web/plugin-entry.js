/**
 * UHUI — Plugin Entry Point
 *
 * Este módulo es inyectado directamente en el index.html de Jellyfin por
 * WebInjectorService al arrancar el servidor. Se ejecuta automáticamente
 * en CADA carga de página, garantizando que el interceptor siempre esté activo.
 */

import { init } from './interceptor.js';

init();
