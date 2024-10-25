// common scripts for the website

function outOfScrollPosition(root, element) {
    try {
        var rPos = root.offsetWidth - root.scrollWidth
        element.style.right = rPos + "px";
    } catch (ex) {
        appInsights.TrackException(e);
        console.log(ex);
    }
}


