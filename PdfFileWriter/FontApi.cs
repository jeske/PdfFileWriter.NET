/////////////////////////////////////////////////////////////////////
//
//	PdfFileWriter
//	PDF File Write C# Class Library.
//
//	FontApi
//	Support for Windows API functions related to fonts and glyphs.
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
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace PdfFileWriter
{
////////////////////////////////////////////////////////////////////
/// <summary>
/// One character/Glyph information class
/// </summary>
/// <remarks>
/// This class defines all the information required to display a
/// character in the output document. Each character has an
/// associated glyph. The glyph geometry is defined in a square.
/// The square is DesignHeight by DesignHeight.
/// </remarks>
////////////////////////////////////////////////////////////////////
public class CharInfo : IComparable<CharInfo>
	{
	/// <summary>
	/// Character code
	/// </summary>
	public Int32 CharCode {get; internal set;}

	/// <summary>
	/// Glyph index
	/// </summary>
	public Int32 GlyphIndex {get; internal set;}

	/// <summary>
	/// Active character
	/// </summary>
	public Boolean ActiveChar {get; internal set;}

	/// <summary>
	/// Character code is greater than 255
	/// </summary>
	public Boolean Type0Font {get; internal set;}

	/// <summary>
	/// Bounding box left in design units
	/// </summary>
	public Int32 DesignBBoxLeft {get; internal set;}

	/// <summary>
	/// Bounding box bottom in design units
	/// </summary>
	public Int32 DesignBBoxBottom {get; internal set;}

	/// <summary>
	/// Bounding box right in design units
	/// </summary>
	public Int32 DesignBBoxRight {get; internal set;}

	/// <summary>
	/// Bounding box top in design units
	/// </summary>
	public Int32 DesignBBoxTop {get; internal set;}

	/// <summary>
	/// Character width in design units
	/// </summary>
	public Int32 DesignWidth {get; internal set;}

	/// <summary>
	/// Character height in design units
	/// </summary>
	//public Int32 DesignHeight {get; internal set;}

	internal Int32		NewGlyphIndex;
	internal Byte[]		GlyphData;
	internal Boolean	Composite;

	////////////////////////////////////////////////////////////////////
	// constructor
	////////////////////////////////////////////////////////////////////

	internal CharInfo
			(
			Int32		CharCode,
			Int32		GlyphIndex,
			FontApi		DC
			)
		{
		// save char code and glyph index
		this.CharCode = CharCode;
		this.GlyphIndex = GlyphIndex;
		this.NewGlyphIndex = -1;
		Type0Font = CharCode >= 256 || GlyphIndex == 0;
		
		// Bounding Box
		Int32 BBoxWidth = DC.ReadInt32();
		Int32 BBoxHeight = DC.ReadInt32();
		DesignBBoxLeft = DC.ReadInt32();
		DesignBBoxTop = DC.ReadInt32();
		DesignBBoxRight = DesignBBoxLeft + BBoxWidth;
		DesignBBoxBottom = DesignBBoxTop - BBoxHeight;

		// glyph advance horizontal and vertical
		DesignWidth = DC.ReadInt16();
		//DesignHeight = DC.ReadInt16();
		return;
		}

	////////////////////////////////////////////////////////////////////
	// constructor for search and sort
	////////////////////////////////////////////////////////////////////

	internal CharInfo
			(
			Int32		GlyphIndex
			)
		{
		// save char code and glyph index
		this.GlyphIndex = GlyphIndex;
		return;
		}

	/// <summary>
	/// Compare two glyphs for sort and binary search
	/// </summary>
	/// <param name="Other">Other CharInfo</param>
	/// <returns>Compare result</returns>
	public Int32 CompareTo
			(
			CharInfo Other
			)
		{
		return(this.GlyphIndex - Other.GlyphIndex);
		}
	}

////////////////////////////////////////////////////////////////////
// IComparer class for new glyph index sort
////////////////////////////////////////////////////////////////////

internal class SortByNewIndex : IComparer<CharInfo>
	{
	public Int32 Compare
			(
			CharInfo CharOne,
			CharInfo CharTwo
			)
		{
		return(CharOne.NewGlyphIndex - CharTwo.NewGlyphIndex);
		}
	}

////////////////////////////////////////////////////////////////////
/// <summary>
/// Font box class
/// </summary>
/// <remarks>
/// FontBox class is part of OUTLINETEXTMETRIC structure
/// </remarks>
////////////////////////////////////////////////////////////////////
public class FontBox
	{
	/// <summary>
	/// Gets left side.
	/// </summary>
	public Int32		Left {get; private set;} 

	/// <summary>
	/// Gets top side.
	/// </summary>
	public Int32		Top {get; private set;} 

	/// <summary>
	/// Gets right side.
	/// </summary>
	public Int32		Right {get; private set;} 

	/// <summary>
	/// Gets bottom side.
	/// </summary>
	public Int32		Bottom {get; private set;}

	internal FontBox
			(
			FontApi DC
			)
		{
		Left = DC.ReadInt32();
		Top = DC.ReadInt32(); 
		Right = DC.ReadInt32(); 
		Bottom = DC.ReadInt32();
		return;
		}
	}

////////////////////////////////////////////////////////////////////
/// <summary>
/// Panose class
/// </summary>
/// <remarks>
/// The PANOSE structure describes the PANOSE font-classification
/// values for a TrueType font. These characteristics are then
/// used to associate the font with other fonts of similar
/// appearance but different names.
/// </remarks>
////////////////////////////////////////////////////////////////////
public class WinPanose
	{
	/// <summary>
	/// Panose family type
	/// </summary>
	public Byte			bFamilyType {get; private set;}

	/// <summary>
	/// Panose serif style
	/// </summary>
	public Byte			bSerifStyle {get; private set;}

	/// <summary>
	/// Panose weight
	/// </summary>
	public Byte			bWeight {get; private set;}

	/// <summary>
	/// Panose proportion
	/// </summary>
	public Byte			bProportion {get; private set;}

	/// <summary>
	/// Panose contrast
	/// </summary>
	public Byte			bContrast {get; private set;}

	/// <summary>
	/// Panose stroke variation
	/// </summary>
	public Byte			bStrokeVariation {get; private set;}

	/// <summary>
	/// Panose arm style
	/// </summary>
	public Byte			bArmStyle {get; private set;}

	/// <summary>
	/// Panose letter form
	/// </summary>
	public Byte			bLetterform {get; private set;}

	/// <summary>
	/// Panose mid line
	/// </summary>
	public Byte			bMidline {get; private set;}

	/// <summary>
	/// Panose X height
	/// </summary>
	public Byte			bXHeight {get; private set;}

	internal WinPanose
			(
			FontApi DC
			)
		{
		bFamilyType = DC.ReadByte();
		bSerifStyle = DC.ReadByte();
		bWeight = DC.ReadByte();
		bProportion = DC.ReadByte();
		bContrast = DC.ReadByte();
		bStrokeVariation = DC.ReadByte();
		bArmStyle = DC.ReadByte();
		bLetterform = DC.ReadByte();
		bMidline = DC.ReadByte();
		bXHeight = DC.ReadByte();
		return;
		}
	}

////////////////////////////////////////////////////////////////////
/// <summary>
/// Kerning pair class
/// </summary>
////////////////////////////////////////////////////////////////////
public class WinKerningPair : IComparable<WinKerningPair>
	{
	/// <summary>
	/// Gets first character
	/// </summary>
	public Char			First {get; private set;}

	/// <summary>
	/// Gets second character
	/// </summary>
	public Char			Second {get; private set;}

	/// <summary>
	/// Gets kerning amount in design units
	/// </summary>
	public Int32		KernAmount {get; private set;}

	internal WinKerningPair
			(
			FontApi DC
			)
		{
		First = DC.ReadChar();
		Second = DC.ReadChar();
		KernAmount = DC.ReadInt32();
		return;
		}

	/// <summary>
	/// Kerning pair constructor
	/// </summary>
	/// <param name="First">First character</param>
	/// <param name="Second">Second character</param>
	public WinKerningPair
			(
			Char	First,
			Char	Second
			)
		{
		this.First = First;
		this.Second = Second;
		return;
		}

	/// <summary>
	/// Compare kerning pairs
	/// </summary>
	/// <param name="Other">Other pair</param>
	/// <returns>Compare result</returns>
	public Int32 CompareTo
			(
			WinKerningPair	Other
			)
		{
		return(this.First != Other.First ? this.First - Other.First : this.Second - Other.Second);
		}
	}

////////////////////////////////////////////////////////////////////
/// <summary>
/// TextMetric class
/// </summary>
/// <remarks>
/// The TEXTMETRIC structure contains basic information about a
/// physical font. All sizes are specified in logical units;
/// that is, they depend on the current mapping mode of the
/// display context.
/// </remarks>
////////////////////////////////////////////////////////////////////
public class WinTextMetric
 	{
	/// <summary>
	/// TextMetric height
	/// </summary>
	public Int32		tmHeight {get; private set;}
	
	/// <summary>
	/// TextMetric ascent
	/// </summary>
	public Int32		tmAscent {get; private set;}
	
	/// <summary>
	/// TextMetric descent
	/// </summary>
	public Int32		tmDescent {get; private set;}
	
	/// <summary>
	/// TextMetric internal leading
	/// </summary>
	public Int32		tmInternalLeading {get; private set;}
	
	/// <summary>
	/// TextMetric external leading
	/// </summary>
	public Int32		tmExternalLeading {get; private set;}
	
	/// <summary>
	/// TextMetric average character width
	/// </summary>
	public Int32		tmAveCharWidth {get; private set;}
	
	/// <summary>
	/// TextMetric maximum character width
	/// </summary>
	public Int32		tmMaxCharWidth {get; private set;}
	
	/// <summary>
	/// TextMetric height
	/// </summary>
	public Int32		tmWeight {get; private set;}
	
	/// <summary>
	/// TextMetric overhang
	/// </summary>
	public Int32		tmOverhang {get; private set;}
	
	/// <summary>
	/// TextMetric digitize aspect X
	/// </summary>
	public Int32		tmDigitizedAspectX {get; private set;}
	
	/// <summary>
	/// TextMetric digitize aspect Y
	/// </summary>
	public Int32		tmDigitizedAspectY {get; private set;}
	
	/// <summary>
	/// TextMetric first character
	/// </summary>
	public UInt16		tmFirstChar {get; private set;}
	
	/// <summary>
	/// TextMetric last character
	/// </summary>
	public UInt16		tmLastChar {get; private set;}
	
	/// <summary>
	/// TextMetric default character
	/// </summary>
	public UInt16		tmDefaultChar {get; private set;}
	
	/// <summary>
	/// TextMetric break character
	/// </summary>
	public UInt16		tmBreakChar {get; private set;}
	
	/// <summary>
	/// TextMetric italic
	/// </summary>
	public Byte			tmItalic {get; private set;}
	
	/// <summary>
	/// TextMetric underlined
	/// </summary>
	public Byte			tmUnderlined {get; private set;}
	
	/// <summary>
	/// TextMetric struck out
	/// </summary>
	public Byte			tmStruckOut {get; private set;}
	
	/// <summary>
	/// TextMetric pitch and family
	/// </summary>
	public Byte			tmPitchAndFamily {get; private set;}
	
	/// <summary>
	/// TextMetric character set
	/// </summary>
	public Byte			tmCharSet {get; private set;}

	internal WinTextMetric
			(
			FontApi DC
			)
		{
		tmHeight = DC.ReadInt32();
		tmAscent = DC.ReadInt32();
		tmDescent = DC.ReadInt32();
		tmInternalLeading = DC.ReadInt32();
		tmExternalLeading = DC.ReadInt32();
		tmAveCharWidth = DC.ReadInt32();
		tmMaxCharWidth = DC.ReadInt32();
		tmWeight = DC.ReadInt32();
		tmOverhang = DC.ReadInt32();
		tmDigitizedAspectX = DC.ReadInt32();
		tmDigitizedAspectY = DC.ReadInt32();
		tmFirstChar = DC.ReadUInt16();
		tmLastChar = DC.ReadUInt16();
		tmDefaultChar = DC.ReadUInt16();
		tmBreakChar = DC.ReadUInt16();
		tmItalic = DC.ReadByte();
		tmUnderlined = DC.ReadByte();
		tmStruckOut = DC.ReadByte();
		tmPitchAndFamily = DC.ReadByte();
		tmCharSet = DC.ReadByte();
		return;
		}
	}

////////////////////////////////////////////////////////////////////
/// <summary>
/// Outline text metric class
/// </summary>
/// <remarks>
/// The OUTLINETEXTMETRIC structure contains metrics describing
/// a TrueType font.
/// </remarks>
////////////////////////////////////////////////////////////////////
public class WinOutlineTextMetric
	{
	/// <summary>
	/// Outline text metric size
	/// </summary>
	public UInt32		otmSize {get; private set;}

	/// <summary>
	/// Outline text metric TextMetric
	/// </summary>
	public WinTextMetric otmTextMetric {get; private set;}

	/// <summary>
	/// Outline text metric panose number
	/// </summary>
	public WinPanose	otmPanoseNumber {get; private set;}

	/// <summary>
	/// Outline text metric FS selection
	/// </summary>
	public UInt32		otmfsSelection {get; private set;}

	/// <summary>
	/// Outline text metric FS type
	/// </summary>
	public UInt32		otmfsType {get; private set;}

	/// <summary>
	/// Outline text metric char slope rise
	/// </summary>
	public Int32		otmsCharSlopeRise {get; private set;}

	/// <summary>
	/// Outline text metric char slope run
	/// </summary>
	public Int32		otmsCharSlopeRun {get; private set;}

	/// <summary>
	/// Outline text metric italic angle
	/// </summary>
	public Int32		otmItalicAngle {get; private set;}

	/// <summary>
	/// Outline text metric EM square
	/// </summary>
	public UInt32		otmEMSquare {get; private set;}

	/// <summary>
	/// Outline text metric ascent
	/// </summary>
	public Int32		otmAscent {get; private set;}

	/// <summary>
	/// Outline text metric descent
	/// </summary>
	public Int32		otmDescent {get; private set;}

	/// <summary>
	/// Outline text metric line gap
	/// </summary>
	public UInt32		otmLineGap {get; private set;}

	/// <summary>
	/// Outline text metric capital M height
	/// </summary>
	public UInt32		otmsCapEmHeight {get; private set;}

	/// <summary>
	/// Outline text metric X height
	/// </summary>
	public UInt32		otmsXHeight {get; private set;}

	/// <summary>
	/// Outline text metric Font box class
	/// </summary>
	public FontBox		otmrcFontBox {get; private set;}

	/// <summary>
	/// Outline text metric Mac ascent
	/// </summary>
	public Int32		otmMacAscent {get; private set;}

	/// <summary>
	/// Outline text metric Mac descent
	/// </summary>
	public Int32		otmMacDescent {get; private set;}

	/// <summary>
	/// Outline text metric Mac line gap
	/// </summary>
	public UInt32		otmMacLineGap {get; private set;}

	/// <summary>
	/// Outline text metric minimum PPEM
	/// </summary>
	public UInt32		otmusMinimumPPEM {get; private set;}

	/// <summary>
	/// Outline text metric subscript size
	/// </summary>
	public Point		otmptSubscriptSize {get; private set;}

	/// <summary>
	/// Outline text metric subscript offset
	/// </summary>
	public Point		otmptSubscriptOffset {get; private set;}

	/// <summary>
	/// Outline text metric superscript size
	/// </summary>
	public Point		otmptSuperscriptSize {get; private set;}

	/// <summary>
	/// Outline text metric superscript offset
	/// </summary>
	public Point		otmptSuperscriptOffset {get; private set;}

	/// <summary>
	/// Outline text metric strikeout size
	/// </summary>
	public UInt32		otmsStrikeoutSize {get; private set;}

	/// <summary>
	/// Outline text metric strikeout position
	/// </summary>
	public Int32		otmsStrikeoutPosition {get; private set;}

	/// <summary>
	/// Outline text metric underscore size
	/// </summary>
	public Int32		otmsUnderscoreSize {get; private set;}

	/// <summary>
	/// Outline text metric underscore position
	/// </summary>
	public Int32		otmsUnderscorePosition {get; private set;}

	/// <summary>
	/// Outline text metric family name
	/// </summary>
	public String		otmpFamilyName {get; private set;}

	/// <summary>
	/// Outline text metric face name
	/// </summary>
	public String		otmpFaceName {get; private set;}

	/// <summary>
	/// Outline text metric style name
	/// </summary>
	public String		otmpStyleName {get; private set;}

	/// <summary>
	/// Outline text metric full name
	/// </summary>
	public String		otmpFullName {get; private set;}

	internal WinOutlineTextMetric
			(
			FontApi DC
			)
		{
		otmSize = DC.ReadUInt32();
		otmTextMetric = new WinTextMetric(DC);
		DC.Align4();
		otmPanoseNumber = new WinPanose(DC);
		DC.Align4();
		otmfsSelection = DC.ReadUInt32();
		otmfsType = DC.ReadUInt32();
		otmsCharSlopeRise = DC.ReadInt32();
		otmsCharSlopeRun = DC.ReadInt32();
		otmItalicAngle = DC.ReadInt32();
		otmEMSquare = DC.ReadUInt32();
		otmAscent = DC.ReadInt32();
		otmDescent = DC.ReadInt32();
		otmLineGap = DC.ReadUInt32();
		otmsCapEmHeight = DC.ReadUInt32();
		otmsXHeight = DC.ReadUInt32();
		otmrcFontBox = new FontBox(DC);
		otmMacAscent = DC.ReadInt32();
		otmMacDescent = DC.ReadInt32();
		otmMacLineGap = DC.ReadUInt32();
		otmusMinimumPPEM = DC.ReadUInt32();
		otmptSubscriptSize = DC.ReadWinPoint();
		otmptSubscriptOffset = DC.ReadWinPoint();
		otmptSuperscriptSize = DC.ReadWinPoint();
		otmptSuperscriptOffset = DC.ReadWinPoint();
		otmsStrikeoutSize = DC.ReadUInt32();
		otmsStrikeoutPosition = DC.ReadInt32();
		otmsUnderscoreSize = DC.ReadInt32();
		otmsUnderscorePosition = DC.ReadInt32();
		otmpFamilyName = DC.ReadString();
		otmpFaceName = DC.ReadString();
		otmpStyleName = DC.ReadString();
		otmpFullName = DC.ReadString();
		return;
		}
	}

////////////////////////////////////////////////////////////////////
/// <summary>
/// Font API class
/// </summary>
/// <remarks>
/// Windows API callable by C# program
/// </remarks>
////////////////////////////////////////////////////////////////////
public class FontApi : IDisposable
	{
	private Bitmap		BitMap;
	private Graphics	GDI;
	private IntPtr		GDIHandle;
	private IntPtr		FontHandle;
	private IntPtr		SavedFont;
	private IntPtr		Buffer;
	private Int32		BufPtr;
	private Int32		DesignHeight;

	////////////////////////////////////////////////////////////////////
	// Device context constructor
	////////////////////////////////////////////////////////////////////

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    private static extern IntPtr SelectObject(IntPtr GDIHandle, IntPtr FontHandle);

	/// <summary>
	/// Font API constructor
	/// </summary>
	/// <param name="DesignFont">Design font</param>
	/// <param name="DesignHeight">Design height</param>
	public FontApi
			(
			Font	DesignFont,
			Int32	DesignHeight
			)
		{
		// save design height
		this.DesignHeight = DesignHeight;

		// define device context
		BitMap = new Bitmap(1, 1);
		GDI = Graphics.FromImage(BitMap);
		GDIHandle = (IntPtr) GDI.GetHdc();

		// select the font into the device context
		FontHandle = DesignFont.ToHfont();
		SavedFont = SelectObject(GDIHandle, FontHandle);

		// exit
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Gets single glyph metric
	////////////////////////////////////////////////////////////////////

    private const UInt32 GGO_METRICS = 0;
    private const UInt32 GGO_BITMAP = 1;
    private const UInt32 GGO_NATIVE = 2;
    private const UInt32 GGO_BEZIER = 3;
    private const UInt32 GGO_GLYPH_INDEX = 128;

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    private static extern Int32 GetGlyphOutline(IntPtr GDIHandle, Int32 CharIndex,
        UInt32 GgoFormat, IntPtr GlyphMetrics, UInt32 Zero, IntPtr Null, IntPtr TransMatrix);

	/// <summary>
	/// Gets glyph metric
	/// </summary>
	/// <param name="CharCode">Character code</param>
	/// <returns>Character info class</returns>
	public CharInfo GetGlyphMetricsApiByCode
			(
			Int32		CharCode
			)
		{
		// get glyph index for char code
		Int32[] GlyphIndexArray = GetGlyphIndicesApi(CharCode, CharCode);

		// get glyph outline
		CharInfo Info = GetGlyphMetricsApiByGlyphIndex(GlyphIndexArray[0]);
		Info.CharCode = CharCode;

		// exit
		return(Info);
		}

	/// <summary>
	/// Gets glyph metric
	/// </summary>
	/// <param name="GlyphIndex">Character code</param>
	/// <returns>Character info class</returns>
	public CharInfo GetGlyphMetricsApiByGlyphIndex
			(
			Int32		GlyphIndex
			)
		{
		// build unit matrix
		IntPtr UnitMatrix = BuildUnitMarix();

		// allocate buffer to receive glyph metrics information
		AllocateBuffer(20);

		// get one glyph
		if(GetGlyphOutline(GDIHandle, GlyphIndex, GGO_GLYPH_INDEX, Buffer, 0, IntPtr.Zero, UnitMatrix) < 0)
			ThrowSystemErrorException("Calling GetGlyphOutline failed");

		// create WinOutlineTextMetric class
		CharInfo Info = new CharInfo(0, GlyphIndex, this);

		// free buffer for glyph metrics
		FreeBuffer();

		// free unit matrix buffer
		Marshal.FreeHGlobal(UnitMatrix);
		
		// exit
		return(Info);
		}

	////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets array of glyph metrics
	/// </summary>
	/// <param name="CharValue">Character code</param>
	/// <returns>Array of character infos</returns>
	////////////////////////////////////////////////////////////////////
	public CharInfo[] GetGlyphMetricsApi
			(
			Int32	CharValue
			)
		{
		// first character of the 256 block
		Int32 FirstChar = CharValue & 0xff00;

		// use glyph index
		Boolean UseGlyphIndex = FirstChar != 0;

		// get character code to glyph index
		// if GlyphIndex[x] is zero glyph is undefined
		Int32[] GlyphIndexArray = GetGlyphIndicesApi(FirstChar, FirstChar + 255);

		// test for at least one valid glyph
		Int32 Start;
		for(Start = 0; Start < 256 && GlyphIndexArray[Start] == 0; Start++);
		if(Start == 256) return(null);

		// build unit matrix
		IntPtr UnitMatrix = BuildUnitMarix();

		// allocate buffer to receive glyph metrics information
		AllocateBuffer(20);

		// result array
		CharInfo[] CharInfoArray = new CharInfo[256];

		// loop for all characters
		for(Int32 CharCode = Start; CharCode < 256; CharCode++)
			{
			// charater not defined
			Int32 GlyphIndex = GlyphIndexArray[CharCode];
			if(GlyphIndex == 0) continue;

			// get one glyph
			if(GetGlyphOutline(GDIHandle, FirstChar + CharCode, GGO_METRICS, Buffer, 0, IntPtr.Zero, UnitMatrix) < 0)
				ThrowSystemErrorException("Calling GetGlyphOutline failed");

			// reset buffer pointer
			BufPtr = 0;

			// create WinOutlineTextMetric class
			CharInfoArray[CharCode] = new CharInfo(FirstChar + CharCode, GlyphIndex, this);
			}

		// free buffer for glyph metrics
		FreeBuffer();

		// free unit matrix buffer
		Marshal.FreeHGlobal(UnitMatrix);
		
		// exit
		return(CharInfoArray);
		}

	////////////////////////////////////////////////////////////////////
	// Get kerning pairs array
	////////////////////////////////////////////////////////////////////

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    private static extern UInt32 GetKerningPairs(IntPtr GDIHandle, UInt32 NumberOfPairs, IntPtr PairArray);

	/// <summary>
	/// Gets kerning pairs array
	/// </summary>
	/// <param name="FirstChar">First character</param>
	/// <param name="LastChar">Last character</param>
	/// <returns>Array of kerning pairs</returns>
	public WinKerningPair[] GetKerningPairsApi
			(
			Int32	FirstChar,
			Int32	LastChar
			)
		{
		// get number of pairs
		Int32 Pairs = (Int32) GetKerningPairs(GDIHandle, 0, IntPtr.Zero);
		if(Pairs == 0) return(null);

		// allocate buffer to receive outline text metrics information
		AllocateBuffer(8 * Pairs);

		// get outline text metrics information
		if(GetKerningPairs(GDIHandle, (UInt32) Pairs, Buffer) == 0) ThrowSystemErrorException("Calling GetKerningPairs failed");

		// create list because the program will ignore pairs that are outside char range
		List<WinKerningPair> TempList = new List<WinKerningPair>();

		// kerning pairs from buffer
		for(Int32 Index = 0; Index < Pairs; Index++)
			{
			WinKerningPair KPair = new WinKerningPair(this);
			if(KPair.First >= FirstChar && KPair.First <= LastChar && KPair.Second >= FirstChar && KPair.Second <= LastChar) TempList.Add(KPair);
			}

		// free buffer for outline text metrics
		FreeBuffer();

		// list is empty
		if(TempList.Count == 0) return(null);

		// sort list
		TempList.Sort();

		// exit
		return(TempList.ToArray());
		}

	////////////////////////////////////////////////////////////////////
	// Get OUTLINETEXTMETRICW structure
	////////////////////////////////////////////////////////////////////

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    private static extern Int32 GetOutlineTextMetrics(IntPtr GDIHandle, Int32 BufferLength, IntPtr Buffer);

	/// <summary>
	/// Gets OUTLINETEXTMETRICW structure
	/// </summary>
	/// <returns>Outline text metric class</returns>
	public WinOutlineTextMetric GetOutlineTextMetricsApi()
		{
		// get buffer size
		Int32 BufSize = GetOutlineTextMetrics(GDIHandle, 0, IntPtr.Zero);
		if(BufSize == 0) ThrowSystemErrorException("Calling GetOutlineTextMetrics (get buffer size) failed");

		// allocate buffer to receive outline text metrics information
		AllocateBuffer(BufSize);

		// get outline text metrics information
		if(GetOutlineTextMetrics(GDIHandle, BufSize, Buffer) == 0) ThrowSystemErrorException("Calling GetOutlineTextMetrics failed");

		// create WinOutlineTextMetric class
		WinOutlineTextMetric WOTM = new WinOutlineTextMetric(this);

		// free buffer for outline text metrics
		FreeBuffer();

		// exit
		return(WOTM);
		}

	////////////////////////////////////////////////////////////////////
	// Get TEXTMETRICW structure
	////////////////////////////////////////////////////////////////////

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    private static extern Int32 GetTextMetrics(IntPtr GDIHandle, IntPtr Buffer);

	/// <summary>
	/// Gets TEXTMETRICW structure
	/// </summary>
	/// <returns>Text metric class</returns>
	public WinTextMetric GetTextMetricsApi()
		{
		// allocate buffer to receive outline text metrics information
		AllocateBuffer(57);

		// get outline text metrics information
		if(GetTextMetrics(GDIHandle, Buffer) == 0) ThrowSystemErrorException("Calling GetTextMetrics API failed.");

		// create WinOutlineTextMetric class
		WinTextMetric WTM = new WinTextMetric(this);

		// free buffer for outline text metrics
		FreeBuffer();

		// exit
		return(WTM);
		}

	////////////////////////////////////////////////////////////////////
	// Get font data tables
	////////////////////////////////////////////////////////////////////

	[DllImport("gdi32.dll", CharSet = CharSet.Auto, CallingConvention=CallingConvention.StdCall, SetLastError = true)]
	private static extern UInt32 GetFontData(IntPtr DeviceContextHandle, UInt32 Table, UInt32 Offset, IntPtr Buffer, UInt32 BufferLength);

	/// <summary>
	/// Gets font data tables
	/// </summary>
	/// <param name="Offset">Table offset</param>
	/// <param name="BufSize">Table size</param>
	/// <returns>Table info as byte array</returns>
	public Byte[] GetFontDataApi
			(
			Int32	Offset,
			Int32	BufSize
			)
		{
		// empty table
		if(BufSize == 0) return(null);

		// allocate buffer to receive outline text metrics information
		AllocateBuffer((Int32) BufSize);

		// get outline text metrics information
		if((Int32) GetFontData(GDIHandle, 0, (UInt32) Offset, Buffer, (UInt32) BufSize) != BufSize) ThrowSystemErrorException("Get font data file header failed");

		// copy api result buffer to managed memory buffer
		Byte[] DataBuffer = new Byte[BufSize];
		Marshal.Copy(Buffer, DataBuffer, 0, BufSize);
		BufPtr = 0;

		// free unmanaged memory buffer
		FreeBuffer();

		// exit
		return(DataBuffer);
		}

	////////////////////////////////////////////////////////////////////
	// Get glyph indices array
	////////////////////////////////////////////////////////////////////

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    private static extern Int32 GetGlyphIndices(IntPtr GDIHandle, IntPtr CharBuffer, Int32 CharCount,
        IntPtr GlyphArray, UInt32 GlyphOptions);

	/// <summary>
	/// Gets glyph indices array
	/// </summary>
	/// <param name="FirstChar">First character</param>
	/// <param name="LastChar">Last character</param>
	/// <returns>Array of glyph indices.</returns>
    public Int32[] GetGlyphIndicesApi
			(
			Int32		FirstChar,
			Int32		LastChar
			)
		{
		// character count
		Int32 CharCount = LastChar - FirstChar + 1;

		// allocate character table buffer in global memory (two bytes per char)
		IntPtr CharBuffer = Marshal.AllocHGlobal(2 * CharCount);

		// create array of all character codes between FirstChar and LastChar (we use Int16 because of Unicode)
		for(Int32 CharPtr = FirstChar; CharPtr <= LastChar; CharPtr++) Marshal.WriteInt16(CharBuffer, 2 * (CharPtr - FirstChar), (Int16) (CharPtr));

		// allocate memory for result
		AllocateBuffer(2 * CharCount);

		// get glyph numbers for all characters including non existing glyphs
		if(GetGlyphIndices(GDIHandle, CharBuffer, CharCount, Buffer, (UInt32) 0) != CharCount) ThrowSystemErrorException("Calling GetGlypeIndices failed");

		// get result array to managed code
		Int16[] GlyphIndex16 = ReadInt16Array(CharCount);

		// free local buffer
		Marshal.FreeHGlobal(CharBuffer);

		// free result buffer
		FreeBuffer();

		// convert to Int32
		Int32[] GlyphIndex32 = new Int32[GlyphIndex16.Length];
		for(Int32 Index = 0; Index < GlyphIndex16.Length; Index++) GlyphIndex32[Index] = (UInt16) GlyphIndex16[Index];

		// exit
		return(GlyphIndex32);
		}

	////////////////////////////////////////////////////////////////////
	// Allocate API result buffer
	////////////////////////////////////////////////////////////////////

	private void AllocateBuffer
			(
			Int32	Size
			)
		{
		// allocate memory for result
		Buffer = Marshal.AllocHGlobal(Size);
		BufPtr = 0;
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Free API result buffer
	////////////////////////////////////////////////////////////////////

	private void FreeBuffer()
		{
		// free buffer
		Marshal.FreeHGlobal(Buffer);
		Buffer = IntPtr.Zero;
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Align buffer pointer to 4 bytes boundry
	////////////////////////////////////////////////////////////////////

	internal void Align4()
		{
		BufPtr = (BufPtr + 3) & ~3;
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Read point (x, y) from data buffer
	////////////////////////////////////////////////////////////////////

	internal Point ReadWinPoint()
		{
		return(new Point(ReadInt32(), ReadInt32()));
		} 

	////////////////////////////////////////////////////////////////////
	// Read byte from data buffer
	////////////////////////////////////////////////////////////////////

	internal Byte ReadByte()
		{
		return(Marshal.ReadByte(Buffer, BufPtr++));
		}

	////////////////////////////////////////////////////////////////////
	// Read character from data buffer
	////////////////////////////////////////////////////////////////////

	internal Char ReadChar()
		{
		Char Value = (Char) Marshal.ReadInt16(Buffer, BufPtr);
		BufPtr += 2;
		return(Value);
		}

	////////////////////////////////////////////////////////////////////
	// Read short integer from data buffer
	////////////////////////////////////////////////////////////////////

	internal Int16 ReadInt16()
		{
		Int16 Value = Marshal.ReadInt16(Buffer, BufPtr);
		BufPtr += 2;
		return(Value);
		}

	////////////////////////////////////////////////////////////////////
	// Read unsigned short integer from data buffer
	////////////////////////////////////////////////////////////////////

	internal UInt16 ReadUInt16()
		{
		UInt16 Value = (UInt16) Marshal.ReadInt16(Buffer, BufPtr);
		BufPtr += 2;
		return(Value);
		}

	////////////////////////////////////////////////////////////////////
	// Read Int16 array from result buffer
	////////////////////////////////////////////////////////////////////

	internal Int16[] ReadInt16Array
			(
			Int32	Size
			)
		{
		// create active characters array
		Int16[] Result = new Int16[Size];
		Marshal.Copy(Buffer, Result, 0, Size);
		return(Result);
		}

	////////////////////////////////////////////////////////////////////
	// Read integers from data buffer
	////////////////////////////////////////////////////////////////////

	internal Int32 ReadInt32()
		{
		Int32 Value = Marshal.ReadInt32(Buffer, BufPtr);
		BufPtr += 4;
		return(Value);
		}

	////////////////////////////////////////////////////////////////////
	// Read Int32 array from result buffer
	////////////////////////////////////////////////////////////////////

	internal Int32[] ReadInt32Array
			(
			Int32	Size
			)
		{
		// create active characters array
		Int32[] Result = new Int32[Size];
		Marshal.Copy(Buffer, Result, 0, Size);
		return(Result);
		}

	////////////////////////////////////////////////////////////////////
	// Read unsigned integers from data buffer
	////////////////////////////////////////////////////////////////////

	internal UInt32 ReadUInt32()
		{
		UInt32 Value = (UInt32) Marshal.ReadInt32(Buffer, BufPtr);
		BufPtr += 4;
		return(Value);
		}

	////////////////////////////////////////////////////////////////////
	// Read string (null terminated) from data buffer
	////////////////////////////////////////////////////////////////////

	internal String ReadString()
		{
		Int32 Ptr = Marshal.ReadInt32(Buffer, BufPtr);
		BufPtr += 4;
		StringBuilder Str = new StringBuilder();
		for(;;)
			{
			Char Chr = (Char) Marshal.ReadInt16(Buffer, Ptr);
			if(Chr == 0) break;
			Str.Append(Chr);
			Ptr += 2;
			}
		return(Str.ToString());
		}

	////////////////////////////////////////////////////////////////////
	// Throw exception showing last system error
	////////////////////////////////////////////////////////////////////

	[DllImport("Kernel32.dll", CharSet = CharSet.Auto, CallingConvention=CallingConvention.StdCall, SetLastError = true)]
	private static extern UInt32 FormatMessage(UInt32 dwFlags, IntPtr lpSource, UInt32 dwMessageId, UInt32 dwLanguageId,
		IntPtr lpBuffer, UInt32 nSize, IntPtr Arguments);

	internal void ThrowSystemErrorException
			(
			String AppMsg
			)
		{
		const UInt32 FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;

		// error message
		StringBuilder ErrMsg = new StringBuilder(AppMsg);

		// get last system error
		UInt32 ErrCode = (UInt32) Marshal.GetLastWin32Error(); // GetLastError();
		if(ErrCode != 0)
			{
			// allocate buffer
			IntPtr ErrBuffer = Marshal.AllocHGlobal(1024);

			// add error code
			ErrMsg.AppendFormat("\r\nSystem error [0x{0:X8}]", ErrCode);

			// convert error code to text
			Int32 StrLen = (Int32) FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, ErrCode, 0, ErrBuffer, 1024, IntPtr.Zero);
			if(StrLen > 0)
				{
				ErrMsg.Append(" ");
				ErrMsg.Append(Marshal.PtrToStringAuto(ErrBuffer, StrLen));
				while(ErrMsg[ErrMsg.Length - 1] <= ' ') ErrMsg.Length--;
				}

			// free buffer
			Marshal.FreeHGlobal(ErrBuffer);
			}

		// unknown error
		else
			{
			ErrMsg.Append("\r\nUnknown error.");
			}

		// exit
		throw new ApplicationException(ErrMsg.ToString());
		}

	////////////////////////////////////////////////////////////////////
	// Build unit matrix in unmanaged memory
	////////////////////////////////////////////////////////////////////

	private IntPtr BuildUnitMarix()
		{
		// allocate buffer for transformation matrix
		IntPtr UnitMatrix = Marshal.AllocHGlobal(16);

		// set transformation matrix into unit matrix
		Marshal.WriteInt32(UnitMatrix, 0, 0x10000);			
		Marshal.WriteInt32(UnitMatrix, 4, 0);			
		Marshal.WriteInt32(UnitMatrix, 8, 0);			
		Marshal.WriteInt32(UnitMatrix, 12, 0x10000);			
		return(UnitMatrix);
		}

	////////////////////////////////////////////////////////////////////
	// Dispose
	////////////////////////////////////////////////////////////////////

	[DllImport("gdi32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
	private static extern IntPtr DeleteObject(IntPtr Handle);

	/// <summary>
	/// Dispose unmanaged resources
	/// </summary>
	public void Dispose()
		{
		// free unmanaged buffer
		Marshal.FreeHGlobal(Buffer);

		// restore original font
		SelectObject(GDIHandle, SavedFont);

		// delete font handle
		DeleteObject(FontHandle);
 
		// release device context handle
		GDI.ReleaseHdc(GDIHandle);

		// release GDI resources
		GDI.Dispose();

		// release bitmap
		BitMap.Dispose();

		// exit
		return;		
		}
	}
}
