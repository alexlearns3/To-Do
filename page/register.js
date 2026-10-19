function $(i){return document.getElementById(i)}

$('rMail').focus();

$('regForm').onsubmit=function(e){
  e.preventDefault();
  var m=$('rMail').value.trim().toLowerCase(),p=$('rPw').value,n=$('rName').value.trim(),err=$('rErr');

  if(!m||!p||!n){err.textContent='Bitte alle Felder ausfüllen.';return}
  if(!/^\S+@\S+\.\S+$/.test(m)){err.textContent='Bitte gib eine gültige E-Mail-Adresse ein.';return}
  if(p.length<6){err.textContent='Das Passwort braucht mindestens 6 Zeichen.';return}

  var users=loadUsers();
  var exists=users.some(function(u){return u.mail===m});
  if(exists){err.textContent='Zu dieser E-Mail gibt es schon ein Konto. Bitte melde dich an.';return}

  users.push({name:n,mail:m,pw:p,color:COLORS[users.length%COLORS.length],todos:[]});
  saveUsers(users);

  var s=loadSession();
  if(s.indexOf(m)<0)s.push(m);
  saveSession(s);

  window.location.href='index.html';
};
