mergeInto(LibraryManager.library, {

	// https://developers.google.com/analytics/devguides/collection/analyticsjs/events
	
	GoogleWebSetUser: function(key, value) {
		window.ga('set', UTF8ToString(key), UTF8ToString(value));
	},
	
	GoogleWebEvent2: function(category, action) {
		window.ga('send', {
			hitType: 'event',
			eventCategory: UTF8ToString(category),
			eventAction: UTF8ToString(action)
		});
	},
	
	GoogleWebEvent3: function(category, action, label) {
		window.ga('send', {
			hitType: 'event',
			eventCategory: UTF8ToString(category),
			eventAction: UTF8ToString(action),
			eventLabel: UTF8ToString(label)
		});
	},
	
	GoogleWebEvent4: function(category, action, label, value) {
		window.ga('send', {
			hitType: 'event',
			eventCategory: UTF8ToString(category),
			eventAction: UTF8ToString(action),
			eventLabel: UTF8ToString(label),
			value: value
		});
	},
	
	GoogleWebView: function(view) {
		window.ga('send', 'pageview', UTF8ToString(view));
	},
	
	GoogleWebRevenue: function(transactionId, revenue, itemName, itemCategory) {
		window.ga('ecommerce:addTransaction', {
			'id': UTF8ToString(transactionId),
			'revenue': UTF8ToString(revenue)
		});

		window.ga('ecommerce:addItem', {
			'id': UTF8ToString(transactionId),
			'name': UTF8ToString(itemName),
			'category': UTF8ToString(itemCategory),
			'price': UTF8ToString(revenue),
			'quantity': '1'
		});
		
		window.ga('ecommerce:send');
	}
});