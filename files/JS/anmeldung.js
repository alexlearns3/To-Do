(async () => {
  const s = getSession();
  if (s && await api.getUser(s.id)) { location.replace('kalender.html'); return; }
  if (getPending()) $('pend').classList.remove('hidden');

  async function login() {
    const err = t => $('err').textContent = t;
    const email = $('em').value.trim().toLowerCase(), pw = $('pw').value;
    if (!/^\S+@\S+\.\S+$/.test(email)) return err('Bitte eine gültige E-Mail eingeben.');
    $('go').disabled = true;
    try {
      const user = await api.getUser(await idOf(email));
      if (!user || await hash(pw, user.salt) !== user.hash) return err('E-Mail oder Passwort ist falsch.');
      setSession(user.id);
      location.href = 'kalender.html';
    } catch (e) { err('Fehler: ' + (e.message || e)); }
    finally { $('go').disabled = false; }
  }
  $('go').onclick = login;
  document.onkeydown = e => { if (e.key === 'Enter') login(); };
})();
