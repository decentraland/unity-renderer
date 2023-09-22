
Nat's WebGL FileSaver
============

Use this plugin to download/save files from inside a WebGL Build.
Built using the (MIT-licensed) FileSaver.js library, made by Eli Grey. (http://purl.eligrey.com/github/FileSaver.js )

This was created for https://nateonus.itch.io/natvox , an online Voxel Editor for Game Development.
https://twitter.com/nateonus for updates.


Usage
------------------

The WebGLFileSaver class has two static functions to use:
1) `public static bool IsSavingSupported()`
	This returns a boolean value of whether saving is supported on the current OS and Browser or not.

2) `public static void SaveFile(string content, string filename, string MIMEType)`
	`public static void SaveFile(byte[] content, string filename, string MIMEType)`
	This prompts the user to download a file named 'filename', with the content 'content'.
	The MIMEType is the file type that will allow the browser to open the file with a default program.
		It can be set to any values in the IANA Media Types (https://www.iana.org/assignments/media-types/media-types.xhtml )
		It's default is set to "text/plain;charset=utf-8" (a plain-text file).
	SaveFile will not run if saving is not supported.
	

Browser Support
------------------

To view which browsers are supported, visit https://github.com/eligrey/FileSaver.js#supported-browsers

	