Imports System.IO

Public Class Form1

    'Caz Kit LZW
    'Caz Kit LZ77
    'https://moddingwiki.shikadi.net/wiki/LZW_Compression#QuickBasic
    'Rosetta code
    'https://github.com/Only3/LZW-Compression (Pubblico Dominio)
    'lord.marte@gmail.com
    '18-05-2023
    ' last version 01-03-2026

    Dim filetocompress As String = ""
    Dim filetodecompress As String = ""


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim ofd As OpenFileDialog = New OpenFileDialog
        ofd.FileName = ""
        ofd.Filter = "Tutti i files|*.*|File compressi LZW|*.lzw"
        ofd.Title = "Apri un file da comprimere..."

        If ofd.ShowDialog(Me) <> DialogResult.OK Then
            Return
        End If

        filetocompress = ofd.FileName
        filetodecompress = ofd.FileName
        Dim intFileLen As Long
        intFileLen = FileLen(filetocompress)

        Dim compressor As New SimpleLZWCompression()
        Dim compressor7 As New LZ77()

        If System.IO.Path.GetExtension(ofd.FileName) = ".lzw" Then
            'Decompressione LZW
            compressor.Decompress(filetodecompress)
        ElseIf System.IO.Path.GetExtension(ofd.FileName) = ".lz77" Then
            'Decompressione LZ77
            Using fs As New FileStream(filetodecompress, FileMode.Open, FileAccess.Read, FileShare.Read)
                Using reader As New StreamReader(fs)
                    Dim decompressed = compressor7.Decompress(fs, 0)
                    Dim outfile As String = "Decomp_" & System.IO.Path.GetFileNameWithoutExtension(ofd.SafeFileName)
                    Dim outPath As String = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filetodecompress), outfile)
                    File.WriteAllBytes(outPath, decompressed)
                End Using
            End Using

        Else
            If RadioButton1.Checked Then
                'Compressione LZW
                compressor.Compress(filetocompress)
            End If
            If RadioButton2.Checked Then
                ' Compressione LZ77 con FileStream

                Dim outputfile As String =
        Path.Combine(
            Path.GetDirectoryName(filetocompress),
            Path.GetFileName(filetocompress) & ".lz77")

                Using inputStream As New FileStream(filetocompress, FileMode.Open, FileAccess.Read, FileShare.Read)
                    Using outputStream As New FileStream(outputfile, FileMode.Create, FileAccess.Write, FileShare.None)

                        ' Leggiamo tutto il file in un buffer dimensionato correttamente
                        Dim fileLength As Long = inputStream.Length

                        If fileLength = 0 Then
                            MessageBox.Show("File vuoto.")
                            Return
                        End If

                        If fileLength > Integer.MaxValue Then
                            MessageBox.Show("File troppo grande per questa implementazione LZ77.")
                            Return
                        End If

                        Dim buffer(CInt(fileLength - 1)) As Byte
                        inputStream.Read(buffer, 0, buffer.Length)

                        Dim compressed As Byte() = compressor7.Compress(buffer)

                        outputStream.Write(compressed, 0, compressed.Length)

                    End Using
                End Using
            End If
        End If

        MsgBox("Operazione eseguita")
    End Sub
End Class



