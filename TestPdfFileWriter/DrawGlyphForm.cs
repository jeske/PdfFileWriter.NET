/////////////////////////////////////////////////////////////////////
//
//	TestPdfFileWriter
//	Test/demo program for PdfFileWrite C# Class Library.
//
//	DrawGlyphForm
//  Display one glyph.
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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TestPdfFileWriter
{
/////////////////////////////////////////////////////////////////////
// Draw one glyph form
/////////////////////////////////////////////////////////////////////

public partial class DrawGlyphForm : Form
	{
	private FontFamily	FontFamily;
	private FontStyle	Style;
	private Int32		CharCode;
	private GraphicsPath GP;
	private RectangleF	Box;
	private Double		ScaleFactor;
	private Double		OriginX;
	private Double		OriginY;
	private Double		PenWidth;

	/////////////////////////////////////////////////////////////////////
	// Constructor
	/////////////////////////////////////////////////////////////////////

	public DrawGlyphForm
			(
			FontFamily	FontFamily,
			FontStyle	Style,
			Int32		CharCode
			)
		{
		this.FontFamily = FontFamily;
		this.Style = Style;
		this.CharCode = CharCode;
		InitializeComponent();
		return;
		}

	/////////////////////////////////////////////////////////////////////
	// Class initialization
	/////////////////////////////////////////////////////////////////////

	private void OnLoad
			(
			object sender,
			EventArgs e
			)
		{
		FillColorButton.BackColor = Color.Blue;
		OutlineColorButton.BackColor = Color.Red;
		FormatComboBox.SelectedIndex = 0;

		// convert character to graphics path
		GP = new GraphicsPath();
		GP.AddString(((Char) CharCode).ToString(), FontFamily, (Int32) Style, 1000, Point.Empty, StringFormat.GenericDefault);
		Box = GP.GetBounds();

		// force resize to calculate scale and origin
		OnResize(null, null);

		// force OnPaint
		Invalidate();
		return;
		}

	/////////////////////////////////////////////////////////////////////
	// Paint user selected character 
	/////////////////////////////////////////////////////////////////////

	private void OnPaint
			(
			object sender,
			PaintEventArgs e
			)
		{
		// shortcut
		Graphics G = e.Graphics;
		Pen OutlinePen = new Pen(OutlineColorButton.BackColor, (Single) PenWidth);
		OutlinePen.MiterLimit = 2;

		G.PageScale = (Single) ScaleFactor;
		G.TranslateTransform((Single) OriginX, (Single) OriginY);
		if(FormatComboBox.SelectedIndex == 0 || FormatComboBox.SelectedIndex == 2) G.FillPath(new SolidBrush(FillColorButton.BackColor), GP);
		if(FormatComboBox.SelectedIndex == 1 || FormatComboBox.SelectedIndex == 2) G.DrawPath(OutlinePen, GP);
		return;
		}

	/////////////////////////////////////////////////////////////////////
	// User changed fill/stroke setting
	/////////////////////////////////////////////////////////////////////

	private void OnOutlineChanged
			(
			object sender,
			EventArgs e
			)
		{
		Invalidate();
		return;
		}


	private void OnFillColor
			(
			object sender,
			EventArgs e
			)
		{
		// dialog box
		ColorDialog Dialog = new ColorDialog();
		Dialog.AllowFullOpen = true;
		Dialog.FullOpen = true;
		Dialog.SolidColorOnly = true;
		Dialog.AnyColor = true;
		Dialog.Color = ((Button) sender).BackColor;
		if(Dialog.ShowDialog(this) == DialogResult.OK) ((Button) sender).BackColor = Dialog.Color;
		Dialog.Dispose();
		}

	/////////////////////////////////////////////////////////////////////
	// User is resizing windows form
	/////////////////////////////////////////////////////////////////////

	private void OnResize
			(
			object sender,
			EventArgs e
			)
		{
		// protect against minimize button
		if(ClientSize.Width == 0) return;

		// buttons
		ButtonsGroupBox.Left = (ClientSize.Width - ButtonsGroupBox.Width) / 2;
		ButtonsGroupBox.Top = ClientSize.Height - ButtonsGroupBox.Height - 4;

		// penwidth
		PenWidth = 0.01 * Math.Sqrt(Box.Width * Box.Width + Box.Height * Box.Height);

		// reset origin
		OriginX = -Box.X + 0.1 * Box.Width;
		OriginY = -Box.Y + 0.1 * Box.Height;

		// width
		Double Width = 1.2 * Box.Width;

		// height
		Double Height = 1.2 * Box.Height;

		// scale factor from user to screen
		ScaleFactor = (Double) ClientSize.Width / Width;
		Double ScaleFactorY = (Double) ButtonsGroupBox.Top / Height;
		if(ScaleFactorY < ScaleFactor) ScaleFactor = ScaleFactorY;
		OriginX += ((Double) ClientSize.Width / ScaleFactor - Width) / 2;
		OriginY += ((Double) ButtonsGroupBox.Top / ScaleFactor - Height) / 2;

		// force OnPaint
		Invalidate();
		return;
		}
	}
}
