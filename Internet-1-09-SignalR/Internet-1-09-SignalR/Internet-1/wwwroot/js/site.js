// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// SignalR bağlantısını oluştur
var connection = new signalR.HubConnectionBuilder()
  .withUrl("/generalhub")
  .build();

// Bağlantıyı başlat
connection
  .start()
  .then(function () {
    console.log("SignalR bağlantısı başarılı!");
  })
  .catch(function (err) {
    console.error("SignalR bağlantı hatası: " + err.toString());
  });

// Video eklendiğinde
connection.on("onVideoAdd", function (count) {
  console.log("Yeni video eklendi. Toplam aktif video sayısı: " + count);
  // Admin panelindeki video sayısını güncelle
  var videoCountElement = document.getElementById("videoCount");
  if (videoCountElement) {
    videoCountElement.textContent = count;
  }
});

// Video güncellendiğinde
connection.on("onVideoUpdate", function (count) {
  console.log("Video güncellendi. Toplam aktif video sayısı: " + count);
  // Admin panelindeki video sayısını güncelle
  var videoCountElement = document.getElementById("videoCount");
  if (videoCountElement) {
    videoCountElement.textContent = count;
  }
});

// Video silindiğinde
connection.on("onVideoDelete", function (count) {
  console.log("Video silindi. Toplam aktif video sayısı: " + count);
  // Admin panelindeki video sayısını güncelle
  var videoCountElement = document.getElementById("videoCount");
  if (videoCountElement) {
    videoCountElement.textContent = count;
  }
});

// Kullanıcı güncellendiğinde
connection.on("onUserUpdate", function (count) {
  console.log("Kullanıcı sayısı güncellendi: " + count);
  // Admin panelindeki kullanıcı sayısını güncelle
  var userCountElement = document.getElementById("userCount");
  if (userCountElement) {
    userCountElement.textContent = count;
  }
});

// Ders eklendiğinde
connection.on("onLessonAdd", function (count) {
  console.log("Yeni ders eklendi. Toplam aktif ders sayısı: " + count);
  // Admin panelindeki ders sayısını güncelle
  var lessonCountElement = document.getElementById("lessonCount");
  if (lessonCountElement) {
    lessonCountElement.textContent = count;
  }
});

// Ders güncellendiğinde
connection.on("onLessonUpdate", function (count) {
  console.log("Ders güncellendi. Toplam aktif ders sayısı: " + count);
  // Admin panelindeki ders sayısını güncelle
  var lessonCountElement = document.getElementById("lessonCount");
  if (lessonCountElement) {
    lessonCountElement.textContent = count;
  }
});

// Ders silindiğinde
connection.on("onLessonDelete", function (count) {
  console.log("Ders silindi. Toplam aktif ders sayısı: " + count);
  // Admin panelindeki ders sayısını güncelle
  var lessonCountElement = document.getElementById("lessonCount");
  if (lessonCountElement) {
    lessonCountElement.textContent = count;
  }
});