Public Class SimpleLZWCompression

    'Classe LZW incorporata

    Private EOF As Integer = -1
    Private BITS As Integer = 14
    Private HASHING_SHIFT As Integer = 4
    Private TABLE_SIZE As Integer = 18041
    Private MAX_VALUE As Integer = (1 << BITS) - 1
    Private MAX_CODE As Integer = MAX_VALUE - 1
    Private AppendChar(TABLE_SIZE) As Byte
    Private CodeValue(TABLE_SIZE) As Integer
    Private PrefixCode(TABLE_SIZE) As Integer
    Private Input As IO.BinaryReader = Nothing
    Private Output As IO.BinaryWriter = Nothing

    Public Function Compress(File As String)
        Input = New IO.BinaryReader(IO.File.Open(File, IO.FileMode.Open))

        Output = New IO.BinaryWriter(IO.File.Open(File & ".lzw", IO.FileMode.OpenOrCreate, IO.FileAccess.Write))    'By lord.marte@gmail.com
        Dim Index As Integer = 0, Character As Integer = 0
        Dim StringCode As Integer = 0, NextCode As Integer = 256
        For i = 0 To TABLE_SIZE - 1
            CodeValue(i) = -1
        Next
        StringCode = ReadByte()
        Character = ReadByte()
        While Character <> -1
            Index = Match(StringCode, Character)
            If CodeValue(Index) <> -1 Then
                StringCode = CodeValue(Index)
            Else
                If NextCode <= MAX_CODE Then
                    CodeValue(Index) = NextCode
                    NextCode += 1
                    PrefixCode(Index) = StringCode
                    AppendChar(Index) = CByte(Character)
                End If
                OutputCode(StringCode)
                StringCode = Character
            End If
            Character = ReadByte()
        End While
        OutputCode(StringCode)
        OutputCode(MAX_VALUE)
        OutputCode(0)
        Input.Close()   'by lord.marte@gmil.com
        Output.Close()
        Dim intFileLen As Long
        intFileLen = FileLen(File & ".lzw")

        Return Nothing
    End Function

    Public Function Decompress(File As String)
        Input = New IO.BinaryReader(IO.File.Open(File, IO.FileMode.Open))
        Dim outfile As String = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(File), "decompresso_" & System.IO.Path.GetFileNameWithoutExtension(File))
        Output = New IO.BinaryWriter(IO.File.Open(outfile, IO.FileMode.OpenOrCreate, IO.FileAccess.Write))

        Dim i As Integer
        Dim NewCode As Integer, OldCode As Integer
        Dim CurrCode As Integer, NextCode As Integer = 256
        Dim Character As Byte, DecodeStack(TABLE_SIZE) As Byte
        OldCode = InputCode()
        Character = CType(OldCode, Byte)
        Output.Write(CByte(OldCode))
        NewCode = InputCode()
        While NewCode <> MAX_VALUE
            If NewCode >= NextCode Then
                DecodeStack(0) = Character
                i = 1
                CurrCode = OldCode
            Else
                i = 0
                CurrCode = NewCode
            End If
            While CurrCode > 255
                DecodeStack(i) = AppendChar(CurrCode)
                i = i + 1
                If i >= MAX_CODE Then Throw New Exception("CurrCode Exception.")
                CurrCode = PrefixCode(CurrCode)
            End While
            DecodeStack(i) = CType(CurrCode, Byte)
            Character = DecodeStack(i)
            While i >= 0
                Output.Write(DecodeStack(i))
                i = i - 1
            End While
            If NextCode <= MAX_CODE Then
                PrefixCode(NextCode) = OldCode
                AppendChar(NextCode) = Character
                NextCode += 1
            End If
            OldCode = NewCode
            NewCode = InputCode()
        End While
        Input.Close()
        Output.Close()      'by lord.marte@gmail.com
        OldCode = Nothing
        NewCode = Nothing
        Character = Nothing
        Return Nothing
    End Function

    Private Sub OutputCode(Code As Integer)
        Static Buffer As Long = 0, Count As Integer = 0
        Buffer = Buffer Or (Code << (32 - BITS - Count))
        Count += BITS
        While Count >= 8
            Output.Write(CByte((Buffer >> 24) And 255))
            Buffer <<= 8
            Count -= 8
        End While
    End Sub

    Private Function InputCode() As Integer
        Dim Value As Long
        Static Buffer As Long = 0, Count As Integer = 0
        Static Mask32 As Long = CLng(2 ^ 32) - 1
        While Count <= 24
            Buffer = (Buffer Or ReadByte() << (24 - Count)) And Mask32
            Count += 8
        End While
        Value = (Buffer >> 32 - BITS) And Mask32
        Buffer = (Buffer << BITS) And Mask32
        Count -= BITS
        Return CInt(Value)
    End Function

    Private Function Match(Prefix As Integer, Character As Integer) As Integer
        Dim Index As Integer = 0, Offset As Integer = 0
        Index = CInt((Character << HASHING_SHIFT) Xor Prefix)
        If Index = 0 Then Offset = 1 Else Offset = TABLE_SIZE - Index
        While True
            If CodeValue(Index) = -1 Then Return Index
            If PrefixCode(Index) = Prefix And AppendChar(Index) = Character Then Return Index
            Index -= Offset
            If Index < 0 Then Index += TABLE_SIZE
        End While
        Return Nothing
    End Function

    Private Function ReadByte() As Integer
        Dim B(1) As Byte
        If Input.Read(B, 0, 1) = 0 Then Return -1
        Return B(0)
    End Function

    'Fine Classe LZW
