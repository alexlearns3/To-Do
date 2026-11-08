var users=loadUsers();
var session=[], cur=0;
var names=['MO','DI','MI','DO','FR','SA','SO'], hours=[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23];
var MONTHS=['Januar','Februar','März','April','Mai','Juni','Juli','August','September','Oktober','November','Dezember'];
var ROW=46; // Höhe einer Stunden-Zeile in px
function $(i){return document.getElementById(i)}
function pad(n){return (n<10?'0':'')+n}
function esc(s){return s.replace(/[&<>"]/g,function(c){return {'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c]})}
function fmt(d){var p=d.split('-');return p[2]+'. '+p[1]+'. '+p[0]}

/* Datums-Helfer (lokale Zeit, kein UTC-Versatz) */
function iso(d){return d.getFullYear()+'-'+pad(d.getMonth()+1)+'-'+pad(d.getDate())}
function parseIso(s){var p=s.split('-');return new Date(+p[0],+p[1]-1,+p[2])}
function addDays(d,n){var x=new Date(d.getFullYear(),d.getMonth(),d.getDate());x.setDate(x.getDate()+n);return x}
function monday(d){return addDays(d,-((d.getDay()+6)%7))}

/* Was gerade angezeigt wird: startet immer bei heute */
var weekStart=monday(new Date());
var miniMonth=new Date(new Date().getFullYear(),new Date().getMonth(),1);

function goTo(date){
  weekStart=monday(date);
  miniMonth=new Date(date.getFullYear(),date.getMonth(),1);
  show();
}
function show(){mini();days();title();render()}
function scrollNow(){$('grid').scrollTop=ROW*Math.max(0,new Date().getHours()-1)}

function title(){
  var a=weekStart,b=addDays(weekStart,6),s;
  if(a.getMonth()===b.getMonth())s=MONTHS[a.getMonth()]+' '+a.getFullYear();
  else if(a.getFullYear()===b.getFullYear())s=MONTHS[a.getMonth()]+' – '+MONTHS[b.getMonth()]+' '+b.getFullYear();
  else s=MONTHS[a.getMonth()]+' '+a.getFullYear()+' – '+MONTHS[b.getMonth()]+' '+b.getFullYear();
  $('calTitle').textContent=s;
}

function side(){
  $('side').innerHTML=session.map(function(u,i){return '<button class="av'+(i===cur?' on':'')+'" style="background:'+u.color+'" data-u="'+i+'" title="'+esc(u.name)+'" aria-label="'+esc(u.name)+'">'+esc(u.name.charAt(0).toUpperCase())+'</button>'}).join('')+'<button class="plus" id="plus" type="button" aria-label="Konto hinzufügen">+</button>';
}
function legend(){
  $('legend').hidden=session.length<2;$('legend').innerHTML=session.map(function(u,i){var on=u.show!==false;return '<button class="chip" type="button" data-l="'+i+'" aria-pressed="'+on+'"><i style="background:'+u.color+'"></i>'+esc(u.name)+'</button>'}).join('');
}
function mini(){
  $('mTitle').textContent=MONTHS[miniMonth.getMonth()].toUpperCase()+' '+miniMonth.getFullYear();
  var t=iso(new Date()),wS=iso(weekStart),wE=iso(addDays(weekStart,6));
  var h='<tr>'+['M','D','M','D','F','S','S'].map(function(x,i){return '<th'+(i>4?' class="we"':'')+'>'+x+'</th>'}).join('')+'</tr>';
  var start=monday(miniMonth);
  for(var r=0;r<6;r++){
    var rowStart=addDays(start,r*7);
    if(r>0&&rowStart.getMonth()!==miniMonth.getMonth())break;
    h+='<tr>';
    for(var c=0;c<7;c++){
      var d=addDays(start,r*7+c),s=iso(d),cl=[];
      if(c>4)cl.push('we');
      if(d.getMonth()!==miniMonth.getMonth())cl.push('out');
      if(s>=wS&&s<=wE)cl.push('wk');
      if(s===t)cl.push('on');
      h+='<td class="'+cl.join(' ')+'" data-d="'+s+'">'+d.getDate()+'</td>';
    }
    h+='</tr>';
  }
  $('mini').innerHTML=h;
}
function days(){
  var t=iso(new Date());
  $('days').innerHTML='<span></span>'+names.map(function(n,i){var d=addDays(weekStart,i);return '<div'+(iso(d)===t?' class="today"':'')+'>'+n+'<b>'+d.getDate()+'</b></div>'}).join('');
}
function render(){
  if(!session.length){$('list').innerHTML='';$('grid').innerHTML='';side();legend();return}
  var u=session[cur],td=u.todos;
  td.sort(function(a,b){return (a.d+a.t)<(b.d+b.t)?-1:1});
  $('todoTitle').textContent=u.name+' TO-DO:';
  $('list').innerHTML=td.length?td.map(function(t,i){return '<li><span class="d">'+fmt(t.d)+'</span><button type="button" aria-label="Löschen" data-i="'+i+'">×</button><span class="t">'+t.t+' Uhr</span><span class="w">'+esc(t.w)+'</span></li>'}).join(''):'<li class="empty">Noch nichts geplant.</li>';
  var h='',now=iso(new Date());
  hours.forEach(function(hr){
    h+='<div class="h">'+pad(hr)+':00</div>';
    for(var i=0;i<7;i++){
      var date=iso(addDays(weekStart,i));
      var ev=session.map(function(p){
        if(p.show===false)return '';
        return p.todos.filter(function(t){return t.d===date&&parseInt(t.t,10)===hr}).map(function(t){return '<div class="ev" style="background:'+p.color+'">'+esc(p.name)+':<br>'+esc(t.w)+'</div>'}).join('');
      }).join('');
      h+='<div class="c'+(date===now?' now':'')+'">'+ev+'</div>';
    }
  });
  var st=$('grid').scrollTop;$('grid').innerHTML=h;$('grid').scrollTop=st; side(); legend();
}

/* Turso-Datenbank */
var DB_URL='https://sever-christophervoglauer.aws-eu-north-1.turso.io';
var DB_TOKEN='eyJhbGciOiJFZERTQSIsInR5cCI6IkpXVCJ9.eyJhIjoicnciLCJpYXQiOjE3OTIzOTQ1MDUsImlkIjoiMDFhMTJlYjktMmYwMS03NTEwLWI5ZjUtZTc2MzE5ZjMwZDVmIiwia2lkIjoiWlRUdFB6WVR0Qy1NMlFqY3NTcG54anZraEYtS2FjSnJLZ0xyN2JOUGswUSIsInJpZCI6IjA3NTk4YzgwLWQ1ZjAtNDQ0Mi04MDc3LWNkNzM5MWZjZjRhMiJ9.EBHICcxG3g8ORizDk76aniqCyazWR1TGgFcK8h_aay_7W_gM-FptrUuP1mgX2262OAVb4oQ4IRG4C7eHY3UgCw'.trim().replace(/^token:\s*/i,''); // nur eyJ... einfügen, ohne "token:"

function dbArg(v){
  if(typeof v==='number'&&Number.isInteger(v))return {type:'integer',value:String(v)};
  return {type:'text',value:String(v)};
}
function dbQuery(sql,args){
  return fetch(DB_URL+'/v2/pipeline',{
    method:'POST',
    headers:{'Authorization':'Bearer '+DB_TOKEN,'Content-Type':'application/json'},
    body:JSON.stringify({requests:[
      {type:'execute',stmt:{sql:sql,args:(args||[]).map(dbArg)}},
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

/* Todos: DB-Format "2026-10-20 10:00:00" <-> App-Format {d:'2026-10-20', t:'10:00', w:'...'} */
function loadTodos(userId){
  return dbQuery('SELECT id, description, date FROM todos WHERE user_id = ? ORDER BY date',[userId])
    .then(function(rows){
      return rows.map(function(r){
        var dt=String(r.date).replace('T',' ');
        return {id:+r.id,d:dt.slice(0,10),t:dt.slice(11,16),w:String(r.description)};
      });
    });
}
function addTodo(userId,t){
  return dbQuery('INSERT INTO todos (user_id, description, date) VALUES (?, ?, ?) RETURNING id',
    [userId,t.w,t.d+' '+t.t+':00'])
    .then(function(rows){t.id=+rows[0].id;return t});
}
function deleteTodo(userId,todoId){
  return dbQuery('DELETE FROM todos WHERE id = ? AND user_id = ?',[todoId,userId]);
}

/* DB-Zeile -> App-User; Farbe bleibt lokal, Todos kommen aus der Datenbank */
var colors=COLORS;
function toUser(row){
  var mail=String(row.email).toLowerCase();
  var u=users.find(function(x){return x.mail===mail});
  if(!u){
    u={mail:mail,name:row.username,color:colors[users.length%colors.length]};
    users.push(u);
  }
  u.id=+row.id;
  if(!u.name)u.name=row.username;
  if(!u.color)u.color=colors[0];
  delete u.pw;
  return loadTodos(u.id).then(function(todos){
    u.todos=todos;
    saveUsers(users);
    return u;
  });
}

/* Todo-Formular */
function openForm(o){$('form').classList.toggle('open',o);$('open').hidden=o;$('err').textContent='';if(o){$('date').value=iso(new Date());$('time').value='';$('what').value='';$('date').focus()}}
$('open').onclick=function(){openForm(true)};
$('cancel').onclick=function(){openForm(false)};
$('save').onclick=function(){
  var d=$('date').value,t=$('time').value,w=$('what').value.trim(),btn=$('save');
  if(!d||!t||!w){$('err').textContent='Bitte Datum, Uhrzeit und Aufgabe ausfüllen.';return}
  var u=session[cur];
  btn.disabled=true;
  addTodo(u.id,{d:d,t:t,w:w})
    .then(function(todo){u.todos.push(todo);openForm(false);goTo(parseIso(todo.d))})
    .catch(function(ex){console.error(ex);$('err').textContent='Fehler: '+ex.message})
    .then(function(){btn.disabled=false});
};
$('list').onclick=function(e){
  var i=e.target.getAttribute('data-i');if(i===null)return;
  var u=session[cur],todo=u.todos[+i];
  e.target.disabled=true;
  deleteTodo(u.id,todo.id)
    .then(function(){
      var k=u.todos.indexOf(todo);if(k>=0)u.todos.splice(k,1);
      render();
    })
    .catch(function(ex){console.error(ex);e.target.disabled=false;alert('Löschen fehlgeschlagen: '+ex.message)});
};
/* Navigation */
$('wPrev').onclick=function(){goTo(addDays(weekStart,-7))};
$('wNext').onclick=function(){goTo(addDays(weekStart,7))};
$('today').onclick=function(){goTo(new Date());scrollNow()};
$('mPrev').onclick=function(){miniMonth=new Date(miniMonth.getFullYear(),miniMonth.getMonth()-1,1);mini()};
$('mNext').onclick=function(){miniMonth=new Date(miniMonth.getFullYear(),miniMonth.getMonth()+1,1);mini()};
$('mini').onclick=function(e){var td=e.target.closest('td[data-d]');if(td)goTo(parseIso(td.getAttribute('data-d')))};
/* Klick auf ein Datum in der To-Do-Liste springt zu dieser Woche */
$('list').addEventListener('click',function(e){var d=e.target.closest('.d');if(!d)return;var p=d.textContent.split('. ');goTo(new Date(+p[2],+p[1]-1,+p[0]))});
/* Pfeiltasten: ← vorherige Woche, → nächste Woche, T = heute */
document.addEventListener('keydown',function(e){
  if(!$('auth').hidden||/INPUT|TEXTAREA|SELECT/.test(e.target.tagName))return;
  if(e.key==='ArrowLeft')goTo(addDays(weekStart,-7));
  else if(e.key==='ArrowRight')goTo(addDays(weekStart,7));
  else if(e.key==='t'||e.key==='T'){goTo(new Date());scrollNow()}
});

$('side').onclick=function(e){
  var b=e.target.closest('button');if(!b)return;
  if(b.id==='plus'){showAuth()}
  else{cur=+b.getAttribute('data-u');openForm(false);render()}
};

/* Anmeldung */
function login(u){
  if($('app').hidden){$('app').hidden=false;setTimeout(scrollNow,0)}
  var i=session.findIndex(function(x){return x.mail===u.mail});if(i<0){session.push(u);i=session.length-1}
  cur=i;
  saveSession(session.map(function(x){return x.mail}));
}
function showAuth(){
  $('aErr').textContent='';
  $('closeAuth').hidden=!session.length;
  $('auth').hidden=false;
  $('aMail').focus();
}
function hideAuth(){$('auth').hidden=true;$('authForm').reset()}
$('closeAuth').onclick=hideAuth;
document.addEventListener('keydown',function(e){if(e.key==='Escape'&&session.length&&!$('auth').hidden)hideAuth()});
$('authForm').onsubmit=function(e){
  e.preventDefault();
  var m=$('aMail').value.trim().toLowerCase(),p=$('aPw').value,err=$('aErr');
  if(!m||!p){err.textContent='Bitte alle Felder ausfüllen.';return}
  if(!/^\S+@\S+\.\S+$/.test(m)){err.textContent='Bitte gib eine gültige E-Mail-Adresse ein.';return}
  dbQuery('SELECT id, username, email FROM users WHERE email = ? AND password = ?',[m,p])
    .then(function(rows){
      if(!rows.length){err.textContent='E-Mail oder Passwort stimmt nicht.';return}
      return toUser(rows[0]).then(function(u){
        login(u);
        hideAuth();openForm(false);render();
      });
    })
    .catch(function(ex){console.error(ex);err.textContent='Fehler: '+ex.message});
};
$('legend').onclick=function(e){var b=e.target.closest('button');if(!b)return;var u=session[+b.getAttribute('data-l')];u.show=u.show===false;render()};

/* Start: angemeldete Konten und ihre Todos aus der Datenbank laden */
show();scrollNow();
Promise.all(loadSession().map(function(m){
  return dbQuery('SELECT id, username, email FROM users WHERE email = ?',[m])
    .then(function(rows){return rows.length?toUser(rows[0]):null})
    .catch(function(ex){console.error(ex);return null});
})).then(function(list){
  list.forEach(function(u){if(u)login(u)});
  if(session.length)cur=0;
  show();scrollNow();
  if(session.length){$('auth').hidden=true}else{showAuth()}
});