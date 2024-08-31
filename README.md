# Bayer Raw Image Viewer

[Click here for English version](#bayer-raw-image-viewer-english)

Bayer Raw Image Viewer 是一個使用 .NET WinForm 和 C# 開發的 Windows 應用程式，專門用於將 Bayer 格式的 RAW 影像檔案進行 demosaic 處理，並提供檢視和轉存為常見影像格式的功能。

![Bayer Raw Image Viewer 介面](https://github.com/user-attachments/assets/9f5fc8cd-8c27-4500-9042-341bcaf1e96e)

## 功能特色

- 支援多種 RAW 影像格式：MIPI Raw、Packed Raw 和 Unpacked Raw
- 可支援的影像深度：8 bit、10 bit、12 bit 和 14 bit
- 支援輸入 stride，能夠正確處理每個 row 後面有額外添加的 byte padding 的 raw
- 影像後處理選項：自動白平衡 (AWB) 和 OB 扣除
- 靈活的輸出選項：可轉存為常見的影像格式如 BMP 和 JPEG

## RAW 格式說明

### Unpacked Raw 格式

Unpacked Raw 是一種常見的 RAW 影像格式，在本程式中我們將其稱為 Unpacked Raw。這種格式的特點是：

- 對於 10 bit、12 bit 和 14 bit 的 raw 影像，每個 pixel 都使用兩個 byte 來儲存。
- 8 bit unpacked raw 是個例外，它使用一個 byte 來儲存一個 pixel。

以下是 10-bit Unpacked Raw 的示意圖：

![10-bit Unpacked Raw 示意圖](https://github.com/user-attachments/assets/7661830a-10be-43a0-885b-bfeab5ba8895)

### Packed Raw 格式

Packed Raw 是某些特定 ISP dump 出來的 raw 格式。它與 Unpacked Raw 的主要差異在於資料的儲存效率：

- Packed Raw 不會浪費任何一個 bit。
- 以 10-bit raw 為例：
  - 第一個 pixel 占用了第一個 byte 完整 8 個 bit 和第二個 byte 的 2 個 low bit。
  - 第二個 pixel 則佔用第二個 byte 的 6 個 high bit，與第三個 byte 的 4 個 low bit。
  - 以此類推。

以下是 10-bit Packed Raw 的示意圖：

![10-bit Packed Raw 示意圖](https://github.com/user-attachments/assets/c41fbc3b-5e77-4c3b-8f7c-180da2d4bca5)

在這些示意圖中：
- A0-A9 代表第一個 10-bit pixel
- B0-B9 代表第二個 10-bit pixel
- 以此類推

## 使用說明

1. 開啟 Bayer Raw Image Viewer 程式。
2. 在程式介面中選擇正確的 RAW 影像類型（Raw Type）。
3. 設定適當的參數：
   - Bit 深度
   - 影像解析度（Resolution）
   - Stride 值
4. 將 RAW 影像檔案直接拖曳到程式介面上，然後放開。
5. 程式將立即處理並顯示影像預覽。
6. 如需要，可以使用後處理選項（如 AWB、OB 扣除）來調整影像。
7. 可以將處理後的影像儲存為 BMP 或 JPEG 等常見格式。

## 貢獻

歡迎提交問題回報和提出拉取請求。對於重大更改，請先開 issue 討論您想要改變的內容。

## 授權條款

[MIT](https://choosealicense.com/licenses/mit/)

## 聯絡方式

如有任何問題或建議，請透過 [issues](https://github.com/benfzc/BayerRawImageViewer/issues/new) 與我聯絡。

---

# Bayer Raw Image Viewer (English)

Bayer Raw Image Viewer is a Windows application developed using .NET WinForm and C#, specifically designed to perform demosaicing on Bayer format RAW image files, and provide viewing and exporting functionality to common image formats.

![Bayer Raw Image Viewer Screenshot](https://github.com/user-attachments/assets/9f5fc8cd-8c27-4500-9042-341bcaf1e96e)

## Features

- Supports various RAW image formats: MIPI Raw, Packed Raw, and Unpacked Raw
- Supported image depths: 8 bit, 10 bit, 12 bit, and 14 bit
- Supports stride input, capable of correctly handling raw files with additional byte padding after each row
- Image post-processing options: Auto White Balance (AWB) and OB (Optical Black) subtraction
- Flexible output options: Can export to common image formats such as BMP and JPEG

## RAW Format Explanation

### Unpacked Raw Format

Unpacked Raw is a common RAW image format, which we refer to as Unpacked Raw in this program. The characteristics of this format are:

- For 10 bit, 12 bit, and 14 bit raw images, each pixel is stored using two bytes.
- 8 bit unpacked raw is an exception, using one byte to store one pixel.

Below is a diagram of 10-bit Unpacked Raw:

![10-bit Unpacked Raw Diagram](https://github.com/user-attachments/assets/7661830a-10be-43a0-885b-bfeab5ba8895)

### Packed Raw Format

Packed Raw is a raw format dumped by certain specific ISPs. Its main difference from Unpacked Raw is in data storage efficiency:

- Packed Raw does not waste any bits.
- Taking 10-bit raw as an example:
  - The first pixel occupies all 8 bits of the first byte and 2 low bits of the second byte.
  - The second pixel then occupies 6 high bits of the second byte and 4 low bits of the third byte.
  - And so on.

Below is a diagram of 10-bit Packed Raw:

![10-bit Packed Raw Diagram](https://github.com/user-attachments/assets/c41fbc3b-5e77-4c3b-8f7c-180da2d4bca5)

In these diagrams:
- A0-A9 represents the first 10-bit pixel
- B0-B9 represents the second 10-bit pixel
- And so on

## Usage Instructions

1. Open the Bayer Raw Image Viewer program.
2. Select the correct RAW image type (Raw Type) in the program interface.
3. Set appropriate parameters:
   - Bit depth
   - Image resolution
   - Stride value
4. Drag and drop the RAW image file directly onto the program interface.
5. The program will immediately process and display the image preview.
6. If needed, you can use post-processing options (such as AWB, OB subtraction) to adjust the image.
7. The processed image can be saved in common formats such as BMP or JPEG.

## Contribution

We welcome issue reports and pull requests. For major changes, please open an issue first to discuss what you would like to change.

## License

[MIT](https://choosealicense.com/licenses/mit/)

## Contact

If you have any questions or suggestions, please contact me through [issues](https://github.com/benfzc/BayerRawImageViewer/issues).
