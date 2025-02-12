mergeInto(LibraryManager.library, {	
	CopyToClipboard: function(text) {
		window.Clipboard.copyToClipboard(UTF8ToString(text));
	}
});