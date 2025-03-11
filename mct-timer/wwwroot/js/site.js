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

function checkpass(pass) {

    const capt = new RegExp('(?=.*[A-Z])');
    const small = new RegExp('(?=.*[a-z])');
    const numb = new RegExp('(?=.*\d)');
    const lngth = new RegExp('(?=.{6})');

    if (capt.test(pass)
        && small.test(pass)
        && numb.test(pass)
        && lngth.test(pass)) {
         return true;
    }

   return false;
}




