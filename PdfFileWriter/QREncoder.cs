/////////////////////////////////////////////////////////////////////
//
//	PdfFileWriter
//	PDF File Write C# Class Library.
//
//	QREncoder
//	Encode text into QR Code.
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
////////////////////////////////////////////////////////////////////
// QR Code Encoder class
////////////////////////////////////////////////////////////////////

internal class QREncoder : QRCode
    {
	internal Boolean[,] OutputMatrix;

	////////////////////////////////////////////////////////////////////
	// Default constructor
	////////////////////////////////////////////////////////////////////

	internal QREncoder() {}

	////////////////////////////////////////////////////////////////////
	// Encode input byte array into QRCode boolean matrix
	////////////////////////////////////////////////////////////////////

	internal void EncodeQRCode
			(
			String			DataString,
			ErrorCorrection	ErrorCorrection,
			Int32			QuietZone
			)
		{
		// make sure data string is not empty
		if(String.IsNullOrEmpty(DataString)) throw new ApplicationException("Input data string is null or empty");

		// split input data string to segments delimited by SegmentMarker
		SegDataString = DataString.Split(new Char[] {PdfQRCode.SegmentMarker}, StringSplitOptions.RemoveEmptyEntries);

		// create QR code boolean aray
		EncodeQRCode(ErrorCorrection, QuietZone);
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Encode input byte array into QRCode boolean matrix
	////////////////////////////////////////////////////////////////////

	internal void EncodeQRCode
			(
			String[]		SegDataString,
			ErrorCorrection	ErrorCorrection,
			Int32			QuietZone
			)
		{
		// save data string array
		this.SegDataString = SegDataString;

		// create QR code boolean aray
		EncodeQRCode(ErrorCorrection, QuietZone);
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Encode input byte array into QRCode boolean matrix
	////////////////////////////////////////////////////////////////////

	private void EncodeQRCode
			(
			ErrorCorrection	ErrorCorrection,
			Int32			QuietZone
			)
		{
		// test input
		if(SegDataString == null || SegDataString.Length == 0) throw new ApplicationException("Input data strings are null or empty");

		// create encoding mode array
		SegEncodingMode = new EncodingMode[SegDataString.Length];

		// initialization
		Initialization(ErrorCorrection);

		// encode data
		EncodeData();

		// calculate error correction
		CalculateErrorCorrection();

		// iterleave data and error correction codewords
		InterleaveBlocks();

		// build base matrix
		BuildBaseMatrix();

		// load base matrix with data and error correction codewords
		LoadMatrixWithData();

		// data masking
		SelectBastMask();

		// add format information (error code level and mask code)
		AddFormatInformation();

		// output matrix size in pixels
		Int32 SidePix = MatrixDimension + 2 * QuietZone;
		OutputMatrix = new Boolean[SidePix, SidePix];

		// convert result matrix to output matrix
		for(Int32 Row = 0; Row < MatrixDimension; Row++)
			{
			for(Int32 Col = 0; Col < MatrixDimension; Col++)
				{
				if((ResultMatrix[Row, Col] & 1) != 0) OutputMatrix[QuietZone + Row, QuietZone + Col] = true;
				}
			}

		// output array
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Initialization
	////////////////////////////////////////////////////////////////////

	private void Initialization
			(
			ErrorCorrection	ErrorCorrection
			)
		{
		// save arguments
		this.ErrorCorrection = ErrorCorrection;

		// save error correction
		if(ErrorCorrection != ErrorCorrection.L && ErrorCorrection != ErrorCorrection.M &&
			ErrorCorrection != ErrorCorrection.Q && ErrorCorrection != ErrorCorrection.H)
				throw new ApplicationException("Invalid error correction mode. Must be L, M, Q or H.");

		// reset to tal encoded data bits
		EncodedDataBits = 0;

		// loop for all segments
		for(Int32 SegIndex = 0; SegIndex < SegDataString.Length; SegIndex++)
			{
			// input string length
			String DataStr = SegDataString[SegIndex];
			Int32 DataLength = DataStr.Length;

			// find encoding mode
			EncodingMode EncodingMode = EncodingMode.Numeric;
			for(Int32 Index = 0; Index < DataLength; Index++)
				{
				Int32 Value = (Int32) DataStr[Index];
				if(Value > 255) throw new ApplicationException("Input string characters must be 0 to 255.");
				Int32 Code = EncodingTable[Value];
				if(Code < 10) continue;
				if(Code < 45)
					{
					EncodingMode = EncodingMode.AlphaNumeric;
					continue;
					}
				EncodingMode = EncodingMode.Byte;
				break;			
				}

			// calculate required bit length
			Int32 DataBits = 4;
			switch(EncodingMode)
				{
				case EncodingMode.Numeric:
					DataBits += 10 * (DataLength / 3);
					if((DataLength % 3) == 1) DataBits += 4; 
					else if((DataLength % 3) == 2) DataBits += 7; 
					break;

				case EncodingMode.AlphaNumeric:
					DataBits += 11 * (DataLength / 2);
					if((DataLength & 1) != 0) DataBits += 6; 
					break;

				case EncodingMode.Byte:
					DataBits += 8 * DataLength;
					break;
				}

			SegEncodingMode[SegIndex] = EncodingMode;
			EncodedDataBits += DataBits;
			}

		// version is not defined yet, find best version
		Int32 TotalDataLenBits = 0;
		for(Version = 1; Version <= 40; Version++)
			{
			// number of bits on each side of the QR code square
			MatrixDimension = MatrixDimensionArray[Version];

			SetDataCodewordsLength();
			TotalDataLenBits = 0;
			for(Int32 Seg = 0; Seg < SegEncodingMode.Length; Seg++) TotalDataLenBits += DataLengthBits(SegEncodingMode[Seg]);
			if(EncodedDataBits + TotalDataLenBits <= MaxDataBits) break;
			}

		if(Version > 40) throw new ApplicationException("Input data string is too long");
		EncodedDataBits += TotalDataLenBits;
		return;
		}
			
	////////////////////////////////////////////////////////////////////
	// QRCode: Convert data to bit array
	////////////////////////////////////////////////////////////////////

	private void EncodeData()
		{
		// codewords array
		CodewordsArray = new Byte[MaxCodewords];

		// reset encoding members
		CodewordsPtr = 0;
		BitBuffer = 0;
		BitBufferLen = 0;

		// loop for all segments
		for(Int32 SegIndex = 0; SegIndex < SegDataString.Length; SegIndex++)
			{
			// input string length
			String DataStr = SegDataString[SegIndex];
			Int32 DataLength = DataStr.Length;

			// first 4 bits is mode indicator
			// numeric code indicator is 0001, alpha numeric 0010, byte 0100
			SaveBitsToCodewordsArray((Int32) SegEncodingMode[SegIndex], 4);

			// character count
			SaveBitsToCodewordsArray(DataLength, DataLengthBits(SegEncodingMode[SegIndex]));
			
			// switch based on encode mode
			switch(SegEncodingMode[SegIndex])
				{				
				// numeric mode
				case EncodingMode.Numeric: 
					// encode digits in groups of 3
					Int32 NumEnd = (DataLength / 3) * 3;
					for(Int32 Index = 0; Index < NumEnd; Index += 3) SaveBitsToCodewordsArray(
						100 * EncodingTable[(Int32) DataStr[Index]] + 10 * EncodingTable[(Int32) DataStr[Index + 1]] + EncodingTable[(Int32) DataStr[Index + 2]], 10);

					// we have one digit remaining
					if(DataLength - NumEnd == 1) SaveBitsToCodewordsArray(EncodingTable[(Int32) DataStr[NumEnd]], 4);

					// we have two digits remaining
					else if(DataLength - NumEnd == 2) SaveBitsToCodewordsArray(10 * EncodingTable[(Int32) DataStr[NumEnd]] + EncodingTable[(Int32) DataStr[NumEnd + 1]], 7);
					break;

				// alphanumeric mode
				case EncodingMode.AlphaNumeric: 
					// encode digits in groups of 2
					Int32 AlphaNumEnd = (DataLength / 2) * 2;
					for(Int32 Index = 0; Index < AlphaNumEnd; Index += 2)
						SaveBitsToCodewordsArray(45 * EncodingTable[(Int32) DataStr[Index]] + EncodingTable[(Int32) DataStr[Index + 1]], 11);

					// we have one character remaining
					if(DataLength - AlphaNumEnd == 1) SaveBitsToCodewordsArray(EncodingTable[(Int32) DataStr[AlphaNumEnd]], 6);
					break;
					

				// byte mode					
				case EncodingMode.Byte: 
					// append the data after mode and character count
					for(Int32 Index = 0; Index < DataLength; Index++) SaveBitsToCodewordsArray((Int32) DataStr[Index], 8);
					break;
				}
			}
			
		// set terminator
		if(EncodedDataBits < MaxDataBits) SaveBitsToCodewordsArray(0, MaxDataBits - EncodedDataBits < 4 ? MaxDataBits - EncodedDataBits : 4);

		// flush bit buffer
		if(BitBufferLen > 0) CodewordsArray[CodewordsPtr++] = (Byte) (BitBuffer >> 24);

		// add extra padding if there is still space
		Int32 PadEnd = MaxDataCodewords - CodewordsPtr;
		for(Int32 PadPtr = 0; PadPtr < PadEnd; PadPtr++) CodewordsArray[CodewordsPtr + PadPtr] = (Byte) ((PadPtr & 1) == 0 ? 0xEC : 0x11); 

		// exit
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Save data to codeword array
	////////////////////////////////////////////////////////////////////

	private void SaveBitsToCodewordsArray
			(
			Int32	Data,
			Int32	Bits
			)
		{
		BitBuffer |= (UInt32) Data << (32 - BitBufferLen - Bits);
		BitBufferLen += Bits;
		while(BitBufferLen >= 8)
			{
			CodewordsArray[CodewordsPtr++] = (Byte) (BitBuffer >> 24);
			BitBuffer <<= 8;
			BitBufferLen -= 8;
			}
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Calculate Error Correction
	////////////////////////////////////////////////////////////////////

	protected void CalculateErrorCorrection()
		{
		// set generator polynomial array
		Byte[] Generator = GenArray[ErrCorrCodewords - 7];

		// error correcion calculation buffer
		Int32 BufSize = Math.Max(DataCodewordsGroup1, DataCodewordsGroup2) + ErrCorrCodewords;
		Byte[] ErrCorrBuff = new Byte[BufSize];

		// initial number of data codewords
		Int32 DataCodewords = DataCodewordsGroup1;
		Int32 BuffLen = DataCodewords + ErrCorrCodewords;

		// codewords pointer
		Int32 DataCodewordsPtr = 0;

		// codewords buffer error correction pointer
		Int32 CodewordsArrayErrCorrPtr = MaxDataCodewords;

		// loop one block at a time
		Int32 TotalBlocks = BlocksGroup1 + BlocksGroup2;
		for(Int32 BlockNumber = 0; BlockNumber < TotalBlocks; BlockNumber++)
			{
			// switch to group2 data codewords
			if(BlockNumber == BlocksGroup1)
				{
				DataCodewords = DataCodewordsGroup2;
				BuffLen = DataCodewords + ErrCorrCodewords;
				}

			// copy next block of codewords to the buffer and clear the remaining part
			Array.Copy(CodewordsArray, DataCodewordsPtr, ErrCorrBuff, 0, DataCodewords);
			Array.Clear(ErrCorrBuff, DataCodewords, ErrCorrCodewords);

			// update codewords array to next buffer
			DataCodewordsPtr += DataCodewords;

			// error correction polynomial division
			PolynominalDivision(ErrCorrBuff, BuffLen, Generator, ErrCorrCodewords);

			// save error correction block			
			Array.Copy(ErrCorrBuff, DataCodewords, CodewordsArray, CodewordsArrayErrCorrPtr, ErrCorrCodewords);
			CodewordsArrayErrCorrPtr += ErrCorrCodewords;
			}
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Interleave data and error correction blocks
	////////////////////////////////////////////////////////////////////

	private void InterleaveBlocks()
		{
		// allocate temp codewords array
		Byte[] TempArray = new Byte[MaxCodewords];

		// total blocks
		Int32 TotalBlocks = BlocksGroup1 + BlocksGroup2;

		// create array of data blocks starting point
		Int32[] Start = new Int32[TotalBlocks];
		for(Int32 Index = 1; Index < TotalBlocks; Index++) Start[Index] = Start[Index - 1] + (Index <= BlocksGroup1 ? DataCodewordsGroup1 : DataCodewordsGroup2);

		// step one. iterleave base on group one length
		Int32 PtrEnd = DataCodewordsGroup1 * TotalBlocks;

		// iterleave group one and two
		Int32 Ptr;
		Int32 Block = 0;
		for(Ptr = 0; Ptr < PtrEnd; Ptr++)
			{
			TempArray[Ptr] = CodewordsArray[Start[Block]];
			Start[Block]++;
			Block++;
			if(Block == TotalBlocks) Block = 0;
			}

		// interleave group two
		if(DataCodewordsGroup2 > DataCodewordsGroup1)
			{
			// step one. iterleave base on group one length
			PtrEnd = MaxDataCodewords;

			Block = BlocksGroup1;
			for(; Ptr < PtrEnd; Ptr++)
				{
				TempArray[Ptr] = CodewordsArray[Start[Block]];
				Start[Block]++;
				Block++;
				if(Block == TotalBlocks) Block = BlocksGroup1;
				}
			}

		// create array of error correction blocks starting point
		Start[0] = MaxDataCodewords;
		for(Int32 Index = 1; Index < TotalBlocks; Index++) Start[Index] = Start[Index - 1] + ErrCorrCodewords;

		// step one. iterleave base on group one length

		// iterleave all groups
		PtrEnd = MaxCodewords;
		Block = 0;
		for(; Ptr < PtrEnd; Ptr++)
			{
			TempArray[Ptr] = CodewordsArray[Start[Block]];
			Start[Block]++;
			Block++;
			if(Block == TotalBlocks) Block = 0;
			}

		// save result
		CodewordsArray = TempArray;
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Load base matrix with data and error correction codewords
	////////////////////////////////////////////////////////////////////

	private void LoadMatrixWithData()
		{
		// input array pointer initialization
		Int32 Ptr = 0;
		Int32 PtrEnd = 8 * MaxCodewords;

		// bottom right corner of output matrix
		Int32 Row = MatrixDimension - 1;
		Int32 Col = MatrixDimension - 1;

		// step state
		Int32 State = 0;
		for(;;) 
			{
			// current module is data
			if((BaseMatrix[Row, Col] & NonData) == 0)
				{
				// load current module with
				if((CodewordsArray[Ptr >> 3] & (1 << (7 - (Ptr & 7)))) != 0) BaseMatrix[Row, Col] = DataBlack;
				if(++Ptr == PtrEnd) break;
				}

			// current module is non data and vertical timing line condition is on
			else if(Col == 6) Col--;

			// update matrix position to next module
			switch(State)
				{
				// going up: step one to the left
				case 0:
					Col--;
					State = 1;
					continue;

				// going up: step one row up and one column to the right
				case 1:
					Col++;
					Row--;
					// we are not at the top, go to state 0
					if(Row >= 0)
						{
						State = 0;
						continue;
						}
					// we are at the top, step two columns to the left and start going down
					Col -= 2;
					Row = 0;
					State = 2;
					continue;

				// going down: step one to the left
				case 2:
					Col--;
					State = 3;
					continue;

				// going down: step one row down and one column to the right
				case 3:
					Col++;
					Row++;
					// we are not at the bottom, go to state 2
					if(Row < MatrixDimension)
						{
						State = 2;
						continue;
						}
					// we are at the bottom, step two columns to the left and start going up
					Col -= 2;
					Row = MatrixDimension - 1;
					State = 0;
					continue;
				}
			}
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Select Mask
	////////////////////////////////////////////////////////////////////

	private void SelectBastMask()
		{
		Int32 BestScore = Int32.MaxValue;
		MaskCode = 0;

		for(Int32 TestMask = 0; TestMask < 8; TestMask++)
			{
			// apply mask
			ApplyMask(TestMask);

			// evaluate 4 test conditions
			Int32 Score = EvaluationCondition1();
			if(Score >= BestScore) continue;
			Score += EvaluationCondition2();
			if(Score >= BestScore) continue;
			Score += EvaluationCondition3();
			if(Score >= BestScore) continue;
			Score += EvaluationCondition4();
			if(Score >= BestScore) continue;

			// save as best mask so far
			ResultMatrix = MaskMatrix;
			MaskMatrix = null;
			BestScore = Score;
			MaskCode = TestMask;
			}
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Evaluation condition #1
	// 5 consecutive or more modules of the same color
	////////////////////////////////////////////////////////////////////

	private Int32 EvaluationCondition1()
		{
		Int32 Score = 0;

		// test rows
		for(Int32 Row = 0; Row < MatrixDimension; Row++)
			{
			Int32 Count = 1;
			for(Int32 Col = 1; Col < MatrixDimension; Col++)
				{
				// current cell is not the same color as the one before
				if(((MaskMatrix[Row, Col - 1] ^ MaskMatrix[Row, Col]) & 1) != 0)
					{
					if(Count >= 5) Score += Count - 2;
					Count = 0;
					}
				Count++;
				}

			// last run
			if(Count >= 5) Score += Count - 2;
			}

		// test columns
		for(Int32 Col = 0; Col < MatrixDimension; Col++)
			{
			Int32 Count = 1;
			for(Int32 Row = 1; Row < MatrixDimension; Row++)
				{
				// current cell is not the same color as the one before
				if(((MaskMatrix[Row - 1, Col] ^ MaskMatrix[Row, Col]) & 1) != 0)
					{
					if(Count >= 5) Score += Count - 2;
					Count = 0;
					}
				Count++;
				}

			// last run
			if(Count >= 5) Score += Count - 2;
			}
		return(Score);
		}

	////////////////////////////////////////////////////////////////////
	// Evaluation condition #2
	// same color in 2 by 2 area
	////////////////////////////////////////////////////////////////////

	private Int32 EvaluationCondition2()
		{
		Int32 Score = 0;
		// test rows
		for(Int32 Row = 1; Row < MatrixDimension; Row++) for(Int32 Col = 1; Col < MatrixDimension; Col++)
			{
			// all are black
			if(((MaskMatrix[Row - 1, Col - 1] & MaskMatrix[Row - 1, Col] & MaskMatrix[Row, Col - 1] & MaskMatrix[Row, Col]) & 1) != 0) Score += 3;

			// all are white
			else if(((MaskMatrix[Row - 1, Col - 1] | MaskMatrix[Row - 1, Col] | MaskMatrix[Row, Col - 1] | MaskMatrix[Row, Col]) & 1) == 0) Score += 3;
			}
		return(Score);
		}

	////////////////////////////////////////////////////////////////////
	// Evaluation condition #3
	// pattern dark, light, dark, dark, dark, light, dark
	// before or after 4 light modules
	////////////////////////////////////////////////////////////////////

	private Int32 EvaluationCondition3()
		{
		Int32 Score = 0;

		// test rows
		for(Int32 Row = 0; Row < MatrixDimension; Row++)
			{
			Int32 Start = 0;

			// look for a lignt run at least 4 modules
			for(Int32 Col = 0; Col < MatrixDimension; Col++)
				{
				// current cell is white
				if((MaskMatrix[Row, Col] & 1) == 0) continue;

				// more or equal to 4
				if(Col - Start >= 4)
					{
					// we have 4 or more white
					// test for pattern before the white space
					if(Start >= 7 && TestHorizontalDarkLight(Row, Start - 7)) Score += 40;

					// test for pattern after the white space
					if(MatrixDimension - Col >= 7 && TestHorizontalDarkLight(Row, Col))
						{
						Score += 40;
						Col += 6;
						}
					}

				// assume next one is white
				Start = Col + 1;
				}

			// last run
			if(MatrixDimension - Start >= 4 && Start >= 7 && TestHorizontalDarkLight(Row, Start - 7)) Score += 40;
			}

		// test columns
		for(Int32 Col = 0; Col < MatrixDimension; Col++)
			{
			Int32 Start = 0;

			// look for a lignt run at least 4 modules
			for(Int32 Row = 0; Row < MatrixDimension; Row++)
				{
				// current cell is white
				if((MaskMatrix[Row, Col] & 1) == 0) continue;

				// more or equal to 4
				if(Row - Start >= 4)
					{
					// we have 4 or more white
					// test for pattern before the white space
					if(Start >= 7 && TestVerticalDarkLight(Start - 7, Col)) Score += 40;

					// test for pattern after the white space
					if(MatrixDimension - Row >= 7 && TestVerticalDarkLight(Row, Col))
						{
						Score += 40;
						Row += 6;
						}
					}

				// assume next one is white
				Start = Row + 1;
				}

			// last run
			if(MatrixDimension - Start >= 4 && Start >= 7 && TestVerticalDarkLight(Start - 7, Col)) Score += 40;
			}

		// exit
		return(Score);
		}

	////////////////////////////////////////////////////////////////////
	// Evaluation condition #4
	// blak to white ratio
	////////////////////////////////////////////////////////////////////

	private Int32 EvaluationCondition4()
		{
		// count black cells
		Int32 Black = 0;
		for(Int32 Row = 0; Row < MatrixDimension; Row++) for(Int32 Col = 0; Col < MatrixDimension; Col++) if((MaskMatrix[Row, Col] & 1) != 0) Black++;

		// ratio
		Double Ratio = (Double) Black / (Double) (MatrixDimension * MatrixDimension);

		// there are more black than white
		if(Ratio > 0.55) return((Int32) (20.0 * (Ratio - 0.5)) * 10);
		else if(Ratio < 0.45) return((Int32) (20.0 * (0.5 - Ratio)) * 10);
		return(0);
		}

	////////////////////////////////////////////////////////////////////
	// Test horizontal dark light pattern
	////////////////////////////////////////////////////////////////////

	private Boolean TestHorizontalDarkLight
			(
			Int32	Row,
			Int32	Col
			)
		{
		return((MaskMatrix[Row, Col] & ~MaskMatrix[Row, Col + 1] & MaskMatrix[Row, Col + 2] & MaskMatrix[Row, Col + 3] &
					MaskMatrix[Row, Col + 4] & ~MaskMatrix[Row, Col + 5] & MaskMatrix[Row, Col + 6] & 1) != 0);
		}

	////////////////////////////////////////////////////////////////////
	// Test vertical dark light pattern
	////////////////////////////////////////////////////////////////////

	private Boolean TestVerticalDarkLight
			(
			Int32	Row,
			Int32	Col
			)
		{
		return((MaskMatrix[Row, Col] & ~MaskMatrix[Row + 1, Col] & MaskMatrix[Row + 2, Col] & MaskMatrix[Row + 3, Col] &
					MaskMatrix[Row + 4, Col] & ~MaskMatrix[Row + 5, Col] & MaskMatrix[Row + 6, Col] & 1) != 0);
		}

	////////////////////////////////////////////////////////////////////
	// Add format information
	// version, error correction code plus mask code
	////////////////////////////////////////////////////////////////////

	private void AddFormatInformation()
		{
		Int32 Mask;

		// version information
		if(Version >= 7)
			{
			Int32 Pos = MatrixDimension - 11;
			Int32 VerInfo = VersionInformation[Version - 7];

			// top right
			Mask = 1;
			for(Int32 Row = 0; Row < 6; Row++) for(Int32 Col = 0; Col < 3; Col++)
				{
				ResultMatrix[Row, Pos + Col] = (VerInfo & Mask) != 0 ? FixedBlack : FixedWhite;
				Mask <<= 1;
				}

			// bottom left
			Mask = 1;
			for(Int32 Col = 0; Col < 6; Col++) for(Int32 Row = 0; Row < 3; Row++)
				{
				ResultMatrix[Pos + Row, Col] =  (VerInfo & Mask) != 0 ? FixedBlack : FixedWhite;
				Mask <<= 1;
				}
			}

		// error correction code and mask number
		Int32 FormatInfo = FormatInfoArray[8 * (Int32) ErrorCorrection + MaskCode];
		Mask = 1;
		for(Int32 Index = 0; Index < 15; Index++)
			{
			Int32 FormatBit = (FormatInfo & Mask) != 0 ? FixedBlack : FixedWhite;
			Mask <<= 1;

			// horizontal line
			Int32 Col = FormatInfoHor[Index];
			if(Col < 0) Col = MatrixDimension + Col;
			ResultMatrix[8, Col] = (Byte) FormatBit;

			// vertical line
			Int32 Row = FormatInfoVer[Index];
			if(Row < 0) Row = MatrixDimension + Row;
			ResultMatrix[Row, 8] = (Byte) FormatBit;
			}
		return;
		}
    }
}
