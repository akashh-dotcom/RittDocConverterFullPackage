if (window.top != window.self) {
    var cssLink = document.createElement("link");
    cssLink.href = "/_Static/Css/error/app-error.css";
    cssLink.rel = "stylesheet";
    cssLink.type = "text/css";
    if (document.head) {
        document.head.appendChild(cssLink);
    }
    
}
