
function enable() {
    let id = document.getElementById('id');
    const x = new XMLHttpRequest();
    x.addEventListener("load", payLoaded);
    x.open("GET", "server.php?id="+ document.getElementById('id').value)
    x.send()
}

function payLoaded() {
    console.log(this.responseText)
    if(this.responseText == 1) {
        document.getElementById('onOffLock').checked = true
    }
    else {
        document.getElementById('onOffLock').checked = false
    }
    document.getElementById('id').disabled = true
    document.getElementById('onOffLock').disabled = false
}

function changeLock() {
    let id = document.getElementById('id');
        
    let a = null;
    a = new XMLHttpRequest();
    a.open("GET", "./server.php?change="+id.value, true);
    a.send()
}

