const COLORS = ['#b9b9ff', '#aee8b8', '#f7c6a0', '#b7dc3f', '#f3a8c8', '#9fdcf0'];
const MONTHS = ['Januar', 'Februar', 'März', 'April', 'Mai', 'Juni', 'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'];
const DAYS = ['MO', 'DI', 'MI', 'DO', 'FR', 'SA', 'SO'];
let me = null, members = [], events = [], sel = null;

function monday(d) { const x = new Date(d.getFullYear(), d.getMonth(), d.getDate()); x.setDate(x.getDate() - ((x.getDay() + 6) % 7)); return x; }
let weekStart = monday(new Date());
const ymd = d => d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
const fmtDE = s => { const [y, m, d] = s.split('-'); return `${+d}. ${+m}. ${y}`; };
const color = id => COLORS[[...id].reduce((a, c) => a + c.charCodeAt(0), 0) % COLORS.length];
const nameOf = id => (members.find(m => m.id === id) || { name: '?' }).name;

// ---- Start ----
async function init() {
  const s = getSession();
  const u = s && await api.getUser(s.id);
  if (!u) { location.replace('anmeldung.html'); return; }      // nicht angemeldet
  me = { id: u.id, email: u.email, name: u.name, calId: u.calId };
  const pending = getPending();                                  // Einladungslink offen?
  if (pending && pending !== me.calId) await joinCal(pending, true);
  clearPending();
  await reload();
}
async function reload() {
  if (!me) return;
  [members, events] = await Promise.all([api.listMembers(me.calId), api.listEvents(me.calId)]);
  if (sel && !members.some(m => m.id === sel)) sel = null;
  render();
}
window.addEventListener('storage', () => reload());            // andere Tabs aktualisieren

async function joinCal(code, silent) {
  code = (code || '').trim().toLowerCase();
  if (!await api.calendarExists(code)) return toast('Dieser Beitrittscode ist ungültig.');
  await api.setCalendar(me.id, code); me.calId = code; sel = null;
  $('modal').classList.add('hidden');
  if (!silent) await reload();
  toast('Du bist dem Kalender beigetreten.');
}

// ---- Ansicht ----
function render() {
  const shown = sel ? events.filter(x => x.uid === sel) : events, todayS = ymd(new Date()), viewId = sel || me.id;

  // Profile links
  $('side').innerHTML = members.map(m => `<button class="av ${m.id === sel ? 'on' : ''}" data-id="${m.id}" title="${esc(m.name)}${m.id === me.id ? ' (du)' : ''}" style="background:${color(m.id)}">${esc(m.name[0].toUpperCase())}</button>`).join('')
    + '<button id="add" title="Leute einladen" aria-label="Leute einladen">+</button>';
  document.querySelectorAll('.av').forEach(b => b.onclick = () => { sel = sel === b.dataset.id ? null : b.dataset.id; render(); });
  $('add').onclick = openInvite;

  // Kopf + Wochentage
  const end = new Date(weekStart); end.setDate(end.getDate() + 6);
  $('title').textContent = MONTHS[weekStart.getMonth()] + ' ' + weekStart.getFullYear() + (end.getMonth() !== weekStart.getMonth() ? ' / ' + MONTHS[end.getMonth()] : '');
  const dates = [...Array(7)].map((_, i) => { const d = new Date(weekStart); d.setDate(d.getDate() + i); return d; });
  $('days').innerHTML = '<div></div>' + dates.map((d, i) => `<div class="dh ${ymd(d) === todayS ? 'today' : ''}">${DAYS[i]}<b>${d.getDate()}</b></div>`).join('');

  // Wochenraster + Termine
  let h = '<div class="hrs">' + [...Array(17)].map((_, i) => `<div>${i + 6}:00</div>`).join('') + '</div>';
  dates.forEach(d => {
    const ev = shown.filter(x => x.date === ymd(d)), grp = {};
    ev.forEach(x => (grp[x.time] = grp[x.time] || []).push(x));
    h += '<div class="col">' + ev.map(x => {
      const g = grp[x.time], i = g.indexOf(x), w = 100 / g.length, [hh, mm] = x.time.split(':').map(Number), top = Math.max(0, (hh - 6 + mm / 60) * 48) + 2;
      return `<div class="ev" style="top:${top}px;left:calc(${i * w}% + 3px);width:calc(${w}% - 6px);right:auto;background:${color(x.uid)}" title="${esc(x.title)}">${esc(nameOf(x.uid))}:<br>${esc(x.title)}</div>`;
    }).join('') + '</div>';
  });
  $('cal').innerHTML = h;
  if (!render.done) { $('scroll').scrollTop = 96; render.done = 1; }

  // Mini-Kalender
  const m = weekStart.getMonth(), y = weekStart.getFullYear(), first = (new Date(y, m, 1).getDay() + 6) % 7, n = new Date(y, m + 1, 0).getDate();
  let mg = DAYS.map(d => `<span class="h">${d[0]}</span>`).join('') + '<span></span>'.repeat(first);
  for (let d = 1; d <= n; d++) { const s = ymd(new Date(y, m, d)); mg += `<span class="${s === todayS ? 't' : shown.some(x => x.date === s) ? 'd' : ''}">${d}</span>`; }
  $('mini').innerHTML = `<h4>${MONTHS[m].toUpperCase()} ${y}</h4><div class="mg">${mg}</div>`;

  // To-do-Liste
  const list = events.filter(x => x.uid === viewId).sort((a, b) => (a.date + a.time).localeCompare(b.date + b.time));
  $('todo').innerHTML = `<button class="addbtn" id="addtodo">+ Hinzufügen</button><h3>${esc(nameOf(viewId))} TO-DO:</h3>`
    + (list.map(x => `<div class="ti"><div><b>${fmtDE(x.date)}</b>${esc(x.time)} Uhr<br>${esc(x.title)}</div>${x.uid === me.id ? `<button class="x" data-id="${x.id}" aria-label="Löschen">✕</button>` : ''}</div>`).join('') || '<span style="color:#bbb">Noch nichts geplant.</span>');
  $('addtodo').onclick = openModal;
  document.querySelectorAll('.x').forEach(b => b.onclick = async () => { await api.deleteEvent(b.dataset.id, me.id); reload(); });
}

