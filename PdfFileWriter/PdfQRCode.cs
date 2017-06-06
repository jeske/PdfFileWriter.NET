/////////////////////////////////////////////////////////////////////
//
//	PdfFileWriter
//	PDF File Write C# Class Library.
//
//	PdfQRCode
//	Display QR Code as image resource.
//
//	Granotech Limited
//	Author: Uzi Granot
//	Version: 1.0
//	Date: April 1, 2013
//	Copyright (C) 2013-2016 Granotech Limited. All Rights Reserved
//
//	PdfFileWriter C# class library and TestPdfFileWriter test/demo
//  application are free software.
//	They is distributed under the Code Project Open License (CPOL).
//	The document PdfFileWriterReadmeAndLicense.pdf contained within
//	the distribution specify the license agreement and other
//	conditions and notes. You must read this document and agree
//	with the conditions specified in order to use this software.
//
//	For version history please refer to PdfDocument.cs
//
/////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace PdfFileWriter
{
/// <summary>
/// QR Code error correction code enumeration
/// </summary>
public enum ErrorCorrection
	{
	/// <summary>
	/// Low
	/// </summary>
	L,

	/// <summary>
	/// Medium
	/// </summary>
	M,

	/// <summary>
	/// Medium-high
	/// </summary>
	Q,

	/// <summary>
	/// High
	/// </summary>
	H
	};

/// <summary>
/// PDF QR Code resource class
/// </summary>
/// <remarks>
/// <para>
/// The QR Code object is a PDF Image object.
/// </para>
/// <para>
/// For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#QRCodeSupport">2.8 QR Code Support</a>
/// </para>
/// </remarks>
public class PdfQRCode : PdfImage
	{
	/// <summary>
	/// Gets matrix dimension.
	/// </summary>
	public Int32 MatrixDimension {get; private set;}

	/// <summary>
	/// Segment marker
	/// </summary>
	public const Char SegmentMarker = (Char) 256;

	////////////////////////////////////////////////////////////////////
	/// <summary>
	/// PDF QR Code constructor
	/// </summary>
	/// <param name="Document">Parent PDF document.</param>
	/// <param name="DataString">Data string to encode.</param>
	/// <param name="ErrorCorrection">Error correction code.</param>
	/// <param name="QuietZone">Quiet zone</param>
	////////////////////////////////////////////////////////////////////
	public PdfQRCode
			(
			PdfDocument		Document,
			String			DataString,
			ErrorCorrection	ErrorCorrection,
			Int32			QuietZone = 4
			) : base(Document)
		{
		// PdfQRCode constructor helper
		ConstructorHelper(DataString, null, ErrorCorrection, QuietZone);
		return;
		}

	////////////////////////////////////////////////////////////////////
	/// <summary>
	/// PDF QR Code constructor
	/// </summary>
	/// <param name="Document">Parent PDF document.</param>
	/// <param name="SegDataString">Data string array to encode.</param>
	/// <param name="ErrorCorrection">Error correction code.</param>
	/// <param name="QuietZone">Quiet zone</param>
	/// <remarks>
	/// The program will calculate the best encoding mode for each segment.
	/// </remarks>
	////////////////////////////////////////////////////////////////////
	public PdfQRCode
			(
			PdfDocument		Document,
			String[]		SegDataString,
			ErrorCorrection	ErrorCorrection,
			Int32			QuietZone = 4
			) : base(Document)
		{
		// PdfQRCode constructor helper
		ConstructorHelper(null, SegDataString, ErrorCorrection, QuietZone);
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Write object to PDF file
	////////////////////////////////////////////////////////////////////

	internal void ConstructorHelper
			(
			String			DataString,
			String[]		SegDataString,
			ErrorCorrection	ErrorCorrection,
			Int32			QuietZone = 4
			)
		{
		// create QR Code object
		QREncoder Encoder = new QREncoder();
		if(DataString != null)
			Encoder.EncodeQRCode(DataString, ErrorCorrection, QuietZone);
		else
			Encoder.EncodeQRCode(SegDataString, ErrorCorrection, QuietZone);

		// output matrix
		// NOTE: Black=true, White=flase
		BWImage = Encoder.OutputMatrix;

		// image width and height in pixels
		MatrixDimension = Encoder.MatrixDimension;
		WidthPix = MatrixDimension + 2 * QuietZone;
		HeightPix = WidthPix;

		// image control for QR code
		ImageControl = new PdfImageControl();
		ImageControl.ReverseBW = true;
		ImageControl.SaveAs = SaveImageAs.BWImage;

		// write stream object
		WriteObjectToPdfFile();
		return;
		}
	}
}
