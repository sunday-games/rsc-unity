mergeInto(LibraryManager.library, {	
	
	// https://developers.facebook.com/docs/facebook-pixel/implementation/conversion-tracking
	// https://developers.facebook.com/docs/facebook-pixel/reference#standard-events

	FacebookWebEvent: function(name) {
		window.fbq('trackCustom', UTF8ToString(name));
	},
	
	FacebookWebEventParameters: function(name, parameters) {
		window.fbq('trackCustom', UTF8ToString(name), JSON.parse(UTF8ToString(parameters) || ""));
	},
	
	FacebookWebRevenue: function(transactionId, productName, productCurrency, productRevenue) {
		window.fbq('track', 'Purchase', {
			content_name: UTF8ToString(productName),
			currency: UTF8ToString(productCurrency),
			value: productRevenue,
			transactionId: UTF8ToString(transactionId)
		});
	},
	
	// FacebookWebRevenueProduct: function(transactionId, productId, productCurrency, productRevenue) {
	//	window.fbq('track', 'Purchase', {
	//		contents: [ { id: UTF8ToString(productId), quantity: 1 } ],
	//		content_type: 'product',
	//		currency: UTF8ToString(productCurrency),
	//		value: productRevenue,
	//		transactionId: UTF8ToString(transactionId)
	//	});
	// },

	FacebookWebView: function(name) {
		window.fbq('track', 'ViewContent', {
			content_name: UTF8ToString(name)
		});
	}
});