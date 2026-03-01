# SimpleCompression

Implementazione in VB.NET degli algoritmi di compressione:

- LZW (Lempel-Ziv-Welch)
- LZ77 (formato compatibile header size << 8)

## 🚀 Funzionalità

- Compressione LZW
- Decompressione LZW
- Compressione LZ77
- Decompressione LZ77
- Supporto file binari grandi
- Gestione sicura degli errori stream

## 📦 Algoritmi implementati

### LZW
Algoritmo dictionary-based ideale per:
- CSV
- Testo
- File strutturati

### LZ77
Sliding window (4096 bytes)
Match length: 3–18 bytes
Formato header: size << 8

## 🧪 Testato con

- CSV
- BMP non compressi
- TIFF raw
- PDF
- DOCX

## ⚠ Note

LZW non è efficace su file già compressi (DOCX, PDF moderni).
LZ77 offre prestazioni migliori su dati binari ripetitivi.

## 🛠 Framework

- .NET Framework / .NET 6+
- Windows Forms

## 📜 Licenza

MIT License