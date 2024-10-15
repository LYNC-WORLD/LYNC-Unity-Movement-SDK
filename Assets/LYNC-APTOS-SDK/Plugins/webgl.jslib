mergeInto(LibraryManager.library, {
  WebGLLogin: function (url, gameObjectName) {
    var _url = UTF8ToString(url);
    var _gameObjectName = UTF8ToString(gameObjectName);
    var windowName = "LYNC - Auth";
    var windowFeatures = "width=600,height=400,top=200,left=200";

    var origin = encodeURIComponent(window.location.origin);
    window.open(_url + "&webGLOrigin=" + origin, windowName, windowFeatures);

    var i = 0;
    function ReadMessage(e) {
      if (e.data.target != "lync-auth" || i != 0) return;
      i++;
      SendMessage(_gameObjectName, "HandleWebGLMessage", e.data.message);
    }

    window.removeEventListener("message", ReadMessage);
    window.addEventListener("message", ReadMessage);
    i = 0;
  },
});
