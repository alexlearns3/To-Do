if (getPending()) $('pend').classList.remove('hidden');

async function register() {
  const err = t => $('err').textContent = t;
  const email = $('em').value.trim().toLowerCase(), pw = $('pw').value, name = $('nm').value.trim();
  if (!/^\S+@\S+\.\S+$/.test(email)) return err('Bitte eine gültige E-Mail eingeben.');
  if (pw.length < 6) return err('Das Passwort braucht mindestens 6 Zeichen.');
  if (!name) return err('Bitte einen Namen eingeben.');
  $('go').disabled = true;
  try {
    const id = await idOf(email);
    if (await api.getUser(id)) return err('Diese E-Mail ist schon registriert.');
    const pending = getPending();
    // Eingeladen? Dann gleich dem Kalender beitreten, sonst eigenen anlegen.
    const calId = (pending && await api.calendarExists(pending)) ? pending : rand(8);
    const salt = rand(16);
    await api.createUser({ id, email, name, salt, hash: await hash(pw, salt), calId });
    clearPending();
    setSession(id);
    location.href = 'kalender.html';
  } catch (e) { err('Fehler: ' + (e.message || e)); }
  finally { $('go').disabled = false; }
}
$('go').onclick = register;
document.onkeydown = e => { if (e.key === 'Enter') register(); };
