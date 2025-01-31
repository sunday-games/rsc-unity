// Copying
function createClipboard(window) {

  class Clipboard {
    constructor() {
      this._copyEventHandler = null;
    }

    _createOnCopyEvent(data) {
      return function(e) {
        e.clipboardData.setData('text/plain', data);
        e.preventDefault(); // We want our data, not data from any selection, to be written to the clipboard
      }
    }

    copyToClipboard(data){
      if(!!this._copyEventHandler) { window.document.removeEventListener('copy', this._copyEventHandler); }
      this._copyEventHandler = this._createOnCopyEvent(data);
      window.document.addEventListener('copy', this._copyEventHandler);
    }
  }

  return new Clipboard();
}

window.createClipboard = createClipboard;

// Pasting
const dT = new DataTransfer();
const evt = new ClipboardEvent('paste', { clipboardData: dT });
document.dispatchEvent(evt);

document.onpaste = function(e){
  const clipdata = e.clipboardData.getData('text/plain');
  console.log('onpaste: ', clipdata);
  unityInstance.SendMessage('Clipboard', 'NotifyPaste', clipdata);
};
