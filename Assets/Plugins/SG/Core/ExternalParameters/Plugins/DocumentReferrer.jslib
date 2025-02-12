mergeInto(LibraryManager.library, {
  
	GetDocumentReferrer: function () {
		var text = document.referrer;
		var bufferSize = lengthBytesUTF8(text) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(text, buffer, bufferSize);
		return buffer;
	}
  
});
