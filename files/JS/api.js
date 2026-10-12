/* =====================================================================
   DATENSCHICHT – hier später SQLite / Server-API einsetzen.
   Alle Seiten rufen NUR diese Funktionen auf. Aktuell: localStorage.
   Später: jede Funktion durch fetch('/api/...') an deinen Server ersetzen.
   Tabellen:
     users(id, email, name, salt, hash, calId)
     events(id, calId, uid, title, date, time)
   ===================================================================== */
const api = {
  _r: k => { try { return JSON.parse(localStorage.getItem(k)) || []; } catch (e) { return []; } },
  _w: (k, v) => localStorage.setItem(k, JSON.stringify(v)),

  async getUser(id)            { return api._r('wp_users').find(u => u.id === id) || null; },
  async createUser(u)          { const a = api._r('wp_users'); a.push(u); api._w('wp_users', a); },
  async setCalendar(id, calId) { const a = api._r('wp_users'), u = a.find(x => x.id === id); if (u) u.calId = calId; api._w('wp_users', a); },
  async calendarExists(c)      { return api._r('wp_users').some(u => u.calId === c); },
  async listMembers(c)         { return api._r('wp_users').filter(u => u.calId === c).map(u => ({ id: u.id, name: u.name })); },

  async listEvents(c)          { return api._r('wp_events').filter(e => e.calId === c); },
  async addEvent(e)            { const a = api._r('wp_events'); e.id = Date.now().toString(36) + Math.random().toString(36).slice(2, 6); a.push(e); api._w('wp_events', a); },
  async deleteEvent(id, uid)   { api._w('wp_events', api._r('wp_events').filter(e => !(e.id === id && e.uid === uid))); }
};
