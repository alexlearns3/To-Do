function $(i){return document.getElementById(i)}
 
/* Turso-Datenbank */
 
function dbQuery(sql,args){
  return fetch(DB_URL+'/v2/pipeline',{
    method:'POST',
    headers:{'Authorization':'Bearer '+DB_TOKEN,'Content-Type':'application/json'},
    body:JSON.stringify({requests:[
      {type:'execute',stmt:{sql:sql,args:args.map(function(v){return{type:'text',value:String(v)}})}},
      {type:'close'}
    ]})
  }).then(function(r){
    if(r.ok)return r.json();
    return r.text().then(function(t){
      console.error('Turso-Antwort:',r.status,t);
      if(r.status===401||r.status===403)throw new Error('Token ungültig oder fehlt');
      throw new Error('HTTP '+r.status+': '+t);
    });
  }).then(function(d){
    var res=d.results[0];
    if(res.type!=='ok')throw new Error(res.error&&res.error.message);
    var cols=res.response.result.cols.map(function(c){return c.name});
    return res.response.result.rows.map(function(row){
      var o={};row.forEach(function(c,i){o[cols[i]]=c.value});return o;
    });
  });
}
 
$('rMail').focus();
 
$('regForm').onsubmit=function(e){
  e.preventDefault();
  var m=$('rMail').value.trim().toLowerCase(),p=$('rPw').value,n=$('rName').value.trim(),err=$('rErr');
 
  if(!m||!p||!n){err.textContent='Bitte alle Felder ausfüllen.';return}
  if(!/^\S+@\S+\.\S+$/.test(m)){err.textContent='Bitte gib eine gültige E-Mail-Adresse ein.';return}
  if(p.length<6){err.textContent='Das Passwort braucht mindestens 6 Zeichen.';return}
 
  var btn=$('regForm').querySelector('button[type=submit]');
  if(btn)btn.disabled=true;
  err.textContent='';
 
  /* 1. Prüfen, ob die E-Mail schon in der Datenbank ist */
  dbQuery('SELECT id FROM users WHERE email = ?',[m])
    .then(function(rows){
      if(rows.length)throw new Error('Zu dieser E-Mail gibt es schon ein Konto. Bitte melde dich an.');
      /* 2. Neuen User in der Datenbank speichern */
      return dbQuery('INSERT INTO users (username, email, password) VALUES (?, ?, ?)',[n,m,p]);
    })
    .then(function(){
      /* 3. Lokal Farbe + leere Todo-Liste anlegen (ohne Passwort) */
      var users=loadUsers();
      if(!users.some(function(u){return u.mail===m})){
        users.push({name:n,mail:m,color:COLORS[users.length%COLORS.length],todos:[]});
        saveUsers(users);
      }
      /* 4. Direkt angemeldet weiter zur App */
      var s=loadSession();
      if(s.indexOf(m)<0)s.push(m);
      saveSession(s);
      window.location.href='index.html';
    })
    .catch(function(ex){
      console.error(ex);
      err.textContent=ex.message;
      if(btn)btn.disabled=false;
    });
};