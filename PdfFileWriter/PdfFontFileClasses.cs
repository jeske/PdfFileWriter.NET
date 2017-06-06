/////////////////////////////////////////////////////////////////////
//
//	PdfFileWriter
//	PDF File Write C# Class Library.
//
//	PdfFontFileClasses
//	Support classes for the PdfFontFile classs.
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

namespace PdfFileWriter
{
/////////////////////////////////////////////////////////////////////
// Font file header
/////////////////////////////////////////////////////////////////////

internal class FontFileHeader
	{
	internal UInt32	FileVersion;		// 0x00010000 for version 1.0.
	internal UInt16	NumTables;			// Number of tables.

	// 16 * (maximum power of 2 <= numTables)
	internal UInt16	SearchRange
		{
		get
			{
			Int32 Mask;
			for(Mask = 1; Mask <= NumTables; Mask <<= 1);
			return((UInt16) (Mask << 3));
			}
		}

	// Log2(maximum power of 2 <= numTables).
	internal UInt16	EntrySelector
		{
		get
			{
			Int32 Power;
			for(Power = 1; (1 << Power) <= NumTables; Power++);
			return((UInt16) (Power - 1));
			}
		}

	// NumTables x 16-searchRange.
	internal UInt16	RangeShift
		{
		get
			{
			return((UInt16) (16 * NumTables - SearchRange));
			}
		}
	}

/////////////////////////////////////////////////////////////////////
// Font file table record
/////////////////////////////////////////////////////////////////////

internal class TableRecord
	{
	internal UInt32	Tag;				// 4 -byte identifier
	internal UInt32	Checksum;			// Checksum for this table
	internal Int32	Offset;				// Offset from beginning of TrueType font file
	internal Int32	Length;				// Length of this table
	internal Byte[]	Data;				// table data in big endian format

	// constructor
	internal TableRecord
				(
				UInt32 Tag
				)
			{
			this.Tag = Tag;
			return;
			}
	}

/////////////////////////////////////////////////////////////////////
// 'cmap' encoding sub-table
/////////////////////////////////////////////////////////////////////

internal class cmapSubTbl : IComparable<cmapSubTbl>
	{
	internal UInt16		PlatformID;				// Platform ID. Should be 3 for windows
	internal UInt16		EncodingID;				// Platform-specific encoding ID. Should be 1 for Unicode and 0 for symbol
	internal UInt16		Format;					// Format number (the program supports format 0 or 4)
	internal UInt32		Offset;					// Byte offset from beginning of table to the sub-table for this encoding
	internal UInt16		Length;					// This is the length in bytes of the sub-table.
	internal UInt16		Language;				// this field is relevant to Macintosh (platform ID 1)
	internal UInt16		SegCount;				// (Format 4) SegCount.
	internal cmapSeg[]	SegArray;				// (Format 4) segment array
	internal UInt16[]		GlyphArray;				// glyph array translate character for format 0 or index for format 4 to glyph code

	// default constructor
	internal cmapSubTbl() {}

	// search constructor
	internal cmapSubTbl
			(
			UInt16		PlatformID,
			UInt16		EncodingID,
			UInt16		Format
			)
		{
		this.PlatformID = PlatformID;
		this.EncodingID = EncodingID;
		this.Format = Format;
		return;
		}

	// compare two sub-tables for sort and binary search
	public Int32 CompareTo
			(
			cmapSubTbl	Other
			)
		{
		if(this.PlatformID != Other.PlatformID) return(this.PlatformID - Other.PlatformID);
		if(this.EncodingID != Other.EncodingID) return(this.EncodingID - Other.EncodingID);
		return(this.Format - Other.Format);
		}

	// 2 x segCount
	internal UInt16 SegCountX2
		{
		get
			{
			return((UInt16) (2 * SegCount));
			}
		}

	// 2 * (maximum power of 2 <= numTables)
	internal UInt16 SearchRange
		{
		get
			{
			Int32 Mask;
			for(Mask = 1; Mask <= SegCount; Mask <<= 1);
			return((UInt16) Mask);
			}
		}

	// Log2(maximum power of 2 <= numTables).
	internal UInt16 EntrySelector
		{
		get
			{
			Int32 Power;
			for(Power = 1; (1 << Power) <= SegCount; Power++);
			return((UInt16) (Power - 1));
			}
		}

