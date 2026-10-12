/* Gemeinsame Hilfsfunktionen für alle Seiten */
const $ = id => document.getElementById(id);
const esc = s => String(s).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

const lsGet = k => { try { return JSON.parse(localStorage.getItem(k)); } catch (e) { return null; } };
const lsSet = (k, v) => { try { v == null ? localStorage.removeItem(k) : localStorage.setItem(k, JSON.stringify(v)); } catch (e) {} };

// Sitzung (wer ist angemeldet) und Beitrittscode aus dem Einladungslink
const getSession = () => lsGet('wp_sess');
const setSession = id => lsSet('wp_sess', { id });
const clearSession = () => lsSet('wp_sess', null);
const getPending = () => lsGet('wp_pending');
const clearPending = () => lsSet('wp_pending', null);
(function captureJoin() {                      // ...#join=CODE merken
  const m = (location.hash + location.search).match(/join=([a-f0-9]+)/);
  if (m) lsSet('wp_pending', m[1]);
})();

function toast(t) {
  const e = $('toast'); if (!e) return;
  e.textContent = t; e.style.display = 'block';
  clearTimeout(toast.t); toast.t = setTimeout(() => e.style.display = 'none', 4000);
}

// Passwort-Verschlüsselung: PBKDF2-SHA256 + zufälliger Salt.
// Mit Server: Hashing dort machen (bcrypt/argon2) und das hier entfernen.
const hex = b => [...new Uint8Array(b)].map(x => x.toString(16).padStart(2, '0')).join('');
const rand = n => hex(crypto.getRandomValues(new Uint8Array(n)));
async function hash(pw, saltHex) {
  const salt = Uint8Array.from(saltHex.match(/../g).map(h => parseInt(h, 16)));
  const key = await crypto.subtle.importKey('raw', new TextEncoder().encode(pw), 'PBKDF2', false, ['deriveBits']);
  return hex(await crypto.subtle.deriveBits({ name: 'PBKDF2', salt, iterations: 150000, hash: 'SHA-256' }, key, 256));
}
const idOf = async e => hex(await crypto.subtle.digest('SHA-256', new TextEncoder().encode(e)));
