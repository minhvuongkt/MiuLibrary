# 📚 Miu Library (C#)
**Thư viện hỗ trợ giao tiếp Socket và các tiện ích hữu ích trong C#**

## ✨ Tính năng chính

### 🖧 1. Kết nối Socket Client
- Hỗ trợ kết nối đến Server thông qua `IP` và `Port`.
- Dễ dàng sử dụng thông qua hàm `SocketClient`.

### 🛠 2. Mục Utils - Công cụ hỗ trợ
- **Sinh số ngẫu nhiên**.
- **Chuyển đổi thời gian** từ mili giây sang giây.
- **Ghi log** dễ dàng bằng hàm:
  ```csharp
  WriteLog("Nội dung log");
  ```
- Và nhiều tiện ích khác...

### 📦 3. Model `Message` cho Socket
- Dùng để truyền nhận dữ liệu với 2 tham số chính:
  ```csharp
  public class Message
  {
      public string MessageType { get; set; }
      public object Data { get; set; }
  }
  ```
- Giúp linh hoạt trong việc truyền tải nhiều kiểu dữ liệu khác nhau.

## 🚀 Hướng dẫn sử dụng
> **Cập nhật sau** – sẽ có hướng dẫn chi tiết cách sử dụng thư viện.

## 📜 Giấy phép
Miu Library được phát triển với mục đích hỗ trợ lập trình viên, sử dụng tự do theo nhu cầu cá nhân hoặc doanh nghiệp.
