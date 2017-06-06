
# PDF File Writer .NET 

Developed by Uzi Granot

Released under the Code Project Open License

Source Imported from PDF File Writer C# Class Library (Version 1.19.1 Enhancement: Document Links)

For a tutorial, please see [PDF File Writer Class Library @ CodeProject](https://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version)

PdfFileWriter currently only works under Windows, at least because:

* It uses Windows DLLImport calls to access detailed font metrics and glyphs, to embed them in the PDF
* It reuses datatypes from System.Drawing, WPF, and System.Windows.Forms

The latter should be easy to change, as PDF File Writer doesn't rasterize PDF, it just reuses datatypes for convenience. The font calls would need to be stubbed out and implemented portably across platforms. 

