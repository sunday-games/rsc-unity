function createCheckout(window, notifyUnity) {
    function formatResponse(success, name, value){
        return {
            success: success,
            [name]: value
        };
    }

    function formatErrorResponse(message) {
        return formatResponse(false, "error", message);
    }

    function formatSuccessResponse(message) {
        return formatResponse(true, "data", message);
    }

    window.addEventListener("message", async (event) => {
        if (event.origin !== location.protocol + "//" + location.host)
            return;

        if(event.data["command"] === 'checkout') {
            if(!!event.data["error"]) {
                let responce = formatErrorResponse(event.data["error"]);

                if(!!event.data["data"]) {
                    responce["data"] = event.data["data"]
                }

                notifyUnity("Checkout", responce);
                return;
            }

            notifyUnity("Checkout", formatSuccessResponse(event.data["data"]))
        }
    }, false);

    if(!window.__STATE__) {
        window.__STATE__ = {};
    }

    return {
        open: (json) => {
            // current request id
            const key = Math.trunc(Math.random() * 10000000);

			      const config = JSON.parse(json || "");

            // set state
            window.__STATE__[key] = config;

            let htmlName = config['env'] === 'production' ? 'checkout.html' : 'checkout.sandbox.html';

            window.Popup.centered(
              htmlName + "?key=" + key,
              'Checkout',
              config['form']['width'],
              config['form']['height'],
              'yes',
              config['callback']['function']);
        }
    };
}

window.createCheckout = createCheckout;