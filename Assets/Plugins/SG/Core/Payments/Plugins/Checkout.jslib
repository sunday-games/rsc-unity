mergeInto(LibraryManager.library, {
	OpenCheckout: function(config) {
        //{
        //     gateway: "paypal",
        //     payload: "", // our client auth data
        //     env: "production" or "sandbox"
		//	   client: { "sandbox":"sandbox clientId", "production":"production clientId" },
        //     createLink: "https://qa.0xuniverse.com/universe-server/orders/paypal/create",
        //     captureLink: "https://qa.0xuniverse.com/universe-server/orders/paypal/capture",
        //     form: { "width":520, "height":640, imgLink:"", name:"", description:"", price:"50 USD" },
		//	   callback: { "object":"Fiat", "function":"NotifyCheckout" },
		//	   or
        //     gateway: "cardpay",
        //     redirectLink: "",
		//	   form: { "width":520, "height":640 },
		//	   callback: { "object":"Fiat", "function":"NotifyCheckout" },
        // };

		window.Checkout.open(UTF8ToString(config));
	}
});