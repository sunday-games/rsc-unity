function createPopup(window, notifyUnity){

  return {
    __AUTO_CLOSE: false,
    __current: undefined,

    triggerAutoClose: () => {
      this["__AUTO_CLOSE"] = true;
    },

    centered: async (url,winName,w,h,scroll,callbackFunction) => {
      LeftPosition = (screen.width) ? (screen.width-w)/2 : 0;
      TopPosition = (screen.height) ? (screen.height-h)/2 : 0;
      settings = 'height='+h+',width='+w+',top='+TopPosition+',left='+LeftPosition+',scrollbars='+scroll+',resizable';
      this.__current = window.open(url, winName, settings);

      const checkIfClosed = setInterval(function() {
        if (this.__current.closed) {
          clearInterval(checkIfClosed);

          if(!!this["__AUTO_CLOSE"]) { this["__AUTO_CLOSE"] = false; return;}

          notifyUnity(callbackFunction, {
            success: false,
            error: "OperationCanceled"
          });
        }
      }, 100);
    }
  }
}

window.createPopup = createPopup;