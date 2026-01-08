// common scripts for the website

function outOfScrollPosition(root, element) {
    try {
        // Calculate the scrollbar width: offsetWidth includes the scrollbar, clientWidth doesn't
        var scrollbarWidth = root.offsetWidth - root.clientWidth;
        
        // For absolutely positioned element, adjust the 'right' property by scrollbar width
        // This moves the element left to prevent it from overlapping the scrollbar
        element.style.right = scrollbarWidth + "px";
    } catch (ex) {
        appInsights.TrackException(ex);
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