End Class



Public Class LZ77
    'Funzione per decompreimere LZ77 altro formato.
    'Se lavori con ROM (come sembra)
    'Nel mondo ROM hacking si usano soprattutto:
    'LZ77 (formato GBA 0x10) → veloce, semplice
    'LZ77 + Huffman (Deflate) → migliore compressione
    'LZSS ottimizzato → spesso migliore di LZW
    'Se il tuo target è GBA/NDS, LZW è una scelta poco comune.

    'https://board.romhackersworld.eu/thread/12810-vb-net-snippet-lz77-dekompression/

    Public Function Decompress(ByVal s As Stream, Optional ByVal offset As Integer = 0) As Byte()
        Dim lzSize As Integer
        Dim DataLeft As Integer
        Dim dcb As Byte
        Dim j As Integer
        Dim br As BinaryReader = New BinaryReader(s)
        br.BaseStream.Position = offset
        lzSize = br.ReadInt32()
        lzSize >>= 8
        DataLeft = lzSize
        Dim Destination(lzSize) As Byte
        'On Error Resume Next
        While Not DataLeft <= 0
            ' Read Decode Byte
            dcb = br.ReadByte
            For i = 1 To 8
                Dim x As Byte = CByte((128 / (2 ^ (i - 1))))
                If CInt((dcb And x) >> (2 ^ (i - 1))) = 0 Then
                    If j >= lzSize Then Exit While
                    Destination(j) = br.ReadByte
                    j += 1
                    DataLeft -= 1
                Else
                    Dim rPos As Integer, howManyByte As Integer
                    Dim tmp As Integer = br.ReadUInt16
                    Dim tmpRev As Integer = (tmp >> 8) + ((tmp And 255) << 8)
                    howManyByte = ((tmpRev And 61440) >> 12) + 3
                    rPos = (tmpRev And 4095) + 1
                    For i2 = 0 To howManyByte - 1
                        Destination(j + i2) = Destination(j - rPos + i2)
                        DataLeft -= 1
                    Next
                    j += howManyByte
                End If
            Next
        End While
        Return Destination
    End Function

    Public Function Compress(input As Byte()) As Byte()
        Dim output As New List(Of Byte)

        ' Header (size << 8)
        Dim size As Integer = input.Length
        Dim header As Integer = size << 8
        output.AddRange(BitConverter.GetBytes(header))

        Dim pos As Integer = 0

        While pos < input.Length
            Dim flagPos As Integer = output.Count
            output.Add(0) ' Placeholder per decode byte

            Dim flagByte As Byte = 0

            For i As Integer = 0 To 7
                If pos >= input.Length Then Exit For

                Dim bestLength As Integer = 0
                Dim bestOffset As Integer = 0

                ' Cerca match nei 4096 byte precedenti
                Dim searchStart As Integer = Math.Max(0, pos - 4096)

                For j As Integer = searchStart To pos - 1
                    Dim length As Integer = 0
                    While length < 18 AndAlso
                      pos + length < input.Length AndAlso
                      input(j + length) = input(pos + length)
                        length += 1
                    End While

                    If length >= 3 AndAlso length > bestLength Then
                        bestLength = length
                        bestOffset = pos - j
                    End If
                Next

                If bestLength >= 3 Then
                    ' Imposta bit compressione
                    flagByte = flagByte Or CByte(128 >> i)

                    Dim lengthField As Integer = (bestLength - 3) << 12
                    Dim offsetField As Integer = (bestOffset - 1)

                    Dim value As UShort = CUShort(lengthField Or offsetField)

                    ' Scrittura big endian
                    output.Add(CByte((value >> 8) And &HFF))
                    output.Add(CByte(value And &HFF))

                    pos += bestLength
                Else
                    output.Add(input(pos))
                    pos += 1
                End If
            Next

            output(flagPos) = flagByte
        End While

        Return output.ToArray()
    End Function

End Class