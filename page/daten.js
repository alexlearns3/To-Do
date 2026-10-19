/* Gemeinsame Daten für index.html und register.html */
var COLORS=['#b9b6ff','#f6c1d9','#ffd98a','#9fe3c8','#a9d1ff'];

var DEFAULT_USERS=[
  {name:'Tom',mail:'tom@beispiel.at',pw:'tom123',color:'#b9b6ff',todos:[{d:'2026-10-12',t:'10:00',w:'laufen gehen'},{d:'2026-10-15',t:'11:00',w:'Deutsch lernen'}]},
  {name:'Mia',mail:'mia@beispiel.at',pw:'mia123',color:'#f6a9c9',todos:[{d:'2026-10-13',t:'17:00',w:'Essen gehen'},{d:'2026-10-17',t:'13:00',w:'Mathe lernen'},{d:'2026-10-15',t:'11:00',w:'Mathe lernen'}]}
];

/* Alle Konten (bleiben im Browser gespeichert) */
function loadUsers(){
  try{
    var s=localStorage.getItem('kal_users');
    if(s){var a=JSON.parse(s);if(Array.isArray(a)&&a.length)return a}
  }catch(e){}
  return JSON.parse(JSON.stringify(DEFAULT_USERS));
}
function saveUsers(u){
  try{localStorage.setItem('kal_users',JSON.stringify(u))}catch(e){}
}

/* Wer gerade angemeldet ist (gilt, solange der Tab offen ist) */
function loadSession(){
  try{
    var l=JSON.parse(sessionStorage.getItem('kal_session')||'[]');
    return Array.isArray(l)?l:[];
  }catch(e){return []}
}
function saveSession(l){
  try{sessionStorage.setItem('kal_session',JSON.stringify(l))}catch(e){}
}