	// NumTables x 16-searchRange.
	internal UInt16 RangeShift
		{
		get
			{
			return((UInt16) (2 * SegCount - SearchRange));
			}
		}
	}

/////////////////////////////////////////////////////////////////////
// 'cmap' format 4 encoding sub-table segment record
/////////////////////////////////////////////////////////////////////

internal class cmapSeg : IComparable<cmapSeg>
	{
	internal UInt16	StartChar;				// Start character code for each segment. Array length=segCount
	internal UInt16	EndChar;				// End characterCode for each segment, last=0xFFFF. Array length=segCount
	internal Int16	IDDelta;				// Delta for all character codes in segment. Array length=segCount
	internal UInt16	IDRangeOffset;			// Offsets (in byte) into glyphIdArray or 0. Array length=segCount

	// search constructor
	internal cmapSeg
			(
			Int32	StartChar,
			Int32	EndChar,
			Int32	IDDelta,
			Int32	IDRangeOffset
			)
		{
		this.StartChar = (UInt16) StartChar;
		this.EndChar = (UInt16) EndChar;
		this.IDDelta = (Int16) IDDelta;
		this.IDRangeOffset = (UInt16) IDRangeOffset;
		return;
		}

	// search constructor
	internal cmapSeg
			(
			Int32	EndCount
			)
		{
		this.EndChar = (UInt16) EndCount;
		return;
		}

