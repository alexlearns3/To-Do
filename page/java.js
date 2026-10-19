var users=[{name:'Tom',mail:'tom@beispiel.at',pw:'tom123',color:'#b9b6ff',todos:[{d:'2026-10-12',t:'10:00',w:'laufen gehen'},{d:'2026-10-15',t:'11:00',w:'Deutsch lernen'}]},{name:'Mia',mail:'mia@beispiel.at',pw:'mia123',color:'#f6a9c9',todos:[{d:'2026-10-13',t:'17:00',w:'Essen gehen'},{d:'2026-10-17',t:'13:00',w:'Mathe lernen'},{d:'2026-10-15',t:'11:00',w:'Mathe lernen'}]}];
var cur=0, mode='login', colors=['#b9b6ff','#f6c1d9','#ffd98a','#9fe3c8','#a9d1ff'];
var names=['MO','DI','MI','DO','FR','SA','SO'], firstMon=12, hours=[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23];
function $(i){return document.getElementById(i)}
function pad(n){return (n<10?'0':'')+n}
function esc(s){return s.replace(/[&<>"]/g,function(c){return {'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c]})}
function fmt(d){var p=d.split('-');return p[2]+'. '+p[1]+'. '+p[0]}

function side(){
  $('side').innerHTML=users.map(function(u,i){return '<button class="av'+(i===cur?' on':'')+'" style="background:'+u.color+'" data-u="'+i+'" title="'+esc(u.name)+'" aria-label="'+esc(u.name)+'">'+esc(u.name.charAt(0).toUpperCase())+'</button>'}).join('')+'<button class="plus" id="plus" type="button" aria-label="Konto hinzufügen">+</button>';
}
function legend(){
  $('legend').innerHTML=users.map(function(u,i){var on=u.show!==false;return '<button class="chip" type="button" data-l="'+i+'" aria-pressed="'+on+'"><i style="background:'+u.color+'"></i>'+esc(u.name)+'</button>'}).join('');
}
function mini(){
  var h='<tr>'+['M','T','W','T','F','S','S'].map(function(x,i){return '<th'+(i>4?' class="we"':'')+'>'+x+'</th>'}).join('')+'</tr><tr>',off=3;
  for(var i=0;i<off;i++)h+='<td></td>';
  for(var d=1;d<=31;d++){var c=(off+d-1)%7,cl=(c>4?'we':'')+(d===19?' on':'');h+='<td class="'+cl+'">'+d+'</td>';if(c===6&&d<31)h+='</tr><tr>'}
  $('mini').innerHTML=h+'</tr>';
}
function days(){$('days').innerHTML='<span></span>'+names.map(function(n,i){return '<div>'+n+'<b>'+(firstMon+i)+'</b></div>'}).join('')}
function render(){
  var u=users[cur],td=u.todos;
  td.sort(function(a,b){return (a.d+a.t)<(b.d+b.t)?-1:1});
  $('todoTitle').textContent=u.name+' TO-DO:';
  $('list').innerHTML=td.length?td.map(function(t,i){return '<li><span class="d">'+fmt(t.d)+'</span><button type="button" aria-label="Löschen" data-i="'+i+'">×</button><span class="t">'+t.t+' Uhr</span><span class="w">'+esc(t.w)+'</span></li>'}).join(''):'<li class="empty">Noch nichts geplant.</li>';
  var h='';
  hours.forEach(function(hr){
    h+='<div class="h">'+pad(hr)+':00</div>';
    for(var i=0;i<7;i++){
      var date='2026-10-'+pad(firstMon+i);
      var ev=users.map(function(p){
        if(p.show===false)return '';
        return p.todos.filter(function(t){return t.d===date&&parseInt(t.t,10)===hr}).map(function(t){return '<div class="ev" style="background:'+p.color+'">'+esc(p.name)+':<br>'+esc(t.w)+'</div>'}).join('');
      }).join('');
      h+='<div class="c">'+ev+'</div>';
    }
  });
  var st=$('grid').scrollTop;$('grid').innerHTML=h;$('grid').scrollTop=st; side(); legend();
}
function openForm(o){$('form').classList.toggle('open',o);$('open').hidden=o;$('err').textContent='';if(o){$('date').value='';$('time').value='';$('what').value='';$('date').focus()}}
$('open').onclick=function(){openForm(true)};
$('cancel').onclick=function(){openForm(false)};
$('save').onclick=function(){
  var d=$('date').value,t=$('time').value,w=$('what').value.trim();
  if(!d||!t||!w){$('err').textContent='Bitte Datum, Uhrzeit und Aufgabe ausfüllen.';return}
  users[cur].todos.push({d:d,t:t,w:w});openForm(false);render();
};
$('list').onclick=function(e){var i=e.target.getAttribute('data-i');if(i!==null){users[cur].todos.splice(+i,1);render()}};
$('side').onclick=function(e){
  var b=e.target.closest('button');if(!b)return;
  if(b.id==='plus'){showAuth('login')}
  else{cur=+b.getAttribute('data-u');openForm(false);render()}
};

/* Anmeldung / Registrierung */
function showAuth(m){
  mode=m;var reg=m==='reg';
  $('aTitle').textContent=reg?'Registrierung':'Anmeldung';
  $('aSub').textContent=reg?'Lege ein neues Konto an.':'Schön, dass du wieder da bist.';
  $('nameRow').hidden=!reg;$('aHint').hidden=reg;
  $('aPw').autocomplete=reg?'new-password':'current-password';
  $('aGo').textContent=reg?'Registrieren':'Anmelden';
  $('aSwitch').innerHTML=reg?'Schon ein Konto? <button type="button" id="sw">Anmelden</button>':'Noch kein Konto? <button type="button" id="sw">Registrieren</button>';
  $('sw').onclick=function(){showAuth(reg?'login':'reg')};
  $('aErr').textContent='';$('auth').hidden=false;
  (reg?$('aName'):$('aMail')).focus();
}
function hideAuth(){$('auth').hidden=true;$('authForm').reset()}
$('closeAuth').onclick=hideAuth;
document.addEventListener('keydown',function(e){if(e.key==='Escape'&&!$('auth').hidden)hideAuth()});
$('authForm').onsubmit=function(e){
  e.preventDefault();
  var n=$('aName').value.trim(),m=$('aMail').value.trim().toLowerCase(),p=$('aPw').value,err=$('aErr');
  if(!m||!p||(mode==='reg'&&!n)){err.textContent='Bitte alle Felder ausfüllen.';return}
  if(!/^\S+@\S+\.\S+$/.test(m)){err.textContent='Bitte gib eine gültige E-Mail-Adresse ein.';return}
  var idx=users.findIndex(function(u){return u.mail===m});
  if(mode==='reg'){
    if(idx>-1){err.textContent='Zu dieser E-Mail gibt es schon ein Konto. Bitte melde dich an.';return}
    if(p.length<6){err.textContent='Das Passwort braucht mindestens 6 Zeichen.';return}
    users.push({name:n,mail:m,pw:p,color:colors[users.length%colors.length],todos:[]});cur=users.length-1;
  }else{
    if(idx<0||users[idx].pw!==p){err.textContent='E-Mail oder Passwort stimmt nicht.';return}
    cur=idx;
  }
  hideAuth();openForm(false);render();
};
$('legend').onclick=function(e){var b=e.target.closest('button');if(!b)return;var u=users[+b.getAttribute('data-l')];u.show=u.show===false;render()};
mini();days();render();$('grid').scrollTop=46*7;