// ---- Fenster ----
function modal(html) {
  const mo = $('modal'); mo.classList.remove('hidden');
  mo.innerHTML = `<div class="mbox" role="dialog" aria-modal="true">${html}</div>`;
  mo.onkeydown = e => { if (e.key === 'Escape') mo.classList.add('hidden'); };
  mo.onclick = e => { if (e.target === mo) mo.classList.add('hidden'); };
}
function openModal() {                                           // Neues To-do
  modal(`<h3>Neues To-do</h3>
  <label for="t">Bezeichnung</label><input id="t" maxlength="60">
  <label for="d">Datum</label><input id="d" type="date" value="${ymd(new Date())}">
  <label for="ti">Uhrzeit</label><input id="ti" type="time" value="12:00">
  <div class="err" id="merr" role="alert"></div>
  <div class="row"><button class="btn sec" id="c">Abbrechen</button><button class="btn" id="s">Speichern</button></div>`);
  $('t').focus();
  $('c').onclick = () => $('modal').classList.add('hidden');
  $('s').onclick = async () => {
    const title = $('t').value.trim(), date = $('d').value, time = $('ti').value;
    if (!title || !date || !time) return $('merr').textContent = 'Bitte Bezeichnung, Datum und Uhrzeit ausfüllen.';
    await api.addEvent({ calId: me.calId, uid: me.id, title, date, time });
    weekStart = monday(new Date(date + 'T12:00'));
    $('modal').classList.add('hidden'); reload();
  };
}
function openInvite() {                                          // Beitrittslink
  const base = location.href.split('#')[0].split('?')[0].replace(/[^/]*$/, '');
  const link = base + 'anmeldung.html#join=' + me.calId;
  modal(`<h3>Leute einladen</h3><p>Schick diesen Link. Wer ihn öffnet und sich anmeldet oder registriert, tritt deinem Kalender bei.</p>
  <input id="lk" readonly value="${esc(link)}" aria-label="Beitrittslink"><div class="row"><button class="btn" id="cp">Link kopieren</button></div>
  <p>Oder gib diesen Code ein: <span class="code">${esc(me.calId)}</span></p>
  <div class="sep"></div><label for="jc">Anderem Kalender beitreten (Code)</label><input id="jc">
  <div class="row"><button class="btn sec" id="c">Schließen</button><button class="btn" id="j">Beitreten</button></div>`);
  $('lk').onfocus = e => e.target.select();
  $('cp').onclick = async () => { try { await navigator.clipboard.writeText(link); toast('Link kopiert.'); } catch (e) { $('lk').select(); toast('Bitte mit Strg+C kopieren.'); } };
  $('c').onclick = () => $('modal').classList.add('hidden');
  $('j').onclick = () => joinCal($('jc').value);
}

$('prev').onclick = () => { weekStart.setDate(weekStart.getDate() - 7); render(); };
$('next').onclick = () => { weekStart.setDate(weekStart.getDate() + 7); render(); };
$('out').onclick = () => { clearSession(); location.href = 'anmeldung.html'; };
init();