	// compare two records for sort and binary search
	public Int32 CompareTo
			(
			cmapSeg Other
			)
		{
		return(this.EndChar - Other.EndChar);
		}
	}

/////////////////////////////////////////////////////////////////////
// 'head' font file header table
/////////////////////////////////////////////////////////////////////

internal class headTable
	{
	internal UInt32	TableVersion;			// 0x00010000 for version 1.0.
	internal UInt32	FontRevision;			// Set by font manufacturer.
	internal UInt32	ChecksumAdjustment;		// font file overall checksum. To compute: set it to 0, sum the entire font, then store 0xB1B0AFBA - sum.
	internal UInt32	MagicNumber;			// Set to 0x5F0F3CF5.
	internal UInt16	Flags;					// Bit 0: Baseline for font at y=0;
											// Bit 1: Left sidebearing point at x=0;
											// Bit 2: Instructions may depend on point size; 
											// Bit 3: Force ppem to integer values for all internal scaler math; may use fractional
											//        ppem sizes if this bit is clear; 
											// Bit 4: Instructions may alter advance width (the advance widths might not scale linearly); 
											// Bits 5-10: These should be set according to Apple's specification.
											//        However, they are not implemented in OpenType. 
											// Bit 11: Font data is 'lossless,' as a result of having been compressed and decompressed
											//         with the Agfa MicroType Express engine.
											// Bit 12: Font converted (produce compatible metrics)
											// Bit 13: Font optimized for ClearType™. Note, fonts that rely on embedded bitmaps (EBDT)
											//         for rendering should not be considered optimized for ClearType,
											//		   and therefore should keep this bit cleared.
											// Bit 14: Reserved, set to 0
											// Bit 15: Reserved, set to 0 
	internal UInt16	UnitsPerEm;				// Valid range is from 16 to 16384. This value should be a power of 2 for fonts that have TrueType outlines.
	internal Int64	TimeCreated;			// Number of seconds since 12:00 midnight, January 1, 1904. 64-bit integer
	internal Int64	TimeModified;			// Number of seconds since 12:00 midnight, January 1, 1904. 64-bit integer
	internal Int16	xMin;					// For all glyph bounding boxes.
	internal Int16	yMin;					// For all glyph bounding boxes.
	internal Int16	xMax;					// For all glyph bounding boxes.
	internal Int16	yMax;					// For all glyph bounding boxes.
	internal UInt16	MacStyle;				// Bit 0: Bold (if set to 1); 
											// Bit 1: Italic (if set to 1) 
											// Bit 2: Underline (if set to 1) 
											// Bit 3: Outline (if set to 1) 
											// Bit 4: Shadow (if set to 1) 
											// Bit 5: Condensed (if set to 1) 
											// Bit 6: Extended (if set to 1) 
											// Bits 7-15: Reserved (set to 0).
	internal UInt16	LowestRecPPEM;			// Smallest readable size in pixels.
	internal Int16	FontDirectionHint;		// Deprecated (Set to 2). 
											// 0: Fully mixed directional glyphs; 
											// 1: Only strongly left to right; 
											// 2: Like 1 but also contains neutrals; 
											// -1: Only strongly right to left; 
											// -2: Like -1 but also contains neutrals. 1
	internal Int16	IndexToLocFormat;		// 0 for short offsets, 1 for long.
	internal Int16	glyphDataFormat;		// 0 for current format.
	}

/////////////////////////////////////////////////////////////////////
// 'head' horizontal header table
/////////////////////////////////////////////////////////////////////

internal class hheaTable
	{
	internal UInt32	TableVersion;			// 0x00010000 for version 1.0.
	internal Int16	Ascender;				// Typographic ascent. (Distance from baseline of highest ascender)
	internal Int16	Descender;				// Typographic descent. (Distance from baseline of lowest descender)
	internal Int16	LineGap;				// Typographic line gap. Negative LineGap values are treated as zero
											// in Windows 3.1, System 6, and System 7.
	internal UInt16	advanceWidthMax;		// Maximum advance width value in 'hmtx' table.
	internal Int16	minLeftSideBearing;		// Minimum left sidebearing value in 'hmtx' table.
	internal Int16	minRightSideBearing;	// Minimum right sidebearing value; calculated as Min(aw - lsb - (xMax - xMin)).
	internal Int16	xMaxExtent;				// Max(lsb + (xMax - xMin)).
	internal Int16	caretSlopeRise;			// Used to calculate the slope of the cursor (rise/run); 1 for vertical.
	internal Int16	caretSlopeRun;			// 0 for vertical.
	internal Int16	caretOffset;			// The amount by which a slanted highlight on a glyph needs to be shifted
											// to produce the best appearance. Set to 0 for non-slanted fonts
	internal Int16	Reserved1;				// set to 0
	internal Int16	Reserved2;				// set to 0
	internal Int16	Reserved3;				// set to 0
	internal Int16	Reserved4;				// set to 0
	internal Int16	metricDataFormat;		// 0 for current format.
	internal UInt16	numberOfHMetrics;		// Number of hMetric entries in 'hmtx' table
	}

/////////////////////////////////////////////////////////////////////
// 'maxp' font maximum values
/////////////////////////////////////////////////////////////////////

internal class maxpTable
	{
	internal UInt32	TableVersion;			// 0x00010000 for version 1.0.
	internal UInt16	numGlyphs;				// The number of glyphs in the font.
	internal UInt16	maxPoints;				// Maximum points in a non-composite glyph.
	internal UInt16	maxContours;			// Maximum contours in a non-composite glyph.
	internal UInt16	maxCompositePoints;		// Maximum points in a composite glyph.
	internal UInt16	maxCompositeContours;	// Maximum contours in a composite glyph.
	internal UInt16	maxZones;				// 1 if instructions do not use the twilight zone (Z0), or
											// 2 if instructions do use Z0; should be set to 2 in most cases.
	internal UInt16	maxTwilightPoints;		// Maximum points used in Z0.
	internal UInt16	maxStorage;				// Number of Storage Area locations.
	internal UInt16	maxFunctionDefs;		// Number of FDEFs.
	internal UInt16	maxInstructionDefs;		// Number of IDEFs.
	internal UInt16	maxStackElements;		// Maximum stack depth2.
	internal UInt16	maxSizeOfInstructions;	// Maximum byte count for glyph instructions.
	internal UInt16	maxComponentElements;	// Maximum number of components referenced at “top level” for any composite glyph.
	internal UInt16	maxComponentDepth;		// Maximum levels of recursion; 1 for simple components.
	}

/////////////////////////////////////////////////////////////////////
// Glyph table support
/////////////////////////////////////////////////////////////////////

// glyph flags for comosite glyphs
internal enum CompFlag
	{
	Arg1AndArg2AreWords = 1,			// bit0	If this is set, the arguments are words; otherwise, they are bytes.
	ArgsAreXYValues = 2,				// bit1	If this is set, the arguments are xy values; otherwise, they are points.
	RoundXYToGrid = 4,					// bit2	For the xy values if the preceding is true.
	WeHaveAScale = 8,					// bit3	This indicates that there is a simple scale for the component. Otherwise, scale = 1.0.
	Reserve = 0x10,						// bit4	This bit is reserved. Set it to 0.
	MoreComponents = 0x20,				// bit5	Indicates at least one more glyph after this one.
	WeHaveXYScale = 0x40,				// bit6	The x direction will use a different scale from the y direction.
	WeHave2By2 = 0x80,					// bit7	There is a 2 by 2 transformation that will be used to scale the component.
	WeHaveInstructions = 0x100,			// bit8	Following the last component are instructions for the composite character.
	UseMyMetrics = 0x200,				// bit9	If set, this forces the aw and lsb (and rsb) for the composite to be equal
										// to those from this original glyph. This works for hinted and unhinted characters.
	OverlapCompound = 0x400,			// bit10 Used by Apple in GX fonts.
	ScaledComponentOffset = 0x800,		// bit11 Composite designed to have the component offset scaled (designed for Apple rasterizer).
	UnscaledComponentOffset = 0x1000,	// bit12 Composite designed not to have the component offset scaled (designed for the Microsoft TrueType rasterizer).
	}
}
